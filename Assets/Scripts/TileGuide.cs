using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System.Linq;

public class TileGuide : MonoBehaviour
{
    private Dictionary<int, Dictionary<Vector3, List<Vector3>>> FloorGuides;
    private Dictionary<Vector3, List<Vector3>> guide;
    private FloorGenerator floorGenerator;
    private TileBase WalkableTile;
    private Tilemap tilemap;
    private List<Vector3> walkableTileLocations = new List<Vector3>();
    private List<List<Vector3>> neighborLocations = new List<List<Vector3>>();
    private float BlockDistance = 1;
    private int i = 0;

    public void GenerateGuide(int floorNumber)
    {
        // Assign variables here since this is called before Start()
        if (FloorGuides == null)
        {
            FloorGuides = new Dictionary<int, Dictionary<Vector3, List<Vector3>>>();
            floorGenerator = (FloorGenerator)transform.parent.GetComponentInParent<FloorGenerator>();
            WalkableTile = floorGenerator.floorTile;
            tilemap = (Tilemap)gameObject.GetComponentInChildren<Tilemap>();
        }

        // Check if guide for current floor has already been generated, if so use it
        if (FloorGuides.ContainsKey(floorNumber))
        {
            guide = FloorGuides[floorNumber];
            Debug.Log("Using previously generated guide for Floor " + floorNumber);
            return;
        }

        // Reset values for each new floor guide
        guide = null;
        walkableTileLocations.Clear();
        neighborLocations.Clear();
        i = 0;

        // Create list of walkable tile locations
        foreach (var pos in tilemap.cellBounds.allPositionsWithin)
        {
            Vector3Int localPlace = new Vector3Int(pos.x, pos.y, pos.z);
            if (tilemap.HasTile(localPlace))
            {
                TileBase tile = tilemap.GetTile(localPlace);
                if (tile == WalkableTile)
                {
                    Vector3 place = tilemap.CellToWorld(localPlace);
                    place.x = place.x + 0.5f;
                    place.y = place.y + 0.5f;
                    walkableTileLocations.Add(place);
                }
            }
        }

        // Remove obstacles from walkable locations
        string obstaclesLog = "Obstacles at ";
        List<Vector3> obstacleLocations = floorGenerator.GetObstacleLocations();
        foreach (Vector3 pos in obstacleLocations)
        {
            if (walkableTileLocations.Contains(pos))
            {
                walkableTileLocations.Remove(pos);
                obstaclesLog = obstaclesLog + (new Vector2(pos.x, pos.y)) + ", ";
            }
        }
        Debug.Log(obstaclesLog);

        // Create list of neighbor walkable locations for each walkable location
        foreach (Vector3 pos in walkableTileLocations)
        {
            neighborLocations.Add(new List<Vector3>());

            Vector3 left = new Vector3(pos.x - BlockDistance, pos.y, pos.z);
            if (walkableTileLocations.Contains(left))
            {
                neighborLocations[i].Add(left);
            }
            Vector3 up = new Vector3(pos.x, pos.y + BlockDistance, pos.z);
            if (walkableTileLocations.Contains(up))
            {
                neighborLocations[i].Add(up);
            }
            Vector3 right = new Vector3(pos.x + BlockDistance, pos.y, pos.z);
            if (walkableTileLocations.Contains(right))
            {
                neighborLocations[i].Add(right);
            }
            Vector3 down = new Vector3(pos.x, pos.y - BlockDistance, pos.z);
            if (walkableTileLocations.Contains(down))
            {
                neighborLocations[i].Add(down);
            }

            i++;
        }

        // Create dictionary
        guide = walkableTileLocations.Zip(neighborLocations, (Vector3 k, List<Vector3> v) => (k, v)).ToDictionary(x => x.k, x => x.v);

        // Save newly generated guide
        if (!FloorGuides.ContainsKey(floorNumber))
        {
            FloorGuides.Add(floorNumber, guide);
            Debug.Log("Created save of FloorGuide for floor " + floorNumber);
        }
    }

    public Dictionary<Vector3, List<Vector3>> GetGuide()
    {
        return guide;
    }
}
