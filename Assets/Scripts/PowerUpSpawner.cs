using UnityEngine;

public class PowerUpSpawner : MonoBehaviour
{
    [Header("What to spawn")]
    public GameObject[] powerUpPrefabs;

    [Header("Where to spawn (set in Inspector)")]
    public Transform[] spawnPoints;

    [Header("Timing")]
    public float spawnInterval = 8f;
    public float firstSpawnDelay = 2f;

    [Header("Rules")]
    public bool onlyOneAtATime = true;

    GameObject current;

    void Start()
    {
        InvokeRepeating(nameof(Spawn), firstSpawnDelay, spawnInterval);
    }

    void Spawn()
    {
        if (powerUpPrefabs == null || powerUpPrefabs.Length == 0) return;
        if (spawnPoints == null || spawnPoints.Length == 0) return;

        if (onlyOneAtATime && current != null) return;

        int prefabIndex = Random.Range(0, powerUpPrefabs.Length);
        int pointIndex = Random.Range(0, spawnPoints.Length);

        var pos = spawnPoints[pointIndex].position;
        current = Instantiate(powerUpPrefabs[prefabIndex], pos, Quaternion.identity);
    }

    // Optional: call this from the power-up when collected
    public void ClearCurrent(GameObject pickedUp)
    {
        if (pickedUp == current) current = null;
    }
}
