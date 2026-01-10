using UnityEngine;

public class LevelSpawner : MonoBehaviour
{
    [Header("Settings")]
    public Transform roomsParent;
    public bool randomSeed = true;
    public int seed = 0;

    void Start()
    {
        GenerateLevel();
    }
    void GenerateLevel()
    {
        int finalSeed;
        if (randomSeed)
        {
            finalSeed = Random.Range(int.MinValue, int.MaxValue);
        }
        else
        {
            finalSeed = seed;
        }

        System.Random rng = new System.Random(finalSeed);

        RoomSpawner[] rooms;
        if (roomsParent != null)
        {
            rooms = roomsParent.GetComponentsInChildren<RoomSpawner>(true);
        }
        else
        {
            // If there is no roomsParent, use the global search API.
            // Don't sort here, sort them later
            rooms = Object.FindObjectsByType<RoomSpawner>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            Debug.LogWarning("Please connect the roomsParent");
        }

        // Sort the Rooms
        System.Array.Sort(rooms, SortByName);

        for (int i = 0; i < rooms.Length; i++)
        {
            rooms[i].SpawnAllPoints(rng);
        }

        Debug.Log("Spawn finished. Seed = " + finalSeed);


    }
    int SortByName(RoomSpawner a, RoomSpawner b)
    {
        return string.Compare(a.name, b.name);
    }


}
