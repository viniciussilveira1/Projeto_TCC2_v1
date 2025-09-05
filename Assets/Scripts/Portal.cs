using UnityEngine;
using UnityEngine.SceneManagement;

public class Portal : MonoBehaviour
{
    [Header("Configurações do Portal")]
    [SerializeField] private string sceneName = "Hall"; // Nome da cena de destino
    [SerializeField] private Animator doorAnimator;     // Animator do SpriteDoor
    [SerializeField] private string openTriggerName = "OpenDoor"; // Trigger da animação

    private bool playerNearby = false; // Sabe se o player está perto

    private void Update()
    {
        // Se o player estiver perto e apertar E
        if (playerNearby && Input.GetKeyDown(KeyCode.E))
        {
            // Toca a animação da porta, se houver Animator
            if (doorAnimator != null)
                doorAnimator.SetTrigger(openTriggerName);

            // Teleporta para a nova cena
            SceneManager.LoadScene(sceneName);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerNearby = true; // Player entrou no trigger
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerNearby = false; // Player saiu do trigger
    }
}
