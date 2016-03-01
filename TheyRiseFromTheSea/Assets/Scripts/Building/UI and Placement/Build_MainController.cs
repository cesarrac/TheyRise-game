using UnityEngine;

class Build_MainController:MonoBehaviour
{
    public static Build_MainController Instance { get; protected set; }
    public bool currentlyBuilding { get; protected set; }

    public NanoBuilding_Handler nanoBuild_handler;
    int currNanoBotCount;

    ObjectPool objPool;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        objPool = ObjectPool.instance;
    }
    void Update()
    {
        // Keeps the nanobot count updated to the NanoBuilding_Handler's nanobot count
        if (nanoBuild_handler)
        {
            if (currNanoBotCount != nanoBuild_handler.nanoBots)
            {
                currNanoBotCount = nanoBuild_handler.nanoBots;
            }
        }
    }
    public void BuildThis(Blueprint building)
    {
        // Set currBuilding to true to lock building options
        currentlyBuilding = true;

        // Mouse position so I can instantiate on the mouse!
        Vector3 m = Mouse_Controller.MouseController.currMouseP;
        Vector3 spawnPos = new Vector3(Mathf.Round(m.x), Mathf.Round(m.y), 0.0f);

        // At this point NanoBuild_handler should have checked if there's ENOUGH nanobots to build AND subtracted the NANOBOTS necessary, so just build!
        string halfName = "half_Built";

        // First: Load an icon representing the Building so player can decide if that's what they want or not.
        GameObject halfBuilt = objPool.GetObjectForType(halfName, true, spawnPos);

        // To get the sprite we can have a database of sprites that is ONLY filled with the sprites that the player has blueprints for building.
       // Sprite theSprite = BuildingSprite_Manager.Instance.GetSprite(building.buildingName);
       // theSprite.bounds.size
        halfBuilt.GetComponent<SpriteRenderer>().sprite = BuildingSprite_Manager.Instance.GetSprite(building.buildingName);


        Building_PositionHandler bPosHand = halfBuilt.GetComponent<Building_PositionHandler>();
        bPosHand.SetCurrentBlueprintID(building.buildingName);
        bPosHand.spawnPos = spawnPos;
        bPosHand.followMouse = true;
        bPosHand.tileType = building.tileType;
        bPosHand.currNanoBotCost = building.nanoBotCost;
        bPosHand.nanoBuild_handler = nanoBuild_handler;
       
    }

    //public void BuildTerraformer()
    //{
    //    // Set currBuilding to true to lock building options
    //    currentlyBuilding = true;

    //    // Mouse position so I can instantiate on the mouse!
    //    Vector3 m = MouseBuilding_Controller.MouseController.currMouseP;
    //    Vector3 spawnPos = new Vector3(Mathf.Round(m.x), Mathf.Round(m.y), 0.0f);

    //    // At this point NanoBuild_handler should have checked if there's ENOUGH nanobots to build AND subtracted the NANOBOTS necessary, so just build!
    //    string halfName = "half_Built";

    //    // First: Load an icon representing the Building so player can decide if that's what they want or not.
    //    GameObject halfBuilt = objPool.GetObjectForType(halfName, true, spawnPos);

    //    // To get the sprite we can have a database of sprites that is ONLY filled with the sprites that the player has blueprints for building.
    //    // Sprite theSprite = BuildingSprite_Manager.Instance.GetSprite(building.buildingName);
    //    // theSprite.bounds.size
    //    halfBuilt.GetComponent<SpriteRenderer>().sprite = BuildingSprite_Manager.Instance.GetSprite("Terraformer");


    //    Building_PositionHandler bPosHand = halfBuilt.GetComponent<Building_PositionHandler>();
    //    bPosHand.spawnPos = spawnPos;
    //    bPosHand.followMouse = true;
    //    bPosHand.tileType = TileData.Types.terraformer;
    //    bPosHand.currNanoBotCost = 0;
    //    bPosHand.nanoBuild_handler = nanoBuild_handler;
    //}

    public void SetCurrentlyBuildingBool(bool newValue)
    {
        currentlyBuilding = newValue;
    }
}
