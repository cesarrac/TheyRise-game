using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System;

public class BlueprintDatabase : MonoBehaviour {

    public static BlueprintDatabase Instance { get; protected set; }

    // FIX ALL OF THIS! Eventually this should read from an external database. For now I'm going to hardcode all the blueprints in the game here.

    Dictionary<string, Blueprint> blueprintsMap;
    Dictionary<string, Blueprint_Extraction> extractorsMap;
    Dictionary<string, Blueprint_Battle> battleTowersMap;

    // FIX THIS! 
    // public int maxBPAllowed = 3;

    List<Blueprint> reserchedBlueprints;

    public int shipLvlIndex = 0, planetLvlIndex = 1, inventoryLvlIndex = 2;

    Blueprint curSelectedBP;

    NanoBuilder hero_nanoBuilder;

    Blueprint lastRequired;

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

        // If this is a New Game, the blueprints need to be initialized
        if (blueprintsMap == null)
        {
            Init();
            InitExtractors();
            InitBattleTowers();
        }
    }

    // NOTE: Start only gets called the first time we enter the Center Level
    void Start()
    {
        hero_nanoBuilder = GameMaster.Instance.theHero.nanoBuilder;

        // If this is a New Game, initialize the Nano Builder with the starting blueprints
        if (hero_nanoBuilder.blueprintsMap.Count == 0)
        {
            AddStartingBlueprints();
        }

        //// Check if this is the first time we load the Blueprint screen. If it is we need to load the Terraformer unto the Hero's blueprints map.
        //if (!hero_nanoBuilder.CheckForBlueprint(TileData.Types.terraformer))
        //{
        //    // As soon as we have access to the Hero, the first blueprint to load into it would be the Terraformer (this assumes BP database has been Initialized)
        //    hero_nanoBuilder.AddBluePrint(TileData.Types.terraformer, blueprintsMap["Terraformer"]);
        //    Debug.Log("Hero added blueprint for " + blueprintsMap["Terraformer"].buildingName);
        //}

        //InitRequiredBlueprint();

        // If we are on the Inventory/Blueprint Loader scene, we should display the memory count on the Hero's nanobuilder
        //if (SceneManager.GetActiveScene().name == "Level_CENTRAL")
        //{
        //    LoadAllBlueprints();
        //    //DisplayBuilderMemory();
        //}

    }


    // The required Blueprint is dependant on the current active mission
    //public void InitRequiredBlueprint()
    //{
    //    LoadAllBlueprints();

    //    //if (Mission_Manager.Instance.ActiveMission != null)
    //    //{
    //    //    // Check if a previous mission had added a Required Blueprint
    //    //    if (lastRequired != null)
    //    //    {
    //    //        // ... Check if the new required Blueprint is NOT of the same type as the old ...
    //    //        if (lastRequired.tileType != Mission_Manager.Instance.ActiveMission.RequiredBlueprint.tileType)
    //    //        {
    //    //            // ... If it's NOT of the same type, the last one needs to be removed from data...
    //    //            hero_nanoBuilder.RemoveBlueprint(lastRequired.tileType);
    //    //            // ... and from UI...
    //    //            UI_Manager.Instance.RemoveBlueprintTextFromBuilder(lastRequired.buildingName);

    //    //            // ... and the new one ADDED.
    //    //            if (!hero_nanoBuilder.CheckForBlueprint(Mission_Manager.Instance.ActiveMission.RequiredBlueprint.tileType))
    //    //            {
    //    //                hero_nanoBuilder.AddBluePrint(Mission_Manager.Instance.ActiveMission.RequiredBlueprint.tileType,
    //    //                                             blueprintsMap[Mission_Manager.Instance.ActiveMission.RequiredBlueprint.buildingName]);

    //    //                // ... and set it as the last required for next time.
    //    //                lastRequired = Mission_Manager.Instance.ActiveMission.RequiredBlueprint;
    //    //            }
    //    //        }

    //    //        // ... If they ARE of the same type then nothing remains to be done because the BP is already loaded.
    //    //    }
    //    //    else
    //    //    {
    //    //        // In the case of last required being null (no previous missions have loaded anything yet!), add bp as normal...
    //    //        if (!hero_nanoBuilder.CheckForBlueprint(Mission_Manager.Instance.ActiveMission.RequiredBlueprint.tileType))
    //    //        {
    //    //            hero_nanoBuilder.AddBluePrint(Mission_Manager.Instance.ActiveMission.RequiredBlueprint.tileType,
    //    //                                         blueprintsMap[Mission_Manager.Instance.ActiveMission.RequiredBlueprint.buildingName]);

    //    //            // ... and set it as the last required for next time.
    //    //            lastRequired = Mission_Manager.Instance.ActiveMission.RequiredBlueprint;
    //    //        }
    //    //    }


    //    //    Debug.Log("BP Database: Initialized the required Blueprint for the active mission!");
    //    //}
    //}

    // *********************************************************************************************
    //                      INITIALIZE BLUEPRINTS
    // *********************************************************************************************
    // Initializes all Blueprints in the game
    public void Init()
    {
        blueprintsMap = new Dictionary<string, Blueprint>
        {
            {"Terraformer", new Blueprint("Terraformer", 0, 0, TileData.Types.terraformer, BuildingType.UTILITY, new BuildRequirement(TileData.Types.rock, 20), "Turns bad soil into good soil.") },
            {"Sniper Gun", new Blueprint("Sniper Gun", 3, 10, TileData.Types.sniper, BuildingType.BATTLE, new BuildRequirement(TileData.Types.rock, 20),"Long distance Target acquisition limited by slow firing rate.") },
            {"Cannons", new Blueprint("Cannons", 3, 10, TileData.Types.cannons, BuildingType.BATTLE, new BuildRequirement(TileData.Types.rock, 20),"Short distance multi target artillery that delivers payload its surrounding areas.")},
            {"Sea-Witch Crag", new Blueprint("Sea-Witch Crag", 3, 10, TileData.Types.seaWitch, BuildingType.BATTLE, new BuildRequirement(TileData.Types.rock, 20),"Manipulate genetic composition to slow, weaken, or shatter an enemy's defenses.") },
            {"Extractor",  new Blueprint("Extractor", 3, 10, TileData.Types.extractor,BuildingType.UTILITY, new BuildRequirement(TileData.Types.rock, 20),"Dig and Mine ore and Dirt with this wonderful piece of machinery.") },
            {"Desalination Pump", new Blueprint("Desalination Pump", 3, 10, TileData.Types.desalt_s, BuildingType.UTILITY, new BuildRequirement(TileData.Types.rock, 20),"Find water. Pump water!") },
            {"Energy Generator", new Blueprint("Energy Generator", 3, 10, TileData.Types.generator, BuildingType.UTILITY, new BuildRequirement(TileData.Types.rock, 20),"Generate energy harnessed from ore with high content of precious metals.") },
            {"Seaweed Farm",  new Blueprint("Seaweed Farm", 3, 10, TileData.Types.farm_s, BuildingType.UTILITY, new BuildRequirement(TileData.Types.rock, 20),"Plant seed, harvest food!")},
            {"Storage", new Blueprint("Storage", 3, 10, TileData.Types.storage, BuildingType.UTILITY, new BuildRequirement(TileData.Types.rock, 20),"Store things and send them to ship automatically!") },
            {"Machine Gun", new Blueprint("Machine Gun", 3, 10, TileData.Types.machine_gun, BuildingType.BATTLE, new BuildRequirement(TileData.Types.rock, 10),"Short distance, fast firing rate, single target acquisition.") },
            {"Plastic Wall", new Blueprint("Plastic Wall", 3, 10, TileData.Types.wall, BuildingType.BATTLE, new BuildRequirement(TileData.Types.rock, 10), "A little better than a child's toy fort.") },
            {"Extractor MkII",  new Blueprint("Extractor MkII", 3, 10, TileData.Types.extractor,BuildingType.UTILITY, new BuildRequirement(TileData.Types.rock, 20),"Dig and Mine ore and Dirt with this wonderful piece of machinery.") },
        };

        InitExtractors();
        InitBattleTowers();


        //blueprints_all["Extractor"].

        //// Add +1 to maxBPAllowed since we are using it as a 0 based index below, and to take into account the Terraformer
        // maxBPAllowed += 1;

        // Always add the terraformer to the Selected Blueprints at its 0 index position
        //selectedBlueprints.Add(blueprints_all["Terraformer"]);
    }

    void InitExtractors()
    {
        extractorsMap = new Dictionary<string, Blueprint_Extraction>();
        extractorsMap.Add("Extractor", new Blueprint_Extraction(10, 25, 1, 50));
        extractorsMap.Add("Desalination Pump", new Blueprint_Extraction(10, 25, 1, 50));
    }

    void InitBattleTowers()
    {
        battleTowersMap = new Dictionary<string, Blueprint_Battle>();
        battleTowersMap.Add("Machine Gun", new Blueprint_Battle(12, 2, 0.12f, 2, 25, 2, 1, 0));
        battleTowersMap.Add("Sniper Gun", new Blueprint_Battle(12, 2, 1.5f, 8 ,25, 2, 1, 0));
        battleTowersMap.Add("Terraformer", new Blueprint_Battle(0, 0, 0, 0, 2500, 0, 2, 0));
        battleTowersMap.Add("Plastic Wall", new Blueprint_Battle(0, 0, 0, 0, 20, 0, 1, 0));
    }
    // *********************************************************************************************
    //                              ACCESSORS FOR BLUEPRINT STATS
    // *********************************************************************************************

    public void GetExtractorStats(string id, Transform objTransform, ExtractionBuilding extractor, TileData.Types resourceType)
    {
        if (extractorsMap.ContainsKey(id))
        {
           // Debug.Log("BP Database: Found stats for " + id);
            extractor.Init(resourceType, extractorsMap[id].extractorStats.extractRate, extractorsMap[id].extractorStats.extractPower,
                extractorsMap[id].extractorStats.extractAmmount, extractorsMap[id].extractorStats.personalStorageCapacity, objTransform);
        }
    }

    public void GetBattleStats(string id, Action<TowerGunStats> InitGunStats, Action<UnitStats> InitUnitStats)
    {
        if (battleTowersMap.ContainsKey(id))
        {
           // Debug.Log("BP Database: Found stats for " + id);
            InitGunStats(battleTowersMap[id].battleStats);
            InitUnitStats(battleTowersMap[id].unitStats);
        }
    }

    public void GetBattleStats(string id, Action<UnitStats> InitUnitStats)
    {
        if (battleTowersMap.ContainsKey(id))
        {
           // Debug.Log("BP Database: Found stats for " + id);
            InitUnitStats(battleTowersMap[id].unitStats);
        }
    }

    public TileStats GetTileStats(string id)
    {
        if (battleTowersMap.ContainsKey(id))
        {
           // Debug.Log("BP DATABASE: Found initial tile stats for " + id);
            return battleTowersMap[id].tileStats;
        }
        else
        {
            return null;
        }
    }

    public BuildingType GetTowerType(string id)
    {
        if (blueprintsMap.ContainsKey(id))
        {
            return blueprintsMap[id].buildingType;
        }
        else
            return BuildingType.NONE;
    }

    // *********************************************************************************************
    //                              HERO NANOBUILDER (LOADING BP'S & RESEARCH)
    // *********************************************************************************************
    // Adds the Starting Blueprints for a New Game
    void AddStartingBlueprints()
    {
        // Initialize Researched blueprints with starting bp's 
        reserchedBlueprints = new List<Blueprint>();
        reserchedBlueprints.Add(blueprintsMap["Machine Gun"]);
        reserchedBlueprints.Add(blueprintsMap["Extractor"]);

        if (hero_nanoBuilder != null)
        {
            hero_nanoBuilder.AddBluePrint(reserchedBlueprints[0].tileType, reserchedBlueprints[0]);
            hero_nanoBuilder.AddBluePrint(reserchedBlueprints[1].tileType, reserchedBlueprints[1]);
        }
    }
    void LoadAllBlueprints()
    {
        foreach (Blueprint bp in reserchedBlueprints)
        {
            hero_nanoBuilder.AddBluePrint(bp.tileType, bp);
        }
    }
    void AddLoadedBlueprintToNanoBuilder()
    {
        if (hero_nanoBuilder.CheckForBlueprint(curSelectedBP.tileType))
        {
            Debug.LogError("Nanobuilder already contains a blueprint for: " + curSelectedBP.buildingName);
            return;
        }
        // Spawn a Text prefab, fill its text with the correct blueprint name, and parent it to the Builder's panel
        UI_Manager.Instance.AddBlueprintToBuilder(curSelectedBP.buildingName);

        // Actually load it to the Hero's nanobuilder
        hero_nanoBuilder.AddBluePrint(curSelectedBP.tileType, curSelectedBP);

        DisplayBuilderMemory();

        // Debug.Log("Hero loaded blueprint for: " + curSelectedBP.buildingName);

    }
    public void ResetNanoBuilder()
    {
        hero_nanoBuilder.RemoveAllLoadedBlueprints();
        UI_Manager.Instance.RemoveAllBlueprintsText();
    }

    // *********************************************************************************************
    //                              METHODS FOR BLUEPRINT UI SYSTEM
    // *********************************************************************************************

    // This will Display the Selected Blueprint's info and store it's info in case the Player decides to Load it to the Builder
    public void SelectBlueprint(string bpType)
    {
        if (blueprintsMap.ContainsKey(bpType))
        {
            curSelectedBP = blueprintsMap[bpType];

            DisplaySelectedBPInfo();
        }

       // Debug.Log("Current selected BP is " + curSelectedBP.buildingName);
    }

    void DisplaySelectedBPInfo()
    {
        // Display the info to the Info Panel from the stored selected BP (curSelectedBP)
        UI_Manager.Instance.DisplayBPInfo(curSelectedBP.buildingName, curSelectedBP.description);
    }

    // Player presses the Load button
    public void LoadBlueprint()
    {
        // Check if the nanobuilder has enough memory left to load the curSelected Blueprint
        if (curSelectedBP != null)
        {
            if (curSelectedBP.memoryCost <= hero_nanoBuilder.cur_memory)
            {
                // Add loaded blueprint to NanoBuilder
                AddLoadedBlueprintToNanoBuilder();
            }
        }
    }
    // Central Level scene is Loaded and UI needs to re-display the Loaded Blueprints
    public void ReloadPreviousLoaded()
    {
        if (hero_nanoBuilder != null)
        {
            if (hero_nanoBuilder.blueprintsMap.Count > 0)
            {
                // The Hero already has Blueprints loaded unto their Builder, so those need to be displayed by the UI Manager
                ReloadFromNanoBuilder();
            }
        }

    }

    void ReloadFromNanoBuilder()
    {
        if (hero_nanoBuilder.blueprintsMap.Count > 0)
        {
            foreach(TileData.Types btype in hero_nanoBuilder.bpTypes)
            {
                UI_Manager.Instance.AddBlueprintToBuilder(hero_nanoBuilder.blueprintsMap[btype].buildingName);
            }

            DisplayBuilderMemory();
        }
    }

    // Player presses the unload button
    public void UnLoadBlueprint()
    {
        if (curSelectedBP != null)
        {
            // Remove from the Builder
            RemoveLoadedBlueprintFromNanoBuilder();
        }
    }

    void RemoveLoadedBlueprintFromNanoBuilder()
    {
        if (!hero_nanoBuilder.CheckForBlueprint(curSelectedBP.tileType))
        {
            Debug.LogError("Nanobuilder does NOT contain a blueprint for: " + curSelectedBP.buildingName);
            return;
        }

        // Make sure that the Player is not trying to UNLOAD the required blueprint for an active mission!
        if (curSelectedBP.tileType == Mission_Manager.Instance.ActiveMission.RequiredBlueprint.tileType)
        {
            return;
        }

        UI_Manager.Instance.RemoveBlueprintTextFromBuilder(curSelectedBP.buildingName);

        // Remove it from the NanoBuilder
        hero_nanoBuilder.RemoveBlueprint(curSelectedBP.tileType);

        DisplayBuilderMemory();
    }

    void DisplayBuilderMemory()
    {
        // Display the memory count on the Hero's nanobuilder
        UI_Manager.Instance.DisplayNanoBuilderMemory(hero_nanoBuilder.cur_memory, hero_nanoBuilder.memoryBank);
    }

    // *********************************************************************************************
    //                              BLUEPRINT RESEARCH & UPGRADES
    // *********************************************************************************************

    public void UpgradeBattleBPAmmo(string id, int newAmmo)
    {
        if (battleTowersMap.ContainsKey(id))
        {
            battleTowersMap[id].UpgradeAmmo(newAmmo);
        }
    }

    public void UpgradeBattleBPReloadSpd(string id, float newSpeed)
    {
        if (battleTowersMap.ContainsKey(id))
        {
            battleTowersMap[id].UpgradeReloadSpeed(newSpeed);
        }
    }


    public void UpgradeBattleBPUnitStats(string id, string statID, float newAmmnt)
    {
        if (battleTowersMap.ContainsKey(id))
        {
            battleTowersMap[id].UpgradeUnitStat(statID, newAmmnt);
        }
    }
}
