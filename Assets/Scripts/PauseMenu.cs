using UnityEngine;
using TMPro;

public class PauseMenu : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] private GameObject panel;
    [SerializeField] private TMP_Text muteButtonText; 
    [SerializeField] private string firstSceneName = "FrontSchool"; 

    private bool isPaused = false;
    private bool isMuted = false;

    private void Start()
    {
        if (panel != null)
            panel.SetActive(false);
            
        UpdateMuteText();
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

    public void ResumeGame()
    {
        if (panel != null)
            panel.SetActive(false);

        Time.timeScale = 1f;
        isPaused = false;
    }

    private void PauseGame()
    {
        if (panel != null)
            panel.SetActive(true);

        Time.timeScale = 0f;
        isPaused = true;
    }

    public void OnRestart()
    {
        Time.timeScale = 1f;

        // Reinicia sistemas do jogo
        SituationCounter.Instance?.ResetAll();
        SessionProgress.ResetAll();

        DialogueManager.Instance?.gameObject.SetActive(true);
        RtVoiceService.I?.StopSpeaking();

        // Recarrega a cena inicial do jogo
        Portal.Travel(firstSceneName);
    }

    public void OnQuit()
    {
        Time.timeScale = 1f;

        // Limpa progressos e sistemas
        SituationCounter.Instance?.ResetAll();
        SessionProgress.ResetAll();
        RtVoiceService.I?.StopSpeaking();

        // 🔹 Destroi o player persistente (que tem DontDestroyOnLoad)
        var player = FindAnyObjectByType<PlayerMovement>(FindObjectsInactive.Include);
        if (player != null)
            Destroy(player.gameObject);

        // 🔹 Destroi também o DialogueManager se ele for persistente
        if (DialogueManager.Instance != null)
            Destroy(DialogueManager.Instance.gameObject);

        // Vai pro menu principal
        Portal.Travel("MainMenu");
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
}
