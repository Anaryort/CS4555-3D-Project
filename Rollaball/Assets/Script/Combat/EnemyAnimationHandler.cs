using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Enemy_Ai_Melee))]
public class EnemyAnimationHandler : MonoBehaviour
{
    private Animator animator;
    private Enemy_Ai_Melee enemyAI;
    private NavMeshAgent agent;

    private bool isIdle;
    private bool isWalking;
    private bool isAttacking;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        enemyAI = GetComponent<Enemy_Ai_Melee>();
        agent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        if (animator == null) return;

        if (animator.GetBool("isDead")) return;

        UpdateAnimationState();
    }

    private void UpdateAnimationState()
    {
        bool walking = agent.velocity.magnitude > 0.1f && !enemyAI.playerInAttackRange;
        bool attacking = enemyAI.playerInAttackRange && enemyAI.playerInSightRange;
        bool idle = !walking && !attacking;

        if (walking != isWalking || attacking != isAttacking || idle != isIdle)
        {
            isWalking = walking;
            isAttacking = attacking;
            isIdle = idle;

            animator.SetBool("isWalking", isWalking);
            animator.SetBool("isAttacking", isAttacking);
            animator.SetBool("isIdle", isIdle);
        }
    }

    public void PlayDeath()
    {
        animator.SetBool("isDead", true);

        if (agent != null)
            agent.isStopped = true;

        if (enemyAI != null)
            enemyAI.enabled = false;
    }
}
