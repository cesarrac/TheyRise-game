using UnityEngine;
using System.Collections;

public class Transporter_Handler : MonoBehaviour {

    /// A Transporter / Launchpad is what allows the Player to go back and forth from the Planet to the ship.

    public int shipLevelIndex = 0, planetLevelIndex = 1;

    bool isPlayerOnPad;

    void Update()
    {
        if (isPlayerOnPad)
        {
            ListenForLaunchButton();
        }
    }

    void ListenForLaunchButton()
    {
        if (Input.GetButtonDown("Launch"))
        {
            if (Application.loadedLevel == shipLevelIndex)
            {
                Launch(planetLevelIndex);
            }
            else if (Application.loadedLevel == planetLevelIndex)
            {
                Launch(shipLevelIndex);
            }
          
        }
    }

    void Launch(int sceneIndex)
    {
        Application.LoadLevel(sceneIndex);
    }


    void OnTriggerEnter2D(Collider2D coll)
    {
        if (coll.gameObject.CompareTag("Citizen"))
        {
            isPlayerOnPad = true;
        }
    }

    void OnTriggerExit2D(Collider2D coll)
    {
        if (coll.gameObject.CompareTag("Citizen"))
        {
            isPlayerOnPad = false;
        }
    }

}
