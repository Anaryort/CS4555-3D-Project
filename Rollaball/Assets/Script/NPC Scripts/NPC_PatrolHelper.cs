using UnityEngine;
using UnityEngine.AI;

public class NPC_PatrolHelper : MonoBehaviour
{
    [Header("Refs")]
    public NavMeshAgent agent;          // child agent (Aias (1))
    public Transform player;            // your player
    public NPC_Patrol patrol;           // auto-finds if left null
    public Transform rotateTarget;      // optional: which transform to yaw-rotate

    [Header("Perception")]
    public float detectRadius = 10f;    // how far he can see
    [Range(1f, 180f)]
    public float fovDegrees = 120f;     // field of view cone

    [Tooltip("Layers that can block line of sight (e.g., Default, Environment)")]
    public LayerMask obstructionMask = ~0;

    [Header("Chase/Leash")]
    public float leashRadius = 15f;     // max distance from home
    public float loseSightAfter = 2f;   // seconds out of sight before giving up

    [Header("Rotation")]
    public float rotateSpeed = 10f;     // yaw smoothing
    public float modelYawOffset = 0f;   // 90 or -90 if your model faces sideways

    private enum State { Patrol, Chase, Return }
    private State state = State.Patrol;

    private Vector3 homePos;            // starting position on NavMesh
    private float lostTimer;
    private Transform returnPoint;      // nearest patrol point to return to

    void Awake()
    {
        if (!agent)  agent  = GetComponentInChildren<NavMeshAgent>(true);
        if (!patrol) patrol = GetComponent<NPC_Patrol>();
        if (!rotateTarget && agent) rotateTarget = agent.transform;

        if (!agent)
        {
            Debug.LogError($"{name}: Need a NavMeshAgent reference.");
            enabled = false; return;
        }

        // Start "home" on the NavMesh
        homePos = agent.transform.position;

        // We control rotation manually for nice yaw turns
        agent.updateRotation = false;
        agent.updateUpAxis   = true;
    }

    void Start()
    {
        // Pick a sensible return point (nearest patrol point), if any
        returnPoint = NearestPatrolPoint();
    }

    void Update()
    {
        if (!agent || !agent.isOnNavMesh) return;

        switch (state)
        {
            case State.Patrol:
                if (CanSeePlayer())
                {
                    state = State.Chase;
                    if (patrol) patrol.enabled = false;       // pause patrol
                    agent.ResetPath();
                    lostTimer = 0f;
                }
                break;

            case State.Chase:
                agent.SetDestination(player.position);

                // leash: too far from home?
                if (FlatDistance(agent.transform.position, homePos) > leashRadius)
                {
                    state = State.Return;
                    lostTimer = 0f;
                    SetReturnDestination();
                }
                else
                {
                    // sight memory
                    if (CanSeePlayer()) lostTimer = 0f;
                    else
                    {
                        lostTimer += Time.deltaTime;
                        if (lostTimer >= loseSightAfter)
                        {
                            state = State.Return;
                            SetReturnDestination();
                        }
                    }
                }
                break;

            case State.Return:
                // When we reach the return point/home, resume patrol
                if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.1f)
                {
                    agent.ResetPath();
                    state = State.Patrol;
                    if (patrol) patrol.enabled = true;         // resume patrol
                }
                break;
        }

        // Smooth yaw toward desired velocity for better look direction
        Vector3 v = agent.desiredVelocity; v.y = 0f;
        if (v.sqrMagnitude > 0.0001f && rotateTarget)
        {
            var look = Quaternion.LookRotation(v) * Quaternion.Euler(0f, modelYawOffset, 0f);
            rotateTarget.rotation = Quaternion.Slerp(rotateTarget.rotation, look, rotateSpeed * Time.deltaTime);
        }
    }

    // ---------- helpers ----------

    bool CanSeePlayer()
    {
        if (!player) return false;

        Vector3 to = player.position - agent.transform.position;
        to.y = 0f;
        if (to.sqrMagnitude > detectRadius * detectRadius) return false;

        float angle = Vector3.Angle(agent.transform.forward, to);
        if (angle > fovDegrees * 0.5f) return false;

        // line-of-sight: if something blocks the ray before reaching the player, can't see
        if (Physics.Raycast(agent.transform.position + Vector3.up * 1.0f, to.normalized, out var hit, detectRadius, obstructionMask))
        {
            if (hit.transform != player && !hit.transform.IsChildOf(player)) return false;
        }
        return true;
    }

    void SetReturnDestination()
    {
        // Prefer returning to the nearest patrol point; otherwise go home position
        var target = returnPoint ? returnPoint.position : homePos;

        if (NavMesh.SamplePosition(target, out var hit, 3f, NavMesh.AllAreas) ||
            NavMesh.SamplePosition(target, out hit, 10f, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
        else
        {
            // Fallback: go to current position (no-op) to avoid invalid paths
            agent.SetDestination(agent.transform.position);
        }
    }

    Transform NearestPatrolPoint()
    {
        if (!patrol || patrol.patrolPoints == null || patrol.patrolPoints.Length == 0) return null;
        float best = float.PositiveInfinity; Transform bestT = null;
        Vector3 here = agent.transform.position; here.y = 0f;
        foreach (var t in patrol.patrolPoints)
        {
            if (!t) continue;
            Vector3 p = t.position; p.y = 0f;
            float d = (p - here).sqrMagnitude;
            if (d < best) { best = d; bestT = t; }
        }
        return bestT;
    }

    static float FlatDistance(Vector3 a, Vector3 b)
    {
        a.y = 0f; b.y = 0f; return Vector3.Distance(a, b);
    }
}
