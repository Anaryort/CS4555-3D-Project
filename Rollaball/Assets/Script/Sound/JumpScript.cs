using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpScript : MonoBehaviour
{
    public GameObject jumpEffect;

    void Start()
    {
        jumpEffect.SetActive(false);
    }

    void Update()
    {
        // Check if jump key is pressed
        if (Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.RightControl))
        {
            Jump();
        }
        else
        {
            StopJump();
        }
    }

    void Jump()
    {
        jumpEffect.SetActive(true);

    }

    void StopJump()
    {
        jumpEffect.SetActive(false);
    }
}