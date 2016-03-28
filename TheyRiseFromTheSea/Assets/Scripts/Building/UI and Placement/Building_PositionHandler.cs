using UnityEngine;
using System.Collections.Generic;

public class Building_PositionHandler : MonoBehaviour {

	/// <summary>
	/// This is on a building that's about to be built (still following the mouse).
	/// Every time the mouse is on a different position we check what tile types are around this building.
	/// BUILDING RULES: (1) Building MUST be on an empty tile, (2) Building MUST have empty tiles 
	/// at position + 1 in all directions UNLESS, (3) its an extractor, then it MUST have ROCK 
	/// at any position +1 in all directions.
	/// </summary>

	// the UI_Handler will feed it the Resource Grid, 
	
	public int mapPosX;
	public int mapPosY;

	public ResourceGrid resourceGrid;
	public bool followMouse;

    [HideInInspector]
    public TileData.Types tileType;

	SpriteRenderer sr; // to handle the alpha change
	Color invalidPosColor;
	Color trueColor;
   

	Vector3 m, lastM;
	int posX;
	int posY;

	bool canBuild, canAfford; // turns true when building is in proper position

	bool onEneposY = false; // can't build on top or near enemies

	public Player_ResourceManager resourceManager;
    //public int currOreCost; // this will charge the resources manager with the cost sent from UI handler
    public int currNanoBotCost;

	public ObjectPool objPool;

    //public Building_UIHandler buildingUI;
    Build_MainController build_controller;

	public Vector3 spawnPos;

    public NanoBuilding_Handler nanoBuild_handler;

    Mouse_Controller mouse_controller;

    // Blueprint name
    public string bpName { get; protected set; }

    List<TileData> build_tilePositions = new List<TileData>();

    Vector3 startingBuildPosition;

    List<GameObject> position_indicator = new List<GameObject>();

    GameObject posIndicator = null;

    void Awake()
	{
        // Get the Sprite Renderer to be able to change the Sprite's color depending on position
		sr = GetComponent<SpriteRenderer> ();

        // Clear red color for Invalid Position
        invalidPosColor =  new Color(1, 0, 0, 0.5f);

        // Clear green color for Valid Position
        trueColor = new Color(0, 1, 0, 0.5f);

        // Begin as clear Color
        sr.color = Color.clear;

        // Set posY transform to this tile's Spawn Position (passed in by Discover Tile)
		transform.position = spawnPos;

        resourceGrid = ResourceGrid.Grid;
        objPool = ObjectPool.instance;
        build_controller = Build_MainController.Instance;

        mouse_controller = Mouse_Controller.MouseController;
    }

    public void SetCurrentBlueprintID(string id)
    {
        bpName = id;
    }


    // Needs:
    /*

            - Perform a check for empty tile under the mouse position
            - The position of the gameobject always is the same as the mouse position
            - If this is an extraction type building: Perform a check to verify adjacent tiles of the correct resource type
            - If all criteria are met, call to swap the empty tile for this new tile/building
            - Display if the position is a correct one or not by changing the "ghost" sprite to red / green color


    */

	void Update () {
        //if (followMouse) {
        //	FollowMouse();
        //}

        //Check to follow the mouse every time this obj's position does not match the mouse's position
        //if (transform.position != mouse_controller.currMouseP)
            FollowTheMouse();
    }

	
    void FollowTheMouse()
    {
        transform.position = mouse_controller.currMouseP;

        ListenForCancelBuild();

        if (CheckPositionToBuild())
        {
            sr.color = trueColor;
            canBuild = true;

            if (Input.GetMouseButtonDown(0))
            {
                Debug.Log("Clicked mouse button!");

                if (CheckCost())
                {
                    startingBuildPosition = transform.position;

                    build_tilePositions.Add(mouse_controller.GetTileUnderMouse());

                    posIndicator = objPool.GetObjectForType("Selection Circle", true, transform.position);

                    position_indicator.Add(posIndicator);
                }
            }
            ListenForMouseClick();
        }
        else
        {
            sr.color = invalidPosColor;
            canBuild = false;
        }

    }

    void ListenForCancelBuild()
    {
        if (Input.GetMouseButtonDown(1))
        {
            // CANCEL THE BUILD

            build_controller.SetCurrentlyBuildingBool(false);

            //// give back the nanobots spent on this building
            //nanoBuild_handler.nanoBots += currNanoBotCost;

            // give back the resources spent on this building because build was Cancelled
            nanoBuild_handler.ReturnBuildResources(tileType);

            build_tilePositions.Clear();

            // Pool all pos indicators
            if (position_indicator.Count > 0)
            {
                for (int i = 0; i < position_indicator.Count; i++)
                {
                    PoolObject(position_indicator[i]);
                }

                position_indicator.Clear();
            }

            PoolObject(gameObject);
        }
    }

    // This method will contain all the necessary criteria for a legal building position
    bool CheckPositionToBuild()
    {
        if (!CheckEmptyBeneath(transform.position))
            return false;
        else if (!CheckForResource(transform.position, tileType))
            return false;
        else
        {
            return true;
        }

    }

    void ListenForMouseClick()
    {

        if (Input.GetMouseButton(0) && transform.position != startingBuildPosition && canBuild && !onEneposY)
        {
            Debug.Log("Holding mouse button!");
            if (mouse_controller.GetTileUnderMouse() != null)
            {
                if (!build_tilePositions.Contains(mouse_controller.GetTileUnderMouse()))
                {
                    // Each time we check cost add +1 to the count because the element hasn't been added to the list yet
                    // This will serve as a multiplier for checking costs. So any resource ammnt * multiplier will be checked
                    // i.e. when trying to build a second unit it checks Cost * (count + 1) = Cost * (1 + 1) = Cost * 2, 
                    // on the third it's Cost * (2 + 1), etc...
                    if (CheckCost(build_tilePositions.Count + 1))
                    {
                        build_tilePositions.Add(mouse_controller.GetTileUnderMouse());

                        posIndicator = objPool.GetObjectForType("Selection Circle", true, new Vector3(mouse_controller.GetTileUnderMouse().posX, mouse_controller.GetTileUnderMouse().posY, 0));

                        position_indicator.Add(posIndicator);
                    }

                }
            }

        }




        // At this point we KNOW the mouse is NOT over a building or a rock, we have found rocks if extractor AND we are not on eneposY
        if (Input.GetMouseButtonUp(0))
        {           // So LEFT CLICK to BUILD!! 
            Debug.Log("Let go of mouse button!");
            Build();
        }

    }

    void Build()
    {
        // Right BEFORE pooling this object we tell Building UI that we are NOT currently building anymore
        build_controller.SetCurrentlyBuildingBool(false);

        //TileData currTile = mouse_controller.GetTileUnderMouse();

        if (build_tilePositions.Count > 1)
        {
            for (int i = 0; i < build_tilePositions.Count; i++)
            {
                if (build_tilePositions[i] != null)
                    resourceGrid.SwapTileType(build_tilePositions[i].posX, build_tilePositions[i].posY, tileType, bpName, currNanoBotCost, sr.sprite.bounds.size.x, sr.sprite.bounds.size.y);
            }
        }
        else
        {
            resourceGrid.SwapTileType(build_tilePositions[0].posX, build_tilePositions[0].posY, tileType, bpName, currNanoBotCost, sr.sprite.bounds.size.x, sr.sprite.bounds.size.y);
        }

        build_tilePositions.Clear();

        // Pool all pos indicators
        if (position_indicator.Count > 0)
        {
            for (int i = 0; i < position_indicator.Count; i++)
            {
                PoolObject(position_indicator[i]);
            }

            position_indicator.Clear();
        }

        // Pool this object
        PoolObject(gameObject); // Pool this because resourceGrid just discovered the tile/building for us
    }

    bool CheckCost(int multiplier = 1)
    {
        return nanoBuild_handler.CheckBuildCost(nanoBuild_handler.GetAvailableBlueprint(tileType), multiplier);
    }

	//void FollowMouse(){


 //       // Move building with the mouse
	//	transform.position = mouse_controller.currMouseP;
 //       posX = Mathf.RoundToInt(transform.position.x);
 //       posY = Mathf.RoundToInt(transform.position.y);

 //       if (CheckEmptyBeneath (transform.position) && !onEneposY) {
			
	//		if (tileType == TileData.Types.extractor) {
	//			if (CheckForResource (posX, posY + 1, "ore")) { // top
	//				sr.color = trueColor;
	//				canBuild = true;
	//			} else if (CheckForResource (posX, posY - 1,  "ore")) { // bottom
	//				sr.color = trueColor;
	//				canBuild = true;
	//			} else if (CheckForResource (posX - 1, posY,  "ore")) { // left
	//				canBuild = true;
	//				sr.color = trueColor;
	//			} else if (CheckForResource (posX + 1, posY,  "ore")) { // right
	//				canBuild = true;
	//				sr.color = trueColor;
	//			} else if (CheckForResource (posX - 1, posY + 1,  "ore")) { // top left
	//				canBuild = true;
	//				sr.color = trueColor;
	//			} else if (CheckForResource (posX + 1, posY + 1,  "ore")) { // top right
	//				canBuild = true;
	//				sr.color = trueColor;
	//			} else if (CheckForResource (posX - 1, posY - 1,  "ore")) { // bottom left
	//				canBuild = true;
	//				sr.color = trueColor;
	//			} else if (CheckForResource (posX + 1, posY - 1,  "ore")) { // bottom right
	//				canBuild = true;
	//				sr.color = trueColor;
	//			} else {				// NOT ON ROCK
	//				sr.color = invalidPosColor;
	//				canBuild = false;
	//			}
	//		} else if (tileType == TileData.Types.desalt_s) { // THIS building is a Water Pump so it needs to detect WATER
	//			if (CheckForResource (posX, posY + 1, "water")) { // top
	//				sr.color = trueColor;
	//				canBuild = true;
	//			} else if (CheckForResource (posX, posY - 1, "water")) { // bottom
	//				sr.color = trueColor;
	//				canBuild = true;
	//			} else if (CheckForResource (posX - 1, posY, "water")) { // left
	//				canBuild = true;
	//				sr.color = trueColor;
	//			} else if (CheckForResource (posX + 1, posY, "water")) { // right
	//				canBuild = true;
	//				sr.color = trueColor;
	//			} else if (CheckForResource (posX - 1, posY + 1, "water")) { // top left
	//				canBuild = true;
	//				sr.color = trueColor;
	//			} else if (CheckForResource (posX + 1, posY + 1, "water")) { // top right
	//				canBuild = true;
	//				sr.color = trueColor;
	//			} else if (CheckForResource (posX - 1, posY - 1, "water")) { // bottom left
	//				canBuild = true;
	//				sr.color = trueColor;
	//			} else if (CheckForResource (posX + 1, posY - 1, "water")) { // bottom right
	//				canBuild = true;
	//				sr.color = trueColor;
	//			} else {				// NOT ON Water
	//				sr.color = invalidPosColor;
	//				canBuild = false;
	//			}
	//		}else if (tileType == TileData.Types.generator){ // Energy generators seek for Minerals
	//			if (CheckForResource (posX, posY + 1, "mineral")) { // top
	//				sr.color = trueColor;
	//				canBuild = true;
	//			} else if (CheckForResource (posX, posY - 1, "mineral")) { // bottom
	//				sr.color = trueColor;
	//				canBuild = true;
	//			} else if (CheckForResource (posX - 1, posY, "mineral")) { // left
	//				canBuild = true;
	//				sr.color = trueColor;
	//			} else if (CheckForResource (posX + 1, posY, "mineral")) { // right
	//				canBuild = true;
	//				sr.color = trueColor;
	//			} else if (CheckForResource (posX - 1, posY + 1, "mineral")) { // top left
	//				canBuild = true;
	//				sr.color = trueColor;
	//			} else if (CheckForResource (posX + 1, posY + 1, "mineral")) { // top right
	//				canBuild = true;
	//				sr.color = trueColor;
	//			} else if (CheckForResource (posX - 1, posY - 1, "mineral")) { // bottom left
	//				canBuild = true;
	//				sr.color = trueColor;
	//			} else if (CheckForResource (posX + 1, posY - 1, "mineral")) { // bottom right
	//				canBuild = true;
	//				sr.color = trueColor;
	//			} else {				// NOT ON Mineral
	//				sr.color = invalidPosColor;
	//				canBuild = false;
	//			}

	//		}else{ // not an extractor or desalt pump or generator

 //               // we are on a legal building position
 //               sr.color = trueColor;
 //               canBuild = true;
 //           }
	//	} else {				// NOT ON EMPTY
	//		sr.color = invalidPosColor;
	//		canBuild = false;
	//	}


 //       if (Input.GetMouseButtonDown(0) && canBuild && !onEneposY)
 //       {
 //           startingBuildPosition = transform.position;

 //           build_tilePositions.Add(mouse_controller.GetTileUnderMouse());
 //       }

 //       if (Input.GetMouseButton(0) && transform.position != startingBuildPosition && canBuild && !onEneposY)
 //       {
 //           if (mouse_controller.GetTileUnderMouse() != null)
 //           {
 //               if (!build_tilePositions.Contains(mouse_controller.GetTileUnderMouse()))
 //               {
 //                   build_tilePositions.Add(mouse_controller.GetTileUnderMouse());
 //               }
 //           }
          
 //       }

 //       if (Input.GetMouseButtonDown(1))
 //       {
 //           // CANCEL THE BUILD
 //           PoolObject(gameObject);
 //           build_controller.SetCurrentlyBuildingBool(false);

 //           //// give back the nanobots spent on this building
 //           //nanoBuild_handler.nanoBots += currNanoBotCost;

 //           // give back the resources spent on this building because build was Cancelled
 //           nanoBuild_handler.ReturnBuildResources(tileType);

 //           build_tilePositions.Clear();
 //       }


 //       // At this point we KNOW the mouse is NOT over a building or a rock, we have found rocks if extractor AND we are not on eneposY
 //       if (Input.GetMouseButtonUp (0) && canBuild && !onEneposY) {			// So LEFT CLICK to BUILD!! 

 //           // Right BEFORE pooling this object we tell Building UI that we are NOT currently building anymore
 //           build_controller.SetCurrentlyBuildingBool(false);

 //           //TileData currTile = mouse_controller.GetTileUnderMouse();

 //           if (build_tilePositions.Count > 1)
 //           {
 //               for (int i = 0; i < build_tilePositions.Count; i++)
 //               {
 //                   resourceGrid.SwapTileType(build_tilePositions[i].posX, build_tilePositions[i].posY, tileType, bpName, currNanoBotCost, sr.sprite.bounds.size.x, sr.sprite.bounds.size.y);
 //               }
 //           }
 //           else
 //           {
 //               resourceGrid.SwapTileType(build_tilePositions[0].posX, build_tilePositions[0].posY, tileType, bpName, currNanoBotCost, sr.sprite.bounds.size.x, sr.sprite.bounds.size.y);
 //           }
			

 //           followMouse = false;

 //           build_tilePositions.Clear();

 //           // Pool this object
 //           PoolObject (gameObject); // Pool this because resourceGrid just discovered the tile/building for us

	//	}
	
	//}

	void PoolObject(GameObject objToPool){
       
		objPool.PoolObject (objToPool);
	}

    bool CheckForResource(Vector3 position, TileData.Types bType)
    {
        bool rCheck = false;

        switch (bType)
        {
            case TileData.Types.extractor:
                if (resourceGrid.TileFromWorldPoint(position + Vector3.up).tileType == TileData.Types.rock)
                {
                    rCheck = true;
                }
                else if (resourceGrid.TileFromWorldPoint(position + (Vector3.up + Vector3.right)).tileType == TileData.Types.rock)
                {
                    rCheck = true;
                }
                else if (resourceGrid.TileFromWorldPoint(position + (Vector3.right)).tileType == TileData.Types.rock)
                {
                    rCheck = true;
                }
                else if (resourceGrid.TileFromWorldPoint(position + (Vector3.down + Vector3.right)).tileType == TileData.Types.rock)
                {
                    rCheck = true;
                }
                else if (resourceGrid.TileFromWorldPoint(position + (Vector3.down)).tileType == TileData.Types.rock)
                {
                    rCheck = true;
                }
                else if (resourceGrid.TileFromWorldPoint(position + (Vector3.down + Vector3.left)).tileType == TileData.Types.rock)
                {
                    rCheck = true;
                }
                else if (resourceGrid.TileFromWorldPoint(position + (Vector3.left)).tileType == TileData.Types.rock)
                {
                    rCheck = true;
                }
                else if (resourceGrid.TileFromWorldPoint(position + (Vector3.up + Vector3.left)).tileType == TileData.Types.rock)
                {
                    rCheck = true;
                }
                break;
            case TileData.Types.desalt_s:
                if (resourceGrid.TileFromWorldPoint(position + Vector3.up).tileType == TileData.Types.water)
                {
                    rCheck = true;
                }
                else if (resourceGrid.TileFromWorldPoint(position + (Vector3.up + Vector3.right)).tileType == TileData.Types.water)
                {
                    rCheck = true;
                }
                else if (resourceGrid.TileFromWorldPoint(position + (Vector3.right)).tileType == TileData.Types.water)
                {
                    rCheck = true;
                }
                else if (resourceGrid.TileFromWorldPoint(position + (Vector3.down + Vector3.right)).tileType == TileData.Types.water)
                {
                    rCheck = true;
                }
                else if (resourceGrid.TileFromWorldPoint(position + (Vector3.down)).tileType == TileData.Types.water)
                {
                    rCheck = true;
                }
                else if (resourceGrid.TileFromWorldPoint(position + (Vector3.down + Vector3.left)).tileType == TileData.Types.water)
                {
                    rCheck = true;
                }
                else if (resourceGrid.TileFromWorldPoint(position + (Vector3.left)).tileType == TileData.Types.water)
                {
                    rCheck = true;
                }
                else if (resourceGrid.TileFromWorldPoint(position + (Vector3.up + Vector3.left)).tileType == TileData.Types.water)
                {
                    rCheck = true;
                }
                break;
            default:
                // As a default case I'll assume that it passed a building type that is NOT an Extraction Building,
                // therefore we'll return true 
                rCheck = true;

                break;
        }

        return rCheck;
    }

 //   bool CheckForResource(int x, int y, string id){
	//	if (id == "ore") {
	//		if (resourceGrid.tiles [x, y].tileType == TileData.Types.rock) {
	//			return true;
	//		} else {
	//			return false;
	//		}
	//	} else if (id == "water") {
	//		if (resourceGrid.tiles [x, y].tileType == TileData.Types.water) {
	//			return true;
	//		} else {
	//			return false;
	//		}
	//	} else {
	//		if (resourceGrid.tiles [x, y].tileType == TileData.Types.mineral) {
	//			return true;
	//		} else {
	//			return false;
	//		}
	//	}
	//}


	//bool CheckForEmptySides(int x, int y){
	//	int top1 = y +1;
	//	//int top2 = y +2;
	//	int bottom1 = y - 1;
	//	//int bottom2 = y - 2;
	//	int left1 = x - 1;
	//	//int left2 = x - 2;
	//	int right1 = x + 1;
	//	//int right2 = x + 2;

	//	if (resourceGrid.tiles [x, top1].tileType == TileData.Types.empty && // top
	//		resourceGrid.tiles [left1, top1].tileType == TileData.Types.empty && // top Left
	//		resourceGrid.tiles [right1, top1].tileType == TileData.Types.empty && // top Right
	//		resourceGrid.tiles [x, bottom1].tileType == TileData.Types.empty && // bottom
	//		resourceGrid.tiles [left1, bottom1].tileType == TileData.Types.empty && // bottom left
	//		resourceGrid.tiles [right1, bottom1].tileType == TileData.Types.empty && // bottom right
	//		resourceGrid.tiles [left1, y].tileType == TileData.Types.empty && // left
	//		resourceGrid.tiles [right1, y].tileType == TileData.Types.empty) {  // right
	//		return true;
	//	} else {
	//		return false;
	//	}
	//	/*
	//	if (resourceGrid.tiles [x, top1].tileType == TileData.Types.empty && // top
	//	    resourceGrid.tiles [x, top2].tileType == TileData.Types.empty &&
	//	    resourceGrid.tiles [left1, top1].tileType == TileData.Types.empty && // top Left
	//	    resourceGrid.tiles [left2, top2].tileType == TileData.Types.empty &&
	//	    resourceGrid.tiles [right1, top1].tileType == TileData.Types.empty && // top Right
	//	    resourceGrid.tiles [right2, top2].tileType == TileData.Types.empty &&
	//	    resourceGrid.tiles [x, bottom1].tileType == TileData.Types.empty && // bottom
	//	    resourceGrid.tiles [x, bottom2].tileType == TileData.Types.empty &&
	//	    resourceGrid.tiles [left1, bottom1].tileType == TileData.Types.empty && // bottom left
	//	    resourceGrid.tiles [left2, bottom2].tileType == TileData.Types.empty &&
	//	    resourceGrid.tiles [right1, bottom1].tileType == TileData.Types.empty && // bottom right
	//	    resourceGrid.tiles [right2, bottom2].tileType == TileData.Types.empty &&
	//	    resourceGrid.tiles [left1, y].tileType == TileData.Types.empty && // left
	//	    resourceGrid.tiles [left2, y].tileType == TileData.Types.empty &&
	//	    resourceGrid.tiles [right1, y].tileType == TileData.Types.empty && // right
	//	    resourceGrid.tiles [right2, y].tileType == TileData.Types.empty) {
	//		return true;
	//	} else {
	//		return false;
	//	}
	//	*/
	//}

	//bool CheckEmptyBeneath(int x, int y){
	//	if (resourceGrid.tiles [x, y].tileType == TileData.Types.empty) {
	//		return true;
	//	} else {
	//		return false;
	//	}
	//}

    bool CheckEmptyBeneath(Vector3 pos)
    {
        if (resourceGrid.TileFromWorldPoint(pos) != null && resourceGrid.TileFromWorldPoint(pos).tileType == TileData.Types.empty)
            return true;
        else
            return false;
    }

	// Avoid building right near where an eneposY unit is walking
	void OnTriggerEnter2D(Collider2D coll){
		if (coll.gameObject.CompareTag ("Enemy")) {
			onEneposY = true;
		}
	}

	void OnTriggerExit2D (Collider2D coll){
		if (coll.gameObject.CompareTag ("Enemy")) {
			onEneposY = false;
		}
	}

}
