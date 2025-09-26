using UnityEngine;

public class MovementScript : MonoBehaviour
{
    public float speed;
    public float rotationSpeed;
    public float jumpSpeed;
    private float ySpeed;
    private CharacterController con;
    public bool isGrounded;

    void Start()
    {
        con = GetComponent<CharacterController>();
    }

    void Update()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 moveDirection = new Vector3(horizontal, 0, vertical);
        moveDirection.Normalize();
        float magnitude = moveDirection.magnitude;
        magnitude = Mathf.Clamp01(magnitude);
        //transform.Translate(moveDirection * speed * Time.deltaTime, Space.World);
        con.SimpleMove(moveDirection * magnitude * speed);

        ySpeed += Physics.gravity.y * Time.deltaTime;
        if (Input.GetButtonDown("Jump"))
        {
            ySpeed = -0.5f;
        }

        Vector3 vel = moveDirection * magnitude;
        vel.y = ySpeed;
        //transform.Translate(vel * Time.deltaTime);
        con.Move(vel * Time.deltaTime);

        if(con.isGrounded)
        {
            ySpeed = -0.5f;
            isGrounded = true;
            if(Input.GetButtonDown("Jump"))
            {
                ySpeed = jumpSpeed;
                isGrounded = false;
            }
        }

        if (moveDirection != Vector3.zero) {
            Quaternion toRotate = Quaternion.LookRotation(moveDirection, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, toRotate, rotationSpeed * Time.deltaTime);
    
        }
    }
}
