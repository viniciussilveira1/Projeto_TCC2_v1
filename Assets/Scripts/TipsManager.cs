using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TipsManager : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] private Canvas tipsCanvas; // Canvas das dicas (pode estar desativado)
    [SerializeField] private Button playButton; // Botão "Jogar"

    private static bool tipsShown = false; // Mantém o estado entre reinícios da cena
    private bool isPaused = false;

    void Start()
    {
        if (tipsShown)
        {
            // Já foi mostrado uma vez — garante que fique oculto
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
            playButton.onClick.AddListener(OnPlayClicked);
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

        tipsShown = true; // marca como já mostrado
    }

    // (Opcional) Se quiser limpar ao trocar de cena:
    void OnDestroy()
    {
        if (isPaused)
        {
            Time.timeScale = 1f;
            AudioListener.pause = false;
        }
    }
}
