using UnityEngine;

public class InteractionDetector : MonoBehaviour
{
    [Header("Referência do ícone (opcional)")]
    public GameObject interactionIcon;

    [Header("Tecla de interação")]
    public KeyCode interactKey = KeyCode.E;

    private NPCDialogue currentNPC;

    public static InteractionDetector Instance { get; private set; }

    private void Awake() => Instance = this;

    private void Start()
    {
        if (interactionIcon != null)
            interactionIcon.SetActive(false);
    }

    private void Update()
    {
        // alvo foi desativado/destruído
        if (currentNPC != null && !currentNPC.gameObject.activeInHierarchy)
        {
            HideIcon();
            currentNPC = null;
        }

        // abrir diálogo
        if (currentNPC != null && Input.GetKeyDown(interactKey))
        {
            if (DialogueManager.Instance == null)
            {
                Debug.LogError("[InteractionDetector] DialogueManager.Instance é nulo.");
                return;
            }

            DialogueManager.Instance.Show(currentNPC);
            HideIcon();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Interactable"))
        {
            var npc = other.GetComponent<NPCDialogue>() ?? other.GetComponentInParent<NPCDialogue>();
            if (npc != null && npc.gameObject.activeInHierarchy)
            {
                if (npc.IsResolved || SessionProgress.IsResolved(npc.SituationId))
                    return;

                currentNPC = npc;
                ShowIcon();
            }
        }
        else if (other.CompareTag("Portal"))
        {
            currentNPC = null;
            ShowIcon();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Interactable"))
        {
            var exitingNPC = other.GetComponent<NPCDialogue>() ?? other.GetComponentInParent<NPCDialogue>();
            if (exitingNPC != null && exitingNPC == currentNPC)
            {
                currentNPC = null;
                HideIcon();
            }
            else if (currentNPC == null)
            {
                HideIcon();
            }
        }
        else if (other.CompareTag("Portal"))
        {
            currentNPC = null;
            ShowIcon();
        }
    }

    private void ShowIcon()
    {
        if (interactionIcon != null && !interactionIcon.activeSelf)
            interactionIcon.SetActive(true);
    }

    private void HideIcon()
    {
        if (interactionIcon != null && interactionIcon.activeSelf)
            interactionIcon.SetActive(false);
    }

    public void HideIfTarget(Transform target)
    {
        if (!target) return;
        var npc = target.GetComponent<NPCDialogue>() ?? target.GetComponentInParent<NPCDialogue>();
        if (npc != null && npc == currentNPC)
        {
            currentNPC = null;
            HideIcon();
        }
    }
}
