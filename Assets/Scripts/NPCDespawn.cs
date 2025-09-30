using System.Collections;
using UnityEngine;

public class NPCDespawn : MonoBehaviour
{
    [Header("Comportamento")]
    [SerializeField] private bool destroyInsteadOfDisable = false;
    [SerializeField] private float delaySeconds = 0f;

    [Header("Extras (opcional)")]
    [SerializeField] private GameObject[] alsoDisable;

    [Header("Animação (opcional)")]
    [SerializeField] private Animator animator;
    [SerializeField] private string despawnTrigger;

    [Header("Colisão")]
    [SerializeField] private bool disableAllColliders = true;

    public void Despawn() => DespawnTarget(gameObject);

    public void DespawnTarget(GameObject target)
    {
        if (!target) return;
        if (delaySeconds <= 0f) ApplyDespawn(target);
        else StartCoroutine(DoDespawn(target));
    }

    public void DespawnTarget(Component target)
    {
        if (!target) return;
        DespawnTarget(target.gameObject);
    }

    private IEnumerator DoDespawn(GameObject go)
    {
        if (animator && !string.IsNullOrEmpty(despawnTrigger))
            animator.SetTrigger(despawnTrigger);

        if (disableAllColliders)
            foreach (var col in go.GetComponentsInChildren<Collider2D>(true))
                col.enabled = false;

        if (delaySeconds > 0f)
            yield return new WaitForSeconds(delaySeconds);

        ApplyDespawn(go);
    }

    private void ApplyDespawn(GameObject go)
    {
        // marca ELIMINADO na sessão (esse NPC/situação some ao recarregar)
        var npc = go.GetComponentInParent<NPCDialogue>(true);
        if (npc != null)
            SessionProgress.MarkEliminated(npc.SituationId);

        // esconde "!" se for o alvo atual
        InteractionDetector.Instance?.HideIfTarget(go.transform);

        // extras visuais
        if (alsoDisable != null)
            foreach (var extra in alsoDisable)
                if (extra) extra.SetActive(false);

        // some de fato
        if (destroyInsteadOfDisable) Destroy(go);
        else go.SetActive(false);
    }

    private void OnDisable()
    {
        if (!gameObject.scene.IsValid()) return; // ignora em troca de cena
        InteractionDetector.Instance?.HideIfTarget(transform);
    }

    [ContextMenu("Test Despawn Now")]
    private void TestDespawnNow() => Despawn();
}
