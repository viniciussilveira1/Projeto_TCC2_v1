using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    [Header("Referências de UI")]
    public GameObject panel;            // DialoguePanel inteiro
    public TMP_Text situationText;      // opcional
    public TMP_Text descriptionText;
    public Button[] optionButtons = new Button[3];

    [Header("Tecla para fechar (opcional)")]
    public KeyCode closeKey = KeyCode.Escape;

    private NPCDialogue currentNPC;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (panel != null) panel.SetActive(false);
    }

    private void Update()
    {
        // Permite fechar com ESC se estiver aberto
        if (panel != null && panel.activeSelf && Input.GetKeyDown(closeKey))
        {
            Hide();
        }
    }

    public void Show(NPCDialogue npc)
    {
        if (npc == null || panel == null || descriptionText == null || optionButtons == null) return;

        currentNPC = npc;

        if (situationText != null)
            situationText.text = npc.situation;

        descriptionText.text = npc.description;

        // Define textos dos botões e listeners
        SetButton(optionButtons[0], npc.optionA, () => OnPick(0));
        SetButton(optionButtons[1], npc.optionB, () => OnPick(1));
        SetButton(optionButtons[2], npc.optionC, () => OnPick(2));

        panel.SetActive(true);

        // Pausa o jogo enquanto o diálogo está aberto
        Time.timeScale = 0f;
    }

    public void Hide()
    {
        if (panel != null) panel.SetActive(false);
        currentNPC = null;

        // Retoma o jogo
        Time.timeScale = 1f;
    }

    private void SetButton(Button btn, string label, UnityEngine.Events.UnityAction onClick)
    {
        if (btn == null) return;

        // Seta o rótulo
        var tmp = btn.GetComponentInChildren<TMP_Text>();
        if (tmp != null) tmp.text = label;

        // Limpa e adiciona callback
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(onClick);
    }

    private void OnPick(int index)
    {
        if (currentNPC == null) { Hide(); return; }

        // 0 = Certa, 1 = Neutra, 2 = Errada (como já estava)
        if (index == 0)
        {
            Debug.Log("[Diálogo] CORRETA");
            currentNPC.onChooseCorrect?.Invoke();
        }
        else if (index == 1)
        {
            Debug.Log("[Diálogo] NEUTRA");
            currentNPC.onChooseNeutral?.Invoke();
        }
        else
        {
            Debug.Log("[Diálogo] ERRADA");
            currentNPC.onChooseWrong?.Invoke();
        }

        // >>> AQUI: registra progresso uma única vez por NPC
        if (currentNPC.countsForProgress && SituationCounter.Instance != null)
            SituationCounter.Instance.Register(currentNPC);

        Hide();
        }

}
