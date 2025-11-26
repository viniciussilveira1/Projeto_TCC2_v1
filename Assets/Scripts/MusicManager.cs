using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    [Header("Configuração de Cenas")]
    [SerializeField] private string firstPlayableSceneName = "FrontSchool";
    [SerializeField] private string[] playableScenes;

    [Header("Clipes de Áudio")]
    [SerializeField] private AudioClip introClip;
    [SerializeField] private AudioClip loopClip;

    [Header("Volume e Fade")]
    [Range(0f, 1f)]
    [SerializeField] private float musicVolume = 0.3f;
    [SerializeField] private float fadeInDuration = 2f;
    [SerializeField] private float introStartDelay = 0f;
    [SerializeField] private float fadeOutDuration = 1f;

    private AudioSource audioSource;
    private bool introPlayed = false;
    private bool loopStarted = false;
    private Coroutine fadeRoutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.volume = 0f;
        audioSource.spatialBlend = 0f;

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        string sceneName = scene.name;

        // Não é cena jogável: fade-out e para
        if (!IsPlayableScene(sceneName))
        {
            if (audioSource.isPlaying)
                StartFadeOutAndStop();

            return;
        }

        // Cena jogável

        // Se voltamos para a primeira cena jogável e a música está parada,
        // trata como "novo jogo": libera a intro de novo.
        if (sceneName == firstPlayableSceneName && !audioSource.isPlaying)
        {
            introPlayed = false;
            loopStarted = false;
        }

        // Primeira vez (ou "novo jogo"): intro + loop
        if (!introPlayed && sceneName == firstPlayableSceneName)
        {
            StartCoroutine(PlayIntroThenLoop());
            return;
        }

        // Intro já tocou: garante loop nas jogáveis
        if (introPlayed)
        {
            EnsureLoopPlaying();
        }
    }

    private bool IsPlayableScene(string sceneName)
    {
        if (playableScenes == null || playableScenes.Length == 0)
            return false;

        for (int i = 0; i < playableScenes.Length; i++)
        {
            if (playableScenes[i] == sceneName)
                return true;
        }

        return false;
    }

    private IEnumerator PlayIntroThenLoop()
    {
        introPlayed = true;
        loopStarted = false;

        if (introStartDelay > 0f)
            yield return new WaitForSeconds(introStartDelay);

        if (introClip != null)
        {
            audioSource.clip = introClip;
            audioSource.loop = false;
            audioSource.volume = 0f;
            audioSource.Play();

            if (fadeInDuration > 0f)
                yield return StartCoroutine(FadeIn(audioSource, fadeInDuration, musicVolume));
            else
                audioSource.volume = musicVolume;

            float remaining = introClip.length - fadeInDuration;
            if (remaining > 0f)
                yield return new WaitForSeconds(remaining);
        }

        StartLoop();
    }

    private void StartLoop()
    {
        if (loopClip == null) return;

        audioSource.clip = loopClip;
        audioSource.loop = true;
        audioSource.volume = musicVolume;
        audioSource.Play();
        loopStarted = true;
    }

    private void EnsureLoopPlaying()
    {
        if (!loopStarted || !audioSource.isPlaying)
            StartLoop();
    }

    private IEnumerator FadeIn(AudioSource source, float duration, float targetVolume)
    {
        float startVol = 0f;
        float t = 0f;

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            source.volume = Mathf.Lerp(startVol, targetVolume, t / duration);
            yield return null;
        }

        source.volume = targetVolume;
    }

    private void StartFadeOutAndStop()
    {
        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);

        fadeRoutine = StartCoroutine(FadeOutAndStop());
    }

    private IEnumerator FadeOutAndStop()
    {
        float startVol = audioSource.volume;
        float t = 0f;
        float dur = Mathf.Max(0.01f, fadeOutDuration);

        while (t < dur)
        {
            t += Time.unscaledDeltaTime;
            audioSource.volume = Mathf.Lerp(startVol, 0f, t / dur);
            yield return null;
        }

        audioSource.Stop();
        audioSource.volume = musicVolume;
        loopStarted = false;
    }

    public void SetMusicMuted(bool muted)
    {
        if (audioSource != null)
            audioSource.mute = muted;
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        if (audioSource != null && !audioSource.mute && audioSource.isPlaying)
            audioSource.volume = musicVolume;
    }
}
