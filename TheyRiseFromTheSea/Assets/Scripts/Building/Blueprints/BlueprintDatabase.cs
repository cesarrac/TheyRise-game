using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BlueprintDatabase : MonoBehaviour {

    public static BlueprintDatabase Instance { get; protected set; }

    // FIX ALL OF THIS! Eventually this should read from an external database. For now I'm going to hardcode all the blueprints in the game here.

    Dictionary<string, Blueprint> blueprints_all;

    // FIX THIS! 
    public int maxBPAllowed = 3;

    List<Blueprint> selectedBlueprints = new List<Blueprint>();

    public int shipLvlIndex = 0, planetLvlIndex = 1, inventoryLvlIndex = 2;

    Blueprint curSelectedBP;

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


        if (Application.loadedLevel == inventoryLvlIndex)
        {
            selectedBlueprints.Clear();

            Init();
        }

        //if (Application.loadedLevel == shipLvlIndex)
        //{
        //    selectedBlueprints.Clear();

        //    Init();

        //    //Test();
        //}

        //if (Application.loadedLevel == planetLvlIndex)
        //{
        //    selectedBlueprints.Clear();

        //    Init();
        //}

    }

    void Test()
    {
        selectedBlueprints.Add(blueprints_all["Machine Gun"]);
        selectedBlueprints.Add(blueprints_all["Cannons"]);
        selectedBlueprints.Add(blueprints_all["Extractor"]);

    }

    void Init()
    {
        blueprints_all = new Dictionary<string, Blueprint>
        {
            {"Terraformer", new Blueprint("Terraformer", 0, 0, TileData.Types.terraformer) }, // < ---- Always include the Terraformer and no cost
            {"Sniper Gun", new Blueprint("Sniper Gun", 3, 10, TileData.Types.sniper) },
            {"Cannons", new Blueprint("Cannons", 3, 10, TileData.Types.cannons)},
            {"Sea-Witch Crag", new Blueprint("Sea-Witch Crag", 3, 10, TileData.Types.seaWitch) },
            {"Extractor",  new Blueprint("Extractor", 3, 10, TileData.Types.extractor) },
            {"Desalination Pump", new Blueprint("Desalination Pump", 3, 10, TileData.Types.desalt_s) },
            {"Energy Generator", new Blueprint("Energy Generator", 3, 10, TileData.Types.generator) },
            {"Seaweed Farm",  new Blueprint("Seaweed Farm", 3, 10, TileData.Types.farm_s)},
            {"Storage", new Blueprint("Storage", 3, 10, TileData.Types.storage) },
            {"Machine Gun", new Blueprint("Machine Gun", 3, 10, TileData.Types.machine_gun) }
        };

        //// Add +1 to maxBPAllowed since we are using it as a 0 based index below, and to take into account the Terraformer
        maxBPAllowed += 1;

        // Always add the terraformer to the Selected Blueprints at its 0 index position
        selectedBlueprints.Add(blueprints_all["Terraformer"]);
    }

    // This will Display the Selected Blueprint's info and store it's info in case the Player decides to Load it to the Builder
    public void SelectBlueprint(string bpType)
    {
        if (blueprints_all.ContainsKey(bpType))
        {
            curSelectedBP = blueprints_all[bpType];

            DisplaySelectedBPInfo();
        }

    }

    void DisplaySelectedBPInfo()
    {
        // Display the info to the Info Panel from the stored selected BP (curSelectedBP)
        UI_Manager.Instance.DisplayBPInfo(curSelectedBP.buildingName);
    }

    public void LoadBlueprint()
    {
        if (curSelectedBP != null && selectedBlueprints.Count < maxBPAllowed)
        {
            if (!selectedBlueprints.Contains(curSelectedBP))
            {
                Debug.Log("Adding this blueprint.");
                selectedBlueprints.Add(curSelectedBP);

                // Add Text field to Builder
                AddLoadedBlueprint();
            }
         
        }
    }

    public void UnLoadBlueprint()
    {
        if (curSelectedBP != null && selectedBlueprints.Count > 0)
        {
            if (selectedBlueprints.Contains(curSelectedBP))
            {
                Debug.Log("Removing this blueprint.");
                selectedBlueprints.Remove(curSelectedBP);
            }

            // Remove the Text field from the Builder
            RemoveLoadedBlueprint(curSelectedBP.buildingName);
        }
    }

    void AddLoadedBlueprint()
    {
        // Spawn a Text prefab, fill its text with the correct blueprint name, and parent it to the Builder's panel
        UI_Manager.Instance.AddBlueprintToBuilder(curSelectedBP.buildingName);
       
    }

    void RemoveLoadedBlueprint(string bpName)
    {
        UI_Manager.Instance.RemoveBlueprintTextFromBuilder(bpName);
    }

    public void LoadToNanoBuilder(NanoBuilding_Handler nano_builder)
    {
        if (selectedBlueprints.Count == maxBPAllowed)
        {
            // Init the Nano builder's blueprint array and dict
            nano_builder.InitBP(maxBPAllowed);

            // Load the terraformer first
            nano_builder.AddBlueprint(0, selectedBlueprints[0].tileType, selectedBlueprints[0]);

            // Start i at 1 since the 0 position is taken by the Terraformer
            for (int i = 1; i < maxBPAllowed; i++)
            {
                nano_builder.AddBlueprint(i, selectedBlueprints[i].tileType, selectedBlueprints[i]);
            }

            nano_builder.SetBPSprites();
        }
    
    }
}
