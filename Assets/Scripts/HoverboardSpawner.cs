using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoverboardSpawner : MonoBehaviour
{
    [Header("References")]
    public GameObject hoverBoardPrefab;
    public Transform player;

    [Header("Spawn Settings")]
    public float spawnInterval = 15f;      // every 15 seconds
    public float spawnDistance = 30f;      // ahead of player
    public float height = 1f;            // on ground or just above
    public float laneDistance = 2.9f;

    private float timer = 0f;
    private void Awake()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        else
        {
            Debug.LogWarning("No GameObject found with tag 'Player'!");
        }
    }
    void Update()
    {
        if (player == null || hoverBoardPrefab == null) return;

        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            timer = 0f;
            SpawnHoverboard();
        }
    }

    void SpawnHoverboard()
    {
        int lane = Random.Range(0, 3);
        float xPos = (lane - 1) * laneDistance;
        float zPos = player.position.z + spawnDistance;

        Vector3 spawnPos = new Vector3(xPos, height, zPos);
        Instantiate(hoverBoardPrefab, spawnPos, Quaternion.identity);
    }
}
