using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class temporalanchor_power : MonoBehaviour
{
    public float anchorDuration = 4f;       
    public float rewindDuration = 1f;       
    public GameObject anchorEffect;         

    private bool isAnchored = false;
    private List<Vector3> positions = new List<Vector3>();
    private List<Quaternion> rotations = new List<Quaternion>();

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isAnchored)
        {
            StartCoroutine(AnchorRoutine(other));
        }
    }

    IEnumerator AnchorRoutine(Collider player)
    {
        isAnchored = true;
        Debug.Log("Anchor activated!");

        // Hide pickup visuals
        GetComponent<MeshRenderer>().enabled = false;
        GetComponent<Collider>().enabled = false;

        // Play anchor effect at pickup
        Instantiate(anchorEffect, player.transform.position, Quaternion.identity);

        // Start recording movement
        yield return StartCoroutine(RecordMovement(player.transform, anchorDuration));

        Debug.Log("Rewind starting...");

        // Disable controls + freeze physics
        PlayerController controller = player.GetComponent<PlayerController>();
        Rigidbody rb = player.GetComponent<Rigidbody>();
        if (controller != null) controller.enabled = false;
        if (rb != null) rb.isKinematic = true;

        // Play rewind in reverse
        yield return StartCoroutine(RewindMovement(player.transform, rewindDuration));

        // Re-enable controls + unfreeze physics
        if (rb != null) rb.isKinematic = false;
        if (controller != null) controller.enabled = true;

        // Optional visual effect at end of rewind
        Instantiate(anchorEffect, player.transform.position, player.transform.rotation);

        // Clean up
        positions.Clear();
        rotations.Clear();
        Destroy(gameObject);
    }

    IEnumerator RecordMovement(Transform player, float duration)
    {
        positions.Clear();
        rotations.Clear();
        float elapsed = 0f;

        while (elapsed < duration)
        {
            positions.Add(player.position);
            rotations.Add(player.rotation);

            elapsed += Time.deltaTime;
            yield return null;
        }
    }

    IEnumerator RewindMovement(Transform player, float rewindTime)
    {
        if (positions.Count == 0) yield break;

        // Reverse lists to play path backwards
        positions.Reverse();
        rotations.Reverse();

        int steps = positions.Count;
        float stepTime = rewindTime / steps;

        for (int i = 0; i < steps; i++)
        {
            player.position = positions[i];
            player.rotation = rotations[i];
            yield return new WaitForSeconds(stepTime);
        }

        // Ensure final position is exact
        player.position = positions[steps - 1];
        player.rotation = rotations[steps - 1];
    }
}


