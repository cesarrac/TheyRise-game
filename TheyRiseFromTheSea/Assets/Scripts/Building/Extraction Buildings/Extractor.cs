using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Extractor : ExtractionBuilding {

    public float ExtractPower;

    public float ExtractRate;

    public int ExtractAmmnt;

    public int startingStorageCap;
    public int PersonalStorageCap { get; protected set; }

    int commonOreCount;
    int enrichedOreCount;

    Rock currRock;

    Vector3 currRockWorldPos;
    Vector3 currRockTilePos;



    void OnEnable()
    {
        currResourceStored = 0;
        currRock = null;
        currRockWorldPos = new Vector3();

        myTransform = transform;

        _state = State.SEARCHING;
    }

    void Awake()
    {
        PersonalStorageCap = startingStorageCap;

        //resource_grid = ResourceGrid.Grid;

        Init(TileData.Types.rock, ExtractRate, ExtractPower, ExtractAmmnt, PersonalStorageCap, transform);


        inventoryTypeCallback = SplitRockByType;

        splitShipInventoryCallback = DefineEnrichedAndCommonOre;
    }

	void Start()
	{
        currRock = null;
        currRockWorldPos = new Vector3();

        // Get the Line renderer
        lineR = GetComponent<LineRenderer>();
        // Then turn it off
        lineR.enabled = false;

    }

    void Update () {

		//if (!selecting && myStorage == null) {

		

		//	// This means that either the Storage we were using was destroyed OR is full, so change state to stop extraction
		//	_state = State.NOSTORAGE;

		//} else if (!selecting && myStorage != null) {

		//	// Give the Player Resource Manager our stats to show on Food Production panel
		//	if (!statsInitialized){
		//		playerResources.CalculateOreProduction(extractAmmnt, extractRate, false);
		//		statsInitialized = true;
		//	}
		//}

		MyStateMachine (_state);
	}

	void MyStateMachine(State curState)
	{
		switch (curState) {

		case State.EXTRACTING:
			//CountDownToExtract();
                if (!isExtracting && !productionHalt)
                {
                    if (currRock == null)
                    {
                        //GameObject currTarget = resource_grid.GetTileGameObjFromWorldPos(resourceWorldPos);
                        GameObject currTarget = resource_grid.GetTileGameObjFromIntCoords((int)currRockTilePos.x, (int)currRockTilePos.y);
                        if (currTarget != null)
                        {
                            if (currTarget.GetComponent<Rock_Handler>() != null)
                            {
                                currRock = currTarget.GetComponent<Rock_Handler>().myRock;
                                currRockWorldPos = resourceWorldPos;
                            }
                        }
           
                    }

                    if (!storageIsFull && currRock != null)
                    {
                        // Store the rock type of the target rock
                        // Define my target tile's gameobject
                        
                        StopCoroutine("ExtractResource");
                        StartCoroutine("ExtractResource");
                        isExtracting = true;

                        StopCoroutine("ShowStatusMessage");
                        StartCoroutine("ShowStatusMessage");
                        statusMessage = "Extracting!";
                    }
                    else
                    {
                        if (currRock == null)
                        {
                            // go back to searching for rock
                            Debug.Log("EXTRACTOR: Curr rock is null, going back to searching!");
                            _state = State.SEARCHING;
                        }
                        else
                        {
                            // storage is full!
                            StopCoroutine("ShowStatusMessage");
                            StartCoroutine("ShowStatusMessage");
                            statusMessage = "Full!";
                            _state = State.NOSTORAGE;
                        }
                     
                    }
                  

                }
			break;

		case State.NOSTORAGE:
                if (!storageIsFull)
                {

                    _state = State.SEARCHING;
                }
                else
                {
                    if (!pickUpSpawned)
                    {
                        SpawnPickUp();
                        commonOreCount = 0;
                        enrichedOreCount = 0;
                        pickUpSpawned = true;
                    }
            
                }
                //else if (statusIndicated && storageIsFull)
                //{
                //    // repeating full status message for player to see!!
                  
                //    StopCoroutine("ShowStatusMessage");
                //    StartCoroutine("ShowStatusMessage");
                //    statusMessage = "Full!";
                //}
                //else
                //{
                //    if (CheckOutputStorage())
                //    {
                       
                //        StopCoroutine("ShowStatusMessage");
                //        StartCoroutine("ShowStatusMessage");
                //        statusMessage = "Sending!";
                //    }
                //    else
                //    {
                //        if (output == null)
                //        {
                //            // No output connected. My storage is full
                //            StopCoroutine("ShowStatusMessage");
                //            StartCoroutine("ShowStatusMessage");
                //            statusMessage = "Full!";
                //        }
                //        else
                //        {
                //            // OUTPUT STORAGE FULL:
                //            StopCoroutine("ShowStatusMessage");
                //            StartCoroutine("ShowStatusMessage");
                //            statusMessage = "Output Full!";
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
                    // Define my resource tile position
                    //currRockWorldPos =
                    currRockTilePos = new Vector3(targetTile.posX, targetTile.posY, 0);

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

    void SplitRockByType(int quantity)
    {
        if (currRock != null)
        {
            if (currRock._rockProductionType == Rock.RockProductionType.common)
            {
                print("Adding Common ore!");
                commonOreCount += quantity;
            }
            else if (currRock._rockProductionType == Rock.RockProductionType.enriched)
            {
                print("Adding Enriched ore!");
                enrichedOreCount += quantity;
            }
        }
 
    }


    void DefineEnrichedAndCommonOre(int currTotal)
    {
        //if (enrichedOreCount + commonOreCount == currTotal)
        //{
        //    Ship_Inventory.Instance.SplitOre(commonOreCount, enrichedOreCount);
        //    // Now make sure that both enriched and common count go back to 0 since this extractor is now empty
        //    commonOreCount = 0;
        //    enrichedOreCount = 0;
        //}
        //else
        //{
        //    Debug.Log("Total inventory = " + currResourceStored + " Common ore = " + commonOreCount + " Enriched = " + enrichedOreCount);
        //    Debug.LogError("This EXTRACTOR's enriched and common count DO NOT equal its current inventory!");
        //}
        int commonOre = 0;
        int enrichedOre = 0;

        if (currRock != null)
        {
            if (currRock._rockProductionType == Rock.RockProductionType.common)
            {
                print("Adding Common ore!");
                commonOre = currTotal;
            }
            else if (currRock._rockProductionType == Rock.RockProductionType.enriched)
            {
                print("Adding Enriched ore!");
                enrichedOre = currTotal;
            }
        }

        Ship_Inventory.Instance.SplitOre(commonOre, enrichedOre);

    }

    //void CountDownToExtract()
    //{
    //	if (extractCountDown <= 0) {

    //		Extract();
    //		extractCountDown = extractRate;

    //	} else {

    //		extractCountDown -= Time.deltaTime;

    //	}
    //}

    //void LineFollowMouse(){
    //	Vector3 m = Camera.main.ScreenToWorldPoint (Input.mousePosition);
    //	mouseEnd = new Vector3 (Mathf.Clamp(m.x, transform.position.x - 10f, transform.position.x + 10f), 
    //	                        Mathf.Clamp(m.y, transform.position.y - 10f, transform.position.y + 10f), 
    //	                        0.0f);
    //	lineR.SetPosition (1, mouseEnd);
    //}

    ///// <summary>
    ///// Sets selecting bool to true. Useful for attaching to
    ///// an On-Click Event on a Button for Resetting this extractor's storage.
    ///// </summary>
    //public void ActivateSelecting(){
    //	if (!selecting) {
    //		selecting = true;
    //	}
    //}

    //void SetStorageAndExtract(){
    //	int mX = Mathf.RoundToInt(mouseEnd.x);
    //	int mY = Mathf.RoundToInt(mouseEnd.y);
    //	if (mX > 2 && mX < resourceGrid.mapSizeX - 2 && mY > 2 && mY < resourceGrid.mapSizeY - 2) {

    //		// Make sure that where we clicked on the Grid is a storage tile
    //		if (resourceGrid.GetTileType (mX, mY) == TileData.Types.storage) {
    //			Debug.Log("Storage found for ore!");

    //			// Selecting is now false to deactivate the Line Renderer gameobject
    //			selecting = false;

    //			// Give Building UI ability to access to building menus by clicking again
    //			buildingUI.currentlyBuilding = false;

    //               // Set my storage
    //               myStorage = resourceGrid.GetTileGameObjFromWorldPos(mouseEnd).GetComponent<Storage>();

    //               // Start extracting by finding which direction rock was found
    //               if (SearchForRock ()) {

    //				// Cycle through the rocks found array to make sure we don't extract from a NULL (represented by Vector2.zero)
    //				CycleRocksArray ();
    //			}


    //		} else {
    //			Debug.Log ("Need a place to store the ore!");
    //			// state will stay on NO STORAGE
    //		}
    //	}
    //}

    //TileData SearchForRock(){
    //       Vector3 top = transform.position + Vector3.up;
    //       Vector3 bottom = transform.position - Vector3.up;
    //       Vector3 left = transform.position + Vector3.left;
    //       Vector3 right = transform.position + Vector3.right;
    //       Vector3 topLeft = top + Vector3.left;
    //       Vector3 topRight = top + Vector3.right;
    //       Vector3 botLeft = bottom + Vector3.left;
    //       Vector3 botRight = bottom + Vector3.right;

    //       //rockTilesDetected.Clear();

    //       if (CheckTileType(top) != null)
    //       { // top

    //           //rockTilesDetected.Add(CheckTileType(top));
    //           return CheckTileType(top);
    //       }
    //	else if (CheckTileType(bottom) != null)
    //       { // bottom

    //           //rockTilesDetected.Add(CheckTileType(bottom));
    //           return CheckTileType(bottom);
    //       }
    //       else if (CheckTileType(left) != null)
    //       { // left
    //           //rockTilesDetected.Add(CheckTileType(left));
    //           return CheckTileType(left);
    //       }
    //       else if (CheckTileType(right) != null) { //right
    //           //rockTilesDetected.Add(CheckTileType(right));
    //           return CheckTileType(right);
    //       }
    //       else if (CheckTileType(topLeft) != null)
    //       { // top left
    //           //rockTilesDetected.Add(CheckTileType(topLeft));
    //           return CheckTileType(topLeft);
    //       }
    //       else if (CheckTileType(topRight) != null)
    //       { // top right
    //         // rockTilesDetected.Add(CheckTileType(topRight));
    //           return CheckTileType(topRight);
    //       }
    //       else if (CheckTileType(botLeft) != null)
    //       { // bottom left
    //           //rockTilesDetected.Add(CheckTileType(botLeft));
    //           return CheckTileType(botLeft);
    //       }
    //       else if (CheckTileType(botRight) != null)
    //       { // bottom right
    //         // rockTilesDetected.Add(CheckTileType(botRight));
    //           return CheckTileType(botRight);
    //       }
    //       else
    //       {
    //           return null;
    //       }

    //}

    //TileData CheckTileType(Vector3 position){
    //       if (resourceGrid.TileFromWorldPoint(position).tileType == TileData.Types.rock)
    //           return resourceGrid.TileFromWorldPoint(position);
    //       else
    //           return null;
    //}

    //   void DefineRockPosition(TileData rock)
    //   {
    //       rockPosX = rock.posX;
    //       rockPosY = rock.posY;
    //   }

    //void CycleRocksArray(){
    //	for (int x =0; x< rocksDetected.Length; x++){
    //		if (rocksDetected[x] != Vector2.zero){
    //			rockPosX = (int) rocksDetected[x].x;
    //			rockPosY = (int) rocksDetected[x].y;
    //			currRockIndex = x;

    //			// Rock has been detected so we can change state
    //			_state = State.EXTRACTING;

    //		}else{
    //			Debug.Log("No rock found at: " + rocksDetected[x]);
    //		}
    //	}
    //}

    //IEnumerator ExtractOre()
    //{
    //    // while true, extract, wait extract rate, check if there's ore left: if no break out and  change state to searching
    //    while (true)
    //    {

    //        yield return new WaitForSeconds(extractRate);

    //        int extract = resourceGrid.MineARock(rockPosX, rockPosY, extractAmmnt);

    //        if ((currOreStored + extract) <= personalStorageCap)
    //        {
    //            currOreStored += extract;
    //            Debug.Log("Extracting. Current ore stored = " + currOreStored);
    //            // Check again if personal storage is full AFTER adding the ore
    //            if (currOreStored >= personalStorageCap)
    //            {
    //                // STORAGE FULL
    //                isExtracting = false;
    //                storageIsFull = true;
    //                statusMessage = "Full!";
    //                StopCoroutine("ShowStatusMessage");
    //                StartCoroutine("ShowStatusMessage");
    //                Debug.Log("EXTRACTOR STORAGE FULL!");
    //                _state = State.NOSTORAGE;
    //                yield break;
    //            }

    //        }

    //        else
    //        {
    //            // STORAGE FULL
    //            isExtracting = false;
    //            storageIsFull = true;
    //            statusMessage = "Full!";
    //            StopCoroutine("ShowStatusMessage");
    //            StartCoroutine("ShowStatusMessage");
    //            Debug.Log("EXTRACTOR STORAGE FULL!");
    //            _state = State.NOSTORAGE;
    //            yield break;
    //        }




    //    }
    //}

    //public int GrabAllOre()
    //{
    //    int allOre = currOreStored;
    //    currOreStored = 0;
    //    storageIsFull = false;
    //    return allOre;
    //}

    //IEnumerator ShowStatusMessage ()
    //{
    //    int repetitions = 0;
    //    statusIndicated = false;

    //    while (true)
    //    {
    //        yield return new WaitForSeconds(2f);
    //        buildingStatusIndicator.CreateStatusMessage(statusMessage);
    //        repetitions++;
    //        if (repetitions >= 3)
    //        {
    //            // Status indicated bool allows this coroutine to be started up again to repeat a message (Like "FULL!!")
    //            statusIndicated = true;
    //            yield break;
    //        }


    //    }

    //}

    //void Extract(){
    //	int q = resourceGrid.tiles [rockPosX, rockPosY].maxResourceQuantity;
    //	int calc = q - extractAmmnt;
    //	if (calc > 0) { // theres still ore left in this rock
    //		Debug.Log ("Extracting!");

    //		// check that storage is not full
    //		if (!myStorage.CheckIfFull (extractAmmnt, false)) {

    //			// add it to Storage
    //			myStorage.AddOreOrWater (extractAmmnt, false);

    //			// subtract it from the tile
    //			resourceGrid.tiles [rockPosX, rockPosY].maxResourceQuantity = calc;

    //			// check if tile is depleted
    //			int newQ = resourceGrid.tiles [rockPosX, rockPosY].maxResourceQuantity;

    //			if (newQ <= 0) {

    //				// This rock is Depleted, so change state to stop extraction while we Search for more rock
    //				_state = State.SEARCHING;

    //				// Deplete the rock and check for more
    //				DepleteRock (rockPosX, rockPosY);
    //			} 

    //		} else {

    //			// Storage is full and extractor stops until it gets a new storage
    //			myStorage = null;

    //			// Change state to stop extraction while we get a new Storage
    //			_state = State.NOSTORAGE;
    //		}


    //	} else { 

    //		// This rock is Depleted, so change state to stop extraction while we Search for more rock
    //		_state = State.SEARCHING;

    //		// Deplete the rock and check for more
    //		DepleteRock (rockPosX, rockPosY);
    //	}
    //}

    //void DepleteRock(int x, int y){

    //	// To Deplete a rock, swap tile to empty
    //	resourceGrid.SwapTileType (x, y, TileData.Types.empty);

    //	// change value of this rock in the array so we don't try extracting from it anymore
    //	rocksDetected [currRockIndex] = Vector3.zero;

    //	// Cycle the array to see if there is any more rock around us
    //	CycleRocksArray ();
    //}
}
