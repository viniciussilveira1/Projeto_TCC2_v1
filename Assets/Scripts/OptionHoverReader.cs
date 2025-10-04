using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class OptionHoverReader : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler,
    ISelectHandler, IDeselectHandler
{
    [Header("Fonte do texto")]
    [SerializeField] private TMP_Text label;          // filho do Button
    [TextArea] public string overrideText;            // opcional: falar algo diferente do texto visível

    [Header("Anti-spam")]
    [SerializeField] private float minInterval = 0.15f; // evita disparos muito rápidos
    private float lastSpeakTime;

    private string CurrentText =>
        string.IsNullOrWhiteSpace(overrideText) ? (label ? label.text : string.Empty) : overrideText;

    private void Reset()
    {
        label = GetComponentInChildren<TMP_Text>(true);
    }

    private void Awake()
    {
        if (!label) label = GetComponentInChildren<TMP_Text>(true);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (Time.unscaledTime - lastSpeakTime < minInterval) return;
        lastSpeakTime = Time.unscaledTime;
        RtVoiceService.I?.Speak(CurrentText, interrupt: true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        RtVoiceService.I?.StopSpeaking();
    }

    public void OnSelect(BaseEventData eventData)
    {
        if (Time.unscaledTime - lastSpeakTime < minInterval) return;
        lastSpeakTime = Time.unscaledTime;
        RtVoiceService.I?.Speak(CurrentText, interrupt: true);
    }

    public void OnDeselect(BaseEventData eventData)
    {
        RtVoiceService.I?.StopSpeaking();
    }
}
