// RtVoiceService.cs
using UnityEngine;
using Crosstales.RTVoice;
using System.Collections;
using Crosstales.RTVoice.Model;

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

    private bool useTTS = false;
    private bool isSpeaking;
    private string currentUid;
    private Coroutine pendingSpeak;
    private float muteUntilTime;
    private bool descriptionSpeaking;
    private bool isMuted;
    private void Awake()
    {
        if (I != null && I != this) { Destroy(gameObject); return; }
        I = this;

        if (!audioSource) audioSource = GetComponent<AudioSource>();
        if (!audioSource) audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        if (mixerGroup && audioSource) audioSource.outputAudioMixerGroup = mixerGroup;

        // Eventos RTVoice
        if (Speaker.Instance != null)
        {
            Speaker.Instance.OnVoicesReady += InitVoz;
            Speaker.Instance.OnSpeakStart += OnSpeakStart;
            Speaker.Instance.OnSpeakComplete += OnSpeakEnd;
        }

        // Inicializa se já houver vozes carregadas
        if (Speaker.Instance != null && Speaker.Instance.Voices != null && Speaker.Instance.Voices.Count > 0)
            InitVoz();

        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        if (Speaker.Instance != null)
        {
            Speaker.Instance.OnVoicesReady -= InitVoz;
            Speaker.Instance.OnSpeakStart -= OnSpeakStart;
            Speaker.Instance.OnSpeakComplete -= OnSpeakEnd;
        }
    }

    /// <summary>
    /// Inicializa TTS apenas se houver voz em português do Brasil (pt-BR).
    /// </summary>
    private void InitVoz()
    {
        if (Speaker.Instance == null || Speaker.Instance.Voices == null || Speaker.Instance.Voices.Count == 0)
        {
            Debug.LogWarning("[RtVoiceService] Nenhuma voz disponível no Speaker.Instance.");
            useTTS = false;
            return;
        }

        var vozPtBr = Speaker.Instance.Voices.Find(v =>
            !string.IsNullOrEmpty(v.Culture) &&
            v.Culture.ToLower().StartsWith("pt-br")
        );

        if (vozPtBr == null)
        {
            Debug.LogWarning("[RtVoiceService] Nenhuma voz pt-BR encontrada. TTS desativado.");
            useTTS = false;
            return;
        }

        Speaker.Instance.VoiceForCulture(vozPtBr.Culture);
        useTTS = true;

        Debug.Log($"[RtVoiceService] Voz pt-BR selecionada: {vozPtBr.Name} ({vozPtBr.Culture})");
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
        else if (Speaker.Instance != null && !Speaker.Instance.isSpeaking)
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

        if (Speaker.Instance != null)
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
        Debug.Log($"[RtVoiceService] texto solicitado: {text}");

        if (pendingSpeak != null)
        {
            StopCoroutine(pendingSpeak);
            pendingSpeak = null;
        }
        pendingSpeak = StartCoroutine(SpeakRoutine(text, interrupt));
    }

    private IEnumerator SpeakRoutine(string text, bool interrupt)
    {
        while (Speaker.Instance == null || Speaker.Instance.Voices == null)
        {
            Debug.Log("[RtVoiceService] Aguardando RTVoice inicializar vozes...");
            yield return null;
        }

        if (!useTTS)
        {
            InitVoz();
        }

        if (!useTTS)
        {
            Debug.Log("[RtVoiceService] Nenhuma voz disponível — pulando TTS.");
            pendingSpeak = null;
            yield break;
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
            voice: null, // usa a voz padrão do Windows
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
        StopSpeaking();
        descriptionSpeaking = true;
        SpeakSafe(text, interrupt: true);
    }

    public bool IsMuted() => isMuted;

    public bool IsDescriptionSpeaking() => descriptionSpeaking;

    // Compatibilidade
    public void Speak(string text, bool interrupt = true) => SpeakSafe(text, interrupt);
}
