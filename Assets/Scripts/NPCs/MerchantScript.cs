using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class MerchantScript : MonoBehaviour
{
    public GameObject shop;
    void Start(){
        shop.SetActive(false);
    }
    void Update()
    {
        
    }

    void OnTriggerEnter2D(Collider2D collider){

        if (collider.CompareTag("Player")){
            shop.SetActive(true);
        }

    }
    void OnTriggerExit2D(Collider2D collider){

        if (collider.CompareTag("Player")){
            shop.SetActive(false);
        }

    }

}


