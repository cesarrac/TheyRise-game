﻿using UnityEngine;
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

    // List<Blueprint> selectedBlueprints = new List<Blueprint>();

    public int shipLvlIndex = 0, planetLvlIndex = 1, inventoryLvlIndex = 2;

    Blueprint curSelectedBP;

    NanoBuilder hero_nanoBuilder;

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


        if ( blueprintsMap == null)
        {
            Init();
            InitExtractors();
            InitBattleTowers();
        }


    }

    void Start()
    {
        hero_nanoBuilder = GameMaster.Instance.theHero.nanoBuilder;

        if (hero_nanoBuilder.blueprintsMap.Count > 0)
        {
            // The Hero already has Blueprints loaded unto their Builder, so those need to be displayed by the UI Manager
            ReloadOnNanoBuilder();
        }

        // Check if this is the firs time we load the Blueprint screen. If it is we need to load the Terraformer unto the Hero's blueprints map.
        if (!hero_nanoBuilder.CheckForBlueprint(TileData.Types.terraformer))
        {
            // As soon as we have access to the Hero, the first blueprint to load into it would be the Terraformer (this assumes BP database has been Initialized)
            hero_nanoBuilder.AddBluePrint(TileData.Types.terraformer, blueprintsMap["Terraformer"]);
            Debug.Log("Hero added blueprint for " + blueprintsMap["Terraformer"].buildingName);
        }

        // If we are on the Inventory/Blueprint Loader scene, we should display the memory count on the Hero's nanobuilder
        if (SceneManager.GetActiveScene().buildIndex == inventoryLvlIndex)
        {
            DisplayBuilderMemory();
        }


    }


    void Init()
    {
        blueprintsMap = new Dictionary<string, Blueprint>
        {
            {"Terraformer", new Blueprint("Terraformer", 0, 0, TileData.Types.terraformer, "Turns bad soil into good soil.") }, // < ---- Always include the Terraformer and no cost
            {"Sniper Gun", new Blueprint("Sniper Gun", 3, 10, TileData.Types.sniper, "Long distance Target acquisition limited by slow firing rate.") },
            {"Cannons", new Blueprint("Cannons", 3, 10, TileData.Types.cannons, "Short distance multi target artillery that delivers payload its surrounding areas.")},
            {"Sea-Witch Crag", new Blueprint("Sea-Witch Crag", 3, 10, TileData.Types.seaWitch, "Manipulate genetic composition to slow, weaken, or shatter an enemy's defenses.") },
            {"Extractor",  new Blueprint("Extractor", 3, 10, TileData.Types.extractor, "Dig and Mine ore and Dirt with this wonderful piece of machinery.") },
            {"Desalination Pump", new Blueprint("Desalination Pump", 3, 10, TileData.Types.desalt_s, "Find water. Pump water!") },
            {"Energy Generator", new Blueprint("Energy Generator", 3, 10, TileData.Types.generator, "Generate energy harnessed from ore with high content of precious metals.") },
            {"Seaweed Farm",  new Blueprint("Seaweed Farm", 3, 10, TileData.Types.farm_s, "Plant seed, harvest food!")},
            {"Storage", new Blueprint("Storage", 3, 10, TileData.Types.storage, "Store things and send them to ship automatically!") },
            {"Machine Gun", new Blueprint("Machine Gun", 3, 10, TileData.Types.machine_gun, "Short distance, fast firing rate, single target acquisition.") }
        };

        //blueprints_all["Extractor"].

        //// Add +1 to maxBPAllowed since we are using it as a 0 based index below, and to take into account the Terraformer
       // maxBPAllowed += 1;

        // Always add the terraformer to the Selected Blueprints at its 0 index position
        //selectedBlueprints.Add(blueprints_all["Terraformer"]);
    }

    void InitExtractors()
    {
        extractorsMap = new Dictionary<string, Blueprint_Extraction>();
        extractorsMap.Add("Extractor", new Blueprint_Extraction(20, 10, 1, 100));
        extractorsMap.Add("Desalination Pump", new Blueprint_Extraction(20, 20, 1, 100));
    }

    void InitBattleTowers()
    {
        battleTowersMap = new Dictionary<string, Blueprint_Battle>();
        battleTowersMap.Add("Machine Gun", new Blueprint_Battle(12, 2, 0.8f, 5, 25, 2, 1, 0));
        battleTowersMap.Add("Sniper Gun", new Blueprint_Battle(12, 2, 1.5f, 8 ,25, 2, 1, 0));
        battleTowersMap.Add("Terraformer", new Blueprint_Battle(0, 0, 0, 0, 2500, 0, 2, 0));
    }

    public void GetExtractorStats(string id, Transform objTransform, ExtractionBuilding extractor, TileData.Types resourceType)
    {
        if (extractorsMap.ContainsKey(id))
        {
            Debug.Log("BP Database: Found stats for " + id);
            extractor.Init(resourceType, extractorsMap[id].extractorStats.extractRate, extractorsMap[id].extractorStats.extractPower,
                extractorsMap[id].extractorStats.extractAmmount, extractorsMap[id].extractorStats.personalStorageCapacity, objTransform);
        }
    }

    public void GetBattleStats(string id, Action<TowerGunStats> InitGunStats, Action<UnitStats> InitUnitStats)
    {
        if (battleTowersMap.ContainsKey(id))
        {
            Debug.Log("BP Database: Found stats for " + id);
            InitGunStats(battleTowersMap[id].battleStats);
            InitUnitStats(battleTowersMap[id].unitStats);
        }
    }

    public void GetBattleStats(string id, Action<UnitStats> InitUnitStats)
    {
        if (battleTowersMap.ContainsKey(id))
        {
            Debug.Log("BP Database: Found stats for " + id);
            InitUnitStats(battleTowersMap[id].unitStats);
        }
    }

    public TileStats GetTileStats(string id)
    {
        if (battleTowersMap.ContainsKey(id))
        {
            Debug.Log("BP DATABASE: Found initial tile stats for " + id);
            return battleTowersMap[id].tileStats;
        }
        else
        {
            return null;
        }
    }

    // This will Display the Selected Blueprint's info and store it's info in case the Player decides to Load it to the Builder
    public void SelectBlueprint(string bpType)
    {
        if (blueprintsMap.ContainsKey(bpType))
        {
            curSelectedBP = blueprintsMap[bpType];

            DisplaySelectedBPInfo();
        }

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
        //if (curSelectedBP != null && selectedBlueprints.Count < maxBPAllowed)
        //{
        //    if (!selectedBlueprints.Contains(curSelectedBP))
        //    {
        //        Debug.Log("Adding this blueprint.");
        //        selectedBlueprints.Add(curSelectedBP);

        //        // Add to Builder
       
        //    }
         
        //}
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

        Debug.Log("Hero loaded blueprint for: " + curSelectedBP.buildingName);

    }

    void ReloadOnNanoBuilder()
    {
        if (hero_nanoBuilder.blueprintsMap.Count > 0)
        {
            foreach(TileData.Types btype in hero_nanoBuilder.bpTypes)
            {
                // Do not load the Terraformer since it is a Blueprint that is loaded by default
                if (btype == TileData.Types.terraformer)
                {
                    continue;
                }

                UI_Manager.Instance.AddBlueprintToBuilder(hero_nanoBuilder.blueprintsMap[btype].buildingName);
            }
        }
    }

    // Player presses the unload button
    public void UnLoadBlueprint()
    {
        if (curSelectedBP != null)
        {
            //if (selectedBlueprints.Contains(curSelectedBP))
            //{
            //    Debug.Log("Removing this blueprint.");
            //    selectedBlueprints.Remove(curSelectedBP);
            //}
            
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

    // UPGRADES:

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
