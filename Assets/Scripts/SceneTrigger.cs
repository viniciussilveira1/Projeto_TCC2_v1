using UnityEngine;
using UnityEngine.SceneManagement; // Necessário para trocar cenas

public class SceneTrigger : MonoBehaviour
{
    public string sceneName; // Nome da cena de destino

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Player entered the trigger");
        if (other.CompareTag("Player")) // Certifique-se de que seu player tem a tag "Player"
        {
            SceneManager.LoadScene(sceneName);
        }
    }
}
