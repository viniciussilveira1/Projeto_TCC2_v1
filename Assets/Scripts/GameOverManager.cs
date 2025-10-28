using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    [Header("Referências UI")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TMP_Text   resultText;
    [SerializeField] private Button     restartButton;
    [SerializeField] private Button     quitButton;

    [Header("Cena inicial do jogo")]
    [SerializeField] private string firstSceneName = "FrontSchool";

    private bool isBound = false;

    private void Start()
    {
        Bind();
        if (gameOverPanel != null) gameOverPanel.SetActive(false);

        if (restartButton != null) restartButton.onClick.AddListener(OnRestart);
        if (quitButton != null)    quitButton.onClick.AddListener(OnQuit);
    }

    private void OnEnable()  { Bind(); }
    private void OnDisable() { Unbind(); }

    private void Bind()
    {
        if (isBound) return;
        if (SituationCounter.Instance != null)
        {
            SituationCounter.Instance.OnGoalReached += HandleGoalReached;
            isBound = true;
        }
    }

    private void Unbind()
    {
        if (!isBound) return;
        if (SituationCounter.Instance != null)
            SituationCounter.Instance.OnGoalReached -= HandleGoalReached;
        isBound = false;
    }

    private void HandleGoalReached()
    {
        Time.timeScale = 0f;

        if (SituationCounter.Instance != null && resultText != null)
        {
            var sc = SituationCounter.Instance;

            var finalResult = "";
             
            if (sc.Score >= 80 && sc.Score <= 100)
            {
                finalResult = "Excelente trabalho! Você demonstrou grande habilidade e dedicação ao resolver as situações apresentadas. Continue assim!";
            }
            else if (sc.Score >= 50 && sc.Score < 80)
            {
                finalResult = "Bom trabalho! Você conseguiu resolver a maioria das situações, mas ainda há espaço para melhorias. Continue praticando!";
            }
            else if (sc.Score >= 30 && sc.Score < 50)
            {
                finalResult = "Você conseguiu resolver algumas situações, mas é importante revisar os conceitos e estratégias para melhorar seu desempenho. Não desanime!";
            }
            else
            {
                finalResult = "Infelizmente, você não conseguiu resolver situações suficientes. Revise o material e tente novamente!";
            }
            resultText.text =
                $"Você concluiu {sc.Current}/{sc.Goal} situações.\n\n" +
                $"{finalResult}";
            }

        if (gameOverPanel != null) gameOverPanel.SetActive(true);
        Time.timeScale = 0f;
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

        // Destroi o player persistente (que tem DontDestroyOnLoad)
        var player = FindAnyObjectByType<PlayerMovement>(FindObjectsInactive.Include);
        if (player != null)
            Destroy(player.gameObject);

        // Destroi também o DialogueManager se ele for persistente
        if (DialogueManager.Instance != null)
            Destroy(DialogueManager.Instance.gameObject);

        // Vai pro menu principal
        Portal.Travel("MainMenu");
    }
}
