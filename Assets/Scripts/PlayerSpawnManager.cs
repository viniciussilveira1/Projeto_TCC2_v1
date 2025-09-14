using UnityEngine;

public class PlayerSpawnManager : MonoBehaviour
{
    void Start()
    {
        string spawnName = PlayerPrefs.GetString("LastSpawn", "");
        if (!string.IsNullOrEmpty(spawnName))
        {
            GameObject spawnPoint = GameObject.Find(spawnName);
            if (spawnPoint != null)
            {
                transform.position = spawnPoint.transform.position;
            }
        }
    }
}
