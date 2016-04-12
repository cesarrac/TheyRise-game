using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

public class NanoBuilding_Handler : MonoBehaviour {


    /* This will actually build any of the Available Blueprints in the Player's gear. When player right clicks anywhere to build,
    check what type of building the player might want to build and place that icon on the mouse to left click and build.*/

    public int nanoBots = 500;
    int curNanoBots = 0;

    //private AudioSource audio_source;
    //public AudioClip buildSound, nanoBotReturnSound;

    // This will need an array of active Blueprints that the player has selected when beaming down to the planet
    // The total ammount of blueprints allowed is set by the NanoBuilders Processing Units
    // For now I'm setting these to be an array of Blueprints containing the three basic battle towers... this means it will only affect
    // the player if they are building battle buildings NOT extractors and such.
    public TileData.Types[] availableBlueprintsTypes;
    //public Dictionary<TileData.Types, Blueprint> availableBlueprints;

    // public string[] bluePrintsAvailable;
    int selectedBPIndex = 0;
    Blueprint selectedBluePrint;
    
    Build_MainController build_controller;

    Unit_StatusIndicator status_indicator;

    NanoBuilder NanoBuilder;

    void Awake()
    {
        status_indicator = GetComponent<Player_HeroAttackHandler>().statusIndicator;
    }

    void Start()
    {
        // InitBluePrints();
        //BlueprintDatabase.Instance.LoadToNanoBuilder(this);

        NanoBuilder = GameMaster.Instance.theHero.nanoBuilder;

        InitBP();

       // audio_source = GetComponent<AudioSource>();

        build_controller = Build_MainController.Instance;
        build_controller.nanoBuild_handler = this;

    }


    public void InitBP()
    {
        availableBlueprintsTypes = NanoBuilder.bpTypes.ToArray();

        SetBPSprites();

    }

    //public void AddBlueprint(int index, TileData.Types bp_type, Blueprint bp)
    //{
    //    availableBlueprintsTypes[index] = bp_type;
    //    availableBlueprints.Add(bp_type, bp);
    //}

    void SetBPSprites()
    {
        Buildings_SpriteDatabase.Instance.SetSprites(NanoBuilder.blueprintsMap);

        // Since this is the last step in initializing the Blueprints, set this here

        //selectedBluePrint = availableBlueprints[TileData.Types.terraformer];
        selectedBluePrint = NanoBuilder.blueprintsMap[TileData.Types.machine_gun];

        if (selectedBluePrint == null)
        {
            Debug.LogError("NANO B: The Selected Blueprint is null! WTF?! Did the GM loose track of the Hero?!");
        }

        // DisplaySelectedBlueprintName(selectedBluePrint.buildingName);

        CreateBuildButtons();
    }

    //// Display Blueprint name as a string on the bottom of the screen
    //void DisplaySelectedBlueprintName(string bpName)
    //{
    //    Player_UIHandler.instance.DisplayBlueprint(bpName);
    //}

    void CreateBuildButtons()
    {
        foreach(TileData.Types tileType in availableBlueprintsTypes)
        {
            UI_Manager.Instance.CreateBuildButton(NanoBuilder.blueprintsMap[tileType].buildingName,
                                                  tileType,
                                                  BuildHalfTile);
        }

        // After creating all build buttons make sure to adjust the build button panel's size
        UI_Manager.Instance.AdjustBuildPanelSize();
    }

    public void SelectBP(TileData.Types tileType)
    {
        foreach (TileData.Types ttype in availableBlueprintsTypes)
        {
            if (ttype == tileType)
                selectedBluePrint = NanoBuilder.blueprintsMap[ttype];
        }
    }

    void Update()
    {
        // For Swapping Current Blueprint for next one:
        //ListenForBPSwapButton();
        //if (build_controller.currentlyBuilding)
        //    Click_Build();

        // For Breaking a building back into Nanobots:
        ListenToBreakBuilding();

        if (curNanoBots != nanoBots)
        {
            KeepNanoBotsUpdated();
        }
    }

    void KeepNanoBotsUpdated()
    {
        
        curNanoBots = nanoBots;

        Player_UIHandler.instance.DisplayNanoBotCount(curNanoBots);

    }

    //void ListenForBPSwapButton()
    //{   // Press Swap button to cycle through the array of Blue Prints from the index currently selected or back to 0
    //    if (Input.GetButtonDown("Swap"))
    //    {
    //        SwapSelectedBP();
    //    }

    //    //Debug.Log(selectedBluePrint);
    //}

    //void SwapSelectedBP()
    //{
    //    if (selectedBPIndex < availableBlueprintsTypes.Length)
    //    {
    //        if (NanoBuilder.CheckForBlueprint(availableBlueprintsTypes[selectedBPIndex]))
    //        {
    //            selectedBluePrint = NanoBuilder.blueprintsMap[availableBlueprintsTypes[selectedBPIndex]];
    //        }

    //        selectedBPIndex += 1;
    //    }
    //    else
    //    {
    //        if (NanoBuilder.CheckForBlueprint(availableBlueprintsTypes[0]))
    //        {
    //            selectedBluePrint = NanoBuilder.blueprintsMap[availableBlueprintsTypes[0]];
    //            selectedBPIndex = 0;
    //        }
    //        else
    //        {
    //            selectedBluePrint = NanoBuilder.blueprintsMap[availableBlueprintsTypes[1]];
    //            selectedBPIndex = 1;
    //        }

    //    }

    //   // DisplaySelectedBlueprintName(selectedBluePrint.buildingName);
    //   // Debug.Log("NANO Buid: Selected Blueprint name is " + selectedBluePrint.buildingName);
    //}

    /// <summary>
    /// Determines what type of building player wants to build
    /// by using the tile's type.
    /// </summary>
    /// <param name="_tileType"></param>
    public void GetBuildingFromTileUnderMouse(TileData.Types _tileType)
    {
        // TODO: Subtract the nanobot cost of this blueprint
        //Building_UIHandler building_handler = Building_UIHandler.BuildingHandler;
        Build_MainController build_controller = Build_MainController.Instance;
        switch (_tileType)
        {
            case TileData.Types.rock:
                Blueprint extractor = GetAvailableBlueprint(TileData.Types.extractor);
                Build(extractor);
                break;

            case TileData.Types.mineral:
                Blueprint generator = GetAvailableBlueprint(TileData.Types.generator);
                Build(generator);
                break;

            case TileData.Types.empty:
                Debug.Log("NANO B: Building on empty!");
                Build(selectedBluePrint);
                break;

            case TileData.Types.water:
                Blueprint waterPump = GetAvailableBlueprint(TileData.Types.desalt_s);
                Build(waterPump);
                break;

            default:
                Debug.Log("Cant build on that type of tile!");
                break;
        }

       // Debug.Log("Nanobots left: " + nanoBots);
    }

    void BuildHalfTile(TileData.Types tileType)
    {
        if (NanoBuilder.blueprintsMap.ContainsKey(tileType))
        {
            if (CheckBuildCost(NanoBuilder.blueprintsMap[tileType]) == false)
            {
                build_controller.SetCurrentlyBuildingBool(false);
                Sound_Manager.Instance.PlaySound("Empty");
                return;
            }
            else
            {
                // Can Build!
                build_controller.SetCurrentlyBuildingBool(true);
                build_controller.BuildThis(NanoBuilder.blueprintsMap[tileType]);
            }
        }
    }

    void Build(Blueprint bp)
    {
        //if (bp != null && ResourceGrid.Grid.terraformer_built)
        if (bp != null)
        {
            //Debug.Log("BP requires " + bp.buildReq.reqResourcesMap.Count + " resources.");
            if (CheckBuildCost(bp) == false)
            {
                build_controller.SetCurrentlyBuildingBool(false);
                Sound_Manager.Instance.PlaySound("Empty");
                return;
            }
            else
            {
                // Can Build!
                build_controller.BuildThis(bp);
               // ChargeBuildResources(bp);
            }
        }
        else
        {
            //status_indicator.CreateStatusMessage("Can't build that yet!");
            Debug.Log("BP = " +  bp);
        }
           
    }

    public bool CheckBuildCost(Blueprint bp, int multiplier = 1)
    {
        // Check cost from the blueprint's required resources
        foreach (TileData.Types resource in bp.buildReq.reqResourcesMap.Keys)
        {
            if (Ship_Inventory.Instance.CheckForResourceByAmmnt(resource, bp.buildReq.reqResourcesMap[resource] * multiplier) == false)
            {
                UI_Manager.Instance.DisplayBuildWarning(resource);
                return false;
            }
        }

        return true;
    }

    public void ChargeBuildResources(Blueprint bp)
    {
        foreach (TileData.Types resource in bp.buildReq.reqResourcesMap.Keys)
        {
            Ship_Inventory.Instance.ChargeResourcesFromTemp(resource, bp.buildReq.reqResourcesMap[resource]);
        }
    }

    public void ReturnBuildResources(TileData.Types bpTileType, int multiplier = 1)
    {
        if (GetAvailableBlueprint(bpTileType) != null)
        {
            Blueprint bp = GetAvailableBlueprint(bpTileType);

            foreach (TileData.Types resource in bp.buildReq.reqResourcesMap.Keys)
            {
                if (bp.buildReq.reqResourcesMap[resource] <= 0)
                    continue;

                if (resource == TileData.Types.rock)
                {
                    Ship_Inventory.Instance.ReceiveTempRock(bp.buildReq.reqResourcesMap[resource] * multiplier, Rock.RockProductionType.steel);
                }
                else
                {
                    Ship_Inventory.Instance.ReceiveTemporaryResources(resource, bp.buildReq.reqResourcesMap[resource] * multiplier);
                }
            }
        }
    }

    void BuildTerraformer()
    {
        build_controller.BuildThis(selectedBluePrint);
        TerraformerHasBeenBuilt();
    }

    void ListenToBreakBuilding()
    {
        if (Input.GetButtonDown("Break"))
        {
            // Check the tile mouse is on and all 8 tiles around it for a building
            TileData tileUnderMouse = Mouse_Controller.Instance.GetTileUnderMouse();
            if (tileUnderMouse.tileType != TileData.Types.empty && tileUnderMouse.tileType != TileData.Types.rock && tileUnderMouse.tileType != TileData.Types.mineral && tileUnderMouse.tileType != TileData.Types.water && tileUnderMouse.tileType != TileData.Types.capital)
            {
                GameObject building = ResourceGrid.Grid.GetTileGameObjFromIntCoords(tileUnderMouse.posX, tileUnderMouse.posY);
                BreakThisBuilding(tileUnderMouse.tileType, building);
            }
        }
    }

    public void BreakThisBuilding (TileData.Types _type, GameObject building)
    {
        // get me the cost of this building by finding its blueprint
        Blueprint bp = GetAvailableBlueprint(_type);
        Building_Handler b_Handler = building.GetComponent<Building_Handler>();
        if (bp != null)
            b_Handler.BreakBuilding();
    }

    public Blueprint GetAvailableBlueprint(TileData.Types _type)
    {
        if (NanoBuilder.CheckForBlueprint(_type))
        {
            return NanoBuilder.blueprintsMap[_type];
        }
        else
            return null;
    }

    void TerraformerHasBeenBuilt()
    {
        // Make the Terraformer blueprint unavailable
        if (NanoBuilder.CheckForBlueprint(TileData.Types.terraformer))
        {
            NanoBuilder.RemoveBlueprint(TileData.Types.terraformer);
        }

        selectedBPIndex += 1;
       // SwapSelectedBP();
    }
}
