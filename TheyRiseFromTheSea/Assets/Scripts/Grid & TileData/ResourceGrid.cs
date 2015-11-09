using UnityEngine;
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

//	public Player_SpawnHandler playerSpawnHandler;
	public GameObject playerCapital; 
	public GameObject playerCapitalFab;// to spawn at the start of a new level
	public int capitalSpawnX, capitalSpawnY;

	public Building_UIHandler buildingUIHandler;
		
	public ObjectPool objPool;

	// BUILDING COSTS: // the array's [0] value is ORE Cost, [1] value is ENERGY Cost
	public int[] extractorCost, machineGunCost, seaWitchCost, harpoonHCost, cannonCost, sFarmCost; 
	public int[] storageCost, sDesaltCost, sniperCost, nutriCost, generatorCost;

	public Player_ResourceManager playerResources;


	// PATHFINDING VARS:
	Node[,] graph;

	// FOR DEBUG PURPOSES (Finding coordinates and displaying them on screen)
	public Text coordinatesText, tiletypeText;


	// NOTE: the total represents the total of leader rock tiles, not really the total
	[Range (5, 55)]
	public int totalRocksOnMap;
	[Range (6, 28)]
	public int totalMineralsOnMap;

	// Center of the map
	public int centerPosX = 0;
	public int centerPosY = 0;

	// Access to the Player Hero
	public GameObject Hero;

	// Access to Master State manager to call when Terraformer is blown up
	public MasterState_Manager master_state;

	public int totalTilesThatAreWater;

	Map_Generator map_generator;

	[HideInInspector]
	public Resource_Sprite_Handler res_sprite_handler;

	public bool terraformer_built;

	public GameObject enemy_waveSpawner;

	GameMaster game_master;

	bool islandVisible;

    [HideInInspector]
	public Transform cameraHolder;

    public List<Vector2> waterTilePositions;
    public Vector2[] waterTilesArray;

    public List<Vector2> emptyTilePositions;
    public Vector2[] emptyTilesArray;

	void Awake()
	{
		if (!master_state)
			master_state = GameObject.FindGameObjectWithTag ("GameController").GetComponent<MasterState_Manager> ();

		if (!map_generator) {
			map_generator = GetComponent<Map_Generator> ();
			mapSizeX = map_generator.width;
			mapSizeY = map_generator.height;
		} else {
			mapSizeX = map_generator.width;
			mapSizeY = map_generator.height;
		}

		if (!res_sprite_handler)
			res_sprite_handler = GetComponent<Resource_Sprite_Handler> ();

		if (!enemy_waveSpawner)
			enemy_waveSpawner = GameObject.FindGameObjectWithTag ("Spawner");

		game_master = GameObject.FindGameObjectWithTag ("GM").GetComponent<GameMaster> ();
		game_master.resourceGrid = this;

		cameraHolder = GameObject.FindGameObjectWithTag ("Camera").transform;

	}

	void Start () 
	{

		// In case Player Resource Manager is null
		if (playerResources == null)
			playerResources = GameObject.FindGameObjectWithTag ("Capital").GetComponent<Player_ResourceManager> ();

//		InitCapitalAndMinerals ();

		// Initialize Tile Data array with this map size
//		tiles = new TileData[mapSizeX, mapSizeY];

		// Initialize spawned Tiles array, all values will be set to null until tiles are spawned
//		spawnedTiles = new GameObject[mapSizeX, mapSizeY];

		// Initialize the Grid, filling tiles positions with Capital, Rocks, and Water
//		InitGrid ();

		// This creates the Initial Pathfinding graph taking into account unwakable tiles already spawned (ex. Rock, Water, Capital)
		InitPathFindingGraph ();

//		BuildTheCapital ();
	}

	// DEBUG NOTE: using this Update method to find map coordinates to match them with map graphics
	void Update()
	{
		if (Input.GetMouseButtonDown (1)) {
			Vector3 m = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			int mX = Mathf.RoundToInt(m.x);
			int mY = Mathf.RoundToInt(m.y);

			if (mX <= mapSizeX && mY <= mapSizeY && mX > 0 && mY > 0){
				if (coordinatesText != null && tiletypeText != null){
					coordinatesText.text = "X: " + mX + " Y: " + mY;
					tiletypeText.text = "Type: " + tiles[mX, mY].tileType;
				}
			}
			print ("map coord:" + TileCoordToWorldCoord(mX, mY).ToString() + "world coord: " + m);
		}

		if (!terraformer_built && islandVisible)
			BuildTheCapital();

		if (Input.GetMouseButtonDown (0)) {
			if (!islandVisible) {
				Vector3 camHolderPos = new Vector3(cameraHolder.position.x,cameraHolder.position.y, -10F);
				MoveTheIslandMapToFront (camHolderPos);
			}
		}
	}

	void BuildTheCapital()
	{
		Vector3 m = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		int mX = Mathf.RoundToInt(m.x);
		int mY = Mathf.RoundToInt(m.y);

		if (mX < mapSizeX && mY < mapSizeY && mX > 0 && mY > 0){
			if (Input.GetMouseButtonDown(0)){
				InitCapital(mX, mY);
			}
		}
	}

	void MoveTheIslandMapToFront(Vector3 camHolderPos)
	{

		islandVisible = true;
		//TODO: Introduction to each level, the island RISES from the sea as the terraformer activates

//		if (cameraHolder) {
//			cameraHolder.position = Vector3.Lerp(cameraHolder.position, camHolderPos, 66 * Time.deltaTime);
//			islandVisible = true;
//		}
	}

	public class OrePatch
	{
		public int leadPositionX;
		public int leadPositionY;
		int pDensity;
		public int density {get{return pDensity;} set{pDensity = Mathf.Clamp(value, 1, 5);}}
		public int totalInPatch;
		public OreTile[] neighborOreTiles;


		public OrePatch(int xPos, int yPos, int _density)
		{
			leadPositionX = xPos;
			leadPositionY = yPos;
			density = _density;
			totalInPatch = _density;
		}

		public void SetFormation()
		{
			/* formation offset,  indicating how far the neighbor ore tile is from its lead tile.
			 * Depending on their density they will have a minor offset(more density) or major offset (less density) */
			int minorOffset = Random.Range (3, 8);
			int majorOffset = Random.Range (9, 15);

			switch (density) {
			case 1:
				// This patch only has one rock or mineral
				break;
			case 2:
				// This patch contains two, so neighbor ore array = 1
				neighborOreTiles = new OreTile[1];
				// This is the position the neighbor ore can be placed on
				neighborOreTiles[0] = new OreTile(leadPositionX + majorOffset, leadPositionY - majorOffset);
				break;
			case 3:
				neighborOreTiles = new OreTile[2];
				neighborOreTiles[0] = new OreTile(leadPositionX + minorOffset, leadPositionY - majorOffset);
				neighborOreTiles[1] = new OreTile(leadPositionX - minorOffset, leadPositionY - majorOffset);
				break;
			case 4:
				neighborOreTiles = new OreTile[3];
				neighborOreTiles[0] = new OreTile(leadPositionX, leadPositionY + minorOffset);
				neighborOreTiles[1] = new OreTile(leadPositionX, leadPositionY - minorOffset);
				neighborOreTiles[2] = new OreTile(leadPositionX - minorOffset , leadPositionY);
				break;
			case 5:
				neighborOreTiles = new OreTile[4];
				neighborOreTiles[0] = new OreTile(leadPositionX -minorOffset, leadPositionY);
				neighborOreTiles[1] = new OreTile(leadPositionX + minorOffset, leadPositionY);
				neighborOreTiles[2] = new OreTile(leadPositionX + minorOffset , leadPositionY + minorOffset);
				neighborOreTiles[3] = new OreTile(leadPositionX , leadPositionY + minorOffset);
				break;
			default:
				neighborOreTiles = new OreTile[1];
				neighborOreTiles[0] = new OreTile(leadPositionX + minorOffset, leadPositionY - minorOffset);
				break;
				
			}
		}

		public class OreTile
		{
			public int posX;
			public int posY;

			public OreTile(int x, int y)
			{
				posX = x;
				posY = y;
			}
		}
	}

	void InitCapital(int _terraPosX, int _terraPosY)
	{
		// SPAWN PLAYER CAPITAL HERE:
		tiles [_terraPosX, _terraPosY] = new TileData("Capital", TileData.Types.capital, 0, 10000, 200, 5,0,0,0,0);
		SpawnDiscoverTile(tiles [_terraPosX, _terraPosY].tileName, new Vector3(_terraPosX, _terraPosY, 0.0f),tiles [_terraPosX, _terraPosY].tileType);

        // Spawn Player / Hero 1 tile down from the terraformer
        Hero =  game_master.SpawnThePlayer(_terraPosX, _terraPosY - 1);

		// TODO: replace capitalPos completely with terraformer pos
		capitalSpawnX = _terraPosX;
		capitalSpawnY = _terraPosY;

		terraformer_built = true;

       
		// Turn on the Enemy Wave spawner
		enemy_waveSpawner.SetActive (true);

	}

	public void InitializeRockandMinerals()
	{
		string rockTypeName = "rock";
		string mineralTypeName = "mineral";


		centerPosX = mapSizeX / 2;
		centerPosY = mapSizeY / 2;

		int magicPosX = 0;
		int magicPosY = 0;

		//Find a position to start creating rock formations
		for (int x = centerPosX - (centerPosX/4); x < centerPosX + (centerPosX/4); x++) {
			for (int y = centerPosY - (centerPosY/4); y < centerPosX + (centerPosY/4); y++) {
				if (CheckIsInMapBounds(x, y)){
                    if (!CheckForWater(x, y))
                    {
                        if (tiles[x, y].tileType == TileData.Types.empty)
                        {
                            magicPosX = x;
                            magicPosY = y;
                            break;
                        }
                    }
				

				}
			}
		} 

		int lastRockPosX = 0;
		int lastRockPosY = 0;

		// Loop through all the rocks
		for (int i = 0; i < totalRocksOnMap; i++) {
//			
			if (i== 0){
				SetNewOrePatch(magicPosX, magicPosY, rockTypeName);
			}else{
				// pick up/left, up/right, down/left, down/right
				int pick = Random.Range(1,5);
				if (pick == 1){
					SetNewOrePatch(magicPosX - 2, magicPosY + 2, rockTypeName);
				}else if (pick == 2){
					SetNewOrePatch(magicPosX + 2, magicPosY + 2, rockTypeName);

				}else if (pick == 3){
					SetNewOrePatch(magicPosX + 2, magicPosY - 2, rockTypeName);
				}else if (pick == 4){
					SetNewOrePatch(magicPosX - 2, magicPosY - 2, rockTypeName);
				}else {
					SetNewOrePatch(magicPosX + 3, magicPosY - 2, rockTypeName);

				}


			}
		}

		lastRockPosX = magicPosX + 3;
		lastRockPosY = magicPosY - 2;

		// Loop through all the minerals
		for (int j = 0; j < totalMineralsOnMap; j++) {
			if (j== 0){
				SetNewOrePatch(lastRockPosX, lastRockPosY, mineralTypeName);
			}else{
				// pick up/left, up/right, down/left, down/right
				int pick = Random.Range(1,5);
				if (pick == 1){
					SetNewOrePatch(lastRockPosX - 2, lastRockPosY + 2, mineralTypeName);
				}else if (pick == 2){
					SetNewOrePatch(lastRockPosX + 2, lastRockPosY + 2, mineralTypeName);
					
				}else if (pick == 3){
					SetNewOrePatch(lastRockPosX + 2, lastRockPosY - 2, mineralTypeName);
				}else if (pick == 4){
					SetNewOrePatch(lastRockPosX - 2, lastRockPosY - 2, mineralTypeName);
				}else {
					SetNewOrePatch(lastRockPosX + 3, lastRockPosY - 2, mineralTypeName);
					
				}
				
				
			}
		}
	}

	bool CheckForWater (int x, int y)
	{
        int waterRange = 2;
		bool hasWater = false;
		// Make sure every position is still within map bounds
		for (int bottomX = x- waterRange; bottomX < x + waterRange; bottomX++){
			for (int bottomY = y- waterRange; bottomY < y + waterRange; bottomY++){
				if (CheckIsInMapBounds(bottomX, bottomY)){
					// It IS in map bounds, now check if it's water
					if (tiles[bottomX, bottomY].tileType == TileData.Types.water){
						hasWater = true;
						break;
					}
				}else{
					// It's NOT in map bounds so just return true because it's most likely water
					hasWater = true;
					break;
				}
			}

		}

		return hasWater;
	}

	public bool CheckIsInMapBounds(int x, int y)
	{
		if (x < mapSizeX && y < mapSizeY && x > 0 && y > 0) {
			return true;
		} else {
			return false;
		}
	}

	void SetNewOrePatch(int leadX, int leadY, string typeName)
	{
		// first make sure that there is NO WATER tiles around this lead tile
		if (!CheckForWater (leadX, leadY)) {
			Debug.Log ("Found tile with no water around it!");
			// Calculate the distance from this lead tile to the center of the map,
			// the closer to the center the DENSER a patch of ore will be
			float distance = Vector2.Distance (new Vector2 (leadX, leadY), new Vector2 (centerPosX, centerPosY)); 
			int density = 0;
			if (distance >= 20) {
				// pick a 1 or 2 density
				int pick = Random.Range (0, 3);
				density = pick;
			} else if (distance < 20 && distance > 8) {
				// pick between 4 or 5 density
				int pick = Random.Range (3, 5);
				density = pick;
			} else {
				density = 5;
			}
			OrePatch patch = new OrePatch (leadX, leadY, density);
			patch.SetFormation ();
			PlaceOrePatch(patch, typeName);
		} else {
			Debug.Log("Could not place ore patch because " + leadX + "," + leadY+ " is a shore!");
		}
	}

	void PlaceOrePatch(OrePatch _patch, string typeName)
	{
		// place the lead ore if there is no rock already on that tile
		if (tiles [_patch.leadPositionX, _patch.leadPositionY].tileType == TileData.Types.empty) {

			if (typeName == "rock"){
				tiles [_patch.leadPositionX, _patch.leadPositionY] = new TileData (TileData.Types.rock, 6000, 10000);
				SpawnDiscoverTile (typeName, new Vector3 (_patch.leadPositionX, _patch.leadPositionY, 0.0F), TileData.Types.rock);

			}else if (typeName == "mineral"){
				tiles [_patch.leadPositionX, _patch.leadPositionY] = new TileData (TileData.Types.mineral, 3000, 10000);
				SpawnDiscoverTile (typeName, new Vector3 (_patch.leadPositionX, _patch.leadPositionY, 0.0F), TileData.Types.mineral);

			}

		}
		if (_patch.neighborOreTiles != null && _patch.neighborOreTiles.Length > 0) {
			// place the neighbors in their positions
			for (int i = 0; i < _patch.neighborOreTiles.Length; i++){
				if (CheckIsInMapBounds(_patch.neighborOreTiles[i].posX, _patch.neighborOreTiles[i].posY)){
					if (!CheckForWater(_patch.neighborOreTiles[i].posX, _patch.neighborOreTiles[i].posY)){
						// Place this rock / mineral if there isn't a rock / mineral already on this tile

						if (tiles[_patch.neighborOreTiles[i].posX, _patch.neighborOreTiles[i].posY].tileType == TileData.Types.empty){
							if (typeName == "rock"){
								tiles [_patch.neighborOreTiles[i].posX, _patch.neighborOreTiles[i].posY] = new TileData (TileData.Types.rock, 6000, 10000);
								
								SpawnDiscoverTile ("rock", new Vector3 (_patch.neighborOreTiles[i].posX,
								                                        _patch.neighborOreTiles[i].posY, 0.0F), TileData.Types.rock);
							}else if (typeName == "mineral"){
								tiles  [_patch.neighborOreTiles[i].posX, _patch.neighborOreTiles[i].posY] = new TileData (TileData.Types.mineral, 3000, 10000);
								
								SpawnDiscoverTile ("mineral", new Vector3 (_patch.neighborOreTiles[i].posX,
								                                        _patch.neighborOreTiles[i].posY, 0.0F), TileData.Types.mineral);
								
							}
						}
					}
				}
			}
		}

	}

	

	/// <summary>
	/// Damages the tile.
	/// </summary>
	/// <param name="x">The x coordinate.</param>
	/// <param name="y">The y coordinate.</param>
	/// <param name="damage">Damage.</param>
	public void DamageTile(int x, int y, float damage)
	{
		// make sure there IS a tile there
		if (spawnedTiles [x, y] != null) {

			// If it has 0 or less HP left, kill tile
			if (tiles [x, y].hp <= 0) {
				if (tiles[x,y].tileType == TileData.Types.capital){
					// call mission failed
					master_state.mState = MasterState_Manager.MasterState.MISSION_FAILED;
		
				}else{
					SwapTileType (x, y, TileData.Types.empty);	// to KILL TILE I just swap it ;)

				}
			}else{
				tiles [x, y].hp = tiles [x, y].hp - damage;
				Debug.Log("Tile: " + tiles[x,y].tileType.ToString() + " damaged for " + damage);
				Debug.Log("It has " + tiles [x, y].hp + " left!");
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
	public GameObject GetTileGameObj(int x, int y)
	{
		if (spawnedTiles [x, y] != null)
			return spawnedTiles [x, y];
		else
			return null;
	}


   

	/// <summary>
	/// Swaps the type of the tile.
	/// </summary>
	/// <param name="x">The x coordinate.</param>
	/// <param name="y">The y coordinate.</param>
	/// <param name="newType">New type.</param>
	public void SwapTileType(int x, int y, TileData.Types newType)
	{

		// MAKE SURE THIS IS NOT A SPAWNED TILE ALREADY!!! 
		// So we don't change the grid tile data where we don't want to!
		if (spawnedTiles [x, y] == null) {
			// Swap the old type to new type
			switch (newType) {
			case TileData.Types.extractor:
				tiles [x, y] = new TileData ("Extractor",newType, 0, 10000, 5, 5, 0, 0, extractorCost[1], extractorCost[0]);
				break;
			case TileData.Types.machine_gun:
				tiles [x, y] = new TileData ("Machine Gun", newType, 0, 10000, 30, 5, 5, 0, machineGunCost[1], machineGunCost[0]);
				break;
			case TileData.Types.cannons:
				tiles [x, y] = new TileData ("Cannons", newType, 0, 10000, 30, 5, 3, 0, seaWitchCost[1], seaWitchCost[0]);
				break;
			case TileData.Types.harpoonHall:
				tiles [x, y] = new TileData ("Harpooner's Hall", newType, 0, 10000, 50, 6, 0, 0, harpoonHCost[1], harpoonHCost[0]);
				break;
			case TileData.Types.farm_s:
				tiles [x, y] = new TileData ("Seaweed Farm", newType, 0, 10000, 25, 1, 0, 0, sFarmCost[1], sFarmCost[0]);
				break;
			case TileData.Types.storage:
				tiles [x, y] = new TileData ("Storage", newType, 0, 10000, 35, 2, 0, 0, storageCost[1], storageCost[0]);
				break;
			case TileData.Types.desalt_s:
				tiles [x, y] = new TileData ("Desalination Pump", newType, 0, 10000, 15, 1, 0, 0, sDesaltCost[1], sDesaltCost[0]);
				break;
			case TileData.Types.sniper:
				tiles [x, y] = new TileData ("Sniper Gun", newType, 0, 10000, 0, 0, 0, 0, sniperCost[1], sniperCost[0]);
				break;
			case TileData.Types.seaWitch:
				tiles [x, y] = new TileData ("Sea-Witch Crag", newType, 0, 10000, 0, 0, 0, 0, seaWitchCost[1], seaWitchCost[0]);
				break;
			case TileData.Types.nutrient:
				tiles [x, y] = new TileData ("Nutrient Generator", newType, 0, 10000, 0, 0, 0, 0, nutriCost[1], nutriCost[0]);
				break;
			case TileData.Types.generator:
				tiles [x, y] = new TileData ("Energy Generator", newType, 0, 10000, 0, 0, 0, 0, generatorCost[1], generatorCost[0]);
				break;
			case TileData.Types.building:
				tiles [x, y] = new TileData (newType, 0, 10000);
				break;
			default:
				print ("No tile changed.");
				break;
			}

			// Discover the tile to display it
			DiscoverTile (x, y, true);

			// IF tile is a Building with an ENERGY COST, apply it to resources
			if (tiles[x,y].energyCost > 0){
				playerResources.totalEnergyCost = playerResources.totalEnergyCost + tiles[x,y].energyCost;
			}

		} else { 

			// if we are swapping an already spawned tile we are MOST LIKELY turning it into an empty tile
			// BUT if this was a building that has an ENERGY cost that must be reflected in Player resources 
			//	by subtracting from the total ENERGY cost
			if (tiles[x,y].energyCost > 0){
				playerResources.totalEnergyCost = playerResources.totalEnergyCost - tiles[x,y].energyCost;
			}

			// ALSO if it's a Farm we need to subtract its FOOD production and its WATER consumed
			if (playerResources.foodProducedPerDay > 0){

				if (tiles[x,y].tileType == TileData.Types.farm_s || tiles[x,y].tileType == TileData.Types.nutrient){

					FoodProduction_Manager foodM = spawnedTiles [x, y].GetComponent<FoodProduction_Manager>();
					playerResources.CalculateFoodProduction(foodM.foodProduced, foodM.productionRate, foodM.waterConsumed, true);

				}
			}

			// AND if it's a STORAGE we need to subtract all the ORE and WATER from the resources
			if (tiles[x,y].tileType == TileData.Types.storage){

				Storage storage = spawnedTiles[x,y].GetComponent<Storage>();
//				
				// remove the storage building from the list
				playerResources.RemoveStorageBuilding(storage);
			}

			// If it's an EXTRACTOR also need to subtract from Ore Produced
			if (tiles[x,y].tileType == TileData.Types.extractor){

				Extractor extra = spawnedTiles [x, y].GetComponent<Extractor>();

				playerResources.CalculateOreProduction(extra.extractAmmnt, extra.extractRate, true);
			}

			// Same thing for a WATER PUMP
			if (tiles[x,y].tileType == TileData.Types.desalt_s || tiles[x,y].tileType == TileData.Types.desalt_m 
			    || tiles[x,y].tileType == TileData.Types.desalt_l){

				DeSalt_Plant pump = spawnedTiles [x, y].GetComponent<DeSalt_Plant>();

				playerResources.CalculateWaterProduction(pump.waterPumped, pump.pumpRate, true);
			}

			// If it's a ENERGY GENERATOR we have to subtract Energy //TODO: Add an energy produced per day panel
			if (tiles[x,y].tileType == TileData.Types.generator){

				Energy_Generator gen = spawnedTiles [x,y].GetComponent<Energy_Generator>();

				playerResources.ChangeResource("Energy", -gen.energyUnitsGenerated);
			}


			/* NANO BOTS Test !!! 
			// RETURN 30% OF THE ORE COST TO THE RESOURCES
			float calc = (float)tiles[x,y].oreCost * 0.3f;
			playerResources.ore = playerResources.ore + (int)calc;
			*/
			// Return bots... THIS WILL NEED TO EQUAL A NANOBOTS COST LATER, JUST USING 10
			NanoBuilding_Handler nanoBuilder = Hero.GetComponent<NanoBuilding_Handler>();
			nanoBuilder.nanoBots += 10;

			// Pool the spawned tile
//			Destroy(spawnedTiles[x,y].gameObject);
			objPool.PoolObject(spawnedTiles[x,y].gameObject);

			// Make it null as a spawnedTiles
			spawnedTiles[x,y] = null;

			// Tell the tiles array what this new tile is
			tiles[x,y] = new TileData(newType, 0,1);
		}
	}

	public void DiscoverTile(int x, int y, bool trueIfSwapping)
	{
		if (spawnedTiles [x, y] == null) { // if it's null it means it hasn't been spawned
			//Dont Spawn a tile if the type is Empty
			// the space will still be walkable because it will still be mapped on the Node Graph
			
			if (tiles [x, y].tileType != TileData.Types.empty) {
				SpawnDiscoverTile (tiles [x, y].tileName, new Vector3 (x, y, 0.0f), tiles [x, y].tileType);
				// set it so it knows it has been spawned
				tiles [x, y].hasBeenSpawned = true;
			}
		} else { // it HAS been spawned
			if (trueIfSwapping){
				// since we know this tile has already been spawned we need to destroy the old one,
				// before adding the new one
				Destroy (spawnedTiles [x, y].gameObject);
				SpawnDiscoverTile (tiles [x, y].tileName, new Vector3 (x, y, 0.0f), tiles [x, y].tileType);
			}
		}
	}



   void SpawnDiscoverTile(string tileName,Vector3 position, TileData.Types type){
		// spawn the half tile from the pool
		GameObject discoverTile = objPool.GetObjectForType ("Half_Tile", false, Vector3.zero);
		if (discoverTile != null) {
			discoverTile.transform.position = position;
			DiscoverTile dTile = discoverTile.GetComponent<DiscoverTile> ();
			dTile.objPool = objPool;
			dTile.master_state = master_state;
			dTile.r_sprite_handler = res_sprite_handler;

			dTile.TileToDiscover(newTileName: tileName, mapPosX: (int) position.x , mapPosY: (int)position.y, tileHolder: tileHolder, grid: this,  tileType: type, playerCapital: playerCapital);
	
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

	void InitPathFindingGraph(){
		// Init array
		graph = new Node[mapSizeX, mapSizeY];
		// Init Nodes as new Nodes
		for (int x =0; x < mapSizeX; x++) {
			for (int y =0; y < mapSizeY; y++) {
				graph[x,y] = new Node();
				
				graph[x,y].x = x;
				graph[x,y].y = y;
				
				graph[x,y].nodeID = x;
			}
		}
		// Populate the graph by calculating the neigbors of each node
		for (int x =0; x < mapSizeX; x++) {
			for (int y =0; y < mapSizeY; y++){

				// grab the neighbors on this node
				if (x > 0){
					// to our left
					graph[x,y].neighbors.Add(graph[x-1, y]);
					if (y > 0){
						graph[x,y].neighbors.Add(graph[x-1, y - 1]); // left down
					}
					if (y < mapSizeY - 1){
						graph[x,y].neighbors.Add(graph[x-1, y +1]); // left up
					}
						
				}
				if (x < mapSizeX - 1){
					// to our right
					graph[x,y].neighbors.Add(graph[x+1, y]);
					if (y > 0){
						graph[x,y].neighbors.Add(graph[x+1, y - 1]); // left down
					}
					if (y < mapSizeY - 1){
						graph[x,y].neighbors.Add(graph[x+1, y +1]); // left up
					}

				}
				if (y > 0){
					// below
					graph[x,y].neighbors.Add(graph[x, y - 1]);
				}
				if (y < mapSizeY - 1){
					// above
					graph[x,y].neighbors.Add(graph[x, y + 1]);
				}
			}
		}
	}
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
	public void GenerateWalkPath(int x, int y, bool trueIfPlayerUnit, int unitX = 0, int unitY = 0){
		int unitPosX;
		int unitPosY;
		// Clear out selected unit's old path
		if (trueIfPlayerUnit) {
			unitOnPath.GetComponent<SelectedUnit_MoveHandler> ().currentPath = null;
			unitPosX = unitX;
			unitPosY = unitY;
		} else {
			pathForEnemy = null;
			unitPosX = unitX;
			unitPosY = unitY;
//			unitOnPath.GetComponent<Enemy_MoveHandler> ().currentPath = null;
//			unitPosX = unitOnPath.GetComponent<Enemy_MoveHandler> ().posX;
//			unitPosY = unitOnPath.GetComponent<Enemy_MoveHandler> ().posY;
		}

		// Every Node that hasn't been checked yet
		List<Node> unvisited = new List<Node> ();
		
		Dictionary<Node, float> dist = new Dictionary<Node, float> ();
		Dictionary<Node, Node> prev = new Dictionary<Node, Node> ();

		Node source = graph [
		                     unitPosX, 
		                     unitPosY
		                     ];
		Node target = graph [
		                     x, 
		                     y
		                     ];	

		foreach (Node v in graph) {
			if(v != source){
				dist[v] = Mathf.Infinity;
				prev[v] = null;
			}else{
				dist [source] = 0; // distance
				prev [source] = null;
			}
			unvisited.Add(v); 
		}

		while (unvisited.Count > 0) {
			Node u = null;
			// I need to sort the list of unvisited every time a v is added
			// this loop gets me the unvisited node with shortest distance
			foreach(Node possibleU in unvisited){
				if (u== null || dist[possibleU] < dist[u]){
					u = possibleU;
				}
			}

			if (u == target){
				break;			// Here we found Target, EXIT while loop
			}

			unvisited.Remove(u);

			foreach(Node v in u.neighbors){
//				float alt = dist[u] + u.DistanceTo(v); // distance to move
//				float alt = dist[u] + CheckIfTileisWalkable(v.x, v.y); // distance to move
				float alt = dist[u] + CheckTileMoveCost(u.x, u.y, v.x, v.y); 
				if(alt < dist[v]){
					dist[v] = alt;
					prev[v] = u;
				}
			}
		}

		// Check if there is no route to our target
		if (prev [target] == null) {
			// no route from target to source!
			return;
		}

		// Here we have a route from source to target
		List<Node> currentPath = new List<Node> ();

		Node curr = target;

		while (curr != null) {
			currentPath.Add(curr);
			curr = prev[curr];
		}

		// This route is right now from our target to our source, so we need to invert it to move the Unit
		currentPath.Reverse ();

		// Give the unit it's NEW PATH!
		if (trueIfPlayerUnit) {
			unitOnPath.GetComponent<SelectedUnit_MoveHandler> ().currentPath = currentPath;
		} else {
			pathForEnemy = currentPath;
//			unitOnPath.GetComponent<Enemy_MoveHandler> ().currentPath = currentPath;
		}

	}// end Movetarget


}
