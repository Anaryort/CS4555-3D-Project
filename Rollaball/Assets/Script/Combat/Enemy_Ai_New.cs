using UnityEngine;
using UnityEngine.AI;

public class Enemy_Ai_New : MonoBehaviour
{
    public NavMeshAgent agent;

    // Multiple players
    public Transform[] players;

    public LayerMask whatIsGround, whatIsPlayer;

    public float health;
    public Transform shootPoint;

    //Patroling
    public Vector3 walkPoint;
    bool walkPointSet;
    public float walkPointRange;

    //Attacking
    public float timeBetweenAttacks;
    bool alreadyAttacked;
    public GameObject projectile;

    //States
    public float sightRange, attackRange;
    public bool playerInSightRange, playerInAttackRange;

    private Transform targetPlayer; // The player currently being targeted

    private void Awake()
    {
        // Find both players by name (adjust as needed)
        GameObject player1 = GameObject.Find("Player1");
        GameObject player2 = GameObject.Find("Player2");

        players = new Transform[] { player1?.transform, player2?.transform };
        agent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        // Find nearest player each frame
        targetPlayer = GetNearestPlayer();

        if (targetPlayer == null) return;

        // Check for sight and attack range relative to the *nearest* player
        playerInSightRange = Physics.CheckSphere(transform.position, sightRange, whatIsPlayer);
        playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, whatIsPlayer);

        if (!playerInSightRange && !playerInAttackRange) Patroling();
        if (playerInSightRange && !playerInAttackRange) ChasePlayer();
        if (playerInAttackRange && playerInSightRange) AttackPlayer();
    }

    private Transform GetNearestPlayer()
    {
        Transform nearest = null;
        float minDistance = Mathf.Infinity;

        foreach (Transform p in players)
        {
            if (p == null) continue;

            float dist = Vector3.Distance(transform.position, p.position);
            if (dist < minDistance)
            {
                minDistance = dist;
                nearest = p;
            }
        }

        return nearest;
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
        if (targetPlayer != null)
            agent.SetDestination(targetPlayer.position);
    }

    private void AttackPlayer()
    {
        agent.SetDestination(transform.position);

        if (targetPlayer == null) return;
        transform.LookAt(targetPlayer);

        if (!alreadyAttacked)
        {
            Vector3 direction = (targetPlayer.position - shootPoint.position).normalized;
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            Rigidbody rb = Instantiate(projectile, shootPoint.position, lookRotation).GetComponent<Rigidbody>();

            rb.AddForce(direction * 32f, ForceMode.Impulse);
            rb.AddForce(Vector3.up * 8f, ForceMode.Impulse);

            alreadyAttacked = true;
            Invoke(nameof(ResetAttack), timeBetweenAttacks);
        }
    }

    private void ResetAttack() => alreadyAttacked = false;

    public void TakeDamage(int damage)
    {
        health -= damage;
        if (health <= 0) Invoke(nameof(DestroyEnemy), 0.5f);
    }

    private void DestroyEnemy() => Destroy(gameObject);

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);
    }
}
