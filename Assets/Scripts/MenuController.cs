using UnityEngine;

public class MenuController : MonoBehaviour
{
    public GameObject menuCanvas;

    void Start()
    {
        if (menuCanvas != null)
            menuCanvas.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            var dm = DialogueManager.Instance;

            if (dm != null && dm.IsOpen)
                return;

            ToggleMenu();
        }
    }

    public void ToggleMenu()
    {
        if (!menuCanvas) return;
        menuCanvas.SetActive(!menuCanvas.activeSelf);
    }
}
