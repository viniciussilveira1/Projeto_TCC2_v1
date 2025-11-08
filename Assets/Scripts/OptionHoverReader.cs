using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class OptionHoverReader : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler,
    ISelectHandler, IDeselectHandler
{
    [Header("Fonte do texto")]
    [SerializeField] private TMP_Text label; // filho do Button
    [TextArea] public string overrideText;   // opcional: falar algo diferente do texto visível

    [Header("Anti-spam")]
    [SerializeField] private float minInterval = 0.15f; // evita disparos muito rápidos
    private float lastSpeakTime;

    private string CurrentText =>
        string.IsNullOrWhiteSpace(overrideText)
            ? (label ? label.text : string.Empty)
            : overrideText;

    private void Reset()
    {
        label = GetComponentInChildren<TMP_Text>(true);
    }

    private void Awake()
    {
        if (!label)
            label = GetComponentInChildren<TMP_Text>(true);
    }

    private bool IsLocked()
    {
        // Bloqueia leitura se DialogueManager estiver travando as opções
        if (DialogueManager.Instance != null)
        {
            var field = typeof(DialogueManager).GetField("optionsLocked",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null && (bool)field.GetValue(DialogueManager.Instance))
                return true;
        }

        // Bloqueia se RtVoiceService estiver mudo (durante fala da descrição)
        if (RtVoiceService.I != null && RtVoiceService.I.IsMuted())
            return true;

        return false;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (IsLocked()) return; // 🔒 não fala enquanto bloqueado
        if (Time.unscaledTime - lastSpeakTime < minInterval) return;

        lastSpeakTime = Time.unscaledTime;
        RtVoiceService.I?.Speak(CurrentText, interrupt: true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (IsLocked()) return;
        RtVoiceService.I?.StopSpeaking();
    }

    public void OnSelect(BaseEventData eventData)
    {
        if (IsLocked()) return;
        if (Time.unscaledTime - lastSpeakTime < minInterval) return;

        lastSpeakTime = Time.unscaledTime;
        RtVoiceService.I?.StopSpeaking();
        RtVoiceService.I?.Speak(CurrentText, interrupt: true);
    }

    public void OnDeselect(BaseEventData eventData)
    {
        if (IsLocked()) return;
        RtVoiceService.I?.StopSpeaking();
    }
}
