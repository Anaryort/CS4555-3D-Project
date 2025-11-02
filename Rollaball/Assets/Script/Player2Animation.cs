using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player2Animation : MonoBehaviour
{
    Animator anim;

    int isWalkingHash;
    int isRunningHash;
    int isJumpingHash;
    int isFallingHash;
    int isPunchingHash;
    int isKickingHash;

    PlayerController playerController;

    void Start()
    {
        anim = GetComponent<Animator>();

        // Animation parameter hashes
        isWalkingHash = Animator.StringToHash("isWalking");
        isRunningHash = Animator.StringToHash("isRunning");
        isJumpingHash = Animator.StringToHash("isJumping");
        isFallingHash = Animator.StringToHash("isFalling");
        isPunchingHash = Animator.StringToHash("isPunching");
        isKickingHash = Animator.StringToHash("isKicking");

        // Reference PlayerController (ground check + jump logic live there)
        playerController = GetComponent<PlayerController>();
    }

    void Update()
    {
        bool isRunning = anim.GetBool(isRunningHash);
        bool isWalking = anim.GetBool(isWalkingHash);

        // Movement keys
        bool forwardPress = Input.GetKey(KeyCode.UpArrow);
        bool backPress = Input.GetKey(KeyCode.DownArrow);
        bool leftPress = Input.GetKey(KeyCode.LeftArrow);
        bool rightPress = Input.GetKey(KeyCode.RightArrow);

        // Combat keys (booleans)
        bool punchPress = Input.GetKey(KeyCode.RightShift);
        bool kickPress = Input.GetKey(KeyCode.Slash);

        // Shift for run
        bool runPress = Input.GetKey(KeyCode.LeftShift);

        // Walking if any arrow key pressed
        bool movePress = forwardPress || backPress || leftPress || rightPress;

        //   WALK  
        if (!isWalking && movePress)
            anim.SetBool(isWalkingHash, true);
        if (isWalking && !movePress)
            anim.SetBool(isWalkingHash, false);

        //   RUN  
        if (!isRunning && (movePress && runPress))
            anim.SetBool(isRunningHash, true);
        if (isRunning && (!movePress || !runPress))
            anim.SetBool(isRunningHash, false);

        // //   JUMP / FALL  
        // if (!playerController.isGrounded)
        // {
        //     if (playerController.rb.linearVelocity.y > 0.1f) // going up
        //     {
        //         anim.SetBool(isJumpingHash, true);
        //         anim.SetBool(isFallingHash, false);
        //     }
        //     else if (playerController.rb.linearVelocity.y < -0.1f) // going down
        //     {
        //         anim.SetBool(isJumpingHash, false);
        //         anim.SetBool(isFallingHash, true);
        //     }
        // }
        // else
        // {
        //     anim.SetBool(isJumpingHash, false);
        //     anim.SetBool(isFallingHash, false);
        // }

        //   PUNCH 
        anim.SetBool(isPunchingHash, punchPress);


        //   KICK 
        anim.SetBool(isKickingHash, kickPress);
    }
}
