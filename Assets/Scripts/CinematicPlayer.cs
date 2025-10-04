using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CinematicPlayer : MonoBehaviour
{
    [Serializable]
    public class CinematicSlide
    {
        public Sprite image;

        [TextArea(2, 5)]
        public string caption;

        [Range(0.5f, 10f)]
        public float holdSeconds = 2.5f;
    }

    [Header("UI (arraste no Inspector)")]
    [SerializeField] private CanvasGroup rootGroup;   // painel principal
    [SerializeField] private Image slideImage;        // imagem do slide
    [SerializeField] private TMP_Text captionText;    // legenda
    [SerializeField] private CanvasGroup fader;       // tela preta por cima (opcional)

    [Header("Conteúdo")]
    [SerializeField] private List<CinematicSlide> slides = new List<CinematicSlide>();

    [Header("Animação")]
    [SerializeField] private float fadeDuration = 0.35f;
    [SerializeField] private float typewriterSpeed = 40f;  // chars/seg

    [Header("Fluxo")]
    [SerializeField] private bool autoPlayOnStart = true;
    [SerializeField] private string nextSceneName = "";
    [SerializeField] private bool allowSkip = true;
    [SerializeField] private KeyCode skipKey = KeyCode.Space;

    private bool _skipping;

    private void Awake()
    {
        if (rootGroup != null) rootGroup.alpha = 0f;
        if (fader != null) fader.alpha = 1f; // começa preto por cima
    }

    private void Start()
    {
        if (autoPlayOnStart) StartCoroutine(PlayRoutine());
    }

    private void Update()
    {
        if (!allowSkip) return;
        if (Input.GetKeyDown(skipKey))
            _skipping = true;
    }

    private IEnumerator PlayRoutine()
    {
        // Fade-in
        if (fader != null)     yield return FadeCanvasGroup(fader, 1f, 0f, 0.5f);
        if (rootGroup != null) yield return FadeCanvasGroup(rootGroup, 0f, 1f, 0.3f);

        for (int i = 0; i < slides.Count; i++)
        {
            _skipping = false;

            // para fala anterior (se houver)
            RtVoiceService.I?.StopSpeaking();

            // imagem do slide
            if (slideImage != null)
            {
                slideImage.sprite = slides[i].image; // (descomentado)
                slideImage.SetNativeSize();          // use "Preserve Aspect" no Image
            }

            // legenda + fala
            yield return ShowCaption(slides[i].caption);

            // hold (ou pulo)
            float t = 0f;
            while (t < slides[i].holdSeconds && !_skipping)
            {
                t += Time.deltaTime;
                yield return null;
            }

            if (_skipping)
                RtVoiceService.I?.StopSpeaking();

            // micro fade entre slides
            if (i < slides.Count - 1 && rootGroup != null)
            {
                yield return FadeCanvasGroup(rootGroup, 1f, 0.85f, 0.12f);
                yield return FadeCanvasGroup(rootGroup, 0.85f, 1f, 0.12f);
            }
        }

        // Fade-out
        if (rootGroup != null) yield return FadeCanvasGroup(rootGroup, 1f, 0f, 0.25f);
        if (fader != null)     yield return FadeCanvasGroup(fader, 0f, 1f, 0.35f);

        // próxima cena (ou desativa GO)
        if (!string.IsNullOrWhiteSpace(nextSceneName))
            UnityEngine.SceneManagement.SceneManager.LoadScene(nextSceneName);
        else
            gameObject.SetActive(false);
    }

    private IEnumerator ShowCaption(string text)
    {
        if (captionText != null) captionText.text = string.Empty;

        // fala direto no serviço
        if (!string.IsNullOrWhiteSpace(text))
        {
            Debug.Log($"[CinematicPlayer] Falando: {text}");
            RtVoiceService.I?.SpeakDescription(text);
        }

        // efeito typewriter
        float acc = 0f;
        int idx = 0;
        while (idx < text.Length && !_skipping)
        {
            acc += Time.deltaTime * typewriterSpeed;
            int next = Mathf.Clamp(Mathf.FloorToInt(acc), 0, text.Length);
            if (next != idx)
            {
                if (captionText != null) captionText.text = text.Substring(0, next);
                idx = next;
            }
            yield return null;
        }

        if (_skipping && captionText != null)
            captionText.text = text;
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup cg, float from, float to, float duration)
    {
        if (cg == null) yield break;

        float t = 0f;
        cg.alpha = from;

        while (t < duration)
        {
            t += Time.deltaTime;
            cg.alpha = Mathf.Lerp(from, to, t / duration);
            yield return null;
        }

        cg.alpha = to;
    }

    // API opcional
    public void PlayNow()
    {
        StopAllCoroutines();
        StartCoroutine(PlayRoutine());
    }
}
