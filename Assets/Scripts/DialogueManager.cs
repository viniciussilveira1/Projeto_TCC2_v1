using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

[DefaultExecutionOrder(-50)]
public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private GameObject panel;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private Button[] optionButtons = new Button[3];
    [SerializeField] private Button btnRepeat; 

    private NPCDialogue currentNPC;
    private AnswerType[] buttonMap = new AnswerType[3];

    // Player / controle (opcional para garantir)
    private PlayerMovement playerMovement;

    // Controle de opções
    private bool optionsLocked = false;        // só bloqueia na 1ª leitura
    private Coroutine routine;

    // Controle de freeze
    private float previousTimeScale = 1f;

    public bool OptionsLocked => optionsLocked;

    private enum AnswerType { Correct = 0, Neutral = 1, Wrong = 2 }
    public bool IsOpen => panel != null && panel.activeInHierarchy;

    private void Awake()
    {
        // Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        ApplySessionStateOnLoad();

        if (panel != null)
            panel.SetActive(false);

        // Configura botão repetir
        if (btnRepeat != null)
        {
            btnRepeat.onClick.RemoveAllListeners();
            btnRepeat.onClick.AddListener(OnRepeatClicked);
            btnRepeat.interactable = false; // começa desativado
        }

        // Player refs opcionais
        var player = GameObject.FindWithTag("Player");
        if (player)
        {
            playerMovement = player.GetComponent<PlayerMovement>();
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
                if (npc.HideTarget) npc.HideTarget.SetActive(false);
                continue;
            }

            if (SessionProgress.IsResolved(npc.SituationId))
                npc.MarkResolved();
        }
    }

    public void Show(NPCDialogue npc)
    {
        if (npc == null || npc.IsResolved || string.IsNullOrWhiteSpace(npc.description))
            return;

        currentNPC = npc;

        if (descriptionText)
            descriptionText.text = npc.description;

        // Monta as opções com tipo real
        var slots = new List<(string text, AnswerType type)>
        {
            (npc.optionA, AnswerType.Correct),
            (npc.optionB, AnswerType.Neutral),
            (npc.optionC, AnswerType.Wrong)
        };

        // Embaralha (Fisher–Yates)
        for (int i = slots.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            (slots[i], slots[j]) = (slots[j], slots[i]);
        }

        Debug.Log($"[DialogueManager] Ordem sorteada: {string.Join(", ", slots.ConvertAll(s => s.type.ToString()))}");

        // Aplica texto, mapa e listener
        for (int i = 0; i < optionButtons.Length && i < slots.Count; i++)
        {
            int btnIndex = i;
            var (text, type) = slots[i];

            var btn = optionButtons[i];
            if (!btn) continue;

            var label = btn.GetComponentInChildren<TMP_Text>(true);
            if (label != null)
                label.text = text ?? string.Empty;

            buttonMap[i] = type;

            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => OnOptionClicked(btnIndex));
        }

        panel.SetActive(true);

        FreezeGame(true);

        // Primeira leitura: bloqueia respostas e repetir
        optionsLocked = true;
        SetOptionsInteractable(false);
        if (btnRepeat) btnRepeat.interactable = false;

        // Fala descrição
        RtVoiceService.I?.SpeakDescription(npc.description);

        // Libera depois que terminar
        if (routine != null)
            StopCoroutine(routine);
        routine = StartCoroutine(UnlockAfterFirstDescription());
    }

    private IEnumerator UnlockAfterFirstDescription()
    {
        while (RtVoiceService.I != null && RtVoiceService.I.IsDescriptionSpeaking())
        {
            if (AudioListener.volume <= 0f)
            {
                RtVoiceService.I.StopSpeaking();
                break;
            }

            yield return null;
        }

        yield return new WaitForSecondsRealtime(0.2f);

        optionsLocked = false;
        SetOptionsInteractable(true);
        if (btnRepeat) btnRepeat.interactable = true;

        routine = null;
    }

    private void SetOptionsInteractable(bool value)
    {
        foreach (var btn in optionButtons)
        {
            if (!btn) continue;

            btn.interactable = value;
            var img = btn.GetComponent<Image>();
            if (img) img.raycastTarget = value;
        }
    }

    private void OnOptionClicked(int index)
    {
        if (optionsLocked)
        {
            Debug.Log("[DialogueManager] Clique ignorado: opções bloqueadas na 1ª leitura.");
            return;
        }

        if (currentNPC == null || currentNPC.IsResolved)
        {
            Close();
            return;
        }

        int safeIndex = Mathf.Clamp(index, 0, buttonMap.Length - 1);
        var type = buttonMap[safeIndex];

        switch (type)
        {
            case AnswerType.Correct:
                currentNPC.onChooseCorrect?.Invoke();
                break;
            case AnswerType.Neutral:
                currentNPC.onChooseNeutral?.Invoke();
                break;
            case AnswerType.Wrong:
                currentNPC.onChooseWrong?.Invoke();
                break;
        }

        string choiceTypeStr = type switch
        {
            AnswerType.Correct => "correct",
            AnswerType.Neutral => "neutral",
            _                  => "wrong"
        };

        if (!string.IsNullOrEmpty(currentNPC.dialogueId))
        {
            AssessmentTracker.Instance?.RegisterAnswer(currentNPC.dialogueId, choiceTypeStr);
        }

        currentNPC.MarkResolved();
        SessionProgress.MarkResolved(currentNPC.SituationId);
        InteractionDetector.Instance?.HideIfTarget(currentNPC.transform);

        int scoreIndex = type switch
        {
            AnswerType.Correct => 0,
            AnswerType.Neutral => 1,
            _ => 2
        };
        SituationCounter.Instance?.RegisterAnswer(scoreIndex);
        SituationCounter.Instance?.Increment(1);

        Close();
    }

    private void OnRepeatClicked()
    {
        if (currentNPC == null || string.IsNullOrWhiteSpace(currentNPC.description))
            return;

        Debug.Log("[DialogueManager] Repetindo descrição...");

        if (btnRepeat)
            btnRepeat.interactable = false;

        RtVoiceService.I?.SpeakDescription(currentNPC.description);

        if (routine != null)
            StopCoroutine(routine);
        routine = StartCoroutine(ReenableRepeatAfterSpeak());
    }

    private IEnumerator ReenableRepeatAfterSpeak()
    {
        while (RtVoiceService.I != null && RtVoiceService.I.IsDescriptionSpeaking())
            yield return null;

        yield return new WaitForSecondsRealtime(0.1f);

        if (panel != null && panel.activeInHierarchy && btnRepeat != null)
            btnRepeat.interactable = true;

        routine = null;
    }

    private void Close()
    {
        if (routine != null)
        {
            StopCoroutine(routine);
            routine = null;
        }

        RtVoiceService.I?.StopSpeaking();

        if (panel)
            panel.SetActive(false);

        currentNPC = null;
        optionsLocked = false;
        SetOptionsInteractable(false);

        if (btnRepeat)
            btnRepeat.interactable = false;

        FreezeGame(false);
    }

    private void FreezeGame(bool freeze)
    {
        if (freeze)
        {
            if (Time.timeScale != 0f)
                previousTimeScale = Time.timeScale;

            Time.timeScale = 0f;

            if (playerMovement) playerMovement.enabled = false;
        }
        else
        {
            Time.timeScale = previousTimeScale <= 0f ? 1f : previousTimeScale;

            if (playerMovement) playerMovement.enabled = true;
        }
    }
}
