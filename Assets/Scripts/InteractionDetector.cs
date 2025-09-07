using UnityEngine;

public class InteractionDetector : MonoBehaviour
{
    [Header("Referência do ícone (opcional)")]
    public GameObject interactionIcon;

    [Header("Tecla de interação")]
    public KeyCode interactKey = KeyCode.E;

    // Guarda o NPC atual em alcance
    private NPCDialogue currentNPC;

    private void Start()
    {
        if (interactionIcon != null)
            interactionIcon.SetActive(false);
    }

    private void Update()
    {
        // Se há um NPC em alcance e o player apertar E, abre o diálogo
        if (currentNPC != null && Input.GetKeyDown(interactKey))
        {
            if (DialogueManager.Instance == null)
            {
                Debug.LogError("[InteractionDetector] DialogueManager.Instance é nulo. Coloque um DialogueManager na cena e ligue as referências.");
                return;
            }

            DialogueManager.Instance.Show(currentNPC);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Interactable"))
        {
            if (interactionIcon != null)
                interactionIcon.SetActive(true);

            // Tenta pegar o NPCDialogue no objeto que você encostou
            currentNPC = other.GetComponent<NPCDialogue>();
            if (currentNPC == null)
            {
                // Se o collider está num filho, tenta subir para o pai
                currentNPC = other.GetComponentInParent<NPCDialogue>();
            }

            if (currentNPC == null)
            {
                Debug.LogWarning("[InteractionDetector] Tag era Interactable, mas não achei NPCDialogue no objeto/parent.");
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Interactable"))
        {
            if (interactionIcon != null)
                interactionIcon.SetActive(false);

            // Se saiu do mesmo NPC, limpa a referência
            var exitingNPC = other.GetComponent<NPCDialogue>() ?? other.GetComponentInParent<NPCDialogue>();
            if (exitingNPC != null && exitingNPC == currentNPC)
            {
                currentNPC = null;
            }
        }
    }
}
