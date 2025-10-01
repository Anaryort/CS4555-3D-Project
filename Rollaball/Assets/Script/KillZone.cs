using UnityEngine;

public class KillZone : MonoBehaviour
{
    public Transform respawnPoint;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            var rb = other.attachedRigidbody;
            if (rb != null)
            {
                // Stop motion
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;

                // Teleport
                rb.position = respawnPoint.position;
                rb.rotation = respawnPoint.rotation;
            }
        }
    }
}
