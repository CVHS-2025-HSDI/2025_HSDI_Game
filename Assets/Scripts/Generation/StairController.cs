using UnityEngine;
using UnityEngine.SceneManagement;

public enum StairType
{
    Down, // Goes down one floor (or exits if on floor #1)
    Up    // Goes up one floor (or does nothing if on floor #N)
}

[RequireComponent(typeof(Collider2D))]
public class StairController : MonoBehaviour
{
    [Header("Stair Info")]
    public StairType stairType;
    public int currentFloor;     // Which floor number is this stair on?
    public int totalFloors = 5;  // The total number of floors in the tower

    private MasterLevelManager _manager;

    void Start()
    {
        // Find the _manager in the scene (assuming it's persistent)
        _manager = FindFirstObjectByType<MasterLevelManager>();
    }

    // We assume the stair has a 2D collider set to "IsTrigger = true"
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return; // Only react if it's the player

        Debug.Log($"[StairController] Player stepped on {stairType} stair, floor {currentFloor}.");

        if (stairType == StairType.Down)
        {
            // If on floor 1, going down can exit the tower or load the "TownScene"
            if (currentFloor == 1)
            {
                Debug.Log("Exiting the tower...");
                // For demonstration, load the scene called "TownScene" in the future:
                SceneManager.LoadScene("TownScene");
            }
            else
            {
                // Otherwise, load the floor below
                int newFloor = currentFloor - 1;
                Debug.Log($"Going down to floor {newFloor}");
                _manager.GenerateAndLoadFloor(newFloor, isFirstFloor: (newFloor == 1));
            }
        }
        else // StairType.Up
        {
            // If on the last floor, there's no further up
            if (currentFloor >= totalFloors)
            {
                Debug.Log("No more floors above!");
                // Possibly do nothing or show a message
            }
            else
            {
                int newFloor = currentFloor + 1;
                Debug.Log($"Going up to floor {newFloor}");
                _manager.GenerateAndLoadFloor(newFloor, isFirstFloor: (newFloor == 1));
            }
        }
    }
}
