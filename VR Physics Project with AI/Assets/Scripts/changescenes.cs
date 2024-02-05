using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

//written by Hunter Jenkins

public class changescenes : MonoBehaviour
{
 
    // Define the name of the scene you want to load
    public string sceneToLoad = "inside";

    // This method is called whenever another object enters the trigger collider
    private void OnTriggerEnter(Collider other)
    {
        // Check if the object that entered the trigger is the player
        if (other.CompareTag("Player")) // Make sure your player GameObject has the "Player" tag
        {
            // Use SceneManager to load the new scene
            SceneManager.LoadScene(sceneToLoad);
        }
    }


}
