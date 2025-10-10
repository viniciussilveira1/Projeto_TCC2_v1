using UnityEngine;
using UnityEngine.SceneManagement;

public class Portal : MonoBehaviour
{
    [Header("Destino")]
    [SerializeField] private string sceneName;
    [SerializeField] private string spawnPointId = "Default";

    [Header("Interação")]
    [SerializeField] private KeyCode interactKey = KeyCode.E;

    private bool playerNearby;

    public static void Travel(string targetScene, string spawnId = "Default")
    {
        if (string.IsNullOrWhiteSpace(targetScene))
        {
            Debug.LogError("[Portal] Target scene inválida.");
            return;
        }

        if (Time.timeScale != 1f) Time.timeScale = 1f;

        PlayerMovement.PendingSpawnId = spawnId;

        SceneManager.LoadScene(targetScene);
    }

    private void Update()
    {
        if (!playerNearby) return;

        if (Input.GetKeyDown(interactKey))
        {
            Travel(sceneName, spawnPointId);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerNearby = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerNearby = false;
    }

    private void OnValidate()
    {
        if (string.IsNullOrWhiteSpace(spawnPointId))
            spawnPointId = "Default";
    }
}
