using UnityEngine;

public class RoomSpawner : MonoBehaviour
{
    public void SpawnAllPoints(System.Random rng)
    {
        SpawnPoint[] points = GetComponentsInChildren<SpawnPoint>(true);

        // Sort the SpawnPoints
        System.Array.Sort(points, SortByName);

        for (int i = 0; i < points.Length; i++)
        {
            points[i].Spawn(rng);
        }
    }

    int SortByName(SpawnPoint a, SpawnPoint b)
    {
        return string.Compare(a.name, b.name);
    }
}
