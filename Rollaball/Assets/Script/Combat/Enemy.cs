using UnityEngine;

public class Enemy : MonoBehaviour
{
    public int maxHealth = 100;
    int currentHealth;

    private EnemyAnimationHandler animationHandler;
    private bool isDead = false;

    void Awake()
    {
        animationHandler = GetComponent<EnemyAnimationHandler>();
    }

    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return; 

        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Die()
    {
        isDead = true;
        if (animationHandler != null)
            animationHandler.PlayDeath();

        Destroy(gameObject, 0.7f);
    }


    public void OnDeathAnimationFinished()
    {
        Destroy(gameObject);
    }
}
