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


    //void InitBluePrints()
    //{
    //    availableBlueprintsTypes = new TileData.Types[10] {TileData.Types.terraformer, TileData.Types.sniper,  TileData.Types.cannons , TileData.Types.seaWitch,
    //        TileData.Types.extractor, TileData.Types.desalt_s,TileData.Types.generator,TileData.Types.farm_s, TileData.Types.storage,TileData.Types.machine_gun};

    //    availableBlueprints = new Dictionary<TileData.Types, Blueprint>
    //    {
    //        {TileData.Types.terraformer, new Blueprint("Terraformer", 0, 0, TileData.Types.terraformer) }, // < ---- Always include the Terraformer and no cost
    //        {TileData.Types.sniper, new Blueprint("Sniper Gun", 3, 10, TileData.Types.sniper) },
    //        { TileData.Types.cannons, new Blueprint("Cannons", 3, 10, TileData.Types.cannons)},
    //        {TileData.Types.seaWitch, new Blueprint("Sea-Witch Crag", 3, 10, TileData.Types.seaWitch) },
    //        {TileData.Types.extractor,  new Blueprint("Extractor", 3, 10, TileData.Types.extractor) },
    //        {TileData.Types.desalt_s, new Blueprint("Desalination Pump", 3, 10, TileData.Types.desalt_s) },
    //        {TileData.Types.generator, new Blueprint("Energy Generator", 3, 10, TileData.Types.generator) },
    //        {TileData.Types.farm_s,  new Blueprint("Seaweed Farm", 3, 10, TileData.Types.farm_s)},
    //        {TileData.Types.storage, new Blueprint("Storage", 3, 10, TileData.Types.storage) },
    //        {TileData.Types.machine_gun, new Blueprint("Machine Gun", 3, 10, TileData.Types.machine_gun) }
    //    };

      
    //                                                         // FIX THIS !!!!!! FOR NOW im forcing this blueprints array unto the sprite database from here
    //    Buildings_SpriteDatabase.Instance.SetSprites(availableBlueprints);
    //}

    public void InitBP()
    {
        //availableBlueprintsTypes = new TileData.Types[totalBPavailable];
        //availableBlueprints = new Dictionary<TileData.Types, Blueprint>();
        //availableBlueprints = GameMaster.Instance.theHero.nanoBuilder.blueprintsMap;
        //availableBlueprintsTypes = GameMaster.Instance.theHero.nanoBuilder.bpTypes.ToArray();

        availableBlueprintsTypes = NanoBuilder.bpTypes.ToArray();

        Debug.Log("NANO B: First BP in available blueprints types is " + availableBlueprintsTypes[0].ToString());
        Debug.Log("NANO B: The Hero's nanobuilder currently has " + NanoBuilder.blueprintsMap.Count + " blueprints loaded.");
        

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
        selectedBluePrint = NanoBuilder.blueprintsMap[Mission_Manager.Instance.ActiveMission.RequiredBlueprint.tileType];

        if (selectedBluePrint != null)
        {
            Debug.Log("NANO B: Selected blueprint's type is currently " + selectedBluePrint.tileType);
        }
        else
        {
            Debug.LogError("NANO B: The Selected Blueprint is null! WTF?! Did the GM loose track of the Hero?!");
        }

        DisplaySelectedBlueprintName(selectedBluePrint.buildingName);
    }

    void DisplaySelectedBlueprintName(string bpName)
    {
        Player_UIHandler.instance.DisplayBlueprint(bpName);
    }

    void Update()
    {
        // For Building:
        if (!build_controller.currentlyBuilding)
            ListenForRightClick();
        // For Swapping Current Blueprint for next one:
        ListenForBPSwapButton();
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

    void ListenForRightClick()
    {
        
        //if (Input.GetMouseButtonDown(1))
        //{
            
        //    GetBuildingFromType(MouseBuilding_Controller.MouseController.GetTileUnderMouse().tileType);

        //    // Play build sound
        //    audio_source.PlayOneShot(buildSound, 0.5f);
        //}
        if (Mouse_Controller.MouseController.isRightClickingForBuilding)
        {
            GetBuildingFromType(Mouse_Controller.MouseController.GetTileUnderMouse().tileType);

            // Play build sound
            // audio_source.PlayOneShot(buildSound, 0.5f);
            Sound_Manager.Instance.PlaySound("Build");
        }
      
    }

    void ListenForBPSwapButton()
    {   // Press Swap button to cycle through the array of Blue Prints from the index currently selected or back to 0
        if (Input.GetButtonDown("Swap"))
        {
            SwapSelectedBP();
        }

        //Debug.Log(selectedBluePrint);
    }

    void SwapSelectedBP()
    {
        if (selectedBPIndex < availableBlueprintsTypes.Length)
        {
            if (NanoBuilder.CheckForBlueprint(availableBlueprintsTypes[selectedBPIndex]))
            {
                selectedBluePrint = NanoBuilder.blueprintsMap[availableBlueprintsTypes[selectedBPIndex]];
            }

            selectedBPIndex += 1;
        }
        else
        {
            if (NanoBuilder.CheckForBlueprint(availableBlueprintsTypes[0]))
            {
                selectedBluePrint = NanoBuilder.blueprintsMap[availableBlueprintsTypes[0]];
                selectedBPIndex = 0;
            }
            else
            {
                selectedBluePrint = NanoBuilder.blueprintsMap[availableBlueprintsTypes[1]];
                selectedBPIndex = 1;
            }

        }

        DisplaySelectedBlueprintName(selectedBluePrint.buildingName);
        Debug.Log("NANO Buid: Selected Blueprint name is " + selectedBluePrint.buildingName);
    }

    /// <summary>
    /// Determines what type of building player wants to build
    /// by using the tile's type.
    /// </summary>
    /// <param name="_tileType"></param>
    public void GetBuildingFromType(TileData.Types _tileType)
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
                if (selectedBluePrint.tileType != TileData.Types.terraformer)
                {
                    Build(selectedBluePrint);
                }
                else
                {
                    BuildTerraformer();
                }
                break;

            case TileData.Types.water:
                Blueprint waterPump = GetAvailableBlueprint(TileData.Types.desalt_s);
                Build(waterPump);
                break;

            default:
                Debug.Log("Cant build on that type of tile!");
                break;
        }

        Debug.Log("Nanobots left: " + nanoBots);
    }

    void Build(Blueprint bp)
    {
        //if (bp != null && ResourceGrid.Grid.terraformer_built)
        if (bp != null)
        {
            // Check Nanobot cost
            if (nanoBots >= bp.nanoBotCost)
            {
                // Can Build!
                build_controller.BuildThis(bp);
                nanoBots -= bp.nanoBotCost;
            }
            else
            {
                status_indicator.CreateStatusMessage("Out of Bots!");
                Sound_Manager.Instance.PlaySound("Empty");

            }

        }
        else
        {
            status_indicator.CreateStatusMessage("Can't build that yet!");
            Debug.Log("BP = " +  bp);
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
            TileData tileUnderMouse = Mouse_Controller.MouseController.GetTileUnderMouse();
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
        Building_ClickHandler b_Handler = building.GetComponent<Building_ClickHandler>();
        if (bp != null)
            b_Handler.BreakBuilding(bp.nanoBotCost);
    }

    Blueprint GetAvailableBlueprint(TileData.Types _type)
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
        SwapSelectedBP();
    }
}
