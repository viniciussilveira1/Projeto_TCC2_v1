using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TipsManager : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] private Canvas tipsCanvas; // Canvas das dicas (pode estar desativado)
    [SerializeField] private Button playButton; // Botão "Jogar"

    // AGORA NÃO É MAIS STATIC
    private bool tipsShown = false;
    private bool isPaused = false;

    void Start()
    {
        if (tipsShown)
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

        tipsShown = true; // marca como já mostrado nesta instância
    }

    void OnDestroy()
    {
        if (isPaused)
        {
            Time.timeScale = 1f;
            AudioListener.pause = false;
        }
    }
}
