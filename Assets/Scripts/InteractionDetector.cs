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
        // só abre se o NPC existe, não está resolvido e o player apertar a tecla
        if (currentNPC != null && Input.GetKeyDown(interactKey))
        {
            if (DialogueManager.Instance == null)
            {
                Debug.LogError("[InteractionDetector] DialogueManager.Instance é nulo. Coloque um DialogueManager na cena e ligue as refer  ências.");
                return;
            }

            DialogueManager.Instance.Show(currentNPC);
        }
    }


    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Interactable") || other.CompareTag("Portal"))
        {
            var npc = other.GetComponent<NPCDialogue>() ?? other.GetComponentInParent<NPCDialogue>();

            if (npc != null )
            {
                currentNPC = npc;
                if (interactionIcon != null)
                    interactionIcon.SetActive(true);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Interactable") || other.CompareTag("Portal"))
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
