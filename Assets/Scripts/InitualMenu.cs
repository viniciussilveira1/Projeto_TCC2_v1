using UnityEngine;
using UnityEngine.SceneManagement;

public class InitualMenu : MonoBehaviour
{
    public void Jogar()
    {
        SceneManager.LoadScene("Intro");
    }
}
