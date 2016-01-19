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

    int levelCount; // using this in a CRUDE way to count how many times the player has been to the surface.

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

    // HERO/PLAYER LOADING (Called by Resource Grid after Map has been initialized):
    public GameObject SpawnThePlayer(int posX, int posY)
    {
        resourceGrid = ResourceGrid.Grid;
        objPool = ObjectPool.instance;

        Vector3 playerPosition = new Vector3(posX, posY, 0.0f);
        GameObject Hero = objPool.GetObjectForType("Hero", true, playerPosition);
        /*
        HERE we need what Items the player chose to take with them to the surface.
        
        We'll need to know Equipped Items: Armor/Space Suit, Weapons, and Tools
        And all Consumable items.
        
        The correct set needs to be spawned as GameObjects and made children of the Hero GObj.
        
        We'll need to tell the Player_HeroAttackHandler how many weapons and tools the player has available and link
        the Weapon and Tool variables in that script to the corresponding child gameobjects.
        
        Default is 2 Weapons, 1 Tool

        Then for the weapons we need to tell its Gun base class to Upgrade any stats if the Weapon was upgraded...

        ...and finally, apply any upgrades to Tools.

        Hero base class.
        Armor.
        List of Weapons.
        List of Tools.
        List of consumables.
        List of abilities.

        upgradeWeapon function (type of upgrade U, type of weapon W){
            Weapons[w].upgrade(U);
        }

        In Weapons class:
        upgrade (type of upgrade U){
            switch(U){
                case attack rate type:
                weapon.stats.attackRate = U.attackRate;
                break;
            }
        }

        ** Solution:
        - Hero prefabs for all possible slot combinations: 
            Default: 1 wpn , 1 tool
            Others:  2 wpn , 1 tool / 3wpns, 1 tool / 4 wpns, 1 tool / 4 wpns, 2 tools
        - GameMaster will know which one to spawn from the weapon and tool count of the Hero stored as a variable (this Hero is of the data class hero)
        -Get the Weapon's Component from the name of the weapon ( from Hero class) for each weapon
        - The same for Tools
        - By default Weapon 1 should be active other weapons and tool gameobjects should be inactive
            

        */
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
