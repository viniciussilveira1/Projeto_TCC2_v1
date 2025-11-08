using UnityEngine;
using UnityEngine.SceneManagement;

public class InitualMenu : MonoBehaviour
{
    public void Jogar()
    {
        SceneManager.LoadScene("Intro");
    }

    public void Sair()
    {
#if UNITY_EDITOR
        // Fecha o modo Play no Editor
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // Fecha o jogo compilado
        Application.Quit();
#endif
    }
}
