using UnityEngine;
using UnityEngine.SceneManagement;

public class Portal : MonoBehaviour
{
    [Header("Destino")]
    [SerializeField] private string sceneName;          // nome exato da cena no Build Settings
    [SerializeField] private string spawnPointId = "spawn_1"; // ID do SpawnPoint NA CENA DESTINO

    [Header("Interação")]
    [SerializeField] private KeyCode interactKey = KeyCode.E;

    private bool playerNearby;

    private void Update()
    {
        if (!playerNearby) return;

        if (Input.GetKeyDown(interactKey))
        {
            // informa ao Player onde aparecer na próxima cena
            PlayerMovement.PendingSpawnId = string.IsNullOrEmpty(spawnPointId) ? "spawn_1" : spawnPointId;

            // carrega a cena destino
            SceneManager.LoadScene(sceneName);
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
            spawnPointId = "spawn_1";
    }
}
