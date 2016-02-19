using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class Transporter_Handler : MonoBehaviour {

    /// A Transporter / Launchpad is what allows the Player to go back and forth from the Planet to the ship.

    public static Transporter_Handler instance { get; protected set; }

    bool isPlayerOnPad;

    bool isLocked;

    Building_StatusIndicator status_indicator;

    void Awake()
    {
        instance = this;

        isLocked = false;
        
        // Lock transport controls when landinng on planet (can only be unlocked once the Terraformer is done)
        if (SceneManager.GetActiveScene().name == "Level_Planet")
        {
            //LockControls(true);

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
        if (Input.GetButtonUp("Launch"))
        {
            if (SceneManager.GetActiveScene().name == "Level_Launch")
            {
                Sound_Manager.Instance.PlaySound("Transporter");
                LaunchToPlanet();
            }
            else
            {
                if (Mission_Manager.Instance.ActiveMission.IsCompleted)
                {
                    LaunchToShip();
                }
                else if (status_indicator != null)
                {
                    status_indicator.CreateStatusMessage("Locked!");
                }
            }

            Debug.Log("TRANSPORTER: Lock is " + isLocked);


        }
    }

    void LaunchToShip()
    {
        GameMaster.Instance.ReturnToShip();
    }

    public void LaunchToPlanet()
    {
        // If we don't have a mission, we can't Launch to Planet!
        if (Mission_Manager.Instance.ActiveMission != null)
            GameMaster.Instance.LaunchToPlanet();
    }


    /// <summary>
    ///  This allows another component to unlock the Launchpad and
    ///  permit the Player to Launch/Transport back to ship.
    /// </summary>
    /// <param name="locked"></param>
    public void LockControls(bool locked)
    {
        isLocked = locked;
        Debug.Log("TRANSPORTER: Lock is " + isLocked);
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
