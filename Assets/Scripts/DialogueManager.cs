using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

[DefaultExecutionOrder(-50)] // aplica estado da sessão antes de qualquer trigger/UI
public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private GameObject panel;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private Button[] optionButtons = new Button[3];

    private NPCDialogue currentNPC;

    // Input/movimento
    private PlayerInput playerInput;
    private PlayerMovement playerMovement;
    private Rigidbody2D playerRb;
    private string previousActionMap;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        ApplySessionStateOnLoad(); // <<< chave para impedir reinteração e ocultar eliminados

        if (panel != null) panel.SetActive(false);

        var player = GameObject.FindWithTag("Player");
        if (player)
        {
            playerInput    = player.GetComponent<PlayerInput>();
            playerMovement = player.GetComponent<PlayerMovement>();
            playerRb       = player.GetComponent<Rigidbody2D>();
        }
    }

    private void ApplySessionStateOnLoad()
    {
        var all = FindObjectsOfType<NPCDialogue>(true);
        foreach (var npc in all)
        {
            if (SessionProgress.IsEliminated(npc.SituationId))
            {
                InteractionDetector.Instance?.HideIfTarget(npc.transform);
                npc.HideTarget.SetActive(false); // some no reload
                continue;
            }

            if (SessionProgress.IsResolved(npc.SituationId))
            {
                // já foi resolvida nesta sessão: não pode reinteragir
                npc.MarkResolved();
            }
        }
    }

    public void Show(NPCDialogue npc)
    {
        if (npc == null || npc.IsResolved) return;

        currentNPC = npc;
        if (descriptionText) descriptionText.text = npc.description;

        string[] opts = { npc.optionA, npc.optionB, npc.optionC };
        for (int i = 0; i < optionButtons.Length; i++)
        {
            int idx = i;
            var label = optionButtons[i].GetComponentInChildren<TMP_Text>(true);
            if (label != null) label.text = opts[i];

            optionButtons[i].onClick.RemoveAllListeners();
            optionButtons[i].onClick.AddListener(() => OnOptionClicked(idx));
        }

        panel.SetActive(true);
        LockPlayer(true);
    }

    private void OnOptionClicked(int index)
    {
        if (currentNPC == null || currentNPC.IsResolved) { Close(); return; }

        // 1) Dispara UnityEvent (se a opção faz sumir, NPCDespawn rodará agora)
        switch (index)
        {
            case 0: currentNPC.onChooseCorrect?.Invoke(); break;
            case 1: currentNPC.onChooseNeutral?.Invoke(); break;
            case 2: currentNPC.onChooseWrong?.Invoke();   break;
        }

        // 2) Marca como resolvido SEMPRE (impede reinteração após reload)
        currentNPC.MarkResolved();
        SessionProgress.MarkResolved(currentNPC.SituationId);

        // 3) Esconde "!" se estiver apontando pra este alvo
        InteractionDetector.Instance?.HideIfTarget(currentNPC.transform);

        // 4) Pontos (seu fluxo)
        SituationCounter.Instance?.RegisterAnswer(index);
        SituationCounter.Instance?.Increment(1);

        // 5) Fecha painel e libera player
        Close();
    }

    private void Close()
    {
        panel?.SetActive(false);
        currentNPC = null;
        LockPlayer(false);
    }

    private void LockPlayer(bool locked)
    {
        if (locked && playerRb) playerRb.linearVelocity = Vector2.zero;

        if (playerInput != null && playerInput.actions != null)
        {
            if (locked)
            {
                previousActionMap = playerInput.currentActionMap != null
                    ? playerInput.currentActionMap.name
                    : null;

                var uiMap = playerInput.actions.FindActionMap("UI", true);
                if (uiMap != null) playerInput.SwitchCurrentActionMap("UI");
                else if (playerMovement) playerMovement.enabled = false;
            }
            else
            {
                if (!string.IsNullOrEmpty(previousActionMap) &&
                    playerInput.actions.FindActionMap(previousActionMap, true) != null)
                {
                    playerInput.SwitchCurrentActionMap(previousActionMap);
                }
                else if (playerMovement) playerMovement.enabled = true;
            }
        }
        else
        {
            if (playerMovement) playerMovement.enabled = !locked;
        }
    }
}
