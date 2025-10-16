using UnityEngine;

public class HUDDebugger : MonoBehaviour
{
    public GameObject topHud;

    void Start()
    {
        if (topHud == null) { Debug.LogWarning("topHud não atribuído!"); return; }
        Debug.Log($"TopHUD activeSelf: {topHud.activeSelf}, activeInHierarchy: {topHud.activeInHierarchy}");
        var rt = topHud.GetComponent<RectTransform>();
        if (rt != null)
        {
            Debug.Log($"RectTransform pos: {rt.anchoredPosition}, z: {rt.localPosition.z}, anchors: min{rt.anchorMin} max{rt.anchorMax}");
        }
        var cg = topHud.GetComponent<CanvasGroup>();
        if (cg != null) Debug.Log($"CanvasGroup alpha: {cg.alpha}, interactable: {cg.interactable}");
        var c = topHud.GetComponentInParent<Canvas>();
        Debug.Log($"Canvas parent: {(c != null ? c.name : "nenhum Canvas pai")}");
    }
    void OnEnable()
    {
        Canvas c = topHud.GetComponentInParent<Canvas>();
        if (c != null)
            Debug.Log($"✅ Canvas detectado: {c.name}, renderMode: {c.renderMode}");
        else
            Debug.LogWarning("❌ Ainda sem Canvas pai!");
    }
    
    void Update()
{
    if (topHud.transform.parent == null)
    {
        Debug.LogWarning("⚠️ TopHUD perdeu o pai Canvas em runtime!");
    }
}


}
