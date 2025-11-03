using UnityEngine;
using UnityEngine.InputSystem; // New Input System namespace

public class PlayerCombat : MonoBehaviour
{
    [Header("Combat Settings")]
    public Transform attackPoint;
    public float attackRange = 0.5f;
    public int attackDamage = 40;
    public LayerMask enemyLayers;
    public float attackRate = 2f;

    private float nextAttackTime = 0f;
    private PlayerInput playerInput;
    private InputAction attackAction;

    void Awake()
    {
        playerInput = GetComponent<PlayerInput>();

        if (playerInput != null)
        {
            attackAction = playerInput.actions["Attack"];
        }
    }

    void OnEnable()
    {
        if (attackAction != null)
            attackAction.performed += OnAttackPerformed;
    }

    void OnDisable()
    {
        if (attackAction != null)
            attackAction.performed -= OnAttackPerformed;
    }

    void Update()
    {
        if (Time.time >= nextAttackTime)
        {
            if (Keyboard.current != null)
            {
                if (Keyboard.current.eKey.wasPressedThisFrame || Keyboard.current.rightShiftKey.wasPressedThisFrame)
                {
                    PerformAttack();
                }
            }
        }
    }

    private void OnAttackPerformed(InputAction.CallbackContext context)
    {
        if (Time.time >= nextAttackTime)
        {
            PerformAttack();
        }
    }

    private void PerformAttack()
    {
        nextAttackTime = Time.time + 1f / attackRate;

        Collider[] hitEnemies = Physics.OverlapSphere(attackPoint.position, attackRange, enemyLayers);

        foreach (Collider enemy in hitEnemies)
        {
            Enemy e = enemy.GetComponent<Enemy>();
            if (e != null)
                e.TakeDamage(attackDamage);
        }
    }


}
