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
	//int roundedPosX;
	//int roundedPosY;

	bool canBuild, canAfford; // turns true when building is in proper position

	bool onEneposY = false; // can't build on top or near enemies

	public Player_ResourceManager resourceManager;
    //public int currOreCost; // this will charge the resources manager with the cost sent from UI handler
    //public int currNanoBotCost;

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

    Dictionary<Vector3, GameObject> position_indicator = new Dictionary<Vector3, GameObject>();

    GameObject posIndicator = null;

    Vector3 newPos;

    public bool canDragBuild = false;

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

        mouse_controller = Mouse_Controller.Instance;
    }

    public void SetCurrentBlueprintID(string id)
    {
        bpName = id;
    }


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
        //roundedPosX = Mathf.RoundToInt(mouse_controller.currMouseP.x);
        //roundedPosY = Mathf.RoundToInt(mouse_controller.currMouseP.y);

        transform.position = ResourceGrid.Grid.WorldPosToTilePos(Mouse_Controller.Instance.currMouseP);

  

        if (CheckPositionToBuild())
        {
            sr.color = trueColor;
            canBuild = true;
        }
        else
        {
            sr.color = invalidPosColor;
            canBuild = false;
        }

        ListenForCancelBuild();
        
        // Begin build
        if (Input.GetMouseButtonDown(0))
        {
            BeginBuild();
        }

        // End build
        if (Input.GetMouseButtonUp(0))
        {
            EndBuild();
        }

        if (canDragBuild)
        {
            if (Input.GetMouseButton(0))
            {
                MarkBuildSpots();
            }
        }
 
    }

    void MarkBuildSpots()
    {
        if (position_indicator.Count > 0)
        {
            if (transform.position != startingBuildPosition)
            {
                // TileData curTile = resourceGrid.GetTileFromWorldPos(new Vector3(roundedPosX, roundedPosY, 0));
                TileData curTile = Mouse_Controller.Instance.GetTileUnderMouse();

                if (curTile != null && CheckPositionToBuild(transform.position))
                {
                    newPos = new Vector3(curTile.posX, curTile.posY, 0);
                    if (!position_indicator.ContainsKey(newPos))
                    {
                        if (CheckCost(position_indicator.Count + 1))
                        {
                            posIndicator = objPool.GetObjectForType("Selection Circle", true, newPos);
                            // Add the pos indicator to its dictionary to easily remove it later
                            position_indicator.Add(newPos, posIndicator);

                        }
 
                    }
                }
            }
         
        }
    }

    void BeginBuild()
    {
        if (canBuild)
        {
            build_controller.SetCurrentlyBuildingBool(true);
            // Record the starting position of this build
           // startingBuildPosition = Mouse_Controller.Instance.currMouseP;
            startingBuildPosition = ResourceGrid.Grid.WorldPosToTilePos(Mouse_Controller.Instance.currMouseP);
        }

        // Set the tile under the mouse to the dictionary that will hold all this build's tiles
        // build_tilePositions.Add(mouse_controller.GetTileUnderMouse());
        // Spawn a position indicator to indicate where the building will be built

        //TileData curTile = resourceGrid.GetTileFromWorldPos(new Vector3(roundedPosX, roundedPosY, 0));
        TileData curTile = Mouse_Controller.Instance.GetTileUnderMouse();


        if (curTile != null && CheckPositionToBuild())
        {
            newPos = new Vector3(curTile.posX, curTile.posY, 0);
            if (!position_indicator.ContainsKey(newPos))
            {
                posIndicator = objPool.GetObjectForType("Selection Circle", true, newPos);
                // Add the pos indicator to its dictionary to easily remove it later
                position_indicator.Add(newPos, posIndicator);
            }
        }
    }

    void EndBuild()
    {
        Vector3 endBuildPosition = (canDragBuild == true) ? ResourceGrid.Grid.WorldPosToTilePos(Mouse_Controller.Instance.currMouseP) 
                                                            : startingBuildPosition;

        int startX = Mathf.FloorToInt(startingBuildPosition.x);
        int endX = Mathf.FloorToInt(endBuildPosition.x);
        if (endX < startX)
        {
            int tmpX = endX;
            endX = startX;
            startX = tmpX;
        }

        int startY = Mathf.FloorToInt(startingBuildPosition.y);
        int endY = Mathf.FloorToInt(endBuildPosition.y);
        if (endY < startY)
        {
            int tmpy = endY;
            endY = startY;
            startY = tmpy;
        }

        int multi = 0;

        for (int x = startX; x <= endX; x++)
        {   
            for (int y = startY; y <= endY; y++)
            {
                TileData curTile = resourceGrid.GetTileFromWorldPos(new Vector3(x, y, 0));
                if (curTile != null && CheckPositionToBuild(new Vector3(x, y, 0)))
                {
                    multi++;
                    if (CheckCost(multi))
                    {
                        // Charge the cost of resources after adding a potential build spot
                       // nanoBuild_handler.ChargeBuildResources(nanoBuild_handler.GetAvailableBlueprint(tileType));

                        if (canDragBuild)
                        {
                            resourceGrid.SwapTileType(curTile.posX, curTile.posY, tileType, bpName, 0, 0);
                        }
                        else
                        {
                            resourceGrid.SwapTileType(curTile.posX, curTile.posY, tileType, bpName, sr.bounds.size.x, sr.bounds.size.y);
                        }
                    }
                }
            }
        }

        // Set currently building to false
        build_controller.SetCurrentlyBuildingBool(false);

        PoolIndicators();
        // Pool this gameobj
        PoolObject(gameObject);
    }

    void ListenForCancelBuild()
    {
        if (Input.GetMouseButtonDown(1))
        {
            // CANCEL THE BUILD

            build_controller.SetCurrentlyBuildingBool(false);

            // give back the resources spent on this building because build was Cancelled

            // We can find out if this was a drag build and how many potential builds need to be cancelled
            // by getting the total position Indicators created
            if (canDragBuild && position_indicator.Count > 1)
            {
                nanoBuild_handler.ReturnBuildResources(tileType, position_indicator.Count);
            }
            else
            {
                nanoBuild_handler.ReturnBuildResources(tileType);
            }



            build_tilePositions.Clear();

            PoolIndicators();

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

    // same check but with a position as parameter
    bool CheckPositionToBuild(Vector3 pos)
    {
        if (!CheckEmptyBeneath(pos))
            return false;
        else if (!CheckForResource(pos, tileType))
            return false;
        else
        {
            return true;
        }

    }


    //void Build()
    //{
    //    // Right BEFORE pooling this object we tell Building UI that we are NOT currently building anymore
    //    build_controller.SetCurrentlyBuildingBool(false);

    //    //TileData currTile = mouse_controller.GetTileUnderMouse();

    //    if (build_tilePositions.Count > 1)
    //    {
    //        for (int i = 0; i < build_tilePositions.Count; i++)
    //        {
    //            if (build_tilePositions[i] != null)
    //                resourceGrid.SwapTileType(build_tilePositions[i].posX, build_tilePositions[i].posY, tileType, bpName, currNanoBotCost, sr.sprite.bounds.size.x, sr.sprite.bounds.size.y);
    //        }
    //    }
    //    else
    //    {
    //        resourceGrid.SwapTileType(build_tilePositions[0].posX, build_tilePositions[0].posY, tileType, bpName, currNanoBotCost, sr.sprite.bounds.size.x, sr.sprite.bounds.size.y);
    //    }

    //    build_tilePositions.Clear();

    //    PoolIndicators();

    //    // Pool this object
    //    PoolObject(gameObject); // Pool this because resourceGrid just discovered the tile/building for us
    //}

    bool CheckCost(int multiplier = 1)
    {
        return nanoBuild_handler.CheckBuildCost(nanoBuild_handler.GetAvailableBlueprint(tileType), multiplier);
    }


    void PoolIndicators()
    {
        // Pool all pos indicators
        if (position_indicator.Count > 0)
        {

            foreach (Vector3 pos in position_indicator.Keys)
            {
                if (position_indicator[pos] != null && position_indicator[pos].activeSelf)
                {
                    PoolObject(position_indicator[pos]);
                }
            }

            position_indicator.Clear();
        }
    }

    void PoolObject(GameObject objToPool){
       
		objPool.PoolObject (objToPool);
	}


    bool CheckForResource(Vector3 position, TileData.Types bType)
    {
        bool rCheck = false;

        switch (bType)
        {
            case TileData.Types.extractor:
                if (resourceGrid.GetTileFromWorldPos(position + Vector3.up).tileType == TileData.Types.rock)
                {
                    rCheck = true;
                }
                else if (resourceGrid.GetTileFromWorldPos(position + (Vector3.up + Vector3.right)).tileType == TileData.Types.rock)
                {
                    rCheck = true;
                }
                else if (resourceGrid.GetTileFromWorldPos(position + (Vector3.right)).tileType == TileData.Types.rock)
                {
                    rCheck = true;
                }
                else if (resourceGrid.GetTileFromWorldPos(position + (Vector3.down + Vector3.right)).tileType == TileData.Types.rock)
                {
                    rCheck = true;
                }
                else if (resourceGrid.GetTileFromWorldPos(position + (Vector3.down)).tileType == TileData.Types.rock)
                {
                    rCheck = true;
                }
                else if (resourceGrid.GetTileFromWorldPos(position + (Vector3.down + Vector3.left)).tileType == TileData.Types.rock)
                {
                    rCheck = true;
                }
                else if (resourceGrid.GetTileFromWorldPos(position + (Vector3.left)).tileType == TileData.Types.rock)
                {
                    rCheck = true;
                }
                else if (resourceGrid.GetTileFromWorldPos(position + (Vector3.up + Vector3.left)).tileType == TileData.Types.rock)
                {
                    rCheck = true;
                }
                break;
            case TileData.Types.desalt_s:
                if (resourceGrid.GetTileFromWorldPos(position + Vector3.up).tileType == TileData.Types.water)
                {
                    rCheck = true;
                }
                else if (resourceGrid.GetTileFromWorldPos(position + (Vector3.up + Vector3.right)).tileType == TileData.Types.water)
                {
                    rCheck = true;
                }
                else if (resourceGrid.GetTileFromWorldPos(position + (Vector3.right)).tileType == TileData.Types.water)
                {
                    rCheck = true;
                }
                else if (resourceGrid.GetTileFromWorldPos(position + (Vector3.down + Vector3.right)).tileType == TileData.Types.water)
                {
                    rCheck = true;
                }
                else if (resourceGrid.GetTileFromWorldPos(position + (Vector3.down)).tileType == TileData.Types.water)
                {
                    rCheck = true;
                }
                else if (resourceGrid.GetTileFromWorldPos(position + (Vector3.down + Vector3.left)).tileType == TileData.Types.water)
                {
                    rCheck = true;
                }
                else if (resourceGrid.GetTileFromWorldPos(position + (Vector3.left)).tileType == TileData.Types.water)
                {
                    rCheck = true;
                }
                else if (resourceGrid.GetTileFromWorldPos(position + (Vector3.up + Vector3.left)).tileType == TileData.Types.water)
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

    bool CheckEmptyBeneath(Vector3 pos)
    {
        if (resourceGrid.GetTileFromWorldPos(pos) != null && resourceGrid.GetTileFromWorldPos(pos).tileType == TileData.Types.empty)
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
