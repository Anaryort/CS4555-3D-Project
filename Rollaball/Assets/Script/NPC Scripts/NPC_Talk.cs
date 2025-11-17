using UnityEngine;
using UnityEngine.AI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class NPC_Talk : MonoBehaviour
{
    [Header("Players")]
    public Transform[] players;          // Player 1 + Player 2
    private Transform activePlayer;      // the one currently interacting

    [Header("Refs")]
    public DialogueUI ui;                // Dialogue UI panel
    public NavMeshAgent agent;           // NPC agent

    [Header("Pause NPC movement while talking")]
    public MonoBehaviour[] pauseNpcWhileTalking;

    [Header("Disable Player control while talking")]
    public MonoBehaviour[] disablePlayerScripts;
#if ENABLE_INPUT_SYSTEM
    public PlayerInput[] playerInputs;   // optional: one PlayerInput per player
#endif

    public KeyCode interactKey = KeyCode.F;
    public KeyCode closeKey = KeyCode.Escape;

    public Rigidbody[] playerRigidbodies; // one rigidbody per player (optional)

    [Header("Interaction (front-of-NPC)")]
    public Transform interactOrigin;
    public float interactRadius = 3.5f;
    [Range(1f, 180f)] public float interactFOV = 110f;
    public bool requireLineOfSight = false;
    public LayerMask losMask = ~0;

    [Header("Dialogue Lines")]
    [TextArea(2, 4)]
    public string[] lines;

    [Header("Facing While Talking")]
    public float faceRotateSpeed = 10f;
    public float modelYawOffset = 0f;

    [Header("Debug")]
    public bool logState = true;
    public bool drawGizmo = true;

    // internals
    bool talking, canInteract;
    int lineIndex = 0;
    float radiusSqr;

    void Awake()
    {
        if (!agent) agent = GetComponentInChildren<NavMeshAgent>(true);
        if (!interactOrigin) interactOrigin = agent ? agent.transform : transform;
        if (!ui) ui = Object.FindFirstObjectByType<DialogueUI>(FindObjectsInactive.Include);

        radiusSqr = interactRadius * interactRadius;
    }

    void Update()
    {
        //-------------------- MULTI-PLAYER SEARCH --------------------
        activePlayer = null;
        float bestDist = float.MaxValue;

        foreach (var p in players)
        {
            if (!p) continue;

            Vector3 npcPos = interactOrigin.position;
            Vector3 playerPos = p.position;

            Vector3 toPlayer = playerPos - npcPos;
            float distSqr = toPlayer.sqrMagnitude;

            if (distSqr > radiusSqr)
                continue;

            // FOV check
            bool inFOV = Vector3.Angle(interactOrigin.forward, toPlayer) <= interactFOV * 0.5f;
            if (!inFOV) continue;

            // LOS check
            bool losOk = true;
            if (requireLineOfSight)
            {
                Vector3 eye = npcPos + Vector3.up * 1.4f;
                Vector3 chest = p.position + Vector3.up * 1.0f;

                if (Physics.Raycast(eye, (chest - eye).normalized, out var hit,
                    Vector3.Distance(eye, chest), losMask))
                {
                    if (hit.transform != p && !hit.transform.IsChildOf(p))
                        losOk = false;
                }
            }

            if (!losOk) continue;

            // pick nearest
            if (distSqr < bestDist)
            {
                bestDist = distSqr;
                activePlayer = p;
            }
        }

        canInteract = activePlayer != null;
        if (ui)
            ui.SetPrompt(!talking && canInteract);

        //--------------------------------------------------------------
        // Start talking
        //--------------------------------------------------------------
        if (!talking && canInteract && PressedInteract())
        {
            BeginDialogue();
            return;
        }

        //--------------------------------------------------------------
        // While talking
        //--------------------------------------------------------------
        if (talking && activePlayer && agent)
        {
            // rotate NPC to face player
            Vector3 to = (activePlayer.position - agent.transform.position);
            to.y = 0f;
            if (to.sqrMagnitude > 0.01f)
            {
                var look = Quaternion.LookRotation(to) * Quaternion.Euler(0f, modelYawOffset, 0f);
                agent.transform.rotation = Quaternion.Slerp(
                    agent.transform.rotation,
                    look,
                    faceRotateSpeed * Time.deltaTime
                );
            }

            // advance line
            if (PressedInteract()) NextLine();

            // close
            if (PressedClose()) EndDialogue();
        }
    }

    //------------------------------------------------------------------
    // Input Handling
    //------------------------------------------------------------------
    bool PressedInteract()
    {
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame)
            return true;
#endif
        return Input.GetKeyDown(interactKey);
    }

    bool PressedClose()
    {
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            return true;
#endif
        return Input.GetKeyDown(closeKey);
    }

    //------------------------------------------------------------------
    // Dialogue Logic
    //------------------------------------------------------------------
    void BeginDialogue()
    {
        talking = true;

        if (agent)
        {
            agent.updateRotation = false;
            agent.isStopped = true;
            agent.ResetPath();
        }

        foreach (var m in pauseNpcWhileTalking)
            if (m) m.enabled = false;

        // Disable ONLY the interacting player's movement scripts
        foreach (var s in disablePlayerScripts)
            if (s && (s.transform == activePlayer || s.transform.IsChildOf(activePlayer)))
                s.enabled = false;

#if ENABLE_INPUT_SYSTEM
        // disable PlayerInput only for the active player
        foreach (var pi in playerInputs)
            if (pi && pi.transform == activePlayer)
                pi.enabled = false;
#endif

        // zero velocity of active player if available
        foreach (var rb in playerRigidbodies)
            if (rb && rb.transform == activePlayer)
                rb.linearVelocity = Vector3.zero;

        // UI
        if (ui)
        {
            ui.SetPrompt(false);
            ui.SetPanel(true);
            ui.SetText(lines != null && lines.Length > 0 ? lines[0] : "");
        }

        if (logState)
            Debug.Log($"{name}: Dialogue started with {activePlayer.name}");

        lineIndex = 0;
    }

    void NextLine()
    {
        if (lines == null || lines.Length == 0)
        {
            EndDialogue(); return;
        }

        lineIndex++;
        if (lineIndex >= lines.Length)
        {
            EndDialogue(); return;
        }

        if (ui) ui.SetText(lines[lineIndex]);
    }

    void EndDialogue()
    {
        talking = false;
        lineIndex = 0;

        foreach (var m in pauseNpcWhileTalking)
            if (m) m.enabled = true;

        if (agent)
        {
            agent.updateRotation = true;
            agent.isStopped = false;
        }

        // Re-enable scripts only for the active player
        foreach (var s in disablePlayerScripts)
            if (s && (s.transform == activePlayer || s.transform.IsChildOf(activePlayer)))
                s.enabled = true;

#if ENABLE_INPUT_SYSTEM
        foreach (var pi in playerInputs)
            if (pi && pi.transform == activePlayer)
                pi.enabled = true;
#endif

        if (ui)
        {
            ui.SetPanel(false);
            ui.SetPrompt(canInteract);
        }

        if (logState)
            Debug.Log($"{name}: Dialogue ended with {activePlayer.name}");
    }

    //------------------------------------------------------------------
    // Gizmos
    //------------------------------------------------------------------
    void OnDrawGizmosSelected()
    {
        if (!drawGizmo) return;

        Transform origin = interactOrigin ? interactOrigin : transform;
        Vector3 c = origin.position; c.y = 0f;

        // radius
        Gizmos.color = new Color(1f, 1f, 0f, 0.9f);
        const int seg = 48;
        Vector3 prev = c + new Vector3(interactRadius, 0, 0);
        for (int i = 1; i <= seg; i++)
        {
            float t = i / (float)seg * Mathf.PI * 2f;
            Vector3 cur = c + new Vector3(Mathf.Cos(t) * interactRadius, 0, Mathf.Sin(t) * interactRadius);
            Gizmos.DrawLine(prev, cur);
            prev = cur;
        }

        // FOV wedge
        Gizmos.color = new Color(1f, 0.6f, 0f, 0.9f);
        Vector3 f = origin.forward; f.y = 0f; f.Normalize();
        Vector3 left = Quaternion.Euler(0, -interactFOV * 0.5f, 0) * f;
        Vector3 right = Quaternion.Euler(0, interactFOV * 0.5f, 0) * f;

        Gizmos.DrawLine(c, c + left * interactRadius);
        Gizmos.DrawLine(c, c + right * interactRadius);
    }
}
