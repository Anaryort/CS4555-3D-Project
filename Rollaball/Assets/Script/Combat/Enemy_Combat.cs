using UnityEngine;

public class Enemy_Combat : MonoBehaviour
{
    public int dmg = 1;

    private void OnCollisionEnter2D(Collision2D collision) // capital "C"!
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(dmg); // use TakeDamage instead of ChangeHealth
            }
        }
    }
}
