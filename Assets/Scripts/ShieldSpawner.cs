using System.Collections;
using UnityEngine;

public class ShieldSpawner : MonoBehaviour
{
    [Header("Shield Settings")]
    public GameObject shieldPrefab;
    public float spawnIntervalMin = 18f;
    public float spawnIntervalMax = 30f;
    public float spawnDistanceAhead = 30f; // how far ahead of player to spawn
    public float yOffset = 1f;             // height above ground

    [Header("Lane Settings")]
    public float laneDistance = 2.9f; // must match PlayerController / LaneManager

    private Transform player;

    private void Start()
    {
        // find player by tag (make sure your Player GameObject has tag "Player")
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
        else Debug.LogWarning("ShieldSpawner: Player with tag 'Player' not found in scene.");

        if (shieldPrefab == null)
            Debug.LogError("ShieldSpawner: shieldPrefab is not assigned!");

        StartCoroutine(SpawnShieldRoutine());
    }

    private IEnumerator SpawnShieldRoutine()
    {
        while (true)
        {
            float wait = Random.Range(spawnIntervalMin, spawnIntervalMax);
            // unscaled wait so pause/menu doesn't break spawning
            yield return new WaitForSecondsRealtime(wait);

            // only spawn when game started AND not game over
            if (GameManager.instance != null)
            {
                if (!GameManager.instance.IsGameStarted() || GameManager.instance.IsGameOver())
                    continue;
            }

            SpawnShield();
        }
    }

    private void SpawnShield()
    {
        if (player == null || shieldPrefab == null) return;

        // pick random lane (0 = left, 1 = center, 2 = right)
        int lane = Random.Range(0, 3);
        float x = (lane - 1) * laneDistance;

        Vector3 spawnPos = new Vector3(x, yOffset, player.position.z + spawnDistanceAhead);
        Instantiate(shieldPrefab, spawnPos, Quaternion.identity);
    }
}
