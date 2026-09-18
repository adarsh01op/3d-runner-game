using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagnetSpawner : MonoBehaviour
{
    [Header("Magnet Settings")]
    public GameObject magnetPrefab;
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
        else Debug.LogWarning("MagnetSpawner: Player with tag 'Player' not found in scene.");

        if (magnetPrefab == null)
            Debug.LogError("MagnetSpawner: magnetPrefab is not assigned!");

        StartCoroutine(SpawnMagnetRoutine());
    }

    private IEnumerator SpawnMagnetRoutine()
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

            SpawnMagnet();
        }
    }

    private void SpawnMagnet()
    {
        if (player == null || magnetPrefab == null) return;

        // pick random lane 0..2 => x = (lane -1)*laneDistance
        int lane = Random.Range(0, 3);
        float x = (lane - 1) * laneDistance;
        Vector3 spawnPos = new Vector3(x, yOffset, player.position.z + spawnDistanceAhead);

        Instantiate(magnetPrefab, spawnPos, Quaternion.identity);
    }
}
