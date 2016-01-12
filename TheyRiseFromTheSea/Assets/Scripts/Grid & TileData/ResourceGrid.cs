using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

//using System;
//Random = UnityEngine.Random;

public class ResourceGrid : MonoBehaviour{
	/// <summary>
	/// Handles creating the grid of resources according to the positions created by Map.
	/// Map call a method here that takes (colums, rows, level, playerspawnPos).
	/// According to level it creates Tiles of different types.
	/// </summary>

	// List of Resource Tiles
//	public List<Tile> resourceTiles = new List<Tile>();

    // Variable used to access the Resource Grid (THERE SHOULD ONLY BE ONE INSTANCE OF THE GRID!)
    public static ResourceGrid Grid { get; protected set; }

//	public TileType[] tileTypes;
	public TileData[,] tiles;

//	TileType.Types tTypes;
//	int[,] tiles;
	public GameObject[,] spawnedTiles;

	// rows and colums determined by level
	[HideInInspector]
	public int mapSizeX, mapSizeY;
	

	public int level;

	public Transform tileHolder; // transform to hold Instantiated tiles in Hierarchy

	public GameObject discoverTileFab; // fab for grey discover tile with discover tile script

	public GameObject unitOnPath;
	public List<Node>pathForEnemy;

	public int transportSpawnX, transportSpawnY;
    public GameObject transporterGObj;

	public Building_UIHandler buildingUIHandler;
		
	public ObjectPool objPool;

	// BUILDING COSTS: // the array's [0] value is ORE Cost, [1] value is ENERGY Cost
	public int[] extractorCost, machineGunCost, seaWitchCost, harpoonHCost, cannonCost, sFarmCost; 
	public int[] storageCost, sDesaltCost, sniperCost, nutriCost, generatorCost;

	public Player_ResourceManager playerResources;


	// PATHFINDING VARS:
	Node[,] graph; // <--------- old one
    Node[,] grid;

	// Access to the Player Hero
	public GameObject Hero;

	// Access to Master State manager to call when Terraformer is blown up
	public MasterState_Manager master_state;

	//public int totalTilesThatAreWater;

	Map_Generator map_generator;

	[HideInInspector]
	public Resource_Sprite_Handler res_sprite_handler;

	public bool transporter_built;

	GameMaster game_master;

	bool islandVisible;

    [HideInInspector]
	public Transform cameraHolder;

    public List<Vector2> waterTilePositions;
    public Vector2[] waterTilesArray;

    public List<Vector2> emptyTilePositions;
    public Vector2[] emptyTilesArray;

    public bool worldGridInitialized { get; protected set; }

    public int MaxSize
    {
        get
        {
            return mapSizeX * mapSizeY;
        }
    }

    void OnEnable()
    {
        worldGridInitialized = false;
    }

    void Awake()
	{
        Grid = this;
		if (!master_state)
			master_state = GameObject.FindGameObjectWithTag ("GameController").GetComponent<MasterState_Manager> ();

		//if (!map_generator) {
		//	map_generator = GetComponent<Map_Generator> ();
		//	mapSizeX = map_generator.width;
		//	mapSizeY = map_generator.height;
		//} else {
		//	mapSizeX = map_generator.width;
		//	mapSizeY = map_generator.height;
		//}

		if (!res_sprite_handler)
			res_sprite_handler = GetComponent<Resource_Sprite_Handler> ();

		game_master = GameObject.FindGameObjectWithTag ("GM").GetComponent<GameMaster> ();

		cameraHolder = GameObject.FindGameObjectWithTag ("Camera").transform;

	}

	void Start () 
	{

		// In case Player Resource Manager is null
		if (playerResources == null)
			playerResources = GameObject.FindGameObjectWithTag ("Capital").GetComponent<Player_ResourceManager> ();

        // tiles and spawnedTiles are initialized by MapGenerator

        InitPathFindingGrid();

        worldGridInitialized = true;

        StartCoroutine("WaitForLandingSiteSelection");

        // Give the waterTilePositions to Enemy_Spawner
        //Enemy_Spawner.instance.InitSpawnPositions(waterTilesArray);
        InitializeRockandMinerals();
    }

    // This will wait for the Player to select where to land on initial level load. Once they left click, this stops and never checks again.
    IEnumerator WaitForLandingSiteSelection()
    {
        while (true)
        {
            if (!transporter_built && islandVisible)
            {
                BuildTheTransporter();
                yield break;
            }
               

            yield return null;
        }
    }

    void Update()
	{

        if (!islandVisible)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Vector3 camHolderPos = new Vector3(cameraHolder.position.x, cameraHolder.position.y, -10F);
                //MoveTheIslandMapToFront(camHolderPos);
                islandVisible = true;
            }
        }

	}

	void BuildTheTransporter()
	{
		Vector3 m = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		int mX = Mathf.RoundToInt(m.x);
		int mY = Mathf.RoundToInt(m.y);

		if (mX < mapSizeX && mY < mapSizeY && mX > 0 && mY > 0){
			if (Input.GetMouseButtonDown(0))
            {
                // Just in case the Wait for Player Landing select coroutine hasn't stopped 
                StopCoroutine("WaitForLandingSiteSelection");
				InitTransporter(mX, mY);
			}
		}
	}

    void InitTransporter(int _terraPosX, int _terraPosY)
    {
        // SPAWN PLAYER CAPITAL HERE:
        tiles[_terraPosX, _terraPosY] = new TileData(_terraPosX, _terraPosY, "Transporter", TileData.Types.capital, 0, 10000, 200, 5, 0, 0, 0);
        DefineMultipleTiles(_terraPosX, _terraPosY, 2, 2, "Transporter", TileData.Types.capital, 0, 100, 200, 5, 0, 0, 0);

        SpawnDiscoverTile(tiles[_terraPosX, _terraPosY].tileName, new Vector3(_terraPosX, _terraPosY, 0.0f), tiles[_terraPosX, _terraPosY].tileType, 2, 2);

        // Spawn Player / Hero 1 tile down from the terraformer
        Hero = game_master.SpawnThePlayer(_terraPosX, _terraPosY - 1);

        // TODO: replace capitalPos completely with terraformer pos
        transportSpawnX = _terraPosX;
        transportSpawnY = _terraPosY;

        transporter_built = true;


        // Turn on the Enemy Wave spawner
        //enemy_waveSpawner.SetActive (true);

    }

    //    void MoveTheIslandMapToFront(Vector3 camHolderPos)
    //	{

    //		islandVisible = true;
    //		//TODO: Introduction to each level, the island RISES from the sea as the terraformer activates

    ////		if (cameraHolder) {
    ////			cameraHolder.position = Vector3.Lerp(cameraHolder.position, camHolderPos, 66 * Time.deltaTime);
    ////			islandVisible = true;
    ////		}
    //	}

    public void InitializeRockandMinerals()
    {
        Rock_Generator.Instance.GenerateRocks();

  
    }

    //	bool CheckForWater (int x, int y)
    //	{
    //        int waterRange = 2;
    //		bool hasWater = false;
    //		// Make sure every position is still within map bounds
    //		for (int bottomX = x- waterRange; bottomX < x + waterRange; bottomX++){
    //			for (int bottomY = y- waterRange; bottomY < y + waterRange; bottomY++){
    //				if (CheckIsInMapBounds(bottomX, bottomY)){
    //					// It IS in map bounds, now check if it's water
    //					if (tiles[bottomX, bottomY].tileType == TileData.Types.water){
    //						hasWater = true;
    //						break;
    //					}
    //				}else{
    //					// It's NOT in map bounds so just return true because it's most likely water
    //					hasWater = true;
    //					break;
    //				}
    //			}

    //		}

    //		return hasWater;
    //	}

    public bool CheckIsInMapBounds(int x, int y)
    {
        if (x < mapSizeX && y < mapSizeY && x > 0 && y > 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }


    public void PlaceOrePatch(OrePatch _patch, Rock.RockType rType)
	{
        // place the lead ore if there is no rock already on that tile
        if (tiles[_patch.leadPositionX, _patch.leadPositionY].tileType == TileData.Types.empty)
        {
            switch (rType)
            {
                case Rock.RockType.sharp:
                    tiles[_patch.leadPositionX, _patch.leadPositionY] = new TileData(_patch.leadPositionX, _patch.leadPositionY, TileData.Types.rock, 6000, 50);
                    SpawnRock("sharp rock", new Vector3(_patch.leadPositionX, _patch.leadPositionY, 0.0F), Rock.RockType.sharp);
                    break;
                case Rock.RockType.tube:
                    tiles[_patch.leadPositionX, _patch.leadPositionY] = new TileData(_patch.leadPositionX, _patch.leadPositionY, TileData.Types.rock, 6000, 50);
                    SpawnRock("tube rock", new Vector3(_patch.leadPositionX, _patch.leadPositionY, 0.0F), Rock.RockType.tube);
                    break;
                case Rock.RockType.hex:
                    tiles[_patch.leadPositionX, _patch.leadPositionY] = new TileData(_patch.leadPositionX, _patch.leadPositionY, TileData.Types.rock, 6000, 50);
                    SpawnRock("hex rock", new Vector3(_patch.leadPositionX, _patch.leadPositionY, 0.0F), Rock.RockType.hex);
                    break;
                default:
                    tiles[_patch.leadPositionX, _patch.leadPositionY] = new TileData(_patch.leadPositionX, _patch.leadPositionY, TileData.Types.rock, 6000, 50);
                    SpawnRock("sharp rock", new Vector3(_patch.leadPositionX, _patch.leadPositionY, 0.0F), Rock.RockType.sharp);
                    break;
            }
            CreateUnWalkableBorder(_patch.leadPositionX, _patch.leadPositionY);
        }
   

        // place the neighbors in their positions
        if (_patch.neighborOreTiles != null && _patch.neighborOreTiles.Length > 0) {
			
			for (int i = 0; i < _patch.neighborOreTiles.Length; i++){

                if (CheckIsInMapBounds(_patch.neighborOreTiles[i].posX, _patch.neighborOreTiles[i].posY))
                {
                    if (tiles[_patch.neighborOreTiles[i].posX, _patch.neighborOreTiles[i].posY].tileType == TileData.Types.empty)
                    {
                        switch (rType)
                        {
                            case Rock.RockType.sharp:
                                tiles[_patch.neighborOreTiles[i].posX, _patch.neighborOreTiles[i].posY] = new TileData(_patch.neighborOreTiles[i].posX, _patch.neighborOreTiles[i].posY, TileData.Types.rock, 6000, 50);
                                SpawnRock("sharp rock", new Vector3(_patch.neighborOreTiles[i].posX, _patch.neighborOreTiles[i].posY, 0.0F), Rock.RockType.sharp);
                                break;
                            case Rock.RockType.tube:
                                tiles[_patch.neighborOreTiles[i].posX, _patch.neighborOreTiles[i].posY] = new TileData(_patch.neighborOreTiles[i].posX, _patch.neighborOreTiles[i].posY, TileData.Types.rock, 6000, 50);
                                SpawnRock("tube rock", new Vector3(_patch.neighborOreTiles[i].posX, _patch.neighborOreTiles[i].posY, 0.0F), Rock.RockType.tube);
                                break;
                            case Rock.RockType.hex:
                                tiles[_patch.neighborOreTiles[i].posX, _patch.neighborOreTiles[i].posY] = new TileData(_patch.neighborOreTiles[i].posX, _patch.neighborOreTiles[i].posY, TileData.Types.rock, 6000, 50);
                                SpawnRock("hex rock", new Vector3(_patch.neighborOreTiles[i].posX, _patch.neighborOreTiles[i].posY, 0.0F), Rock.RockType.hex);
                                break;
                            default:
                                tiles[_patch.neighborOreTiles[i].posX, _patch.neighborOreTiles[i].posY] = new TileData(_patch.neighborOreTiles[i].posX, _patch.neighborOreTiles[i].posY, TileData.Types.rock, 6000, 50);
                                SpawnRock("sharp rock", new Vector3(_patch.neighborOreTiles[i].posX, _patch.neighborOreTiles[i].posY, 0.0F), Rock.RockType.sharp);
                                break;
                        }

                        CreateUnWalkableBorder(_patch.neighborOreTiles[i].posX, _patch.neighborOreTiles[i].posY);
                    }
                
                }

                
            }
		}

	}

    void CreateUnWalkableBorder(int sourceX, int sourceY)
    {
        int width = 1;
        int height = 1;
        // This will only work previous to initializing the pathfinding graph
        for (int w = 0; w <= width; w++)
        {
            for (int h = 0; h <= height; h++)
            {
                tiles[sourceX + w, sourceY + h].isWalkable = false;
            }
        }

    }



    /// <summary>
    /// Damages the tile.
    /// </summary>
    /// <param name="x">The x coordinate.</param>
    /// <param name="y">The y coordinate.</param>
    /// <param name="damage">Damage.</param>
    public void DamageTile(TileData tile, float damage)
	{
		// make sure there IS a tile there
		if (spawnedTiles [tile.posX, tile.posY] != null) {

			// If it has 0 or less HP left, kill tile
			if (tile.hp <= 0) {
				if (tile.tileType == TileData.Types.capital){
					// call mission failed
					master_state.mState = MasterState_Manager.MasterState.MISSION_FAILED;
		
				}else{
                    /*SwapTileType (tile.posX, tile.posY, TileData.Types.empty);*/    // to KILL TILE I just swap it to an empty! ;)
                                                                                      // OR! call break this building from the Hero's nanobuilding_handler
                    Hero.GetComponent<NanoBuilding_Handler>().BreakThisBuilding(tile.tileType, spawnedTiles[tile.posX, tile.posY]);

				}
			}else{
				tile.hp -= damage;
				Debug.Log("Tile: " + tile.tileType + " damaged for " + damage);
				Debug.Log("It has " + tile.hp + " left!");

                // Check again if it needs to be killed, hp <= 0
                if (tile.hp <= 0)
                {
                    if (tile.tileType == TileData.Types.capital)
                    {
                        // call mission failed
                        master_state.mState = MasterState_Manager.MasterState.MISSION_FAILED;

                    }
                    else
                    {
                        spawnedTiles[tile.posX, tile.posY].GetComponent<Building_ClickHandler>().BreakBuilding(tile.nanoBotCost); // Call Break Building from within the building's click handler

                        //SwapTileType(tile.posX, tile.posY, TileData.Types.empty);   // to KILL TILE I just swap it to an empty! ;) < ----- NOPE! :P

                    }
                }
            }

		} else {
			Debug.Log("GRID: Could NOT find tile to damage!");
		}
	}

	/// <summary>
	/// Gets the type of the tile.
	/// </summary>
	/// <returns>The tile type.</returns>
	/// <param name="x">The x coordinate.</param>
	/// <param name="y">The y coordinate.</param>
	public TileData.Types GetTileType(int x, int y)
	{
		return tiles[x,y].tileType;
	}

	/// <summary>
	/// Gets the tile game object from spawned tiles array.
	/// </summary>
	/// <returns>The tile game object.</returns>
	/// <param name="x">The x coordinate.</param>
	/// <param name="y">The y coordinate.</param>
	public GameObject GetTileGameObjFromWorldPos(Vector3 worldPos)
	{
        Vector3 worldBottomLeft = transform.position - Vector3.right * mapSizeX / 2 - Vector3.up * mapSizeY / 2;
        Vector3 worldPoint = worldBottomLeft + Vector3.right * worldPos.x + Vector3.up * worldPos.y;

        int x = Mathf.RoundToInt(worldPoint.x);
        int y = Mathf.RoundToInt(worldPoint.y);

        return spawnedTiles[x, y];
    }

    public GameObject GetTileGameObjFromIntCoords(int x, int y)
    {
        if (spawnedTiles[x, y] != null)
            return spawnedTiles[x, y];
        else
            return null;
    }

    public int ExtractFromTile(int x, int y, int ammnt, bool isHandDrill = false)
    {
        int resourceMined = 0;

        if (tiles[x,y].maxResourceQuantity >= ammnt)
        {
            // mine it
            tiles[x, y].maxResourceQuantity -= ammnt;
            Debug.Log(tiles[x, y].tileType + " extracted by " + ammnt + " out of " + tiles[x, y].maxResourceQuantity);

            if (tiles[x, y].maxResourceQuantity > 0)
            {
                // Spawn rock chunks IF this is a handrill
                if (isHandDrill)
                {
                    // FOR TESTING, just shrink down the rock every time we mine & spawn rock chunks of this type
                    if (spawnedTiles[x, y] != null)
                    {
                        Rock_Handler rockHandler = spawnedTiles[x, y].GetComponent<Rock_Handler>();
                        rockHandler.ShrinkDownSize();
                        // chunks
                        StartCoroutine(SpawnRockChunks(ammnt, rockHandler.myRockType, spawnedTiles[x, y].transform.position));
                    }
                }
              
            }
            else
            {
                // Deplete the tile
                SwapTileType(x, y, TileData.Types.empty);
            }

            resourceMined = ammnt;
        }
        else if (tiles[x,y].maxResourceQuantity > 0)
        {
            // take what's left and deplete it
            resourceMined = tiles[x, y].maxResourceQuantity;
            SwapTileType(x, y, TileData.Types.empty);
        }
        else
        {
            // nothing left
            resourceMined = 0;
            SwapTileType(x, y, TileData.Types.empty);
        }

        return resourceMined;
    }

    IEnumerator SpawnRockChunks(int chunkCount, Rock.RockType rockType, Vector3 position)
    {
        for (int i = 0; i <= chunkCount; i++)
        {
            GameObject chunk = objPool.GetObjectForType("Chunk of Rock", true, position);
            if (chunk)
            {
                /* Currently NOT using the rocktype for anything BUT I could use it to assign the chunk's correct sprite.
                The Problem with that is it might not change fast enough to not show up as the wrong sprite for a moment before popping back.
                I'll need to assign the correct sprite from an array (probably on the res_sprite manager) of rock chunks, separated by type and maybe size. 
                ALSO will need to change the spawned chunk's tag to its rock type's tag so when the Player picks up the chunk they get the correct resource added. */
                SpriteRenderer sr = chunk.GetComponent<SpriteRenderer>();
                sr.sprite = res_sprite_handler.GetChunkSprite(rockType);


                Rigidbody2D rb = chunk.GetComponent<Rigidbody2D>();
                int randomForceDirection = Random.Range(0, 4);
                float forceAmmt = 10;
                switch (randomForceDirection)
                {
                    case 0:
                        rb.AddForce(Vector2.down * forceAmmt, ForceMode2D.Impulse);
                        break;
                    case 1:
                        rb.AddForce(Vector2.up * forceAmmt, ForceMode2D.Impulse);
                        break;
                    case 2:
                        rb.AddForce(Vector2.right * forceAmmt, ForceMode2D.Impulse);
                        break;
                    case 3:
                        rb.AddForce(Vector2.left * forceAmmt, ForceMode2D.Impulse);
                        break;
                    default:
                        rb.AddForce(Vector2.up * forceAmmt, ForceMode2D.Impulse);
                        break;
                }

                // Change their tag according to rock type
                switch (rockType)
                {
                    case Rock.RockType.sharp:
                        chunk.tag = "Sharp Chunk";
                        break;
                    case Rock.RockType.hex:
                        chunk.tag = "Hex Chunk";
                        break;
                    case Rock.RockType.tube:
                        chunk.tag = "Tube Chunk";
                        break;
                    default:
                        chunk.tag = "Sharp Chunk";
                        break;
                }
           
            }

            yield return new WaitForSeconds(0.05f);

        }

        yield break;

    }

   

	/// <summary>
	/// Swaps the type of the tile.
	/// </summary>
	/// <param name="x">The x coordinate.</param>
	/// <param name="y">The y coordinate.</param>
	/// <param name="newType">New type.</param>
	public void SwapTileType(int x, int y, TileData.Types newType, int nanoBotCost = 0, float spriteSizeX = 0, float spriteSizeY = 0)
	{
        
		// MAKE SURE THIS IS NOT A SPAWNED TILE ALREADY!!! 
		// So we don't change the grid tile data where we don't want to!
		if (spawnedTiles [x, y] == null) {
            // If spawnedTiles[x, y] is null it means that this tile is an EMPTY land tile,
            // so in ALL cases below it will be turning the EMPTY walkable tile into a BUILDING unwalkable tile
            // SO.. update the Node Grid here as well:
            grid[x, y].isWalkable = false;

            // sprite size X represents how many tiles will be needed for its width, y for the height
            // round them down to ints
            int spriteWidth = Mathf.RoundToInt(spriteSizeX);
            int spriteHeight = Mathf.RoundToInt(spriteSizeY);
            Debug.Log(newType + " 's sprite size x: " + spriteWidth + " and size y: " + spriteHeight);
            switch (newType) {
			    case TileData.Types.extractor:

                    if (spriteWidth > 0 && spriteHeight > 0)
                        DefineMultipleTiles(x, y, spriteWidth, spriteHeight, "Extractor", newType, 0, 10000, 5, 5, 0, 0, nanoBotCost);
                    
                    else
                        tiles [x, y] = new TileData (x, y, "Extractor",newType, 0, 10000, 5, 5, 0, 0, nanoBotCost);
                   
				break;
			    case TileData.Types.machine_gun:
				    
                    if (spriteWidth > 0 && spriteHeight > 0)
                    {
                        DefineMultipleTiles(x, y, spriteWidth, spriteHeight, "Machine Gun", newType, 0, 10000, 30, 5, 5, 0, nanoBotCost);
                    }
                    else
                    {
                        tiles[x, y] = new TileData(x, y, "Machine Gun", newType, 0, 10000, 30, 5, 5, 0, nanoBotCost);
                    }
                    break;
			    case TileData.Types.cannons:
                    if (spriteWidth > 0 && spriteHeight > 0)
                        DefineMultipleTiles(x, y, spriteWidth, spriteHeight, "Cannons", newType, 0, 10000, 30, 5, 3, 0, nanoBotCost);
                    else
                        tiles [x, y] = new TileData (x, y, "Cannons", newType, 0, 10000, 30, 5, 3, 0, nanoBotCost);
				break;
			    case TileData.Types.harpoonHall:
                    if(spriteWidth > 0 && spriteHeight > 0)
                        DefineMultipleTiles(x, y, spriteWidth, spriteHeight, "Harpooner's Hall", newType, 0, 10000, 50, 6, 0, 0, nanoBotCost);
                    else
				        tiles [x, y] = new TileData (x, y, "Harpooner's Hall", newType, 0, 10000, 50, 6, 0, 0, nanoBotCost);
				break;
			    case TileData.Types.farm_s:
                    if (spriteWidth > 0 && spriteHeight > 0)
                        DefineMultipleTiles(x, y, spriteWidth, spriteHeight, "Seaweed Farm", newType, 0, 10000, 25, 1, 0, 0, nanoBotCost);
                    else
                        tiles [x, y] = new TileData (x, y, "Seaweed Farm", newType, 0, 10000, 25, 1, 0, 0, nanoBotCost);
				break;
			    case TileData.Types.storage:
                    if (spriteWidth > 0 && spriteHeight > 0)
                        DefineMultipleTiles(x, y, spriteWidth, spriteHeight, "Storage", newType, 0, 10000, 35, 2, 0, 0, nanoBotCost);
                    else
                        tiles [x, y] = new TileData (x, y, "Storage", newType, 0, 10000, 35, 2, 0, 0, nanoBotCost);
				break;
			    case TileData.Types.desalt_s:
                    if (spriteWidth > 0 && spriteHeight > 0)
                        DefineMultipleTiles(x, y, spriteWidth, spriteHeight, "Desalination Pump", newType, 0, 10000, 15, 1, 0, 0, nanoBotCost);
                    else
                        tiles [x, y] = new TileData (x, y, "Desalination Pump", newType, 0, 10000, 15, 1, 0, 0, nanoBotCost);
				break;
			    case TileData.Types.sniper:
                    if (spriteWidth > 0 && spriteHeight > 0)
                        DefineMultipleTiles(x, y, spriteWidth, spriteHeight, "Sniper Gun", newType, 0, 10000, 0, 0, 0, 0, nanoBotCost);
                    else
                        tiles [x, y] = new TileData (x, y, "Sniper Gun", newType, 0, 10000, 0, 0, 0, 0, nanoBotCost);
				break;
			    case TileData.Types.seaWitch:
                    if (spriteWidth > 0 && spriteHeight > 0)
                        DefineMultipleTiles(x, y, spriteWidth, spriteHeight, "Sea-Witch Crag", newType, 0, 10000, 0, 0, 0, 0, nanoBotCost);
                    else
                        tiles [x, y] = new TileData (x, y, "Sea-Witch Crag", newType, 0, 10000, 0, 0, 0, 0, nanoBotCost);
				break;
			    case TileData.Types.nutrient:
                    if (spriteWidth > 0 && spriteHeight > 0)
                        DefineMultipleTiles(x, y, spriteWidth, spriteHeight, "Nutrient Generator", newType, 0, 10000, 0, 0, 0, 0, nanoBotCost);
                    else
                        tiles [x, y] = new TileData (x, y, "Nutrient Generator", newType, 0, 10000, 0, 0, 0, 0, nanoBotCost);
				break;
			    case TileData.Types.generator:
                    if (spriteWidth > 0 && spriteHeight > 0)
                        DefineMultipleTiles(x, y, spriteWidth, spriteHeight, "Energy Generator", newType, 0, 10000, 0, 0, 0, 0, nanoBotCost);
                    else
                        tiles [x, y] = new TileData (x, y, "Energy Generator", newType, 0, 10000, 0, 0, 0, 0, nanoBotCost);
				break;
                case TileData.Types.terraformer:
                    if (spriteWidth > 0 && spriteHeight > 0)
                        DefineMultipleTiles(x, y, spriteWidth, spriteHeight, "Terraformer", newType, 0, 10000, 0, 0, 0, 0, nanoBotCost);
                    else
                        tiles[x, y] = new TileData(x, y, "Terraformer", newType, 0, 10000, 0, 0, 0, 0, nanoBotCost);
                    break;

                case TileData.Types.building:
				tiles [x, y] = new TileData (x, y, newType, 0, 10000);
				break;
			default:
				print ("No tile changed.");
				break;
			}

            // Discover the tile to display it
            DiscoverTile(x, y, true, spriteWidth, spriteHeight);

            // IF tile is a Building with an ENERGY COST, apply it to resources
            //if (tiles[x,y].energyCost > 0){
            //	playerResources.totalEnergyCost = playerResources.totalEnergyCost + tiles[x,y].energyCost;
            //}

        }
        else { 

			// if we are swapping an already spawned tile we are MOST LIKELY turning it into an empty tile
			// BUT if this was a building that has an ENERGY cost that must be reflected in Player resources 
			//	by subtracting from the total ENERGY cost
			//if (tiles[x,y].energyCost > 0){
			//	playerResources.totalEnergyCost = playerResources.totalEnergyCost - tiles[x,y].energyCost;
			//}

//			// ALSO if it's a Farm we need to subtract its FOOD production and its WATER consumed
//			if (playerResources.foodProducedPerDay > 0){

//				if (tiles[x,y].tileType == TileData.Types.farm_s || tiles[x,y].tileType == TileData.Types.nutrient){

//					FoodProduction_Manager foodM = spawnedTiles [x, y].GetComponent<FoodProduction_Manager>();
//					playerResources.CalculateFoodProduction(foodM.foodProduced, foodM.productionRate, foodM.waterConsumed, true);

//				}
//			}

//			// AND if it's a STORAGE we need to subtract all the ORE and WATER from the resources
//			if (tiles[x,y].tileType == TileData.Types.storage){

//				Storage storage = spawnedTiles[x,y].GetComponent<Storage>();
////				
//				// remove the storage building from the list
//				playerResources.RemoveStorageBuilding(storage);
//			}

//			// If it's an EXTRACTOR also need to subtract from Ore Produced
//			if (tiles[x,y].tileType == TileData.Types.extractor){

//				Extractor extra = spawnedTiles [x, y].GetComponent<Extractor>();

//				playerResources.CalculateOreProduction(extra.extractAmmnt, extra.extractRate, true);
//			}

//			// Same thing for a WATER PUMP
//			if (tiles[x,y].tileType == TileData.Types.desalt_s || tiles[x,y].tileType == TileData.Types.desalt_m 
//			    || tiles[x,y].tileType == TileData.Types.desalt_l){

//				DeSalt_Plant pump = spawnedTiles [x, y].GetComponent<DeSalt_Plant>();

//				playerResources.CalculateWaterProduction(pump.waterPumped, pump.pumpRate, true);
//			}

//			// If it's a ENERGY GENERATOR we have to subtract Energy //TODO: Add an energy produced per day panel
//			if (tiles[x,y].tileType == TileData.Types.generator){

//				Energy_Generator gen = spawnedTiles [x,y].GetComponent<Energy_Generator>();

//				playerResources.ChangeResource("Energy", -gen.energyUnitsGenerated);
//			}


            //*********   NANO BOTS RETURNED:

            // Return bots... THIS WILL NEED TO EQUAL A NANOBOTS COST LATER, JUST USING 10
            int returnNanoCost = tiles[x, y].nanoBotCost;

            NanoBuilding_Handler nanoBuilder = Hero.GetComponent<NanoBuilding_Handler>();
        
            nanoBuilder.nanoBots += returnNanoCost;

            Debug.Log("GRID: Returning " + returnNanoCost + " NANOBOTS ");



            // ***********  DEFINE NEW EMPTY TILES:

            // Store the gameobject before doing anything to it.
            GameObject tileToBeDestroyed = spawnedTiles[x, y];

            // If a tile was set as a group of multiple tiles to cover the space of its sprite, we nee to turn ALL of them to empty
            if (spriteSizeX > 0 || spriteSizeY > 0) // < ----- the way we are swapping for an empty tile, these are always = 0 in this case
            {

                tiles[x, y] = new TileData(x, y, newType, 0, 1);
            }
            else
            {
                // we need the size of the tile that WAS here. We can get the gameobject from spawnedTiles[,], and from that get the Sprite
                if (tileToBeDestroyed != null)
                {
                   
                    int width = Mathf.RoundToInt(spawnedTiles[x, y].GetComponent<SpriteRenderer>().sprite.bounds.size.x);
                    int height = Mathf.RoundToInt(spawnedTiles[x, y].GetComponent<SpriteRenderer>().sprite.bounds.size.y);
                    // Define these as empty using this new width and height (doing this since the arguments passed in would be 0 for an empty SwapTile)
                    DefineMultipleEmptyTiles(x, y, width, height, tileToBeDestroyed);
                }
                else
                {
                    Debug.Log("GRID: Could not find the Sprite Renderer. spawnedTiles gameobject = " + spawnedTiles[x, y]);
                    tiles[x, y] = new TileData(x, y, newType, 0, 1);
                }


            }


            //*********   POOL SPAWNED TILE:

            // Use the stored gameobject variable from above to pool it. This represents the base tile of this object, the other placemarkers in the spawnedTiles array have been removed.
            objPool.PoolObject(tileToBeDestroyed);
        
        }
	}

    void DefineMultipleTiles(int x, int y, int spriteWidth, int spriteHeight, string name, TileData.Types newType, int quantity, int moveCost, float hp, float defence, float attack, float shield, int nanoBotCost  )
    {
        for (int w = -(spriteWidth - 1); w < spriteWidth; w++)
        {
            for (int h = 0; h < spriteHeight; h++)
            {
                // ******** MAKE SURE we are not changing tiles that are NOT empty
                if(tiles[x + w, y + h].tileType == TileData.Types.empty)
                {
                    tiles[x + w, y + h] = new TileData(x + w, y + h, name, newType, quantity, moveCost, hp, defence, attack, shield, nanoBotCost);

                    grid[x + w, y + h].isWalkable = tiles[x + w, y + h].isWalkable;
                }
              

            }

        }
    }

    //void DefineMultipleTilesAsGameObjects(int x, int y, int spriteWidth, int spriteHeight, GameObject tileGameObj)
    //{
    //    for (int w = -(spriteWidth - 1); w < spriteWidth; w++)
    //    {
    //        for (int h = -1; h < spriteHeight; h++)
    //        {
    //            spawnedTiles[x + w, y + h] = tileGameObj;
    //        }
    //    }
    //}

    void DefineMultipleEmptyTiles(int x, int y, int spriteWidth, int spriteHeight, GameObject oldGameObj)
    {
        // Store the current tiletype to check against
        TileData.Types formerType = tiles[x, y].tileType;
        Debug.Log("Defining multiple empty tiles of " + formerType);
        for (int w = -(spriteWidth - 1); w < spriteWidth; w++)
        {
            for (int h = 0; h < spriteHeight; h++)
            {
                // ONLY swap the tiles that are equal to tile type stored above and the same gameobject
                if (tiles[x + w, y + h].tileType == formerType && spawnedTiles[x + w, y + h] == oldGameObj)
                {
                    tiles[x + w, y + h] = new TileData(x + w, y + h, TileData.Types.empty, 0, 1);

                    grid[x + w, y + h].isWalkable = tiles[x + w, y + h].isWalkable;

                    if (spawnedTiles[x + w, y + h] != null)
                        spawnedTiles[x + w, y + h] = null;
                }

            }

        }
    }

    //void DefineResourceTiles(int x, int y, int areaWidth, int areaHeight, TileData.Types newType, int quantity)
    //{
    //    for (int w = 0; w < areaWidth; w++)
    //    {
    //        for (int h = 0; h < areaHeight; h++)
    //        {
    //            // ******** MAKE SURE we are not changing tiles that are NOT empty
    //            if (tiles[x + w, y + h].tileType == TileData.Types.empty)
    //            {
    //                tiles[x + w, y + h] = new TileData(x, y, newType, quantity, 10000);

    //               // grid[x + w, y + h].isWalkable = tiles[x + w, y + h].isWalkable; < ----------- NOT affecting the grid here because it will become unwalkable when Grid is initialized on Start()
    //            }


    //        }

    //    }
    //}



	public void DiscoverTile(int x, int y, bool trueIfSwapping, int spriteWidth = 0, int spriteHeight = 0)
	{
		if (spawnedTiles [x, y] == null) { // if it's null it means it hasn't been spawned
			//Dont Spawn a tile if the type is Empty
			// the space will still be walkable because it will still be mapped on the Node Graph
			
			if (tiles [x, y].tileType != TileData.Types.empty) {
				SpawnDiscoverTile (tiles [x, y].tileName, new Vector3 (x, y, 0.0f), tiles [x, y].tileType, spriteWidth, spriteHeight);
				// set it so it knows it has been spawned
				tiles [x, y].hasBeenSpawned = true;
			}
		} else { // it HAS been spawned
			if (trueIfSwapping){
				// since we know this tile has already been spawned we need to destroy the old one,
				// before adding the new one
				Destroy (spawnedTiles [x, y].gameObject);
				SpawnDiscoverTile (tiles [x, y].tileName, new Vector3 (x, y, 0.0f), tiles [x, y].tileType, spriteWidth, spriteHeight);
			}
		}
	}



   void SpawnDiscoverTile(string tileName,Vector3 position, TileData.Types type, int _spriteWidth = 0, int _spriteHeight = 0)
    {
		// spawn the half tile from the pool
		GameObject discoverTile = objPool.GetObjectForType ("Half_Tile", false, Vector3.zero);
        if (discoverTile != null) {
            DiscoverTile dTile = discoverTile.GetComponent<DiscoverTile>();
            dTile.transform.position = position;
            dTile.objPool = objPool;
            dTile.master_state = master_state;
            dTile.r_sprite_handler = res_sprite_handler;

            if (_spriteHeight > 0 && _spriteWidth > 0)
            {
                dTile.TileToDiscover(newTileName: tileName, mapPosX: (int)position.x, mapPosY: (int)position.y, tileHolder: tileHolder, tileType: type, spriteWidth: _spriteWidth, spriteHeight: _spriteHeight);
            }
            else
            {
                dTile.TileToDiscover(newTileName: tileName, mapPosX: (int)position.x, mapPosY: (int)position.y, tileHolder: tileHolder, tileType: type);
            }

        }
	}

    void SpawnRock(string rockName, Vector3 position, Rock.RockType rockType)
    {
        GameObject discoverTile = objPool.GetObjectForType("Half_Tile", false, Vector3.zero);
        if (discoverTile != null)
        {
            DiscoverTile dTile = discoverTile.GetComponent<DiscoverTile>();
            dTile.transform.position = position;
            dTile.objPool = objPool;
            dTile.master_state = master_state;
            dTile.r_sprite_handler = res_sprite_handler;
            dTile.DiscoverRock(rockName, (int)position.x, (int)position.y, tileHolder, rockType);
        }
    }

	/// <summary>
	/// Adds the credits for kill to Resources
	/// according to the value of that unit stats' credit value.
	/// </summary>
	/// <param name="creditValue">Credit value.</param>
	public void AddCreditsForKill(int creditValue)
	{
		// Add the credit value for the unit killed to Player Resources
		playerResources.ChangeResource ("Credits", creditValue);
		Debug.Log ("GRID: Collected bounty of " + creditValue);
	}



    // ** PATHFINDING GRAPH


    public void InitPathFindingGrid()
    {
        grid = new Node[mapSizeX, mapSizeY];
        Vector3 worldBottomLeft = transform.position - Vector3.right * mapSizeX / 2 - Vector3.up * mapSizeY / 2;
       // print("World Bottom Left: " + worldBottomLeft);
        // This is called by the map generator once it is done defining the tiles[,]
        for (int x = 0; x < mapSizeX; x++)
        {
            for (int y = 0; y < mapSizeY; y++)
            {
                Vector3 worldPoint = worldBottomLeft + Vector3.right * x + Vector3.up * y;
                //print("World Point: " + worldPoint);
                bool walkable = tiles[x, y].isWalkable;
                grid[x, y] = new Node(walkable, worldPoint, x, y, tiles[x, y].movementCost);
            }
        }
    }

    public Node NodeFromWorldPoint(Vector3 worldPos)
    {
        Vector3 worldBottomLeft = transform.position - Vector3.right * mapSizeX / 2 - Vector3.up * mapSizeY / 2;
        Vector3 worldPoint = worldBottomLeft + Vector3.right * worldPos.x + Vector3.up * worldPos.y;

        int x = Mathf.RoundToInt(worldPoint.x);
        int y = Mathf.RoundToInt(worldPoint.y);

        return grid[x, y];
    }

    public TileData TileFromWorldPoint(Vector3 worldPos)
    {
        Vector3 worldBottomLeft = transform.position - Vector3.right * mapSizeX / 2 - Vector3.up * mapSizeY / 2;
        Vector3 worldPoint = worldBottomLeft + Vector3.right * worldPos.x + Vector3.up * worldPos.y;

        int x = Mathf.RoundToInt(worldPoint.x);
        int y = Mathf.RoundToInt(worldPoint.y);


        return tiles[x, y];
    }

    public void SwitchTileWalkability (int x, int y, bool trueIfWalkable)
    {
        tiles[x, y].isWalkable = trueIfWalkable;
        grid[x, y].isWalkable = trueIfWalkable;
    }

    public List<Node> GetNeighbors(Node node)
    {
        // Where is this node in the grid?
        List<Node> neighbors = new List<Node>();

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                // skip the center node
                if (x == 0 && y == 0)
                {
                    continue;
                }

                int checkX = node.gridX + x;
                int checkY = node.gridY + y;

                // Check that this is within the grid
                if (checkX >= 0 && checkX < mapSizeX && checkY >= 0 && checkY < mapSizeY)
                {
                    neighbors.Add(grid[checkX, checkY]);
                }
            }
        }

        return neighbors;
    }

    //void InitPathFindingGraph(){
    //	// Init array
    //	graph = new Node[mapSizeX, mapSizeY];
    //	// Init Nodes as new Nodes
    //	for (int x =0; x < mapSizeX; x++) {
    //		for (int y =0; y < mapSizeY; y++) {
    //			graph[x,y] = new Node();

    //			graph[x,y].x = x;
    //			graph[x,y].y = y;

    //			graph[x,y].nodeID = x;
    //		}
    //	}
    //	// Populate the graph by calculating the neigbors of each node
    //	for (int x =0; x < mapSizeX; x++) {
    //		for (int y =0; y < mapSizeY; y++){

    //			// grab the neighbors on this node
    //			if (x > 0){
    //				// to our left
    //				graph[x,y].neighbors.Add(graph[x-1, y]);
    //				if (y > 0){
    //					graph[x,y].neighbors.Add(graph[x-1, y - 1]); // left down
    //				}
    //				if (y < mapSizeY - 1){
    //					graph[x,y].neighbors.Add(graph[x-1, y +1]); // left up
    //				}

    //			}
    //			if (x < mapSizeX - 1){
    //				// to our right
    //				graph[x,y].neighbors.Add(graph[x+1, y]);
    //				if (y > 0){
    //					graph[x,y].neighbors.Add(graph[x+1, y - 1]); // left down
    //				}
    //				if (y < mapSizeY - 1){
    //					graph[x,y].neighbors.Add(graph[x+1, y +1]); // left up
    //				}

    //			}
    //			if (y > 0){
    //				// below
    //				graph[x,y].neighbors.Add(graph[x, y - 1]);
    //			}
    //			if (y < mapSizeY - 1){
    //				// above
    //				graph[x,y].neighbors.Add(graph[x, y + 1]);
    //			}
    //		}
    //	}
    //}
    //TODO: Properly check for the tile map coords against world coords, right now this only works b/c map is set to 0,0 in world space
    public Vector2 TileCoordToWorldCoord(int x, int y){

		return new Vector2 ((float)x, (float)y);

	}

	/// <summary>
	/// Checks the tile move cost,
	/// adds extra cost to DIAGONAL movement.
	/// </summary>
	/// <returns>The tile move cost.</returns>
	/// <param name="sourceX">Source x.</param>
	/// <param name="sourceY">Source y.</param>
	/// <param name="targetX">Target x.</param>
	/// <param name="targetY">Target y.</param>
	float CheckTileMoveCost(int sourceX, int sourceY, int targetX, int targetY){
		float moveCost = (float)tiles[targetX,targetY].movementCost;
		if (sourceX != targetX && sourceY != targetY) {
			// MOVING Diagonal! change cost so it's more expensive
			moveCost += 0.001f;
		}
		return moveCost;
		// If there is no difference in cost between straight and diagonal, it moves weird
	}

	public bool UnitCanEnterTile(int x, int y){
		return tiles[x,y].isWalkable;
	}

	/// <summary>
	/// Generates the walk path for both player-controlled units and enemy AI.
	/// </summary>
	/// <param name="x">The x coordinate.</param>
	/// <param name="y">The y coordinate.</param>
	/// <param name="trueIfPlayerUnit">If set to <c>true</c> 
	/// Only true if player unit so method can be shared while X and Y coordinates
	/// are set properly.</param>
	/// <param name="enemyX">Enemy x.
	/// Use when enemy, else default is 0</param>
	/// <param name="enemyY">Enemy y.
	/// Same as x</param>
//	public void GenerateWalkPath(int x, int y, bool trueIfPlayerUnit, int unitX = 0, int unitY = 0){
//		int unitPosX;
//		int unitPosY;
//		// Clear out selected unit's old path
//		if (trueIfPlayerUnit) {
//			unitOnPath.GetComponent<SelectedUnit_MoveHandler> ().currentPath = null;
//			unitPosX = unitX;
//			unitPosY = unitY;
//		} else {
//			pathForEnemy = null;
//			unitPosX = unitX;
//			unitPosY = unitY;
////			unitOnPath.GetComponent<Enemy_MoveHandler> ().currentPath = null;
////			unitPosX = unitOnPath.GetComponent<Enemy_MoveHandler> ().posX;
////			unitPosY = unitOnPath.GetComponent<Enemy_MoveHandler> ().posY;
//		}

//		// Every Node that hasn't been checked yet
//		List<Node> unvisited = new List<Node> ();
		
//		Dictionary<Node, float> dist = new Dictionary<Node, float> ();
//		Dictionary<Node, Node> prev = new Dictionary<Node, Node> ();

//		Node source = graph [
//		                     unitPosX, 
//		                     unitPosY
//		                     ];
//		Node target = graph [
//		                     x, 
//		                     y
//		                     ];	

//		foreach (Node v in graph) {
//			if(v != source){
//				dist[v] = Mathf.Infinity;
//				prev[v] = null;
//			}else{
//				dist [source] = 0; // distance
//				prev [source] = null;
//			}
//			unvisited.Add(v); 
//		}

//		while (unvisited.Count > 0) {
//			Node u = null;
//			// I need to sort the list of unvisited every time a v is added
//			// this loop gets me the unvisited node with shortest distance
//			foreach(Node possibleU in unvisited){
//				if (u== null || dist[possibleU] < dist[u]){
//					u = possibleU;
//				}
//			}

//			if (u == target){
//				break;			// Here we found Target, EXIT while loop
//			}

//			unvisited.Remove(u);

//			foreach(Node v in u.neighbors){
////				float alt = dist[u] + u.DistanceTo(v); // distance to move
////				float alt = dist[u] + CheckIfTileisWalkable(v.x, v.y); // distance to move
//				float alt = dist[u] + CheckTileMoveCost(u.x, u.y, v.x, v.y); 
//				if(alt < dist[v]){
//					dist[v] = alt;
//					prev[v] = u;
//				}
//			}
//		}

//		// Check if there is no route to our target
//		if (prev [target] == null) {
//			// no route from target to source!
//			return;
//		}

//		// Here we have a route from source to target
//		List<Node> currentPath = new List<Node> ();

//		Node curr = target;

//		while (curr != null) {
//			currentPath.Add(curr);
//			curr = prev[curr];
//		}

//		// This route is right now from our target to our source, so we need to invert it to move the Unit
//		currentPath.Reverse ();

//		// Give the unit it's NEW PATH!
//		if (trueIfPlayerUnit) {
//			unitOnPath.GetComponent<SelectedUnit_MoveHandler> ().currentPath = currentPath;
//		} else {
//			pathForEnemy = currentPath;
////			unitOnPath.GetComponent<Enemy_MoveHandler> ().currentPath = currentPath;
//		}

//	}// end Movetarget


}
