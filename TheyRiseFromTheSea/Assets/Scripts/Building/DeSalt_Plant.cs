using UnityEngine;
using System.Collections;

public class DeSalt_Plant : ExtractionBuilding {

    //LineRenderer lineR;
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

    public float ExtractRate;

    public int ExtractAmmnt;

    public int startingStorageCap;
    public int PersonalStorageCap { get; protected set; }

    void Awake(){

        // Store the Line Renderer
        //lineR = GetComponent<LineRenderer> ();
        PersonalStorageCap = startingStorageCap;

       // resource_grid = ResourceGrid.Grid;

        Init(TileData.Types.water, ExtractRate, ExtractAmmnt, PersonalStorageCap, transform);

        _state = State.SEARCHING;
    }


	void Start ()
    {
        //b_statusIndicator = GetComponent<Building_ClickHandler>().buildingStatusIndicator;

        //// In case Player Resources is null
        //if (playerResources == null) {
        //	playerResources = GameObject.FindGameObjectWithTag("Capital").GetComponent<Player_ResourceManager>();
        //}

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

		MyStateMachine (_state);
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
                    if (output != null && isConnectedToOutput)
                    {
                        if (CheckOutputStorage())
                        {
                            statusMessage = "Sending!";
                            StopCoroutine("ShowStatusMessage");
                            StartCoroutine("ShowStatusMessage");
                        }
                        else
                        {
                            // OUTPUT STORAGE FULL:
                            statusMessage = "Output Full!";
                            StopCoroutine("ShowStatusMessage");
                            StartCoroutine("ShowStatusMessage");
                        }
                    }
                }


                break;

            case State.SEARCHING:

                statusMessage = "Callibrating...";
                StopCoroutine("ShowStatusMessage");
                StartCoroutine("ShowStatusMessage");

                if (CheckSearchForResource())
                {
                    // If it's true it means the position of rock has been defined
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
