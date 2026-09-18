using UnityEngine;
using System.Collections.Generic;

public class DiamondSpawner : MonoBehaviour
{
    public GameObject diamondPrefab;
    public Transform player;

    [Header("Spawn Settings")]
    public float spawnDistance = 30f;
    public float laneOffset = 2.9f;
    public float height = 4.7f;
    public float spacing = 4f;           // distance between each diamond
    public int diamondsPerBatch = 10;    // number of diamonds spawned each burst
    public float batchInterval = 2f;     // how often a new line of diamonds spawns

    private float timer;
    private PlayerController playerController;
    private List<GameObject> spawnedDiamonds = new List<GameObject>();


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
    void Start()
    {
        playerController = player.GetComponent<PlayerController>();
    }

    void Update()
    {
        if (playerController == null) return;

        // Only spawn while flying
        if (!playerController.isFlying) return;

        timer += Time.deltaTime;
        if (timer >= batchInterval)
        {
            timer = 0f;
            SpawnDiamondBatch();
        }
    }

    void SpawnDiamondBatch()
    {
        // Choose a random lane (-1, 0, 1)
        int lane = Random.Range(-1, 2);
        Vector3 basePos = player.position + Vector3.forward * spawnDistance;
        basePos.x = lane * laneOffset;

        // Create a "trail" of diamonds in a straight line ahead
        for (int i = 0; i < diamondsPerBatch; i++)
        {
            Vector3 spawnPos = basePos + Vector3.forward * (i * spacing);
            spawnPos.y = height;
            GameObject diamond = Instantiate(diamondPrefab, spawnPos, Quaternion.identity);
            spawnedDiamonds.Add(diamond);
        }
    }
}
