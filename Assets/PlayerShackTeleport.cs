using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.SceneManagement;

public class PlayerTowerTeleporter : MonoBehaviour
{
    public string buildingTilemapTag = "BuildingTilemap";
    public Vector3 teleportTarget;

    private Tilemap buildingTilemap;
    private Collider2D playerCollider;

    private void Start(){
        SceneManager.LoadScene("Townsville", LoadSceneMode.Additive);

        playerCollider = GetComponent<Collider2D>();

        GameObject tilemapObject = GameObject.FindGameObjectWithTag(buildingTilemapTag);

        if(tilemapObject != null){
            buildingTilemap = tilemapObject.GetComponent<Tilemap>();
            //Debug.LogError("Building tilemap found");
        }
        if(tilemapObject == null){
            Debug.LogError("Building tilemap not found");
        }
    }

    private void OnTriggerEnter2D(Collider2D other){
        if(buildingTilemap==null || !other.GetComponent<TilemapCollider2D>()){
            return;
        }

            Debug.Log("player triggered collision");
            
            Bounds bounds = playerCollider.bounds;
            //Vector3Int cellPos = new Vector3Int(x, y, 0); // Was causing an error, uncomment when needed
        }
    
}
