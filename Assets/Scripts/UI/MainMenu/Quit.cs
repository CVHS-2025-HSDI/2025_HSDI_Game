using UnityEngine;

public class Quit : MonoBehaviour
{
    public void QuitGame () {
        Application.Quit();
        Debug.Log("Game is exiting");
    }
}
