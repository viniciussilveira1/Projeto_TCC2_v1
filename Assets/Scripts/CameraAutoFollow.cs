// CameraAutoFollow.cs (Cinemachine 3)
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Cinemachine;

public class CameraAutoFollow : MonoBehaviour
{
    [SerializeField] private CinemachineCamera vcam;
    [SerializeField] private CinemachineConfiner2D confiner;

    void Awake()
    {
        if (!vcam) vcam = GetComponent<CinemachineCamera>();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Start()
    {
        Bind(); // garante a cena inicial
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Bind();
        if (confiner) confiner.InvalidateBoundingShapeCache();
    }

    void Bind()
    {
        if (!vcam) return;

        var player = GameObject.FindWithTag("Player");
        if (!player) return;
Debug.Log($"CameraAutoFollow: vinculado a {player}");
        // Cinemachine 3: use Target.TrackingTarget (e opcional LookAtTarget)
        vcam.Target.TrackingTarget = player.transform;

        if (vcam.Target.LookAtTarget == null)
            vcam.Target.LookAtTarget = player.transform;
    }
}
