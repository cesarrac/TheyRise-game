using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class Transporter_Handler : MonoBehaviour {

    /// A Transporter / Launchpad is what allows the Player to go back and forth from the Planet to the ship.

    public static Transporter_Handler instance { get; protected set; }

    public int shipLevelIndex = 0, planetLevelIndex = 1;

    bool isPlayerOnPad;

    public bool isLocked { get; protected set; }

    void Awake()
    {
        instance = this;

        isLocked = false;
        
        // Lock transport controls when landinng on planet (can only be unlocked once the Terraformer is done)
        if (SceneManager.GetActiveScene().name == "Level_Planet")
        {
            LockControls(true);
        }
    }

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
                LaunchToPlanet();
            }
            else if (Application.loadedLevel == planetLevelIndex)
            {
                LaunchToShip();
            }
          
        }
    }

    void LaunchToShip()
    {
        // Tell Ship Inventory to register items that were on the Temporary inventory, since now the Player is finally taking them to the ship
        Ship_Inventory.Instance.RegisterTempInventoryToShip();

        Application.LoadLevel(shipLevelIndex);
    }

    public void LaunchToPlanet()
    {
        Application.LoadLevel(planetLevelIndex);
    }


    /// <summary>
    ///  This allows another component to unlock the Launchpad and
    ///  permit the Player to Launch/Transport back to ship.
    /// </summary>
    /// <param name="isLock"></param>
    public void LockControls(bool isLock)
    {
        isLocked = isLock;
    }


    void OnTriggerEnter2D(Collider2D coll)
    {
        if (coll.gameObject.CompareTag("Citizen") && !isLocked)
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
