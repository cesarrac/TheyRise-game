﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class Player_ResourceManager : MonoBehaviour {

	public int maxHeroCount;
	private int _ore, _food, _credits, _water, _heroCount, _energy;
	public int ore { get { return _ore; } set { _ore = value; } }
	public int food { get { return _food; } set { _food = value; } }
	public int credits { get { return _credits; } set { _credits = value; } }
	public int water { get { return _water; } set { _water = value; } }
	public int heroCount { get { return _heroCount; } set { _heroCount = value; } }
	public int energy { get { return _energy; } set { _energy = value; } }
	
	public int startOre;
	public int startFood;
	public int startCredits;
	public int startWater;
	public int startEnergy;


	// NOTE: Replacing Food Cost with Energy cost:

	public int totalEnergyCost; // this is the energy cost for all the buildings. It gets added & subtracted by the Grid SwapTile method.
								// If totalEnergyCost exceeds energy stored, buildings begin to power down to compensate for the lack of power.

	private float powerCountDown = 10f; // in seconds

	public int totalFoodCost; // this is the cost of food every turn/cycle. It gets added & subtracted by the resource grid whenever it Swaps tiles.

	public float dayTime; // How long a DAY is. Leaving this public for now but it can be set by the level the player is on

	bool feeding;
	bool starveMode;
	int turnsStarving = 0;

	Building_UIHandler buildingUI;

	public ResourceGrid resourceGrid;

	List <GameObject> buildingsStarved = new List<GameObject>();

	GameObject lastBuildingPicked, currStarvedBuilding, currPowerDownBuilding;

	// Keep track of all Farms built and how much they produce per day
	public int farmCount{ get; private set; }
	public int foodProducedPerDay { get; private set; }

	// Keep track of all Storage buildings keeping ore and water, each storage adds itself when instantiated
	public List<Storage> storageBuildings = new List<Storage> ();

	// Keep track of all Water plants/pumps
	public int waterPumpCount{ get; private set; }
	private int _waterProducedPDay;
	public int waterProducedPerDay{ get {return _waterProducedPDay;} set {_waterProducedPDay = Mathf.Clamp(value, 0, value);}}
	public int totalWaterCost;

	// Keep track of all Extractors gathering Ore
	public int extractorCount{ get; private set; }
	private int _oreExtractedPDay;
	public int oreExtractedPerDay { get{return _oreExtractedPDay;} set{ _oreExtractedPDay = Mathf.Clamp(value, 0, value);}}


	// Hero logic: Player selects their chosen hero before the level begins. This Hero spawns on start then will spawn again, if dead,
	// every 4 seconds.
	[Header ("Optional Hero Pre-Fab: ")]
	public GameObject chosenHero;

	[SerializeField]
	private GameObject _curHero;


	public float timeToSpawn;

	private Vector3 spawnPosition;

	private IEnumerator _spwnCoRoutine;

	public ObjectPool objPool;

	private int _booster;
	public int booster {get {return _booster;} set{_booster = Mathf.Clamp(value, 0, 4 );}}

	private int _boostCalc = 0;

	void Start()
	{
		// Initialize credit booster at 0
		booster = 0;

		// Get Building UI from child 
		buildingUI = GetComponentInChildren<Building_UIHandler> ();


		feeding = true;

		// If Object Pool is null
		if (objPool == null) 
			objPool = GameObject.FindGameObjectWithTag ("Pool").GetComponent<ObjectPool> ();

		// NOTE: Need to change this Chosen Hero logic to the current one Hero mechanic
//		if (chosenHero != null) {
//
//			// get the spawn position
//			spawnPosition = new Vector3(resourceGrid.capitalSpawnX, resourceGrid.capitalSpawnY - 1.2f, 0.0f);
//
//			_spwnCoRoutine = WaitToSpawn(timeToSpawn);
//			StartCoroutine(_spwnCoRoutine);
//		}

		// TODO: Create a method that takes care of loading resources gathered from previous levels
		//Initialize the Starting Resources for this level
		InitStartingResources (startFood, startCredits, startOre);

	}

	public void InitStartingResources(int _food, int _credits, int _ore)
	{
		// These 3 are filled by the GM
		food = _food;
		credits = _credits;
		ore = _ore;

		energy = startEnergy;
		water = startWater;

	}


	// Spawn Counter for Chosen Hero logic:
//	IEnumerator WaitToSpawn(float time)
//	{
//		yield return new WaitForSeconds (time);
//		if (_curHero == null) {
//
//			_curHero = Instantiate(chosenHero, spawnPosition, Quaternion.identity) as GameObject;
//			_curHero.GetComponent<SelectedUnit_MoveHandler> ().resourceGrid = resourceGrid;
//			_curHero.GetComponentInChildren<Player_AttackHandler> ().objPool = objPool;
//			_curHero.GetComponentInChildren<Player_AttackHandler> ().resourceGrid = resourceGrid;
//		}else{
//			// get its hp
//			if (_curHero.GetComponentInChildren<Player_AttackHandler>() != null){
//				Player_AttackHandler handler = _curHero.GetComponentInChildren<Player_AttackHandler>();
//				if (handler.stats.curHP <= 1)
//					_curHero = null;
//			}else{
//				_curHero = null;
//			}
//		}
//
//	}

	/// <summary>
	/// Calculates the food production per day.
	/// Each farm takes X time to create Y food. A day takes T time.
	/// Production Rate = T divided by X ( how many times food is produced in a day).
	/// Total food produced per day = Y * Production Rate
	/// </summary>
	/// <param name="foodProduced">Food produced.</param>
	/// <param name="rateOfProd">Rate of prod.</param>
	/// <param name="trueIfSubtracting">Set to true if this Farm is being destroyed.</param>
	public void CalculateFoodProduction(int foodProduced, float rateOfProd, int waterNeeded, bool trueIfSubtracting){
	
		float productionRate = dayTime / rateOfProd;
//		Debug.Log ("Production Rate: " + productionRate);

		int perDay = Mathf.RoundToInt (foodProduced * productionRate);
//		Debug.Log ("PerDay: " + perDay);

		if (!trueIfSubtracting) {
			farmCount++;
			foodProducedPerDay = foodProducedPerDay + perDay;
			// add this farm's water needed stat to keep track of how much water we need
			totalWaterCost = totalWaterCost + waterNeeded;
		} else {
			farmCount--;
			foodProducedPerDay = foodProducedPerDay - perDay;

			totalWaterCost = totalWaterCost - waterNeeded;
		}
		Debug.Log ("Food Produced Per Day = " + foodProducedPerDay + " from " + farmCount + " Farms.");
	}

	public void CalculateWaterProduction(int waterPumped, float rateOfPump, bool trueIfSubtracting){
		float productionRate = dayTime / rateOfPump;
		
		int perDay = Mathf.RoundToInt (waterPumped * productionRate);
		
		if (!trueIfSubtracting) {
			waterPumpCount++;
			waterProducedPerDay = waterProducedPerDay + perDay;
		} else {
			waterPumpCount--;
			waterProducedPerDay = waterProducedPerDay - perDay;
		}
	}

	public void CalculateOreProduction(int oreExtracted, float rateOfExtract, bool trueIfSubtracting){
		float productionRate = dayTime / rateOfExtract;
		
		int perDay = Mathf.RoundToInt (oreExtracted * productionRate);
		
		if (!trueIfSubtracting) {
			extractorCount++;
			oreExtractedPerDay = oreExtractedPerDay + perDay;
		} else {
			extractorCount--;
			oreExtractedPerDay = oreExtractedPerDay - perDay;
		}
	}


	
	void Update(){
	
//		if (totalFoodCost > 0) {
//			if (feeding)
//				StartCoroutine(WaitToFeed());
//		}


	
		/*
		// TODO: STOP THIS FROM CALLING WHEN WE HAVE ENOUGH POWER!!
		// if the total Energy Cost exceeds the energy stored, Buildings will begin to power down
		if (totalEnergyCost > energy) {
			// Begin a countdown that when finished will turn off a building to compensate
			Power_Countdown(false);
		} else {
			if (buildingsStarved.Count > 0){
				Power_Countdown(true);
			}
		}

		if (Input.GetKeyDown (KeyCode.T))
			Debug.Log ("Energy cost: " + totalEnergyCost);
		*/
	}


	// NOTE: CHANGING ALL FOOD STARVING LOGIC TO ENERGY STARVING - BUILDINGS POWER DOWN WHEN THERE ISN'T ENOUGH ENERGY TO POWER ALL

	void Power_Countdown(bool powerOn)
	{
		if (powerCountDown <= 0) {
			// NOTE: Still using STARVED as the state building's that are powered down will be in
			if (!powerOn){
				// Choose building to power down & STARVE it of energy
				GetBuildingToPowerDown();
			}else{
				// OR Power Up
				PowerOnBuildings();
			}
			// reset countdown
			powerCountDown = 10f;
		} else {
			powerCountDown -= Time.deltaTime;
		}
	}

	void GetBuildingToPowerDown(){
		
		foreach (GameObject tile in resourceGrid.spawnedTiles) {

				if (tile != null){
					
					if (tile.CompareTag("Building")){
						
						if (tile != lastBuildingPicked){
							// turn off the building
							currPowerDownBuilding = tile;

							BuildingPowerControl(currPowerDownBuilding, true);
							break;
						}
					}
				}
		}
	}

	void BuildingPowerControl (GameObject building, bool powerDown)
	{
		// Make sure building is not null
		if (building != null) {

			// Finding what kind of building it is by name
			string buildingName = building.name;

			if (powerDown)
				// add the current building to the list IF powering down
				buildingsStarved.Add (building);

			// Store as last building picked to avoid picking again
			lastBuildingPicked = building;
			
			switch (buildingName) {						//** WARNING! ALL ATTACKING BUILDINGS HAVE THE COMPONENT IN CHILDREN!!!
			case "Extractor":	// extractor
				if (powerDown) {
					if (building != null && building.GetComponent<Extractor> ()){
						// Change the State to stop extraction
						building.GetComponent<Extractor> ().state = Extractor.State.STARVED;
						
						//change building status image to RED
						building.GetComponent<Building_ClickHandler> ().ChangeBuildingStatus ("Starve");
						
						//Create an INDICATOR for the user, warning them the building stopped working
						buildingUI.CreateIndicator ("An " + buildingName + " stopped working.");

						//Subtract their energy cost from the total energy (The Power system is compensating by turning this building OFF)
						totalEnergyCost = totalEnergyCost - buildingUI.extractCost[1];
					}
				} else {
					if (building != null && building.GetComponent<Extractor> ()){
						// Change the state back to extraction
						building.GetComponent<Extractor> ().state = Extractor.State.EXTRACTING;
						
						//change building status image to GREEN
						building.GetComponent<Building_ClickHandler> ().ChangeBuildingStatus ("Unstarve");
						
						// Indicate to the Player that the building is back on
						buildingUI.CreateIndicator (buildingName + " back online!");

						//Add their energy cost back to the total energy
						totalEnergyCost = totalEnergyCost + buildingUI.extractCost[1];
					}
				}
				break;
				
			case "Machine Gun": // machine gun
				if (powerDown) {
					if (building != null && building.GetComponentInChildren<Tower_TargettingHandler> ()){
						building.GetComponentInChildren<Tower_TargettingHandler> ().state = Tower_TargettingHandler.State.STARVED;
						
						//change building status image to RED
						building.GetComponent<Building_ClickHandler> ().ChangeBuildingStatus ("Starve");
						
						buildingUI.CreateIndicator ("A " + buildingName + " stopped working.");

						//Subtract their energy cost from the total energy (The Power system is compensating by turning this building OFF)
						totalEnergyCost = totalEnergyCost - buildingUI.mGunCost[1];
					}
					
				} else {
					if (building != null && building.GetComponentInChildren<Tower_TargettingHandler> ()){
						building.GetComponentInChildren<Tower_TargettingHandler> ().state = Tower_TargettingHandler.State.SEEKING;
					
						//change building status image to GREEN
						building.GetComponent<Building_ClickHandler> ().ChangeBuildingStatus ("Unstarve");
						
						buildingUI.CreateIndicator (buildingName + " back online!");

						//Add their energy cost back to the total energy
						totalEnergyCost = totalEnergyCost + buildingUI.mGunCost[1];
					}

				}
				break;
				
			case "Cannons": // cannons
				if (powerDown) {
					if (building != null && 	building.GetComponentInChildren<Tower_AoETargettingHandler> ()){
						building.GetComponentInChildren<Tower_AoETargettingHandler> ().state = Tower_AoETargettingHandler.State.STARVED;
						
						building.GetComponent<Building_ClickHandler> ().ChangeBuildingStatus ("Starve");
						
						buildingUI.CreateIndicator (buildingName + " stopped working.");

						totalEnergyCost = totalEnergyCost - buildingUI.cannonCost[1];

					}
					
				} else {
					if (building != null && building.GetComponentInChildren<Tower_AoETargettingHandler> ()){
						building.GetComponentInChildren<Tower_AoETargettingHandler> ().state = Tower_AoETargettingHandler.State.SEEKING;
						
						building.GetComponent<Building_ClickHandler> ().ChangeBuildingStatus ("Unstarve");
						
						buildingUI.CreateIndicator (buildingName + " back online!");

						totalEnergyCost = totalEnergyCost + buildingUI.cannonCost[1];
					}
				}
				break;
				
			case "Sniper Gun": 
				if (powerDown) {

					if (building != null && building.GetComponentInChildren<Tower_TargettingHandler> ()){
						building.GetComponentInChildren<Tower_TargettingHandler> ().state = Tower_TargettingHandler.State.STARVED;
						
						//change building status image to RED
						building.GetComponent<Building_ClickHandler> ().ChangeBuildingStatus ("Starve");
						
						buildingUI.CreateIndicator ("A " + buildingName + " stopped working.");

						totalEnergyCost = totalEnergyCost - buildingUI.sniperCost[1];

					}
				} else {

					if (building != null && building.GetComponentInChildren<Tower_TargettingHandler> ()){
						building.GetComponentInChildren<Tower_TargettingHandler> ().state = Tower_TargettingHandler.State.SEEKING;
						
						//change building status image to GREEN
						building.GetComponent<Building_ClickHandler> ().ChangeBuildingStatus ("Unstarve");
						
						buildingUI.CreateIndicator (buildingName + " back online!");

						totalEnergyCost = totalEnergyCost + buildingUI.sniperCost[1];
					}
				}
				break;
				
			case "Sea-Witch Crag": 
				if (powerDown) {
					if (building != null && building.GetComponentInChildren<Tower_DeBuffer> ()){
						building.GetComponentInChildren<Tower_DeBuffer> ().state = Tower_DeBuffer.State.STARVED;
						
						//change building status image to RED
						building.GetComponent<Building_ClickHandler> ().ChangeBuildingStatus ("Starve");
						
						buildingUI.CreateIndicator ("A " + buildingName + " stopped working.");

						totalEnergyCost = totalEnergyCost - buildingUI.seaWCost[1];
					}
				} else {

					if (building != null && building.GetComponentInChildren<Tower_DeBuffer> ()){
						building.GetComponentInChildren<Tower_DeBuffer> ().state = Tower_DeBuffer.State.SEEKING;
						
						//change building status image to GREEN
						building.GetComponent<Building_ClickHandler> ().ChangeBuildingStatus ("Unstarve");
						
						buildingUI.CreateIndicator (buildingName + " back online!");

						totalEnergyCost = totalEnergyCost + buildingUI.seaWCost[1];
					}
				}
				break;
				
			default:
				Debug.Log ("couldn't starve " + buildingName + " building!");
				break;
			}
		} else {
			// building is null so remove it from list
			buildingsStarved.Remove(building);
		}
	}

	void PowerOnBuildings(){
		for (int i = 0; i < buildingsStarved.Count; i++) {

			// First make sure that powering this building back on won't send Total Energy cost over my Energy
			int calc = totalEnergyCost + GetCost(buildingsStarved[i].name);
			if (calc <= energy){
				// turn the building back on
				BuildingPowerControl(buildingsStarved[i], false);
				// then remove that object from the list
				buildingsStarved.RemoveAt(i);
			}

		}
	}

	int GetCost(string name)
	{
		switch (name) {
		case "Extractor":
			return buildingUI.extractCost[1];
			break;
		case "Machine Gun": 
			return buildingUI.mGunCost[1];
			break;
		case "Cannons":
			return buildingUI.cannonCost[1];
			break;
		case "Sniper Gun":
			return buildingUI.sniperCost[1];
			break;
		case "Sea-Witch Crag":
			return buildingUI.seaWCost[1];
			break;
		default:
			Debug.Log("Can't get the energy cost for " + name);
			return 0;
			break;
		}
	}

//	// Feed Counter, when Counter is up Manager tries to feed the population with the food it has stored
//	IEnumerator WaitToFeed()
//	{
//		feeding = false;
//		yield return new WaitForSeconds (dayTime); // feeding ONCE per day
//		FeedPopulation ();
//	}
//
//	void FeedPopulation(){
//		if (food >= totalFoodCost) {
//
//			if (buildingsStarved.Count > 0)
//				UnStarveBuildings();
//
//			int calc = food - totalFoodCost;
//			Debug.Log ("Feeding population!");
//			if (calc > 0){
//				food = calc;
//				feeding = true;
//			}else{
//				food = calc;
//				feeding = true;
//				buildingUI.CreateIndicator("NO FOOD LEFT!");
//			}
//		} else {
//			Debug.Log("Not enough food!");
//			feeding = true;
//
//			// WAIT three cycles of the CoRoutine in starvation before turning off a building
//				// Give 3 different WARNINGS to the Player informing them of the lack of food
//			turnsStarving++;
//			if (turnsStarving == 1){
//				buildingUI.CreateIndicator("Population is HUNGRY!");
//				GetBuildingToStarve();
//			}else if (turnsStarving == 2){
//				buildingUI.CreateIndicator("Population NEEDS FOOD!");
//			}else if (turnsStarving == 3){
//				buildingUI.CreateIndicator("Population is STARVING!");
//				turnsStarving = 0;		// reset turns Starving ( this causes the manager to wait ANOTHER 3 turns before turning off anymore buildings)
//				GetBuildingToStarve();
//			}
//		}
//
//	}

//	void GetBuildingToStarve(){
//
//		foreach (GameObject tile in resourceGrid.spawnedTiles) {
//			// check if another building had already been picked
//			if (lastBuildingPicked != null){
//				if (tile != null){
//
//					if (tile.CompareTag("Building")){
//
//						if (tile != lastBuildingPicked){
//
//							if (CheckIfFoodCostBuilding((int)tile.transform.position.x, (int) tile.transform.position.y)){
//								// turn off the building
//								currStarvedBuilding = tile;
//								StarveBuildingControl(currStarvedBuilding, true);
//								break;
//
//							}
//						}
//					}
//				}
//			}else{
//				if (tile != null){
//					if (tile.CompareTag("Building")){
//						// turn off the building
//						currStarvedBuilding = tile;
//						StarveBuildingControl(currStarvedBuilding, true);
//						break;
//					}
//				}
//			}
//		}
//	}

//	bool CheckIfFoodCostBuilding (int x, int y){
//
//		if (resourceGrid.GetTileType (x, y) == TileData.Types.desalt_s || 
//			resourceGrid.GetTileType (x, y) == TileData.Types.nutrient ||
//			resourceGrid.GetTileType (x, y) == TileData.Types.extractor ||
//		    resourceGrid.GetTileType(x, y) == TileData.Types.storage ||
//		    resourceGrid.GetTileType(x, y) == TileData.Types.farm_s) 
//		{
//			return false;
//		} 
//		else 
//		{
//			return true;
//		}
//	
//	}


//	void UnStarveBuildings(){
//		for (int i = 0; i < buildingsStarved.Count; i++) {
//			// call starve control with starving as false
//			StarveBuildingControl(buildingsStarved[i], false);
//			// then remove that object from the list
//			buildingsStarved.RemoveAt(i);
//		}
//	}


	/// <summary>
	/// Starves the building by using the object's name in a switch.
	/// Using the building's own state machine it will set its State to starved or not, accordingly.
	/// *NOTE: This used to work using starvedMode bool
	/// </summary>
	/// <param name="building">Building.</param>
	/// <param name="starving">If set to <c>true</c> turns starving mode true.</param>
//	void StarveBuildingControl (GameObject building, bool starving)
//	{
//		// Make sure building is not null
//		if (building != null) {
//			// first starve the building by finding what kind of building it is by name
//			string buildingName = building.name;
//
//			// add the current building to the list
//			buildingsStarved.Add (building);
//
//			// store it as the last building picked
//			lastBuildingPicked = building;
//
//			switch (buildingName) {						//** WARNING! ALL ATTACKING BUILDINGS HAVE THE COMPONENT IN CHILDREN!!!
//			case "Extractor":	// extractor
//				if (starving) {
//
//					// Change the State to stop extraction
//					building.GetComponent<Extractor> ().state = Extractor.State.STARVED;
//
//					//change building status image to RED
//					building.GetComponent<Building_ClickHandler> ().ChangeBuildingStatus ("Starve");
//
//					//Create an INDICATOR for the user, warning them the building stopped working
//					buildingUI.CreateIndicator ("An " + buildingName + " stopped working.");
//
//				} else {
//
//					// Change the state back to extraction
//					building.GetComponent<Extractor> ().state = Extractor.State.EXTRACTING;
//
//					//change building status image to GREEN
//					building.GetComponent<Building_ClickHandler> ().ChangeBuildingStatus ("Unstarve");
//
//					// Indicate to the Player that the building is back on
//					buildingUI.CreateIndicator (buildingName + " back online!");
//				}
//				break;
//
//			case "Machine Gun": // machine gun
//				if (starving) {
//
//					building.GetComponentInChildren<Tower_TargettingHandler> ().state = Tower_TargettingHandler.State.STARVED;
//
//					//change building status image to RED
//					building.GetComponent<Building_ClickHandler> ().ChangeBuildingStatus ("Starve");
//
//					buildingUI.CreateIndicator ("A " + buildingName + " stopped working.");
//
//				} else {
//
//					building.GetComponentInChildren<Tower_TargettingHandler> ().state = Tower_TargettingHandler.State.SEEKING;
//
//					//change building status image to GREEN
//					building.GetComponent<Building_ClickHandler> ().ChangeBuildingStatus ("Unstarve");
//
//					buildingUI.CreateIndicator (buildingName + " back online!");
//				}
//				break;
//
//			case "Cannons": // cannons
//				if (starving) {
//
//					building.GetComponentInChildren<Tower_AoETargettingHandler> ().state = Tower_AoETargettingHandler.State.STARVED;
//
//					building.GetComponent<Building_ClickHandler> ().ChangeBuildingStatus ("Starve");
//
//					buildingUI.CreateIndicator (buildingName + " stopped working.");
//
//				} else {
//					building.GetComponentInChildren<Tower_AoETargettingHandler> ().state = Tower_AoETargettingHandler.State.SEEKING;
//
//					building.GetComponent<Building_ClickHandler> ().ChangeBuildingStatus ("Unstarve");
//
//					buildingUI.CreateIndicator (buildingName + " back online!");
//				}
//				break;
//
//			case "Sniper Gun": 
//				if (starving) {
//				
//					building.GetComponentInChildren<Tower_TargettingHandler> ().state = Tower_TargettingHandler.State.STARVED;
//				
//					//change building status image to RED
//					building.GetComponent<Building_ClickHandler> ().ChangeBuildingStatus ("Starve");
//
//					buildingUI.CreateIndicator ("A " + buildingName + " stopped working.");
//				
//				} else {
//				
//					building.GetComponentInChildren<Tower_TargettingHandler> ().state = Tower_TargettingHandler.State.SEEKING;
//				
//					//change building status image to GREEN
//					building.GetComponent<Building_ClickHandler> ().ChangeBuildingStatus ("Unstarve");
//
//					buildingUI.CreateIndicator (buildingName + " back online!");
//				}
//				break;
//
//			case "Sea-Witch Crag": 
//				if (starving) {
//				
//					building.GetComponentInChildren<Tower_DeBuffer> ().state = Tower_DeBuffer.State.STARVED;
//				
//					//change building status image to RED
//					building.GetComponent<Building_ClickHandler> ().ChangeBuildingStatus ("Starve");
//
//					buildingUI.CreateIndicator ("A " + buildingName + " stopped working.");
//				
//				} else {
//				
//					building.GetComponentInChildren<Tower_DeBuffer> ().state = Tower_DeBuffer.State.SEEKING;
//				
//					//change building status image to GREEN
//					building.GetComponent<Building_ClickHandler> ().ChangeBuildingStatus ("Unstarve");
//				
//					buildingUI.CreateIndicator (buildingName + " back online!");
//				}
//				break;
//
//			default:
//				Debug.Log ("couldn't starve " + buildingName + " building!");
//				break;
//			}
//		} else {
//			// building is null so remove it from list
//			buildingsStarved.Remove(building);
//		}
//	}

	public void SetBooster()
	{
		if (booster == 0) { // default starting value is 0

			// Boosting 1/4th of ammount
			booster = 4;

		} else if (booster == 4) {

			// Boosting 1/2 of ammount
			booster = 2;

		} else if (booster == 2) {

			// Doubling the ammount
			booster = 1;
		} else {

			booster = 1;
		}
		Debug.Log ("R MANAGER: booster at " + booster);
	}


	/// <summary>
	/// Add or subtract a resource. This is changing the total ammount that is then seen in the UI.
	/// </summary>
	/// <param name="id">Identifier.</param>
	/// <param name="quantity">Quantity.</param>
	public void ChangeResource (string id, int quantity){
		Debug.Log ("Changing " + id + " by " + quantity);

		switch (id) {
		case "Ore":
			ore = ore + quantity;
			 
			break;
		case "Food":

			food = food + quantity;

			break;
		case "Credits":
			// Credit reward get BOOSTED if Player calls enemies to spawn early by clicking on a spawn indicator
			if (booster > 0)
				_boostCalc = quantity / booster;

			credits = credits + (quantity + _boostCalc);
			Debug.Log("R MANAGER: Received " + quantity + " boosted by " + _boostCalc);
			break;
		case "Water":
			water  = water + quantity;

			break;
		case "Energy":
			energy  = energy + quantity;
			
			break;
		default:
			print ("R MANAGER: Cant find that resource type!");
			break;
		}
	}


	// ***** V BELOW IS THE OLD LOGIC FOR CHARGING ORE FROM STORAGE. 
		// I replaced this with a button on each storage for the player to withdraw resources manually



//	public bool CheckStorageForResource(string id, int ammnt){
//		if (storageBuildings.Count > 0) {
//			for (int i =0; i < storageBuildings.Count; i++) {
//				if (id == "Ore") {
//					if (storageBuildings [i].oreStored >= ammnt)
//						return true;
//				} else if (id == "Water") {
//					if (storageBuildings [i].waterStored >= ammnt)
//						return true;
//				} else {
//					// no other resource but ore and water in storage right now
//					return false;
//				}
//			}
//
//			// if none of the storages have returned true then just return false
//			return false;
//
//		} else {
//
//			return false;
//
//		}
//
//	}

	/// <summary>
	/// Charges an ammount of WATER or ORE from the first
	/// storage that contains more than or equal the charge ammount.
	/// </summary>
	/// <param name="id">Identifier.</param>
	/// <param name="charge">Charge.</param>
//	public void ChargeFromStorage(int charge, string id){
//		if (storageBuildings.Count > 0) {
//			for (int x=0; x < storageBuildings.Count; x++){
//				if (id == "Ore"){
//					if (storageBuildings[x].oreStored >= charge){
//						storageBuildings[x].ChargeResource(charge, "Ore");
//						break;
//					}
//				}else if (id == "Water"){
//					if (storageBuildings[x].waterStored >= charge){
//						storageBuildings[x].ChargeResource(charge, "Water");
//						break;
//					}
//				}
//			}
//		}
//	}

//	public void ChargeOreorWater(string id, int ammnt){
//		// this assumes that I've already checked there is more than ammnt in the total Ore / Water
//
//		// First, check if there are any storage buildings
//		if (storageBuildings.Count > 0) {
//			// Since there ARE storage buildings let's try charging this ammnt from one of them
//			// here we check if there's a storage with enough of that resource, using NEGATIVE value so it turns positive
//			if (CheckStorageForResource(id, -ammnt)){ 
//				Debug.Log ("R MANAGER: Found Ore in Storage!");
//				ChargeFromStorage(ammnt, id); // this TAKES the ammnt from a storage
//
//			}else{
//				// Here we haven't found that ammount in any of our storage, so we know this is being charged
//				// directly from the total ore (so it is coming from a surplus NOT in storage)
//				Debug.Log ("R MANAGER: Could NOT find Ore in Storage. Charging total surplus instead.");
//				ChangeResource(id, ammnt);
//			}
//		
//		} else {
//			// if there are NO storage buildings we just charge from the total Ore / Water
//			Debug.Log ("R MANAGER: NO Storage in List. Charging total surplus.");
//			ChangeResource(id, ammnt);
//
//		}
//	}


	/// <summary>
	/// Removes the storage building from the list.
	/// </summary>
	/// <param name="storage">Storage.</param>
	public void RemoveStorageBuilding(Storage storage){
		storageBuildings.Remove (storage);
	}

}
