using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class Transporter_Handler : MonoBehaviour {

    /// A Transporter / Launchpad is what allows the Player to go back and forth from the Planet to the ship.

    public static Transporter_Handler instance { get; protected set; }

    public int shipLevelIndex = 0, planetLevelIndex = 1;

    bool isPlayerOnPad;

    public bool isLocked { get; protected set; }

    Building_StatusIndicator status_indicator;

    void Awake()
    {
        instance = this;

        isLocked = false;
        
        // Lock transport controls when landinng on planet (can only be unlocked once the Terraformer is done)
        if (SceneManager.GetActiveScene().name == "Level_Planet")
        {
            LockControls(true);

            // The PreFab for the Transporter only contains the Click Handler if it is on the Planet!
            status_indicator = GetComponent<Building_ClickHandler>().buildingStatusIndicator;
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
            if (SceneManager.GetActiveScene().name == "Level_Launch")
            {
                Sound_Manager.Instance.PlaySound("Transporter");
                LaunchToPlanet();
            }
            else if (SceneManager.GetActiveScene().name == "Level_Planet" && !isLocked)
            {
                LaunchToShip();
            }
            else if (isLocked && status_indicator != null)
            {
                status_indicator.CreateStatusMessage("Locked!");
            }
          
        }
    }

    void LaunchToShip()
    {
        GameMaster.Instance.LaunchToShip();
    }

    public void LaunchToPlanet()
    {
        GameMaster.Instance.LaunchToPlanet();
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
