using UnityEngine;

public class Enemy_Combat : MonoBehaviour
{
    public int dmg = 1;
    private void OncollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.CompareTag("Player"))
        {
            collision.gameObject.GetComponent<PlayerHealth>().ChangeHealth(-dmg);
        }
    }
}
