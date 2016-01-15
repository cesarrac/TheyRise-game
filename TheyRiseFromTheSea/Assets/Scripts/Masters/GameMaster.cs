using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class GameMaster : MonoBehaviour {

    public static GameMaster Instance { get; protected set; }

	private int _storedCredits;
	public int curCredits { get { return _storedCredits; }set{ _storedCredits = Mathf.Clamp (value, 0, 100000000); }}
	
	ResourceGrid resourceGrid;
    Build_MainController build_mainController;
	public Player_GunBaseClass player_weapon;

    ObjectPool objPool;

	public bool _canFireWeapon;

	bool levelInitialized;

	void Awake()
	{
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            DestroyImmediate(gameObject);
        }

        // CHEATING!!!
        curCredits = 1000;
		Debug.Log ("GM is awake!");

        if (Application.loadedLevel == 1)
        {
            resourceGrid = ResourceGrid.Grid;
            objPool = ObjectPool.instance;

        }

	}

 
	// LEVEL LOADING:

	void OnLevelWasLoaded (int level){
		if (level == 1) {
			// When we go into a "level" initialize the supplies bought at the store
			InitializeInventoryAndSupplies();

			// UnPause the game in case it was paused for mission failed
			if (Time.timeScale == 0)
				Time.timeScale = 1;
		}
	}

	public void LoadLevel()
	{
		if (Application.loadedLevel == 2) {
			Application.LoadLevel (1);
		} else {
			Application.LoadLevel(2);
		}
	}

    // HERO/PLAYER LOADING:
    public GameObject SpawnThePlayer(int posX, int posY)
    {
        resourceGrid = ResourceGrid.Grid;
        objPool = ObjectPool.instance;
        /* Going to need a position that is 100% for sure an empty land tile.
        To do that I'll need to load the player once the map generator and resource Grid have done their thing */
        Vector3 playerPosition = new Vector3(posX, posY, 0.0f);
        GameObject Hero = objPool.GetObjectForType("Hero", true, playerPosition);

        if (Hero)
        {
            Hero.GetComponent<Player_MoveHandler>().resourceGrid = resourceGrid;
            Hero.GetComponent<Player_HeroAttackHandler>().objPool = objPool;
            Hero.GetComponent<Player_PickUpItems>().objPool = objPool;
            resourceGrid.cameraHolder.gameObject.GetComponent<PixelPerfectCam>().followTarget = Hero;
            return Hero;
        }
        else
        {
            return null;
        }

    }

	void InitializeInventoryAndSupplies()
	{
		Debug.Log ("Initializing a level! Ore: " + inventory.ore + " Food: " + inventory.food + " Credits: " + curCredits);
		Player_ResourceManager resourceMan = GameObject.FindGameObjectWithTag("Capital").GetComponent<Player_ResourceManager>();
		resourceMan.InitStartingResources (inventory.food, curCredits, 10000);
	}

//	void InitializeHeroAndResourceGrid()
//	{
//		resourceGrid = GameObject.FindGameObjectWithTag ("Map").GetComponent<ResourceGrid> ();
//		player_weapon = GameObject.FindGameObjectWithTag ("Citizen").GetComponentInChildren<Player_GunBaseClass> ();
//		building_UIHandler = GameObject.FindGameObjectWithTag ("Capital").GetComponent<Building_UIHandler> ();
//
//		Debug.Log ("GM: Initialized Grid, Hero & B UI");
//		// TODO: Spawn the Hero here
//	}


	// Restart a level by loading it again
	public void MissionRestart()
	{
		Application.LoadLevel (1);
	}

	public void GoBackToShip()
	{
		// load the ship level
		Application.LoadLevel (0);
	}

	// DURING A LEVEL:
	
	void Update()
	{
		
        if (Application.loadedLevel == 1)
            CheckIfBuilding(); // this is so Player can know if they can fire or not
    }

	void CheckIfBuilding()
	{

			if (ResourceGrid.Grid.transporter_built && !Build_MainController.Instance.currentlyBuilding) {
				_canFireWeapon = true;
			} else {
				_canFireWeapon = false;
			}
		

	}

	// INVENTORY:

	public class ExpeditionInventory
	{
		public int ore; // the building blocks
		public int food;

		// TODO: these strings will match the name or enum type of weapon, suit, and tool so the Hero is loaded with them equipped
		public string weapon;
		public string suit;
		public string tool;

        public Blueprint[] blueprints;

		public ExpeditionInventory()
		{

		}

		public ExpeditionInventory(int _ore, int _food, string _wpn, string _suit, string _tool)
		{
			ore = _ore;
			food = _food;
			weapon = _wpn;
			suit = _suit;
			tool = _tool;
		}
	}

	public ExpeditionInventory inventory = new ExpeditionInventory();

	public void NewExpeditionInventory(int _ore, int _food, string _weapon, string _suit, string _tool)
	{
		inventory = new ExpeditionInventory (_ore, _food, _weapon, _suit, _tool);
		// Now when loading a new level pass in this inventory to the starting values of Player Resource Manager
		// & the Hero's equipment slots
	}

	public void EndLevel(int credits)
	{
		AddOrSubtractCredits (credits);
		// Load ship level
	}


	/// <summary>
	/// Adds the or subtract credits.
	/// Pass in argument as a negative value to subtract.
	/// </summary>
	/// <param name="amount">Amount.</param>
	public void AddOrSubtractCredits (int amount)
	{
		curCredits = curCredits + amount;
	}

}
