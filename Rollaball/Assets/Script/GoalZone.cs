using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GoalZone : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI message;
    [Tooltip("Override message (optional)")]
    public string completedText = "Level Completed!";

    [Header("After completion")]
    public bool freezePlayer = true;     // stop movement on finish
    public float autoLoadDelay = -1f;    // set >0 to auto-load next scene after seconds

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        // show message
        if (message != null)
        {
            message.text = string.IsNullOrEmpty(completedText) ? message.text : completedText;
            message.gameObject.SetActive(true);
        }

        if (freezePlayer)
        {
            var rb = other.attachedRigidbody;
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                // (Optional) lock them in place:
                rb.constraints |= RigidbodyConstraints.FreezePosition;
            }
        }

        // prevent retrigger spamming
        var col = GetComponent<Collider>();
        if (col) col.enabled = false;
    }
}
