using UnityEngine;

public class Tile : MonoBehaviour
{
    public GameObject[] obstaclePrefabs;
    public GameObject coinPrefab;

    public void SpawnItems()
    {
        //Example : spawn 2-3 obstacless per tile
        for(int i = 0;i<3;i++)
        {
            float laneX = LaneManager.GetRandomLane();
            float zPos = transform.position.z + Random.Range(10f, 65f);

            Vector3 spawnPos = new Vector3(laneX, 0f, zPos);
            Instantiate(obstaclePrefabs[Random.Range(0, obstaclePrefabs.Length)], spawnPos, Quaternion.identity, transform);
            
        }

        // Example: spawn 5-10 coins per tile
        for(int i =0;i<10;i++)
        {
            float laneX = LaneManager.GetRandomLane();
            float zPos = transform.position.z + Random.Range(5f, 70f);

            Vector3 spwanPos = new Vector3(laneX, 1f, zPos);
            Instantiate(coinPrefab, spwanPos, Quaternion.identity, transform);
        }

    }
}
