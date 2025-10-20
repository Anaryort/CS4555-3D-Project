using UnityEngine;
using UnityEngine.AI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class NPC_Talk : MonoBehaviour
{
    [Header("Refs")]
    public Transform player;                 // drag your Player; or tag "Player"
    public DialogueUI ui;                    // drag DialogueUI from the scene
    public NavMeshAgent agent;               // drag the child agent (Aias (1))

    [Header("Pause NPC movement while talking")]
    public MonoBehaviour[] pauseNpcWhileTalking; // e.g., NPC_Patrol, NPC_Wander

    [Header("Disable Player control while talking")]
    public MonoBehaviour[] disablePlayerScripts;  // e.g., your PlayerMovement script(s)
#if ENABLE_INPUT_SYSTEM
    public PlayerInput playerInput;               // if using the new Input System
#endif
    public Rigidbody playerRigidbody;             // optional: zero velocity on open

    [Header("Interaction (front-of-NPC)")]
    [Tooltip("Where to measure distance/FOV from. Default = child with NavMeshAgent.")]
    public Transform interactOrigin;
    public float interactRadius = 3.5f;
    [Range(1f, 180f)] public float interactFOV = 110f; // must be inside this cone
    public bool requireLineOfSight = false;
    public LayerMask losMask = ~0;                 // what can block LOS

    public KeyCode interactKey = KeyCode.F;        // fallback for old input
    public KeyCode closeKey = KeyCode.Escape;

    [Header("Dialogue Lines")]
    [TextArea(2, 4)]
    public string[] lines = { "Hello there, traveler.", "Nice weather we’re having, huh?", "Safe roads to you." };

    [Header("Facing While Talking")]
    public float faceRotateSpeed = 10f;
    public float modelYawOffset = 0f;              // 90 or -90 if model faces sideways

    [Header("Debug")]
    public bool logState = true;
    public bool drawGizmo = true;

    // internals
    bool talking, canInteract;
    float radiusSqr;

    void Awake()
    {
        if (!agent) agent = GetComponentInChildren<NavMeshAgent>(true);
        if (!interactOrigin) interactOrigin = agent ? agent.transform : transform;
        if (!player)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p) player = p.transform;
        }
        if (!ui) ui = Object.FindFirstObjectByType<DialogueUI>(FindObjectsInactive.Include);

        if (!player) Debug.LogWarning($"{name}: Player not set (drag it or tag 'Player').");
        if (!ui) Debug.LogWarning($"{name}: DialogueUI not set (panel/prompt won't show).");
        if (!agent) Debug.LogWarning($"{name}: NavMeshAgent not set (drag child agent).");

        radiusSqr = interactRadius * interactRadius;
    }


    void Update()
    {
        if (!player) return;

        // ----- FRONT-OF-NPC TEST -----
        Transform origin = interactOrigin ? interactOrigin : transform;

        Vector3 npcPos = origin.position; npcPos.y = 0f;
        Vector3 playerPos = player.position; playerPos.y = 0f;

        Vector3 toPlayer = playerPos - npcPos;
        bool inRadius = toPlayer.sqrMagnitude <= radiusSqr;

        bool inFOV = true;
        if (toPlayer.sqrMagnitude > 0.0001f)
            inFOV = Vector3.Angle(origin.forward, toPlayer) <= interactFOV * 0.5f;

        bool losOk = true;
        if (requireLineOfSight)
        {
            Vector3 eye = origin.position + Vector3.up * 1.4f;
            Vector3 chest = player.position + Vector3.up * 1.0f;
            if (Physics.Raycast(eye, (chest - eye).normalized, out var hit, Vector3.Distance(eye, chest), losMask))
            {
                // blocked by something that isn't the player
                if (hit.transform != player && !hit.transform.IsChildOf(player)) losOk = false;
            }
        }

        canInteract = inRadius && inFOV && losOk;

        // Prompt only when eligible and not already talking
        if (ui) ui.SetPrompt(!talking && canInteract);

        // Start talking
        if (!talking && canInteract && PressedInteract())
        {
            BeginDialogue();
            return;
        }

        // While talking: keep NPC facing the player, advance/close
        if (talking && agent)
        {
            Vector3 to = (player.position - agent.transform.position);
            to.y = 0f;
            if (to.sqrMagnitude > 0.0001f)
            {
                var look = Quaternion.LookRotation(to) * Quaternion.Euler(0f, modelYawOffset, 0f);
                agent.transform.rotation = Quaternion.Slerp(agent.transform.rotation, look, faceRotateSpeed * Time.deltaTime);
            }

            if (PressedInteract()) NextLine();
            if (PressedClose()) EndDialogue();
        }
    }

    bool PressedInteract()
    {
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame) return true;
#endif
        return Input.GetKeyDown(interactKey);
    }

    bool PressedClose()
    {
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame) return true;
#endif
        return Input.GetKeyDown(closeKey);
    }

    void BeginDialogue()
    {
        talking = true;

        if (agent)
        {
            agent.updateRotation = false; // we rotate manually while talking
            agent.isStopped = true;
            agent.ResetPath();
        }
        foreach (var m in pauseNpcWhileTalking) if (m) m.enabled = false;

        foreach (var s in disablePlayerScripts) if (s) s.enabled = false;
#if ENABLE_INPUT_SYSTEM
        if (playerInput) playerInput.enabled = false;
#endif
        if (playerRigidbody) playerRigidbody.linearVelocity = Vector3.zero;

        if (ui)
        {
            ui.SetPrompt(false);
            ui.SetPanel(true);
            ui.SetText(lines != null && lines.Length > 0 ? lines[0] : "");
        }
        else
        {
            Debug.Log($"{name} [DIALOGUE]: {(lines != null && lines.Length > 0 ? lines[0] : "(no lines)")} ");
        }

        if (logState) Debug.Log($"{name}: Dialogue started");
    }

    int lineIndex = 0;
    void NextLine()
    {
        if (lines == null || lines.Length == 0) { EndDialogue(); return; }
        lineIndex++;
        if (lineIndex >= lines.Length) { EndDialogue(); return; }
        if (ui) ui.SetText(lines[lineIndex]);
        else Debug.Log($"{name} [DIALOGUE]: {lines[lineIndex]}");
    }

    void EndDialogue()
    {
        talking = false;
        lineIndex = 0;

        foreach (var m in pauseNpcWhileTalking) if (m) m.enabled = true;

        if (agent)
        {
            agent.updateRotation = true;
            agent.isStopped = false;
        }

        foreach (var s in disablePlayerScripts) if (s) s.enabled = true;
#if ENABLE_INPUT_SYSTEM
        if (playerInput) playerInput.enabled = true;
#endif

        if (ui)
        {
            ui.SetPanel(false);
            ui.SetPrompt(canInteract); // show again if still eligible
        }

        if (logState) Debug.Log($"{name}: Dialogue ended");
    }

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
        float half = interactFOV * 0.5f * Mathf.Deg2Rad;
        Vector3 left = Quaternion.Euler(0, -interactFOV * 0.5f, 0) * f;
        Vector3 right = Quaternion.Euler(0, interactFOV * 0.5f, 0) * f;
        Gizmos.DrawLine(c, c + left * interactRadius);
        Gizmos.DrawLine(c, c + right * interactRadius);
    }
}
