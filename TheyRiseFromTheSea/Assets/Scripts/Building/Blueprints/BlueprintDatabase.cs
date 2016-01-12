using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BlueprintDatabase : MonoBehaviour {

    // FIX ALL OF THIS! Eventually this should read from an external database. For now I'm going to hardcode all the blueprints in the game here.

    Dictionary<string, Blueprint> blueprints_all;

    // FIX THIS! 
    public int maxBPallowed = 5;

    List<Blueprint> selectedBlueprints = new List<Blueprint>();

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

        // Add +1 to maxBPAllowed since we are using it as a 0 based index below
        maxBPallowed += 1;
    }

    public void SelectBlueprint(string bpType)
    {
        if (blueprints_all.ContainsKey(bpType) && selectedBlueprints.Count < maxBPallowed)
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
        if (selectedBlueprints.Count == maxBPallowed)
        {
            // Init the Nano builder's blueprint array and dict
            nano_builder.InitBP(maxBPallowed);

            for (int i = 0; i < maxBPallowed; i++)
            {
                nano_builder.AddBlueprint(i, selectedBlueprints[i].tileType, selectedBlueprints[i]);
            }

            nano_builder.SetBPSprites();
        }
    
    }
}
