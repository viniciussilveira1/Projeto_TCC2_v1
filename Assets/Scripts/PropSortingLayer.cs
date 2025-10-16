using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class PropSortingLayer : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    public string sortingLayerBehind = "WalkBehind";
    public string sortingLayerFront = "WalkInFront";

    [Header("Referência do Player (opcional)")]
    public Transform player;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Se o player não foi setado manualmente, tenta achar automaticamente
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }
    }

    void Update()
    {
        if (player == null) return;

        // Se o player está abaixo no eixo Y → está "à frente" do objeto
        if (player.position.y < transform.position.y)
        {
            spriteRenderer.sortingLayerName = sortingLayerFront;
        }
        else
        {
            spriteRenderer.sortingLayerName = sortingLayerBehind;
        }
    }
}
