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
        buildingSprites = new BuildingSprite[8];

        // Machine Gun
        buildingSprites[0] = new BuildingSprite("Machine Gun", buildings[0]);
        Debug.Log("Machine gun's sprite size: " + buildingSprites[0].sprite.bounds.size);
        // Sniper Gun
        buildingSprites[1] = new BuildingSprite("Sniper Gun", buildings[10]);
        // Cannons
        buildingSprites[2] = new BuildingSprite("Cannons", buildings[10]);
        // Sea-Witch Crag
        buildingSprites[3] = new BuildingSprite("Sea-Witch Crag", buildings[7]);
        // Extractor
        buildingSprites[4] = new BuildingSprite("Extractor", buildings[5]);
        // Water Pump
        buildingSprites[5] = new BuildingSprite("Desalination Pump", buildings[5]);
        //Energy Generator
        buildingSprites[6] = new BuildingSprite("Energy Generator", buildings[5]);
        // Seaweed Farm
        buildingSprites[7] = new BuildingSprite("Seaweed Farm", buildings[8]);


    }

    // Once the blueprints have been set, GM would call this method before spawning the hero
    public void SetSprites(Blueprint[] blueprints)
    {
        List<BuildingSprite> bSprites = new List<BuildingSprite>();

        // For each blueprint
        foreach(Blueprint bp in blueprints)
        {
            // Check if this BP name matches any Building sprite in my array
            foreach (BuildingSprite bs in buildingSprites)
            {
                if (bp.buildingName == bs.name)
                {
                    // add it to available sprites
                    bSprites.Add(bs);
                }
            }
        }

        // These are now the available sprites with matching building names, restricted to Blueprints available, to use by this level's building managers
        availableSprites = bSprites.ToArray();

        BuildingSprite_Manager.Instance.buildingSprites = availableSprites;

        // Now that sprites are set, Destoy this database
        Destroy(this.gameObject);
    }
}


