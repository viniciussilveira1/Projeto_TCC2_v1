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
             
            if (sc.Score >= 90 && sc.Score <= 100)
            {
                finalResult = "Parabéns, você arrasou! Suas Escolhas foram Super Certas e mostraram que você é um verdadeiro cidadão nota 10! O mundo precisa de pessoas como você, que espalham o bem por onde passam!";
            }
            else if (sc.Score >= 50 && sc.Score <= 80)
            {
                finalResult = "Você fez algumas boas escolhas, mas dá para melhorar ainda mais. Que tal jogar de novo, pensar um pouco, se colocar no lugar da outra pessoa, melhorar as escolhas e virar um campeão da cidadania?";
            }
            else if (sc.Score >= 0 && sc.Score <= 40)
            {
                finalResult = "Parece que você não tem feito Escolhas Certas. Suas decisões não foram muito legais. Mas não tem problema! Tente outra vez! Todo mundo pode aprender. Tente outra vez e mostre que você também sabe fazer o certo.";
            }
            resultText.text =
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
