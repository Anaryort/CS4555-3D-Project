using UnityEngine;

public class KillZone : MonoBehaviour
{
    [Header("Default / single-player respawn")]
    public Transform respawnPoint;   // keeps your old behavior

    [Header("Optional 2-player respawn")]
    public Rigidbody player1;
    public Transform respawnPointP1;
    public Rigidbody player2;
    public Transform respawnPointP2;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        var rb = other.attachedRigidbody;
        if (rb == null)
            return;

        // Start with the old default respawnPoint
        Transform target = respawnPoint;

        // If this rigidbody is player1 and we have a P1 respawn assigned, use that
        if (rb == player1 && respawnPointP1 != null)
        {
            target = respawnPointP1;
        }
        // If this rigidbody is player2 and we have a P2 respawn assigned, use that
        else if (rb == player2 && respawnPointP2 != null)
        {
            target = respawnPointP2;
        }

        // If somehow nothing is assigned, just do nothing instead of crashing
        if (target == null)
            return;

        // Stop motion (same as your original)
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // Teleport to the chosen respawn point
        rb.position = target.position;
        rb.rotation = target.rotation;
    }
}
