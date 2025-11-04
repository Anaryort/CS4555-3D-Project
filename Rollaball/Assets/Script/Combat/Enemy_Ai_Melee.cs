using UnityEngine;
using UnityEngine.AI;

public class Enemy_Ai_Melee : MonoBehaviour
{
    public NavMeshAgent agent;

    public Transform player1;
    public Transform player2;
    private Transform currentTarget;

    public LayerMask whatIsGround, whatIsPlayer;

    public float health;

    // Patroling
    public Vector3 walkPoint;
    bool walkPointSet;
    public float walkPointRange = 10f;

    // Attacking
    public float timeBetweenAttacks = 1.5f;
    bool alreadyAttacked;
    public int attackDamage = 10;

    // States
    public float sightRange = 10f;
    public float attackRange = 2f;
    public bool playerInSightRange, playerInAttackRange;

    private void Awake()
    {
        // Try to find both players by name or tag
        GameObject p1 = GameObject.Find("Player1");
        GameObject p2 = GameObject.Find("Player2");

        if (p1 != null) player1 = p1.transform;
        if (p2 != null) player2 = p2.transform;

        agent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        if (player1 == null && player2 == null)
            return;

        // Find nearest player
        currentTarget = GetClosestPlayer();

        // Check for sight and attack range
        playerInSightRange = Physics.CheckSphere(transform.position, sightRange, whatIsPlayer);
        playerInAttackRange = currentTarget != null &&
                              Vector3.Distance(transform.position, currentTarget.position) <= attackRange;

        if (!playerInSightRange && !playerInAttackRange)
            Patroling();
        else if (playerInSightRange && !playerInAttackRange)
            ChasePlayer();
        else if (playerInAttackRange && playerInSightRange)
            AttackPlayer();
    }

    private Transform GetClosestPlayer()
    {
        Transform closest = null;
        float closestDist = Mathf.Infinity;

        if (player1 != null)
        {
            float dist = Vector3.Distance(transform.position, player1.position);
            if (dist < closestDist)
            {
                closestDist = dist;
                closest = player1;
            }
        }

        if (player2 != null)
        {
            float dist = Vector3.Distance(transform.position, player2.position);
            if (dist < closestDist)
            {
                closestDist = dist;
                closest = player2;
            }
        }

        return closest;
    }

    private void Patroling()
    {
        if (!walkPointSet) SearchWalkPoint();

        if (walkPointSet)
            agent.SetDestination(walkPoint);

        Vector3 distanceToWalkPoint = transform.position - walkPoint;

        if (distanceToWalkPoint.magnitude < 1f)
            walkPointSet = false;
    }

    private void SearchWalkPoint()
    {
        float randomZ = Random.Range(-walkPointRange, walkPointRange);
        float randomX = Random.Range(-walkPointRange, walkPointRange);

        walkPoint = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);

        if (Physics.Raycast(walkPoint, -transform.up, 2f, whatIsGround))
            walkPointSet = true;
    }

    private void ChasePlayer()
    {
        if (currentTarget != null)
            agent.SetDestination(currentTarget.position);
    }

    private void AttackPlayer()
    {
        if (currentTarget == null) return;

        // Stop moving
        agent.SetDestination(transform.position);

        transform.LookAt(currentTarget);

        if (!alreadyAttacked)
        {
            PlayerHealth playerHealth = currentTarget.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(attackDamage);
            }

            alreadyAttacked = true;
            Invoke(nameof(ResetAttack), timeBetweenAttacks);
        }
    }

    private void ResetAttack()
    {
        alreadyAttacked = false;
    }

    public void TakeDamage(int damage)
    {
        health -= damage;

        if (health <= 0)
            Invoke(nameof(DestroyEnemy), 0.5f);
    }

    private void DestroyEnemy()
    {
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);
    }
}
