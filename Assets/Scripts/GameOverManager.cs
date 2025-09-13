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
            resultText.text =
                $"Você concluiu {sc.Current}/{sc.Goal} situações.\n\n" +
                $"Acertos: {sc.Correct}\n" +
                $"Neutras: {sc.Neutral}\n" +
                $"Erradas: {sc.Wrong}";
        }

        if (gameOverPanel != null) gameOverPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    private void OnRestart()
    {
        Time.timeScale = 1f;

        if (SituationCounter.Instance != null)
            SituationCounter.Instance.ResetAll();

        if (!string.IsNullOrEmpty(firstSceneName))
            SceneManager.LoadScene(firstSceneName);
        else
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void OnQuit()
    {
        Time.timeScale = 1f;
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
