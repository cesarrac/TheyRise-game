using UnityEngine;
using System.Collections;

public class DeSalt_Plant : ExtractionBuilding {


    //public bool selecting;
    //public float pumpRate;
    //public int waterPumped;
    //Vector3 mouseEnd;

    //public ResourceGrid resourceGrid;

    //Storage myStorage; // is set when player connects the plant to a storage building

    //Building_UIHandler buildingUI;

    //public Player_ResourceManager playerResources;

    //bool statsInitialized;

    //SpriteRenderer sr;


    //public State state { get { return _state; } set { _state = value; } }

    //private float pumpCountdown;

    //public float ExtractRate;

    //public int ExtractAmmnt;

    //public float ExtractPower;

    //public int startingStorageCap;
    //public int PersonalStorageCap { get; protected set; }


    Building_Handler build_handler;

    void OnEnable()
    {
        currResourceStored = 0;

        myTransform = transform;

        build_handler = GetComponent<Building_Handler>();

        _state = State.SEARCHING;
    }

    void Awake(){

        //PersonalStorageCap = startingStorageCap;

       // Init(TileData.Types.water, ExtractRate, ExtractPower,  ExtractAmmnt, PersonalStorageCap, transform);
        BlueprintDatabase.Instance.GetExtractorStats("Desalination Pump", myTransform, this, TileData.Types.water);

    }


	void Start ()
    {

        // Get the Line renderer
        lineR = GetComponent<LineRenderer>();
        // Then turn it off
        lineR.enabled = false;

        //lineR.SetPosition (0, transform.position);

        //		selecting = true;

    }
	

	void Update () 
	{

        //if (!selecting && myStorage == null) {

        //	lineR.enabled = false;

        //	// This means that either the Storage we were using was destroyed OR is full, so change _state to stop extraction
        //	_state = State.NOSTORAGE;

        //} else if (!selecting && myStorage != null) {

        //	// Give the Player Resource Manager our stats to show on Food Production panel
        //	if (!statsInitialized){
        //		playerResources.CalculateWaterProduction(waterPumped, pumpRate, false);
        //		statsInitialized = true;
        //	}
        //}

        if (build_handler.state == Building_Handler.State.READY)
        {
            MyStateMachine(_state);
        }
    }


    void MyStateMachine(State curState)
    {
        switch (curState)
        {

            case State.EXTRACTING:
                //CountDownToExtract();
                if (!isExtracting && !productionHalt)
                {
                    if (!storageIsFull)
                    {
                        StopCoroutine("ExtractResource");
                        StartCoroutine("ExtractResource");
                        isExtracting = true;

                        StopCoroutine("ShowStatusMessage");
                        StartCoroutine("ShowStatusMessage");
                        statusMessage = "Pumping!";
                    }
                    else
                    {
                        statusMessage = "Full!";
                        StopCoroutine("ShowStatusMessage");
                        StartCoroutine("ShowStatusMessage");
                        _state = State.NOSTORAGE;
                    }


                }
                break;

            case State.NOSTORAGE:
                if (!storageIsFull)
                {

                    _state = State.SEARCHING;
                }
                else if (statusIndicated && storageIsFull)
                {
                    // repeating full status message for player to see!!
                    statusMessage = "Full!";
                    StopCoroutine("ShowStatusMessage");
                    StartCoroutine("ShowStatusMessage");
                }
                else
                {
                    if (!pickUpSpawned)
                    {
                        SpawnResourceDrop();
                        pickUpSpawned = true;
                    }

                }
                //else
                //{
                //    if (output != null && isConnectedToOutput)
                //    {
                //        if (CheckOutputStorage())
                //        {

                //            StopCoroutine("ShowStatusMessage");
                //            StartCoroutine("ShowStatusMessage");
                //            statusMessage = "Sending!";
                //        }
                //        else
                //        {
                //            if (output == null)
                //            {
                //                // No output connected. My storage is full
                //                StopCoroutine("ShowStatusMessage");
                //                StartCoroutine("ShowStatusMessage");
                //                statusMessage = "Full!";
                //            }
                //            else
                //            {
                //                // OUTPUT STORAGE FULL:
                //                StopCoroutine("ShowStatusMessage");
                //                StartCoroutine("ShowStatusMessage");
                //                statusMessage = "Output Full!";
                //            }

                //        }
                //    }
                //}


                break;

            case State.SEARCHING:

                statusMessage = "Callibrating...";
                StopCoroutine("ShowStatusMessage");
                StartCoroutine("ShowStatusMessage");

                pickUpSpawned = false;

                if (CheckSearchForResource())
                {
                    // If it's true it means the position of water has been defined
                    _state = State.EXTRACTING;
                }
                else
                {
                    Debug.Log("EXTRACTOR: Cant find my resource of type: " + resourceType);
                    if (statusIndicated)
                    {
                        // Repeat callibrating message if it CANT find a rock
                        statusMessage = "Callibrating...";
                        StopCoroutine("ShowStatusMessage");
                        StartCoroutine("ShowStatusMessage");
                    }
                }


                break;

            default:
                // starved / no power
                break;
        }
    }

 //   void CountDownToPump()
	//{
	//	if (pumpCountdown <= 0) {
			
	//		PumpIt ();
	//		pumpCountdown = pumpRate;
			
	//	} else {
			
	//		pumpCountdown -= Time.deltaTime;
			
	//	}
	//}


	//void LineFollowMouse()
	//{
	//	Vector3 m = Camera.main.ScreenToWorldPoint (Input.mousePosition);
	//	mouseEnd = new Vector3 (Mathf.Clamp(m.x, transform.position.x - 10f, transform.position.x + 10f), 
	//	                        Mathf.Clamp(m.y, transform.position.y - 10f, transform.position.y + 10f), 
	//	                        0.0f);
	//	lineR.SetPosition (1, mouseEnd);
	//}

	//// Used by Set Storage Button:
	//public void ActivateSelecting(){
	//	if (!selecting) {
	//		selecting = true;
	//	}
	//}

	//void SetStorageAndPump()
	//{
	//	int mX = Mathf.RoundToInt(mouseEnd.x);
	//	int mY = Mathf.RoundToInt(mouseEnd.y);
	//	if (mX > 2 && mX < resourceGrid.mapSizeX - 2 && mY > 2 && mY < resourceGrid.mapSizeY - 2) {

	//		// Check that the tile we clicked on is in fact a Storage tile
	//		if (resourceGrid.GetTileType (mX, mY) == TileData.Types.storage) {

	//			// Selecting is false to deactivate Line Renderer
	//			selecting = false;

	//			// Give Building UI ability to click on building menus again
	//			buildingUI.currentlyBuilding = false;

	//			// Set my storage
	//			myStorage = resourceGrid.GetTileGameObjFromWorldPos(mouseEnd).GetComponent<Storage> ();

	//			// Start pumping!!
	//			_state = State.PUMPING;

	//		} else {
	//			Debug.Log ("Need a place to store the water!");
	//			// State stays at No Storage
	//		}
	//	}
	//}


	//void PumpIt()
	//{
	//	// check that storage is not full
	//	if (!myStorage.CheckIfFull (waterPumped, true)) {

	//		// add it to Storage
	//		myStorage.AddOreOrWater (waterPumped, true);

	//	} else {

	//		// storage is full and pump stops until it gets a new storage
	//		myStorage = null;
	//		_state = State.NOSTORAGE;
	//	}
	//}
}
