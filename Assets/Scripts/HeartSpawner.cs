using UnityEngine;
using System.Collections;

public class HeartSpawner : MonoBehaviour
{
    [Header("Heart Settings")]
    public GameObject heartPrefab;
    public float spawnIntervalMin = 10f;
    public float spawnIntervalMax = 20f;
    public float spawnDistanceAhead = 30f; // how far ahead of player to spawn
    public float yOffset = 1f;             // height above ground

    [Header("Lane Settings")]
    public float laneDistance = 2.9f; // must match PlayerController / LaneManager

    private Transform player;

    private void Start()
    {
        // find player by tag (make sure your player GameObject has tag "Player")
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
        else Debug.LogWarning("HeartSpawner: Player with tag 'Player' not found in scene.");

        if (heartPrefab == null)
            Debug.LogError("HeartSpawner: heartPrefab is not assigned!");

        StartCoroutine(SpawnHeartsRoutine());
    }

    private IEnumerator SpawnHeartsRoutine()
    {
        while (true)
        {
            float wait = Random.Range(spawnIntervalMin, spawnIntervalMax);
            // unscaled wait so menu/timeScale doesn't block the coroutine
            yield return new WaitForSecondsRealtime(wait);

            // only spawn when game started AND not game over
            if (GameManager.instance != null)
            {
                if (!GameManager.instance.IsGameStarted() || GameManager.instance.IsGameOver())
                    continue;
            }

            SpawnHeart();
        }
    }

    private void SpawnHeart()
    {
        if (player == null || heartPrefab == null) return;

        // pick random lane 0..2 => x = (lane -1)*laneDistance
        int lane = Random.Range(0, 3);
        float x = (lane - 1) * laneDistance;
        Vector3 spawnPos = new Vector3(x, yOffset, player.position.z + spawnDistanceAhead);

        Instantiate(heartPrefab, spawnPos, Quaternion.identity);
    }
}
