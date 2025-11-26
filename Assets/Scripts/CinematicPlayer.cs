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
    [SerializeField] private float holdSkipDuration = 2f; // segurar para pular toda a cinematic

    private float _skipHoldTime;

    private void Awake()
    {
        if (rootGroup != null) rootGroup.alpha = 0f;
        if (fader != null) fader.alpha = 1f;
    }

    private void Start()
    {
        if (autoPlayOnStart)
            StartCoroutine(PlayRoutine());
    }

    private void Update()
    {
        if (!allowSkip) return;

        if (Input.GetKey(skipKey))
        {
            _skipHoldTime += Time.deltaTime;

            if (_skipHoldTime >= holdSkipDuration)
            {
                StopAllCoroutines();
                StartCoroutine(FinishCinematic());
            }
        }
        else
        {
            _skipHoldTime = 0f;
        }
    }

    private IEnumerator PlayRoutine()
    {
        // Pré-configuração para não aparecer "New Text"
        if (slides != null && slides.Count > 0)
        {
            if (slideImage != null)
            {
                slideImage.sprite = slides[0].image;
                slideImage.preserveAspect = true;
                slideImage.enabled = true;
                slideImage.color = new Color(1f, 1f, 1f, 1f);
            }

            if (captionText != null)
            {
                captionText.text = string.Empty;
            }
        }
        else
        {
            if (slideImage != null) slideImage.sprite = null;
            if (captionText != null) captionText.text = string.Empty;
        }

        if (rootGroup != null && !rootGroup.gameObject.activeSelf)
            rootGroup.gameObject.SetActive(true);

        if (fader != null) fader.alpha = 1f;
        if (rootGroup != null) rootGroup.alpha = 0f;

        Canvas.ForceUpdateCanvases();
        yield return null;

        if (fader != null) yield return FadeCanvasGroup(fader, 1f, 0f, 0.5f);
        if (rootGroup != null) yield return FadeCanvasGroup(rootGroup, 0f, 1f, 0.3f);

        for (int i = 0; i < slides.Count; i++)
        {
            RtVoiceService.I?.StopSpeaking();

            if (slideImage != null)
            {
                slideImage.sprite = slides[i].image;
                FitImageToRect(slideImage, slides[i].image);
            }

            // 1) Mostra a legenda com efeito e deixa o espaço completar o texto
            yield return ShowCaption(slides[i].caption);

            // garante texto completo ao final
            if (captionText != null)
                captionText.text = slides[i].caption;

            // 2) Espera o jogador apertar ESPAÇO para ir para o próximo slide
            yield return WaitForSpaceToAdvance();

            RtVoiceService.I?.StopSpeaking();

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

        while (idx < text.Length)
        {
            // se apertar espaço durante a digitação → mostra tudo
            if (Input.GetKeyDown(skipKey))
            {
                if (captionText != null) captionText.text = text;
                break;
            }

            acc += Time.deltaTime * typewriterSpeed;
            int next = Mathf.Clamp(Mathf.FloorToInt(acc), 0, text.Length);
            if (next != idx)
            {
                if (captionText != null) captionText.text = text.Substring(0, next);
                idx = next;
            }
            yield return null;
        }

        if (captionText != null)
            captionText.text = text;
    }

    private IEnumerator WaitForSpaceToAdvance()
    {
        // espera um novo pressionar de espaço (GetKeyDown)
        bool pressed = false;
        while (!pressed)
        {
            if (Input.GetKeyDown(skipKey))
            {
                pressed = true;
            }
            yield return null;
        }
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
            Portal.Travel(nextSceneName);
        else
            gameObject.SetActive(false);
    }

    public void PlayNow()
    {
        StopAllCoroutines();
        StartCoroutine(PlayRoutine());
    }

    private void FitImageToRect(Image img, Sprite sprite)
    {
        if (img == null) return;

        img.sprite = sprite;
        img.preserveAspect = true;
        img.SetNativeSize();

        RectTransform rt = img.rectTransform;
        RectTransform parentRt = rt.parent as RectTransform;

        if (parentRt == null) parentRt = rt;

        float maxW = parentRt.rect.width;
        float maxH = parentRt.rect.height;

        float w = rt.rect.width;
        float h = rt.rect.height;

        if (w <= 0f || h <= 0f || maxW <= 0f || maxH <= 0f)
            return;

        float scale = Mathf.Min(maxW / w, maxH / h, 1f);

        Vector2 newSize = new Vector2(w * scale, h * scale);
        rt.sizeDelta = newSize;
        rt.anchoredPosition = Vector2.zero;
    }
}
