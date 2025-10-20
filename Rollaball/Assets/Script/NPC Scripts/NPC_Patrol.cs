using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class NPC_Patrol : MonoBehaviour
{
    [Header("Path")]
    public Transform[] patrolPoints;          // Waypoints in the scene (not the NPC itself)
    public bool loop = true;

    [Header("Timing")]
    public float pauseDuration = 1.5f;
    public float arrivalThreshold = 0.2f;

    [Header("Agent")]
    [Tooltip("Assign the NavMeshAgent on your child (Aias (1)).")]
    public NavMeshAgent agent;                // <-- drag Aias (1)'s agent here
    public float speed = 2f;                  // mirrors agent.speed
    public float rotateSpeed = 10f;           // yaw turn smoothing

    [Header("Optional")]
    [Tooltip("Leave null to rotate the agent's transform. Otherwise pick a model root.")]
    public Transform rotateTarget;
    [Tooltip("Use if your model faces sideways. Try 90 or -90.")]
    public float modelYawOffset = 0f;

    // --- internals ---
    private int currentIndex = 0;
    private bool isPaused = false;

    void Awake()
    {
        if (agent == null)
        {
            // Support your parent/child layout: find in children if not assigned
            agent = GetComponent<NavMeshAgent>();
            if (agent == null) agent = GetComponentInChildren<NavMeshAgent>(true);
        }

        if (agent == null)
        {
            Debug.LogError($"{name}: NavMeshAgent not found. Assign it in the Inspector.");
            enabled = false;
            return;
        }

        if (rotateTarget == null) rotateTarget = agent.transform;

        // We control rotation manually (nicer yaw-only turning)
        agent.updateRotation = false;
        agent.updateUpAxis   = true;

        agent.speed = speed;
        agent.autoBraking = true;
        agent.stoppingDistance = Mathf.Max(agent.stoppingDistance, arrivalThreshold);
    }

    void Start()
    {
        // Snap agent onto NavMesh (helps if spawned slightly off-mesh)
        SnapToNearestNavmesh();

        // Filter out any patrol point that is literally the agent transform
        if (patrolPoints != null)
        {
            for (int i = 0; i < patrolPoints.Length; i++)
                if (patrolPoints[i] == agent.transform)
                    patrolPoints[i] = null;
        }

        if (patrolPoints == null || patrolPoints.Length < 2)
        {
            Debug.LogWarning($"{name}: Need at least 2 patrol points on the NavMesh.");
            enabled = false;
            return;
        }

        currentIndex = Mathf.Clamp(currentIndex, 0, patrolPoints.Length - 1);
        SetDestinationToCurrentPoint();
    }

    void Update()
    {
        if (isPaused) return;

        // Smoothly face the desired movement direction (yaw only)
        Vector3 v = agent.desiredVelocity; v.y = 0f;
        if (v.sqrMagnitude > 0.0001f)
        {
            Quaternion look = Quaternion.LookRotation(v) * Quaternion.Euler(0f, modelYawOffset, 0f);
            rotateTarget.rotation = Quaternion.Slerp(rotateTarget.rotation, look, rotateSpeed * Time.deltaTime);
        }

        // Arrival detection
        if (!agent.pathPending)
        {
            if (agent.remainingDistance <= agent.stoppingDistance)
            {
                if (!agent.hasPath || agent.velocity.sqrMagnitude < 0.01f)
                    StartCoroutine(AdvanceAfterPause());
            }
        }
    }

    IEnumerator AdvanceAfterPause()
    {
        isPaused = true;

        agent.isStopped = true;
        agent.ResetPath();

        yield return new WaitForSeconds(pauseDuration);

        currentIndex++;
        if (currentIndex >= patrolPoints.Length)
        {
            if (loop) currentIndex = 0;
            else { enabled = false; yield break; }
        }

        SetDestinationToCurrentPoint();

        agent.isStopped = false;
        isPaused = false;
    }

    void SetDestinationToCurrentPoint()
    {
        var p = patrolPoints[currentIndex];
        if (p == null) return;

        Vector3 raw = p.position;

        // Ensure destination is on NavMesh
        if (NavMesh.SamplePosition(raw, out NavMeshHit hit, 2f, NavMesh.AllAreas) ||
            NavMesh.SamplePosition(raw, out hit, 6f, NavMesh.AllAreas))
        {
            agent.speed = speed; // keep in sync if you tweak at runtime
            agent.SetDestination(hit.position);
        }
        else
        {
            Debug.LogWarning($"{name}: Patrol point {currentIndex} is not on/near the NavMesh.");
        }
    }

void SnapToNearestNavmesh()
{
    var t = agent.transform;

    // Try a large-radius sample first (handles big height gaps)
    if (NavMesh.SamplePosition(t.position, out var hit, 100f, NavMesh.AllAreas))
    {
        agent.Warp(hit.position);
        return;
    }

    // Fallback: raycast down to find ground, then sample there (needs colliders on ground)
    if (Physics.Raycast(t.position + Vector3.up * 200f, Vector3.down, out var rh, 1000f, ~0))
    {
        if (NavMesh.SamplePosition(rh.point, out hit, 5f, NavMesh.AllAreas))
            agent.Warp(hit.position);
    }
}


    void OnDrawGizmosSelected()
    {
        if (patrolPoints == null || patrolPoints.Length == 0) return;
        Gizmos.color = Color.cyan;
        for (int i = 0; i < patrolPoints.Length; i++)
        {
            var a = patrolPoints[i];
            if (a == null) continue;
            Gizmos.DrawSphere(a.position + Vector3.up * 0.05f, 0.12f);
            var b = patrolPoints[(i + 1) % patrolPoints.Length];
            if (b != null)
                Gizmos.DrawLine(a.position + Vector3.up * 0.05f, b.position + Vector3.up * 0.05f);
        }
    }
}
