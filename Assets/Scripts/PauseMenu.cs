using UnityEngine;
using TMPro;

public class PauseMenu : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] private GameObject background;
    [SerializeField] private GameObject panel;
    [SerializeField] private TMP_Text muteButtonText;
    [SerializeField] private string firstSceneName = "FrontSchool";

    private bool isPaused = false;
    private bool isMuted = false;

    private void Start()
    {
        // garante que começa desligado
        background?.SetActive(false);
        panel?.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }
    }

    private void PauseGame()
    {
        if (background != null) background.SetActive(true);
        if (panel != null) panel.SetActive(true);

        Time.timeScale = 0f;
        isPaused = true;

        // 🔇 Para o TTS imediatamente
        RtVoiceService.I?.StopSpeaking();
    }

    public void ResumeGame()
    {
        if (background != null) background.SetActive(false);
        if (panel != null) panel.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
    }

    public void ToggleMute()
    {
        isMuted = !isMuted;
        AudioListener.volume = isMuted ? 0f : 1f;
        UpdateMuteText();
    }

    private void UpdateMuteText()
    {
        if (muteButtonText != null)
            muteButtonText.text = isMuted ? "Som: Desligado" : "Som: Ligado";
    }
    public void OnRestart()
    {
        Time.timeScale = 1f;

        // Reinicia sistemas do jogo
        SituationCounter.Instance?.ResetAll();
        SessionProgress.ResetAll();

        DialogueManager.Instance?.gameObject.SetActive(true);
        RtVoiceService.I?.StopSpeaking();

        Portal.Travel(firstSceneName);
    }

    public void OnQuit()
    {
        Time.timeScale = 1f;

        // Limpa progressos e sistemas
        SituationCounter.Instance?.ResetAll();
        SessionProgress.ResetAll();
        RtVoiceService.I?.StopSpeaking();

        var player = FindAnyObjectByType<PlayerMovement>(FindObjectsInactive.Include);
        if (player != null)
            Destroy(player.gameObject);

        if (DialogueManager.Instance != null)
            Destroy(DialogueManager.Instance.gameObject);

        Portal.Travel("MainMenu");
    }

}
