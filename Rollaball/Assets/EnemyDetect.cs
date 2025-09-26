using UnityEngine;
using UnityEngine.AI;

public class EnemyDetect : MonoBehaviour
{
    public float detectionRange = 10f;
    public float attackRange = 2f;
    public Transform Player;
    public NavMeshAgent navMeshAgent;
    public Animator anim;

    int isWalkingHash;
    int isAttackingHash;

    void Start()
    {
        anim = GetComponent<Animator>();

        isWalkingHash = Animator.StringToHash("isWalking");
        isAttackingHash = Animator.StringToHash("isAttacking");
    }

    void Update()
    {
        float distance = Vector3.Distance(transform.position, Player.position);

        if (distance < detectionRange)
        {
            navMeshAgent.destination = Player.position;

            if (distance > attackRange)
            {
                // Movement
                if (navMeshAgent.velocity.magnitude > 0.1f)
                {
                    anim.SetBool(isWalkingHash, true);
                    anim.SetBool(isAttackingHash, false);
                }
                else
                {
                    anim.SetBool(isWalkingHash, false);
                    anim.SetBool(isAttackingHash, false);
                }
            }
            else
            {
                anim.SetBool(isWalkingHash, false);
                anim.SetBool(isAttackingHash, true);
            }
        }
        else
        {
            anim.SetBool(isWalkingHash, false);
            anim.SetBool(isAttackingHash, false);
        }
    }
}
