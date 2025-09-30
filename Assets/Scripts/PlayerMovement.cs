// PlayerMovement.cs
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    public static string PendingSpawnId;

    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private string fallbackSpawnId = "spawn_1";

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Vector2 lastNonZeroDir = Vector2.down; // idle “virado” correto
    private Animator animator;

    private static PlayerMovement instance;

    void Awake()
    {
        // singleton + persistência
        if (instance != null && instance != this) { Destroy(gameObject); return; }
        instance = this;
        // garanta que este GameObject é root na Hierarchy ao chamar:
        DontDestroyOnLoad(gameObject);
    }

    void OnEnable()  => SceneManager.sceneLoaded += OnSceneLoaded;
    void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
    }

    void FixedUpdate()
    {
        rb.linearVelocity = moveInput * moveSpeed;
    }

    public void Move(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();

        if (!animator) return;

        bool walking = (context.performed || context.canceled == false) && moveInput.sqrMagnitude > 0.0001f;

        if (walking)
        {
            lastNonZeroDir = moveInput; // guarda última direção válida
        }

        animator.SetBool("isWalking", walking);
        animator.SetFloat("InputX", moveInput.x);
        animator.SetFloat("InputY", moveInput.y);

        if (context.canceled)
        {
            animator.SetBool("isWalking", false);
            animator.SetFloat("LastInputX", lastNonZeroDir.x);
            animator.SetFloat("LastInputY", lastNonZeroDir.y);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        string id = string.IsNullOrEmpty(PendingSpawnId) ? fallbackSpawnId : PendingSpawnId;

        // pega spawn points inclusive inativos
#if UNITY_2023_1_OR_NEWER
        var spawns = Object.FindObjectsByType<SpawnPoint>(FindObjectsInactive.Include, FindObjectsSortMode.None);
#else
        var spawns = Object.FindObjectsOfType<SpawnPoint>(true);
#endif
        foreach (var sp in spawns)
        {
            if (sp.id == id)
            {
                transform.position = sp.transform.position;
                if (rb) rb.linearVelocity = Vector2.zero;
                break;
            }
        }

        PendingSpawnId = null;
    }
}
