using UnityEngine;

public class InteractionDetector : MonoBehaviour
{
    [Header("Referência do ícone")]
    public GameObject interactionIcon;

    private void Start()
    {
        Debug.Log(interactionIcon);
        if (interactionIcon != null)
            interactionIcon.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log(other);

        if (other.CompareTag("Interactable"))
        {
            if (interactionIcon != null)
                interactionIcon.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        Debug.Log(other);

        if (other.CompareTag("Interactable"))
        {
            if (interactionIcon != null)
                interactionIcon.SetActive(false);
        }
    }
}
