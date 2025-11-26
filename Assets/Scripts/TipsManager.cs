using UnityEngine;
using UnityEngine.UI;

public class TipsManager : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] private Canvas tipsCanvas; // Canvas das dicas (pode estar desativado)
    [SerializeField] private Button playButton; // Botão "Jogar"

    private static bool tipsShownGlobal = false;

    private bool isPaused = false;

    void Start()
    {
        // Se já mostramos em algum momento desta execução do jogo, não mostra de novo
        if (tipsShownGlobal)
        {
            if (tipsCanvas) tipsCanvas.gameObject.SetActive(false);
            return;
        }

        ShowTips();
    }

    private void ShowTips()
    {
        if (!tipsCanvas) return;

        tipsCanvas.gameObject.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;
        AudioListener.pause = true;

        if (playButton != null)
        {
            playButton.onClick.RemoveAllListeners();
            playButton.onClick.AddListener(OnPlayClicked);
        }
    }

    private void OnPlayClicked()
    {
        HideTips();
    }

    private void HideTips()
    {
        if (!tipsCanvas) return;

        tipsCanvas.gameObject.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
        AudioListener.pause = false;

        tipsShownGlobal = true;
    }

    void OnDestroy()
    {
        if (isPaused)
        {
            Time.timeScale = 1f;
            AudioListener.pause = false;
        }
    }

    public static void ResetTips()
    {
        tipsShownGlobal = false;
    }
}
