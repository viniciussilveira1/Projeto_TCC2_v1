using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(SortingGroup))]
public class PropGroupSortingLayer : MonoBehaviour
{
    public string sortingLayerBehind = "WalkBehind";
    public string sortingLayerFront = "WalkInFront";
    public Transform player;

    private SortingGroup group;

    void Start()
    {
        group = GetComponent<SortingGroup>();

        if (player == null)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }
    }

    void LateUpdate()
    {
        if (player == null) return;

        // Se o player está abaixo no eixo Y, ele deve aparecer na frente do grupo
        group.sortingLayerName = player.position.y < transform.position.y
            ? sortingLayerFront
            : sortingLayerBehind;
    }
}
