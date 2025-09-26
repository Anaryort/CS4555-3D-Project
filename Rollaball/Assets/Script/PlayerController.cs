using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class PlayerController : MonoBehaviour
{
    
    public Rigidbody rb;


    private float movementX;
    private float movementY;

    // Player stats
    public float speed = 5f;         
    public float rotationSpeed = 10f; 
    public float jumpForce = 5f;
    public float groundCheckDistance = 0f;

    

    public bool isGrounded;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }

    void Update()
    {
        // Check if grounded using a Raycast
        isGrounded = Physics.Raycast(transform.position, Vector3.down, 1f + groundCheckDistance);
    }

    // Take input, convert to Vector2
    void OnMove(InputValue movementValue)
    {
        Vector2 movementVector = movementValue.Get<Vector2>();
        movementX = movementVector.x;
        movementY = movementVector.y;
    }

    // Jump
    void OnJump(InputValue jumpValue)
    {
        if (jumpValue.isPressed && isGrounded)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z); // Reset Y velocity
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    void FixedUpdate() {

        // Still rotates in idle animation if coming into contact with walls/blocks. 
       
        // Raw input vector
        Vector3 inputDir = new Vector3(movementX, 0f, movementY);

        // Movement
        Vector3 targetVelocity = inputDir.normalized * speed;
        rb.linearVelocity = new Vector3(targetVelocity.x, rb.linearVelocity.y, targetVelocity.z);

        // Look rotation only if input is significant
        float deadzone = 0.1f; // ignore very small inputs
        if (Mathf.Abs(movementX) > deadzone || Mathf.Abs(movementY) > deadzone)
        {
            Quaternion targetRotation = Quaternion.LookRotation(inputDir);
            rb.rotation = Quaternion.Slerp(rb.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
        }
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("PickUp"))
        {
            other.gameObject.SetActive(false);
        }
    }


}
