using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackScript : MonoBehaviour
{
    public GameObject attackEffect;

    void Start()
    {
        attackEffect.SetActive(false);
    }

    void Update()
    {
        // Check if attack key is pressed
        if (Input.GetKey(KeyCode.E) || Input.GetKey(KeyCode.RightShift))
        {
            Attack();
        }
        else
        {
            StopAttack();
        }
    }

    void Attack()
    {
        attackEffect.SetActive(true);
        // Here you could also call a method to deal damage
        // e.g., PlayerController.Instance.Attack();
    }

    void StopAttack()
    {
        attackEffect.SetActive(false);
    }
}
