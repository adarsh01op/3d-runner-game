using System.Collections.Generic;
using UnityEngine;

public class TileManager : MonoBehaviour
{
    public GameObject[] tilePrefabs;
    public Transform player;
    public int numberOfTiles = 4;

    private List<GameObject> activeTiles = new List<GameObject>();
    private Transform lastSpawnPoint; // keeps track of where to spawn next


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
        // spawn initial tiles
        for (int i = 0; i < numberOfTiles; i++)
        {
            if (i == 0)
                SpawnTile(0); // first tile, at origin
            else
                SpawnTile();
        }
    }

    void Update()
    {
        // Check if the current player reference is missing or disabled due to character swapping
        if (player == null || !player.gameObject.activeInHierarchy)
        {
            GameObject activePlayer = GameObject.FindGameObjectWithTag("Player");
            if (activePlayer != null)
            {
                player = activePlayer.transform;
            }
            else
            {
                return; // Stop running this frame if no active player is found to avoid errors
            }
        }

        // Spawn new tile when player moves forward enough
        if (player.position.z - 35 > activeTiles[0].transform.position.z + 80f)
        {
            SpawnTile();
            DeleteTile();
        }
    }

    void SpawnTile(int prefabIndex = -1)
    {
        if (prefabIndex == -1)
            prefabIndex = Random.Range(0, tilePrefabs.Length);

        GameObject newTile;

        if (lastSpawnPoint == null)
            //First tile at origin
            newTile = Instantiate(tilePrefabs[prefabIndex], Vector3.zero, Quaternion.identity);
        else // Spawn at the last tile's SpawnPoint
            newTile = Instantiate(tilePrefabs[prefabIndex], lastSpawnPoint.position, Quaternion.identity);

        // update the lastSpawnPont to this tile's SpawnPoint
        Transform spawnPoint = newTile.transform.Find("SpawnPoint");
        if (spawnPoint != null)
            lastSpawnPoint = spawnPoint;

        // spawn coins/obstacles
        Tile tileScript = newTile.GetComponent<Tile>();
        if (tileScript != null)
            tileScript.SpawnItems();

        activeTiles.Add(newTile);
    }





    void DeleteTile()
    {
        Destroy(activeTiles[0]);
        activeTiles.RemoveAt(0);
    }
}
