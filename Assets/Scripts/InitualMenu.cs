using UnityEngine;

public class InitualMenu : MonoBehaviour
{
    public GameObject quitButton;

    void Start()
    {
#if UNITY_WEBGL
        if (quitButton != null)
            quitButton.SetActive(false);
#endif
    }

    public void Jogar()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("FormScene");
    }

    public void Sair()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#elif UNITY_WEBGL
        // Não faz nada porque não existe "sair" no navegador
        Debug.Log("Sair não é suportado no Web.");
#else
        Application.Quit();
#endif
    }
}
