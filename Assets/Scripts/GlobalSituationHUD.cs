using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GlobalSituationHUD : MonoBehaviour
{
    public static GlobalSituationHUD Instance;

    [SerializeField] private TMP_Text counterText;
    [SerializeField] private string format = "Situações: {0} / {1}";
    [SerializeField] private int fallbackGoal = 10;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject); // mantém o HUD entre cenas
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        Wire();
        Refresh();
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        if (SituationCounter.Instance != null)
            SituationCounter.Instance.OnChanged -= HandleChanged;
    }

    private void OnSceneLoaded(Scene s, LoadSceneMode m)
    {
        Wire();
        Refresh();
    }

    private void Wire()
    {
        if (SituationCounter.Instance == null) return;
        SituationCounter.Instance.OnChanged -= HandleChanged;
        SituationCounter.Instance.OnChanged += HandleChanged;
    }

    private void HandleChanged(int current, int goal)
    {
        if (counterText != null)
            counterText.text = string.Format(format, current, goal);
    }

    private void Refresh()
    {
        if (counterText == null) return;
        int c = SituationCounter.Instance ? SituationCounter.Instance.Current : 0;
        int g = SituationCounter.Instance ? SituationCounter.Instance.Goal    : fallbackGoal;
        counterText.text = string.Format(format, c, g);
    }
}
