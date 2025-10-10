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
    [SerializeField] private CanvasGroup rootGroup;
    [SerializeField] private Image slideImage;
    [SerializeField] private TMP_Text captionText;
    [SerializeField] private CanvasGroup fader;

    [Header("Conteúdo")]
    [SerializeField] private List<CinematicSlide> slides = new List<CinematicSlide>();

    [Header("Animação")]
    [SerializeField] private float fadeDuration = 0.35f;
    [SerializeField] private float typewriterSpeed = 40f;

    [Header("Fluxo")]
    [SerializeField] private bool autoPlayOnStart = true;
    [SerializeField] private string nextSceneName = "";
    [SerializeField] private bool allowSkip = true;
    [SerializeField] private KeyCode skipKey = KeyCode.Space;
    [SerializeField] private float holdSkipDuration = 2f; // tempo segurando para pular toda a cinematics

    private bool _skipSlide;
    private float _skipHoldTime;

    private void Awake()
    {
        if (rootGroup != null) rootGroup.alpha = 0f;
        if (fader != null) fader.alpha = 1f;
    }

    private void Start()
    {
        if (autoPlayOnStart) StartCoroutine(PlayRoutine());
    }

    private void Update()
    {
        if (!allowSkip) return;

        if (Input.GetKey(skipKey))
        {
            _skipHoldTime += Time.deltaTime;

            if (_skipHoldTime >= holdSkipDuration)
            {
                // segura tempo suficiente → pular toda a cinematics
                StopAllCoroutines();
                StartCoroutine(FinishCinematic());
            }
        }
        else if (Input.GetKeyUp(skipKey))
        {
            // liberou a tecla antes do tempo → pular apenas o slide
            _skipSlide = true;
            _skipHoldTime = 0f;
        }
        else
        {
            _skipHoldTime = 0f;
        }
    }

    private IEnumerator PlayRoutine()
    {
        // Fade-in
        if (fader != null) yield return FadeCanvasGroup(fader, 1f, 0f, 0.5f);
        if (rootGroup != null) yield return FadeCanvasGroup(rootGroup, 0f, 1f, 0.3f);

        for (int i = 0; i < slides.Count; i++)
        {
            _skipSlide = false;

            RtVoiceService.I?.StopSpeaking();

            if (slideImage != null)
            {
                slideImage.sprite = slides[i].image;
                FitImageToRect(slideImage, slides[i].image);
            }

            yield return ShowCaption(slides[i].caption);

            // hold do slide
            float t = 0f;
            while (t < slides[i].holdSeconds && !_skipSlide)
            {
                t += Time.deltaTime;
                yield return null;
            }

            RtVoiceService.I?.StopSpeaking();

            // micro fade entre slides
            if (i < slides.Count - 1 && rootGroup != null)
            {
                yield return FadeCanvasGroup(rootGroup, 1f, 0.85f, 0.12f);
                yield return FadeCanvasGroup(rootGroup, 0.85f, 1f, 0.12f);
            }
        }

        yield return FinishCinematic();
    }

    private IEnumerator ShowCaption(string text)
    {
        if (captionText != null) captionText.text = string.Empty;

        if (!string.IsNullOrWhiteSpace(text))
        {
            RtVoiceService.I?.SpeakDescription(text);
        }

        float acc = 0f;
        int idx = 0;
        while (idx < text.Length && !_skipSlide)
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

        if (_skipSlide && captionText != null)
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

    private IEnumerator FinishCinematic()
    {
        RtVoiceService.I?.StopSpeaking();

        if (rootGroup != null) yield return FadeCanvasGroup(rootGroup, rootGroup.alpha, 0f, 0.25f);
        if (fader != null) yield return FadeCanvasGroup(fader, fader.alpha, 1f, 0.35f);

        if (!string.IsNullOrWhiteSpace(nextSceneName))
            Portal.Travel(nextSceneName);   // usa spawn padrão "Default"
        else
            gameObject.SetActive(false);
    }

    // API opcional
    public void PlayNow()
    {
        StopAllCoroutines();
        StartCoroutine(PlayRoutine());
    }
    
    private void FitImageToRect(Image img, Sprite sprite)
    {
        if (img == null) return;

        // set sprite
        img.sprite = sprite;
        img.preserveAspect = true; // importante

        // start with native size so temos dimensões reais do sprite
        img.SetNativeSize();

        RectTransform rt = img.rectTransform;
        RectTransform parentRt = rt.parent as RectTransform;

        // se não tiver parent válido, usamos a própria rect
        if (parentRt == null) parentRt = rt;

        // dimensões disponíveis para encaixar (em unidades de UI)
        float maxW = parentRt.rect.width;
        float maxH = parentRt.rect.height;

        // dimensões do sprite já aplicadas ao rect da Image
        float w = rt.rect.width;
        float h = rt.rect.height;

        if (w <= 0f || h <= 0f || maxW <= 0f || maxH <= 0f)
            return;

        // escala para "fit inside" mantendo proporção
        float scale = Mathf.Min(maxW / w, maxH / h, 1f);

        // aplica novo tamanho mantendo o centro
        Vector2 newSize = new Vector2(w * scale, h * scale);
        rt.sizeDelta = newSize;
        rt.anchoredPosition = Vector2.zero; // centraliza (ajuste se preferir)
    }

}
