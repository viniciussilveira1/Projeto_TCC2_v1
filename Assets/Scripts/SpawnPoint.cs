using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    [Tooltip("ID que os Portais/Player usam para te localizar")]
    public string id = "spawn_1";

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 0.2f);
    }
}
