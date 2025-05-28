using UnityEngine;
using UnityEngine.Tilemaps;


[CreateAssetMenu(menuName = "Scriptable Objects/Item")]
public class Item : ScriptableObject{

    ///[Header ("GAMEPLAY")]
    public Itemtype type;
    public Actiontype actionType;
    public Vector2Int range = new Vector2Int(5,4);

    //[Header ("ONLY UI")] 
    public bool stackable = true;

    //[Header "BOTH"]
    public Sprite image; 
    public string itemName;
    public int healthAmount = 0;
    public int stackCount = 0;
    public GameObject itemPrefab;  
    public GameObject worldDropPrefab; 
}
    
    public enum Itemtype{
        Weapon,
        Potion,
        Arrows,
        Gold,
        Key,
    }

    public enum Actiontype{
        None,
        Attack,
        Use,
    }






