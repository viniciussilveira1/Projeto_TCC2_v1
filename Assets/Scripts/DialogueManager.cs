using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;
using System;
using System.Collections.Generic;

[DefaultExecutionOrder(-50)]
public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private GameObject panel;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private Button[] optionButtons = new Button[3];

    private NPCDialogue currentNPC;

    // Mapeia cada botão ao tipo real da opção após embaralhar
    private AnswerType[] buttonMap = new AnswerType[3];

    // Input/movimento
    private PlayerInput playerInput;
    private PlayerMovement playerMovement;
    private Rigidbody2D playerRb;
    private string previousActionMap;

    private enum AnswerType { Correct = 0, Neutral = 1, Wrong = 2 }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        ApplySessionStateOnLoad();

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
        var all = FindObjectsByType<NPCDialogue>(FindObjectsSortMode.None);
        foreach (var npc in all)
        {
            if (SessionProgress.IsEliminated(npc.SituationId))
            {
                InteractionDetector.Instance?.HideIfTarget(npc.transform);
                npc.HideTarget.SetActive(false);
                continue;
            }

            if (SessionProgress.IsResolved(npc.SituationId))
            {
                npc.MarkResolved();
            }
        }
    }

    public void Show(NPCDialogue npc)
    {
        if (npc == null || npc.IsResolved || string.IsNullOrWhiteSpace(npc.description)) return;

        currentNPC = npc;
        if (descriptionText) descriptionText.text = npc.description;

        // 1) Cria lista (texto + tipo real)
        var slots = new List<(string text, AnswerType type)>
        {
            (npc.optionA, AnswerType.Correct),
            (npc.optionB, AnswerType.Neutral),
            (npc.optionC, AnswerType.Wrong)
        };

        // 2) Embaralha (Fisher–Yates)
        for (int i = slots.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            (slots[i], slots[j]) = (slots[j], slots[i]);
        }

        // DEBUG: mostrar a ordem sorteada no console
        Debug.Log($"[DialogueManager] Ordem sorteada: {string.Join(", ", slots.ConvertAll(s => s.type.ToString()))}");

        // 3) Aplica nos botões e registra o mapa
        for (int i = 0; i < optionButtons.Length && i < slots.Count; i++)
        {
            int btnIndex = i;
            var (text, type) = slots[i];

            var label = optionButtons[i].GetComponentInChildren<TMP_Text>(true);
            if (label != null) label.text = text ?? string.Empty;

            buttonMap[i] = type;

            optionButtons[i].onClick.RemoveAllListeners();
            optionButtons[i].onClick.AddListener(() => OnOptionClicked(btnIndex));
        }

        panel.SetActive(true);
        LockPlayer(true);
        RtVoiceService.I?.Speak(npc.description);
    }

    private void OnOptionClicked(int index)
    {
        if (currentNPC == null || currentNPC.IsResolved) { Close(); return; }

        // Descobre o tipo real da opção clicada
        var type = buttonMap[Mathf.Clamp(index, 0, buttonMap.Length - 1)];

        // 1) Dispara o UnityEvent correto
        switch (type)
        {
            case AnswerType.Correct: currentNPC.onChooseCorrect?.Invoke(); break;
            case AnswerType.Neutral: currentNPC.onChooseNeutral?.Invoke(); break;
            case AnswerType.Wrong:   currentNPC.onChooseWrong?.Invoke();   break;
        }

        // 2) Marca como resolvido
        currentNPC.MarkResolved();
        SessionProgress.MarkResolved(currentNPC.SituationId);

        // 3) Esconde "!"
        InteractionDetector.Instance?.HideIfTarget(currentNPC.transform);

        // 4) Pontuação: mantém o protocolo 0/1/2 (Certa/Neutra/Errada)
        int scoreIndex = type switch
        {
            AnswerType.Correct => 0,
            AnswerType.Neutral => 1,
            _ => 2
        };
        SituationCounter.Instance?.RegisterAnswer(scoreIndex);
        SituationCounter.Instance?.Increment(1);

        // 5) Fecha
        Close();
    }

    private void Close()
    {
        RtVoiceService.I?.StopSpeaking();
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
