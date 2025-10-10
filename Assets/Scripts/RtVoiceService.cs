// RtVoiceService.cs
using UnityEngine;
using Crosstales.RTVoice;
using Crosstales.RTVoice.Model;
using System.Collections;
using System.Linq;

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

    // Voz selecionada (pt-BR)
    private Voice vozPtBr;
    // Se temos TTS disponível e permitido (somente se achar voz pt-br)
    private bool useTTS = false;

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
        audioSource.playOnAwake = false;
        if (mixerGroup && audioSource) audioSource.outputAudioMixerGroup = mixerGroup;

        // Inscreve nos eventos do RTVoice
        if (Speaker.Instance != null)
        {
            Speaker.Instance.OnVoicesReady += InitVoz;
            Speaker.Instance.OnSpeakStart  += OnSpeakStart;
            Speaker.Instance.OnSpeakComplete += OnSpeakEnd;
        }

        // Se já há vozes carregadas, inicializa imediatamente
        if (Speaker.Instance != null && Speaker.Instance.Voices != null && Speaker.Instance.Voices.Count > 0)
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

    /// <summary>
    /// Inicializa a seleção de voz: procura por pt-BR, depois por pt, e também tenta por nomes contendo "Portuguese"/"Português".
    /// Se não encontrar nada, desativa o uso de TTS (useTTS = false).
    /// </summary>
    private void InitVoz()
    {
        vozPtBr = null;
        useTTS = false;

        if (Speaker.Instance == null || Speaker.Instance.Voices == null || Speaker.Instance.Voices.Count == 0)
        {
            Debug.LogWarning("[RtVoiceService] Nenhuma voz disponível no Speaker.Instance.");
            return;
        }

        // 1) Busca voz que tenha culture exata "pt-BR"
        vozPtBr = Speaker.Instance.Voices.Find(v => !string.IsNullOrEmpty(v.Culture) && v.Culture.Equals("pt-BR", System.StringComparison.OrdinalIgnoreCase));

        // 2) Se não encontrou, busca por culture que comece com "pt"
        if (vozPtBr == null)
            vozPtBr = Speaker.Instance.Voices.Find(v => !string.IsNullOrEmpty(v.Culture) && v.Culture.StartsWith("pt", System.StringComparison.OrdinalIgnoreCase));

        // 3) Se ainda não encontrou, tenta nomes que contenham "Portuguese" ou "Português"
        if (vozPtBr == null)
        {
            vozPtBr = Speaker.Instance.Voices.Find(v =>
                (!string.IsNullOrEmpty(v.Name) && (v.Name.ToLower().Contains("portuguese") || v.Name.ToLower().Contains("português")))
                || (!string.IsNullOrEmpty(v.Description) && (v.Description.ToLower().Contains("portuguese") || v.Description.ToLower().Contains("português")))
            );
        }

        if (vozPtBr != null)
        {
            useTTS = true;
            Debug.Log($"[RtVoiceService] Voz pt encontrada e habilitada: {vozPtBr.Name} ({vozPtBr.Culture})");
        }
        else
        {
            useTTS = false;
            Debug.LogWarning("[RtVoiceService] Nenhuma voz pt-BR encontrada no sistema. TTS será desativado (não haverá fala).");
        }
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
        // Aguarda RTVoice estar pronto e com pelo menos 1 voz (ou até Speaker existir)
        while (Speaker.Instance == null || Speaker.Instance.Voices == null)
        {
            Debug.Log("[RtVoiceService] Aguardando RTVoice inicializar vozes...");
            yield return null;
        }

        // Reconfere a voz caso tenha sido carregada tardiamente
        if (!useTTS)
        {
            InitVoz();
        }

        // Se não temos voz pt-br disponível, não fala
        if (!useTTS || vozPtBr == null)
        {
            Debug.Log("[RtVoiceService] Voz pt-BR não disponível — pulando TTS.");
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
            voice: vozPtBr,      // voz garantida ser pt-br (ou similar)
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
