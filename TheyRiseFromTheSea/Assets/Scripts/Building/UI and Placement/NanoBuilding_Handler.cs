using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

public class NanoBuilding_Handler : MonoBehaviour {


    /* This will actually build any of the Available Blueprints in the Player's gear. When player right clicks anywhere to build,
    check what type of building the player might want to build and place that icon on the mouse to left click and build.*/

    public int nanoBots = 500;

    private AudioSource audio_source;
    public AudioClip buildSound, nanoBotReturnSound;

    // This will need an array of active Blueprints that the player has selected when beaming down to the planet
    // The total ammount of blueprints allowed is set by the NanoBuilders Processing Units
    // For now I'm setting these to be an array of Blueprints containing the three basic battle towers... this means it will only affect
    // the player if they are building battle buildings NOT extractors and such.
    public TileData.Types[] availableBlueprintsTypes;
    public Dictionary<TileData.Types, Blueprint> availableBlueprints;

    // public string[] bluePrintsAvailable;
    int selectedBPIndex = 0;
    Blueprint selectedBluePrint;

    GameObject bpPanel;
    Text bluePrintTextBox;

    Build_MainController build_controller;

    void Start()
    {
        InitBluePrints();
        bpPanel = GameObject.FindGameObjectWithTag("BP Panel");
        bluePrintTextBox = bpPanel.GetComponentsInChildren<Text>()[1];

        selectedBluePrint = availableBlueprints[TileData.Types.terraformer];

        bluePrintTextBox.text = selectedBluePrint.buildingName;

        audio_source = GetComponent<AudioSource>();

        build_controller = Build_MainController.Instance;
        build_controller.nanoBuild_handler = this;

    }


    void InitBluePrints()
    {
        availableBlueprintsTypes = new TileData.Types[10] {TileData.Types.terraformer, TileData.Types.sniper,  TileData.Types.cannons , TileData.Types.seaWitch,
            TileData.Types.extractor, TileData.Types.desalt_s,TileData.Types.generator,TileData.Types.farm_s, TileData.Types.storage,TileData.Types.machine_gun};

        availableBlueprints = new Dictionary<TileData.Types, Blueprint>
        {
            {TileData.Types.terraformer, new Blueprint("Terraformer", 0, 0, TileData.Types.terraformer) }, // < ---- Always include the Terraformer and no cost
            {TileData.Types.sniper, new Blueprint("Sniper Gun", 3, 10, TileData.Types.sniper) },
            { TileData.Types.cannons, new Blueprint("Cannons", 3, 10, TileData.Types.cannons)},
            {TileData.Types.seaWitch, new Blueprint("Sea-Witch Crag", 3, 10, TileData.Types.seaWitch) },
            {TileData.Types.extractor,  new Blueprint("Extractor", 3, 10, TileData.Types.extractor) },
            {TileData.Types.desalt_s, new Blueprint("Desalination Pump", 3, 10, TileData.Types.desalt_s) },
            {TileData.Types.generator, new Blueprint("Energy Generator", 3, 10, TileData.Types.generator) },
            {TileData.Types.farm_s,  new Blueprint("Seaweed Farm", 3, 10, TileData.Types.farm_s)},
            {TileData.Types.storage, new Blueprint("Storage", 3, 10, TileData.Types.storage) },
            {TileData.Types.machine_gun, new Blueprint("Machine Gun", 3, 10, TileData.Types.machine_gun) }
        };

      
                                                             // FIX THIS !!!!!! FOR NOW im forcing this blueprints array unto the sprite database from here
        Buildings_SpriteDatabase.Instance.SetSprites(availableBlueprints);
    }
	
	void Update () {
        // For Building:
        if (!build_controller.currentlyBuilding)
            ListenForRightClick();
        // For Swapping Current Blueprint for next one:
        ListenForBPSwapButton();
        // For Breaking a building back into Nanobots:
        ListenToBreakBuilding();
    }

    void ListenForRightClick()
    {
        
        if (Input.GetMouseButtonDown(1))
        {
            
            GetBuildingFromType(MouseBuilding_Controller.MouseController.GetTileUnderMouse().tileType);

            // Play build sound
            audio_source.PlayOneShot(buildSound, 0.5f);
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
            if (availableBlueprints.ContainsKey(availableBlueprintsTypes[selectedBPIndex]))
            {
                selectedBluePrint = availableBlueprints[availableBlueprintsTypes[selectedBPIndex]];
            }

            selectedBPIndex += 1;
        }
        else
        {
            if (availableBlueprints.ContainsKey(availableBlueprintsTypes[0]))
            {
                selectedBluePrint = availableBlueprints[availableBlueprintsTypes[0]];
                selectedBPIndex = 0;
            }
            else
            {
                selectedBluePrint = availableBlueprints[availableBlueprintsTypes[1]];
                selectedBPIndex = 1;
            }

        }

        bluePrintTextBox.text = bluePrintTextBox.text = selectedBluePrint.buildingName;
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
                Blueprint extractor = CheckForAvailableBlueprint(TileData.Types.extractor);
                if (extractor != null)
                {
                    if (nanoBots >= extractor.nanoBotCost)
                    {
                        build_controller.BuildThis(extractor);
                        nanoBots -= extractor.nanoBotCost;
                    }
                }
                
                
                break;
            case TileData.Types.mineral:
                Blueprint generator = CheckForAvailableBlueprint(TileData.Types.generator);
                if (generator != null)
                {
                    if (nanoBots >= generator.nanoBotCost)
                    {
                        build_controller.BuildThis(generator);
                        nanoBots -= generator.nanoBotCost;
                    }
                }
                break;

            case TileData.Types.empty:
                if (nanoBots >= selectedBluePrint.nanoBotCost)
                {
                    build_controller.BuildThis(selectedBluePrint);
                    nanoBots -= selectedBluePrint.nanoBotCost;
                    if (selectedBluePrint.tileType == TileData.Types.terraformer)
                    {
                        TerraformerHasBeenBuilt();
                    }
                }
                break;

            case TileData.Types.water:
                Blueprint waterPump = CheckForAvailableBlueprint(TileData.Types.desalt_s);
                if (waterPump != null)
                {
                    if (nanoBots >= waterPump.nanoBotCost)
                    {
                        build_controller.BuildThis(waterPump);
                        nanoBots -= waterPump.nanoBotCost;
                    }
                }

                break;
            default:
                Debug.Log("Cant build on that type of tile!");
                break;
        }

        Debug.Log("Nanobots left: " + nanoBots);
    }

    void ListenToBreakBuilding()
    {
        if (Input.GetButtonDown("Break"))
        {
            // Check the tile mouse is on and all 8 tiles around it for a building
            TileData tileUnderMouse = MouseBuilding_Controller.MouseController.GetTileUnderMouse();
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
        Blueprint bp = CheckForAvailableBlueprint(_type);
        Building_ClickHandler b_Handler = building.GetComponent<Building_ClickHandler>();
        if (bp != null)
            b_Handler.BreakBuilding(bp.nanoBotCost);
    }

    Blueprint CheckForAvailableBlueprint (TileData.Types _type)
    {
        if (availableBlueprints.ContainsKey(_type))
        {
            return availableBlueprints[_type];
        }
        else
            return null;
    }

    void TerraformerHasBeenBuilt()
    {
        // Make the Terraformer blueprint unavailable
        if (availableBlueprints.ContainsKey(TileData.Types.terraformer))
        {
            availableBlueprints.Remove(TileData.Types.terraformer);
        }

        selectedBPIndex += 1;
        SwapSelectedBP();
    }
}
