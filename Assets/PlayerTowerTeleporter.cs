using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.SceneManagement;

public class PlayerTowerTeleporter : MonoBehaviour
{
    public string buildingTilemapTag = "BuildingTilemap";
    public Color teleportColor = Color.white;
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
            Vector3Int min = buildingTilemap.WorldToCell(bounds.min);
            Vector3Int max = buildingTilemap.WorldToCell(bounds.max);

            for(int x = min.x; x<= max.x; x++){
                for(int y = min.y; y <= max.y; y++){
                    Vector3Int cellPos = new Vector3Int(x, y, 0);
                    Color tileColor = buildingTilemap.GetColor(cellPos);

                    if(Approximately(tileColor, teleportColor)){
                        Debug.Log("White tile hit - calling SetupGameplayAndTower()");
                        StartScript startScript = FindObjectOfType<StartScript>();
                        if (startScript != null)
                        {
                            startScript.SetupGameplayAndTower();
                        }
                        else
                        {
                            Debug.LogError("StartScript not found in scene!");
                        }
                        return;
                    }
                }
            }

        }
    

    private bool Approximately(Color a, Color b, float tolerance = 0.5f){
        return Mathf.Abs(a.r - b.r) < tolerance && Mathf.Abs(a.g - b.g) < tolerance && Mathf.Abs(a.b - b.b) < tolerance;
    }
}
