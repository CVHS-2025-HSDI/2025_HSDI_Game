using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System.Linq;

public class TileGuide : MonoBehaviour
{
    private List<Vector3> walkableTileLocations = new List<Vector3>();
    private TileBase WalkableTile;
    private Tilemap tilemap;
    private List<List<Vector3>> neighborLocations = new List<List<Vector3>>();
    private float BlockDistance = 1;
    private int i = 0;
    private Dictionary<Vector3, List<Vector3>> guide;

    void Start()
    {
        WalkableTile = ((FloorGenerator)transform.parent.GetComponentInParent<FloorGenerator>()).floorTile;

        //assign tilemap
        tilemap = (Tilemap)GameObject.FindWithTag("FloorTileMap").GetComponent("Tilemap");

        //create list of walkable tile locations
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

        //create list of neighbor walkable locations for each walkable tile location
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

        //create dictionary
        guide = walkableTileLocations.Zip(neighborLocations, (Vector3 k, List<Vector3> v) => (k, v)).ToDictionary(x => x.k, x => x.v);
    }

    public Dictionary<Vector3, List<Vector3>> GetGuide()
    {
        return guide;
    }
}
