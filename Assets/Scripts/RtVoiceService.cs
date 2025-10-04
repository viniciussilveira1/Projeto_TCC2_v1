// RtVoiceService.cs
using UnityEngine;
using Crosstales.RTVoice;
using Crosstales.RTVoice.Model;
using System.Collections;

[DefaultExecutionOrder(-100)] // << inicializa antes do Binder/Player
public class RtVoiceService : MonoBehaviour
{
    public static RtVoiceService I { get; private set; }

    [Header("Áudio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private UnityEngine.Audio.AudioMixerGroup mixerGroup;

    [Header("TTS")]
    [Range(0.1f, 3f)] public float rate = 1f;
    [Range(0.5f, 2f)] public float pitch = 1f;
    [Range(0f, 1f)] public float volume = 1f;

    [Header("Proteções")]
    [SerializeField] private float speakDelayAfterSilence = 0.15f;
    [SerializeField] private bool ignoreWhileSpeaking = false;

    private Voice vozPtBr;
    private bool isSpeaking;
    private string currentUid;
    private Coroutine pendingSpeak;
    private float muteUntilTime;
    private bool descriptionSpeaking;

    private void Awake()
    {
        if (I != null && I != this) { Destroy(gameObject); return; }
        I = this;

        if (!audioSource) audioSource = GetComponent<AudioSource>();
        if (!audioSource) audioSource = gameObject.AddComponent<AudioSource>(); // << garante uma fonte
        if (mixerGroup && audioSource) audioSource.outputAudioMixerGroup = mixerGroup;

        // Inscreve nos eventos do RTVoice
        Speaker.Instance.OnVoicesReady += InitVoz;
        Speaker.Instance.OnSpeakStart  += OnSpeakStart;
        Speaker.Instance.OnSpeakComplete += OnSpeakEnd;

        if (Speaker.Instance.Voices.Count > 0)
            InitVoz();

        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        if (Speaker.Instance != null)
        {
            Speaker.Instance.OnVoicesReady -= InitVoz;
            Speaker.Instance.OnSpeakStart  -= OnSpeakStart;
            Speaker.Instance.OnSpeakComplete -= OnSpeakEnd;
        }
    }

    private void InitVoz()
    {
        vozPtBr = Speaker.Instance.VoiceForCulture("pt-BR");
        if (vozPtBr == null)
            vozPtBr = Speaker.Instance.Voices.Find(v => !string.IsNullOrEmpty(v.Culture) && v.Culture.StartsWith("pt"));
        Debug.Log($"[RtVoiceService] Voz selecionada: {vozPtBr?.Name ?? "(padrão do sistema)"}");
    }

    private void OnSpeakStart(Wrapper w)
    {
        isSpeaking = true;
        currentUid = w?.Uid;
    }

    private void OnSpeakEnd(Wrapper w)
    {
        if (w != null && w.Uid == currentUid)
        {
            isSpeaking = false;
            currentUid = null;
        }
        else if (!Speaker.Instance.isSpeaking)
        {
            isSpeaking = false;
            currentUid = null;
        }

        descriptionSpeaking = false;
    }

    public void StopSpeaking()
    {
        if (pendingSpeak != null)
        {
            StopCoroutine(pendingSpeak);
            pendingSpeak = null;
        }

        Speaker.Instance.Silence();
        isSpeaking = false;
        currentUid = null;
        descriptionSpeaking = false;

        if (audioSource) audioSource.Stop();
    }

    public void MuteFor(float seconds) => muteUntilTime = Time.unscaledTime + seconds;

    public void SpeakSafe(string text, bool interrupt = true)
    {
        if (string.IsNullOrWhiteSpace(text)) return;
        if (Time.unscaledTime < muteUntilTime) return;
        if (ignoreWhileSpeaking && isSpeaking) return;
        Debug.Log($"[RtVoiceService] texto: {text}");

        if (pendingSpeak != null)
        {
            StopCoroutine(pendingSpeak);
            pendingSpeak = null;
        }
        pendingSpeak = StartCoroutine(SpeakRoutine(text, interrupt));
    }

    private IEnumerator SpeakRoutine(string text, bool interrupt)
    {
        // Aguarda RTVoice estar pronto e com pelo menos 1 voz
        while (Speaker.Instance == null || Speaker.Instance.Voices == null || Speaker.Instance.Voices.Count == 0)
        {
            Debug.Log("[RtVoiceService] Aguardando RTVoice inicializar vozes...");
            yield return null;
        }

        if (interrupt)
        {
            Speaker.Instance.Silence();
            if (speakDelayAfterSilence > 0f)
                yield return new WaitForSecondsRealtime(speakDelayAfterSilence);
        }

        currentUid = Speaker.Instance.Speak(
            text: text,
            source: audioSource,
            voice: vozPtBr,      // pode ser null -> RTVoice usa padrão
            speakImmediately: true,
            rate: rate,
            pitch: pitch,
            volume: volume,
            forceSSML: false
        );

        Debug.Log($"[RtVoiceService] Speak -> uid: {currentUid}, text: {text}");
        pendingSpeak = null;
    }

    public void SpeakDescription(string text)
    {
        StopSpeaking();                 // já interrompe anterior
        descriptionSpeaking = true;
        SpeakSafe(text, interrupt: true);
    }

    public bool IsDescriptionSpeaking() => descriptionSpeaking;

    // Compat
    public void Speak(string text, bool interrupt = true) => SpeakSafe(text, interrupt);
}
