using UnityEngine;

public class Projectile : MonoBehaviour
{
    public int damage = 15;  // Set damage value in Inspector
                             // public float lifeTime = 5f;

    // private void Start()
    // {
    //     Destroy(gameObject, lifeTime); 
    // }
    // renable for different types of ranged enemies (i.e limiting shots for beeg enemy)

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
            }
        }

    }
}
