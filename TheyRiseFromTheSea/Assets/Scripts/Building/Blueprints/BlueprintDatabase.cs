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


        //if (Application.loadedLevel == inventoryLvlIndex)
        //{
        //    selectedBlueprints.Clear();

        //    Init();
        //}

        if (Application.loadedLevel == shipLvlIndex)
        {
            selectedBlueprints.Clear();

            Init();

            Test();
        }

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

    public void SelectBlueprint(string bpType)
    {
        if (blueprints_all.ContainsKey(bpType) && selectedBlueprints.Count < maxBPAllowed)
        {
            Debug.Log("Adding this blueprint.");
            selectedBlueprints.Add(blueprints_all[bpType]);
        }
    }

    public void DeselectBlueprint(string bpType)
    {
        if (blueprints_all.ContainsKey(bpType) && selectedBlueprints.Count > 0)
        {
            Debug.Log("Removing this blueprint.");
            selectedBlueprints.Remove(blueprints_all[bpType]);
        }
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
