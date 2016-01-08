using UnityEngine;
using System.Collections.Generic;

class Buildings_SpriteDatabase : MonoBehaviour
{
 // This database should only exist during Ship and Supply level. Once supplies are loaded and the Sprite manager is filled, this component can be destroyed.   
   
    public Sprite[] buildings { get; protected set; }
    public static Buildings_SpriteDatabase Instance { get; protected set; }
    public Blueprint[] availableBlueprints;


    BuildingSprite[] buildingSprites;
    public BuildingSprite[] availableSprites { get; protected set; }

    void Awake()
    {
        Instance = this;
        buildings = Resources.LoadAll<Sprite>("Sprites/Buildings/newBuildings3");
        InitSprites();
    }

  
    void InitSprites()
    {
        buildingSprites = new BuildingSprite[10];

        // Machine Gun
        buildingSprites[0] = new BuildingSprite("Machine Gun", buildings[0], TileData.Types.machine_gun);
        // Sniper Gun
        buildingSprites[1] = new BuildingSprite("Sniper Gun", buildings[10], TileData.Types.sniper);
        // Cannons
        buildingSprites[2] = new BuildingSprite("Cannons", buildings[10], TileData.Types.cannons);
        // Sea-Witch Crag
        buildingSprites[3] = new BuildingSprite("Sea-Witch Crag", buildings[7], TileData.Types.seaWitch);
        // Extractor
        buildingSprites[4] = new BuildingSprite("Extractor", buildings[5], TileData.Types.extractor);
        // Water Pump
        buildingSprites[5] = new BuildingSprite("Desalination Pump", buildings[5], TileData.Types.desalt_s);
        //Energy Generator
        buildingSprites[6] = new BuildingSprite("Energy Generator", buildings[5], TileData.Types.generator);
        // Seaweed Farm
        buildingSprites[7] = new BuildingSprite("Seaweed Farm", buildings[8], TileData.Types.farm_s);
        // Storage
        buildingSprites[8] = new BuildingSprite("Storage", buildings[8], TileData.Types.storage);
        // Terraformer
        buildingSprites[9] = new BuildingSprite("Terraformer", buildings[4], TileData.Types.terraformer);


    }

    // Once the blueprints have been set, GM would call this method before spawning the hero
    public void SetSprites(Dictionary<TileData.Types,Blueprint> blueprints)
    {
        List<BuildingSprite> bSprites = new List<BuildingSprite>();

        foreach (BuildingSprite bs in buildingSprites)
        {
            // Check if dictionary contains this building sprite name
            if (blueprints.ContainsKey(bs.tileType))
            {
                // If it does, add it as an available sprite
                bSprites.Add(bs);
            }
        }
    

        // These are now the available sprites with matching building names, restricted to Blueprints available, to use by this level's building managers
        availableSprites = bSprites.ToArray();

        BuildingSprite_Manager.Instance.buildingSprites = availableSprites;

        // Now that sprites are set, Destoy this database
        Destroy(this.gameObject);
    }
}


