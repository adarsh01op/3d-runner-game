using UnityEngine;

public class LaneManager : MonoBehaviour
{
    // Laane positions in world space
    public static readonly float[] lanes = { -2.9f, 0f, 2.9f };

    // Get random lane position
    public static float GetRandomLane()
    {
        int index = Random.Range(0, lanes.Length);
        return lanes[index];
    }
}
