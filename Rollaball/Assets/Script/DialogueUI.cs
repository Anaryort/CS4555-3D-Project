using UnityEngine;
using TMPro;

public class DialogueUI : MonoBehaviour
{
    [Header("Assign these")]
    public Canvas rootCanvas;                 // Screen Space - Overlay
    public GameObject panel;                  // A panel/background GameObject
    public TMP_Text dialogueText;             // TextMeshProUGUI for dialogue lines
    public GameObject prompt;                 // Small "Press F to Talk" text

    void Awake()
    {
        if (!rootCanvas) rootCanvas = GetComponentInChildren<Canvas>(true);
        SetPrompt(false);
        SetPanel(false);
    }

    public void SetPrompt(bool show)
    {
        if (prompt) prompt.SetActive(show);
    }

public void SetPanel(bool show)
{
    if (panel) panel.SetActive(show);
    if (dialogueText) dialogueText.gameObject.SetActive(show);
}



    public void SetText(string text)
    {
        if (dialogueText) dialogueText.text = text ?? "";
    }
}
