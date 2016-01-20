﻿using UnityEngine;
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

    public Hero theHero { get; protected set; }

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

        CreateHero();

	}

    // CHARACTER CREATION:
    // Create the Hero data class that will hold all the information of the player's Hero character
    void CreateHero()
    {
        // Default constructor for test needs a weapon, a tool, and armor.

        // FIX THIS! This data should be coming from one giant database of Weapons, Armor and Tools!!!
        // Default Gun: Kinetic Rifle
        Weapon kinetic_rifle = new Weapon("Kinetic Rifle", 0.3f, 12, 2f, 2, 15, "explosive bullet");
        // Default Tool: Hand Drill
        Tool hand_drill = new Tool("Mining Drill");
        // Default Armor: Vacum Suit
        Armor vac_suit = new Armor("Vacumn Suit", 2, 0);

        theHero = new Hero(kinetic_rifle, vac_suit, hand_drill);

        Debug.Log("Hero Created!");
        Debug.Log("Hero is wielding: " + theHero.weapons[0].itemName + " armor: " + theHero.armor.itemName + " and tool: " + theHero.tools[0].itemName);
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
        GameObject Hero = objPool.GetObjectForType("Test Hero", true, playerPosition);
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
     
        -Get the Weapon's Component from the name of the weapon ( from Hero class) for each weapon
        - The same for Tools
        - By default Weapon 1 should be active other weapons and tool gameobjects should be inactive
            

        */
        if (Hero)
        {
            // ***** FIX THIS! Right now this assumes that ALL weapons are guns and have gunstats. There should be a weapon type to 
            // differentiate what a Melee weapon's component might need in terms of stats VS. what a gun gets for gunStats.

            // Get the Hero's attack handler.
            Player_HeroAttackHandler hero_attkHandler = Hero.GetComponent<Player_HeroAttackHandler>();

            // Spawn Weapons:

            // Get the first weapon by using the item's name. Name should match the gameobject pre-loaded by Object Pool
            GameObject wpn1 = ObjectPool.instance.GetObjectForType(theHero.weapons[0].itemName, true, Hero.transform.position);
            if (wpn1)
            {
                // Set the Wpn 1's transform to be a child of the Hero...
                wpn1.transform.SetParent(Hero.transform);

                // ... then shift it slightly into position.
                wpn1.transform.localPosition = new Vector3(-0.18f, 0.65f, 0);

                // Then through its Gun base class, set its stats (this will include any upgrades the Hero might have added)
                wpn1.GetComponent<Player_GunBaseClass>().gunStats = theHero.weapons[0].gunStats;

                // Assign it to the Hero's attack handler
                hero_attkHandler.AddEquippedItems(wpn1);
            }

            // Keep spawning weapons if the Hero has more equipped...
            if (theHero.weapons.Count > 1)
            {
                for (int i = 1; i < theHero.weapons.Count; i++)
                {
                    // Spawn the next gun...
                    GameObject otherWpn = ObjectPool.instance.GetObjectForType(theHero.weapons[i].itemName, true, Hero.transform.position);

                    if (otherWpn)
                    {
                        otherWpn.transform.SetParent(Hero.transform);

                        otherWpn.transform.localPosition = new Vector3(-0.18f, 0.65f, 0);

                        otherWpn.GetComponent<Player_GunBaseClass>().gunStats = theHero.weapons[i].gunStats;

                        // Assign it to the Hero's attack handler
                        hero_attkHandler.AddEquippedItems(otherWpn);

                        // ... making sure to set its gameobject to inactive.
                        otherWpn.SetActive(false);
                    }
                }
            }

            // Spawn Tools:
            // Do the same process for the tools.
            GameObject tool1 = ObjectPool.instance.GetObjectForType(theHero.tools[0].itemName, true, Hero.transform.position);
            if (tool1)
            {
                // Set parent to Hero...
                tool1.transform.SetParent(Hero.transform);

                // ... and shift position.
                tool1.transform.localPosition = new Vector3(-0.18f, 0.65f, 0);

                // This gameobject (tool1) needs to start as inactive, so pass in false to its SetActive func.
                tool1.SetActive(false);

                // Assign it to the attack handler.
                hero_attkHandler.AddEquippedItems(tool1);
            }
            // Again we check if more tools are equipped. (The player is limited to two tools at a time)
            if (theHero.tools.Count > 1)
            {
                for (int i = 1; i < theHero.weapons.Count; i++)
                {
                    // Spawn the next Tool...
                    GameObject otherTool = ObjectPool.instance.GetObjectForType(theHero.tools[i].itemName, true, Hero.transform.position);

                    if (otherTool)
                    {
                        otherTool.transform.SetParent(Hero.transform);

                        otherTool.transform.localPosition = new Vector3(-0.18f, 0.65f, 0);

                        // ... making sure to set its gameobject to inactive.
                        otherTool.SetActive(false);

                        // Assign it to the attack handler.
                        hero_attkHandler.AddEquippedItems(otherTool);
                    }
                }
            }

            // Set the Hero's unit stats
            hero_attkHandler.stats = theHero.heroStats;

            // Then set the rest of the necessary values
            Hero.GetComponent<Player_MoveHandler>().resourceGrid = resourceGrid;
            hero_attkHandler.objPool = objPool;
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
