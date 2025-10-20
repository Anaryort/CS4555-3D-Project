using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class NPC_Wander : MonoBehaviour
{
    [Header("Wander Area (Circle on XZ)")]
    [Tooltip("Center of the wander circle. If null, uses the agent's start position.")]
    public Transform areaCenter;
    public float wanderRadius = 10f;
    [Tooltip("If true, pick points on the circle edge (like your 2D edges). If false, anywhere inside the circle.")]
    public bool edgeOnly = true;

    [Header("Timing & Movement")]
    public float pauseDuration = 1f;        // wait at each stop
    public float arrivalThreshold = 0.25f;  // extra tolerance beyond stoppingDistance
    public float repickIfStuckSeconds = 3f; // repick if barely moving
    public float sampleRadius = 3f;         // NavMesh.SamplePosition radius near target

    [Header("Agent")]
    [Tooltip("Assign the NavMeshAgent on your child (Aias (1)).")]
    public NavMeshAgent agent;
    public float agentSpeed = 2f;
    public bool snapToNavmeshOnStart = true;

    [Header("Rotation (like Patrol)")]
    [Tooltip("Which transform rotates visually. Leave null to rotate the agent's transform.")]
    public Transform rotateTarget;
    public float rotateSpeed = 10f;
    [Tooltip("If your model faces sideways, set 90 or -90.")]
    public float modelYawOffset = 0f;

    [Header("Animator (optional)")]
    public Animator animator;               // auto-find in children if null
    public string speedParam = "Speed";
    public bool setAnimatorSpeed = true;

    // --- internals ---
    private bool isPaused;
    private Vector3 currentTarget;
    private Vector3 centerPos;
    private float stuckTimer;

    void Awake()
    {
        if (!agent)
        {
            agent = GetComponent<NavMeshAgent>();
            if (!agent) agent = GetComponentInChildren<NavMeshAgent>(true);
        }
        if (!agent)
        {
            Debug.LogError($"{name}: NavMeshAgent not found. Assign it in the Inspector.");
            enabled = false; return;
        }

        if (!animator) animator = GetComponentInChildren<Animator>();
        if (!rotateTarget) rotateTarget = agent.transform;

        // We rotate manually (just like Patrol).
        agent.updateRotation = false;
        agent.updateUpAxis   = true;

        agent.speed = agentSpeed;
        agent.autoBraking = true;
        agent.stoppingDistance = Mathf.Max(agent.stoppingDistance, arrivalThreshold);
    }

    void Start()
    {
        if (snapToNavmeshOnStart) SnapToNearestNavmesh();

        centerPos = areaCenter ? areaCenter.position : agent.transform.position;
        StartCoroutine(PauseAndPickNewDestination());
    }

    void Update()
    {
        if (!agent || !agent.isOnNavMesh) return;

        // Smooth yaw toward desired velocity (like Patrol)
        Vector3 v = agent.desiredVelocity; v.y = 0f;
        if (v.sqrMagnitude > 0.0001f && rotateTarget)
        {
            var look = Quaternion.LookRotation(v) * Quaternion.Euler(0f, modelYawOffset, 0f);
            rotateTarget.rotation = Quaternion.Slerp(rotateTarget.rotation, look, rotateSpeed * Time.deltaTime);
        }

        // Animator speed (optional)
        if (setAnimatorSpeed && animator && animator.HasParameterOfType(speedParam, AnimatorControllerParameterType.Float))
        {
            float flatSpeed = new Vector2(agent.velocity.x, agent.velocity.z).magnitude;
            animator.SetFloat(speedParam, flatSpeed);
        }

        if (isPaused) return;

        // Arrived?
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + arrivalThreshold)
        {
            StartCoroutine(PauseAndPickNewDestination());
            return;
        }

        // Stuck detection
        if (agent.velocity.sqrMagnitude < 0.0004f) stuckTimer += Time.deltaTime;
        else stuckTimer = 0f;

        if (stuckTimer >= repickIfStuckSeconds || agent.pathStatus == NavMeshPathStatus.PathInvalid)
            StartCoroutine(PauseAndPickNewDestination());
    }

    IEnumerator PauseAndPickNewDestination()
    {
        isPaused = true;

        agent.isStopped = true;
        agent.ResetPath();

        yield return new WaitForSeconds(pauseDuration);

        currentTarget = edgeOnly ? GetRandomPointOnCircleXZ() : GetRandomPointInCircleXZ();

        if (NavMesh.SamplePosition(currentTarget, out var hit, sampleRadius, NavMesh.AllAreas) ||
            NavMesh.SamplePosition(currentTarget, out hit, sampleRadius * 3f, NavMesh.AllAreas))
        {
            agent.speed = agentSpeed;               // sync runtime tweaks
            agent.SetDestination(hit.position);
        }
        else
        {
            Debug.LogWarning($"{name}: Wander target off NavMesh; will repick.");
        }

        agent.isStopped = false;
        stuckTimer = 0f;
        isPaused = false;
    }

    // -------- helpers --------

    Vector3 GetRandomPointOnCircleXZ()
    {
        float ang = Random.Range(0f, 2f * Mathf.PI);
        float x = centerPos.x + Mathf.Cos(ang) * wanderRadius;
        float z = centerPos.z + Mathf.Sin(ang) * wanderRadius;
        return new Vector3(x, centerPos.y, z);
    }

    Vector3 GetRandomPointInCircleXZ()
    {
        // Uniform inside a circle using sqrt of random radius
        float ang = Random.Range(0f, 2f * Mathf.PI);
        float r = wanderRadius * Mathf.Sqrt(Random.Range(0f, 1f));
        float x = centerPos.x + Mathf.Cos(ang) * r;
        float z = centerPos.z + Mathf.Sin(ang) * r;
        return new Vector3(x, centerPos.y, z);
    }

    void SnapToNearestNavmesh()
    {
        var t = agent.transform;
        if (NavMesh.SamplePosition(t.position, out var hit, 100f, NavMesh.AllAreas))
        {
            agent.Warp(hit.position);
        }
        else if (Physics.Raycast(t.position + Vector3.up * 200f, Vector3.down, out var rh, 1000f, ~0))
        {
            if (NavMesh.SamplePosition(rh.point, out hit, 5f, NavMesh.AllAreas))
                agent.Warp(hit.position);
        }
    }

    void OnDrawGizmosSelected()
    {
        // Visualize the circle
        Vector3 center = areaCenter ? areaCenter.position
                                    : (agent ? agent.transform.position : transform.position);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(center, wanderRadius);

        // Show current target
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(currentTarget + Vector3.up * 0.05f, 0.12f);
    }
}

public static class AnimatorExt
{
    public static bool HasParameterOfType(this Animator a, string name, AnimatorControllerParameterType type)
    {
        if (!a) return false;
        foreach (var p in a.parameters)
            if (p.type == type && p.name == name) return true;
        return false;
    }
}
