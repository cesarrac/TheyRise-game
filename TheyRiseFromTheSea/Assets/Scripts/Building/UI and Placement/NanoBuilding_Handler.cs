using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class NanoBuilding_Handler : MonoBehaviour {


    /* This will actually build any of the Available Blueprints in the Player's gear. When player right clicks anywhere to build,
    check what type of building the player might want to build and place that icon on the mouse to left click and build.*/

    public int nanoBots = 50;

    private AudioSource audio_source;
    public AudioClip buildSound, nanoBotReturnSound;

    // This will need an array of active Blueprints that the player has selected when beaming down to the planet
    // The total ammount of blueprints allowed is set by the NanoBuilders Processing Units
    // For now I'm setting these to be an array of Blueprints containing the three basic battle towers... this means it will only affect
    // the player if they are building battle buildings NOT extractors and such.
    public Blueprint[] availableBlueprints;

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

        selectedBluePrint = availableBlueprints[0];

        bluePrintTextBox.text = selectedBluePrint.buildingName;

        audio_source = GetComponent<AudioSource>();

        build_controller = Build_MainController.Instance;
        build_controller.nanoBuild_handler = this;

    }


    void InitBluePrints()
    {
        availableBlueprints = new Blueprint[9];
        availableBlueprints[0] = new Blueprint("Machine Gun", 3, 10, TileData.Types.machine_gun);
        availableBlueprints[1] = new Blueprint("Sniper Gun", 3, 10, TileData.Types.sniper);
        availableBlueprints[2] = new Blueprint("Cannons", 3, 10, TileData.Types.cannons);
        availableBlueprints[3] = new Blueprint("Sea-Witch Crag", 3, 10, TileData.Types.seaWitch);
        availableBlueprints[4] = new Blueprint("Extractor", 3, 10, TileData.Types.extractor);
        availableBlueprints[5] = new Blueprint("Desalination Pump", 3, 10, TileData.Types.desalt_s);
        availableBlueprints[6] = new Blueprint("Energy Generator", 3, 10, TileData.Types.generator);
        availableBlueprints[7] = new Blueprint("Seaweed Farm", 3, 10, TileData.Types.farm_s);
        availableBlueprints[8] = new Blueprint("Storage", 3, 10, TileData.Types.storage);
        // FOR NOW im forcing this blueprints array unto the sprite database from here
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
            MouseBuilding_Controller.MouseController.GetTileUnderMouse();

            
            GetBuildingFromType(MouseBuilding_Controller.TileTypeUnderMouse);

            // Play build sound
            audio_source.PlayOneShot(buildSound, 0.5f);
        }
      
    }

    void ListenForBPSwapButton()
    {   // Press Swap button to cycle through the array of Blue Prints from the index currently selected or back to 0
        if (Input.GetButtonDown("Swap"))
        {
            if (selectedBPIndex + 1 < availableBlueprints.Length)
            {
                selectedBluePrint = availableBlueprints[selectedBPIndex + 1];
                selectedBPIndex += 1;
            }
            else
            {
                selectedBluePrint = availableBlueprints[0];
                selectedBPIndex = 0;
            }

            bluePrintTextBox.text = bluePrintTextBox.text = selectedBluePrint.buildingName;
        }

        //Debug.Log(selectedBluePrint);
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
            MouseBuilding_Controller mouse_controller = MouseBuilding_Controller.MouseController;

            //for (int x = -1; x <= 1; x++)
            //{
            //    for (int y = -1; y <= 1; y++)
            //    {
            //        mouse_controller.GetTileUnderMouse(x, y);
            //        TileData.Types tile = MouseBuilding_Controller.TileTypeUnderMouse;
            //        if (tile != TileData.Types.empty && tile != TileData.Types.rock && tile != TileData.Types.mineral && tile != TileData.Types.water && tile != TileData.Types.capital) 
            //        {

            //            mouse_controller.GetTileGObj();
            //            GameObject building = mouse_controller.tileUnderMouseAsGameObj;
            //            BreakThisBuilding(tile, building);
            //            break; 
            //        }
            //    }
            //}
            mouse_controller.GetTileUnderMouse();
            TileData.Types tile = MouseBuilding_Controller.TileTypeUnderMouse;
            if (tile != TileData.Types.empty && tile != TileData.Types.rock && tile != TileData.Types.mineral && tile != TileData.Types.water && tile != TileData.Types.capital)
            {

                mouse_controller.GetTileGObj();
                GameObject building = mouse_controller.tileUnderMouseAsGameObj;
                BreakThisBuilding(tile, building);
            }
        }
    }

    public void BreakThisBuilding (TileData.Types tileType, GameObject building)
    {
        // get me the cost of this building by finding its blueprint
        Blueprint bp = CheckForAvailableBlueprint(tileType);
        Building_ClickHandler b_Handler = building.GetComponent<Building_ClickHandler>();
        if (bp != null)
            b_Handler.BreakBuilding(bp.nanoBotCost);
    }

    Blueprint CheckForAvailableBlueprint (TileData.Types tileType)
    {
        for (int i = 0; i < availableBlueprints.Length; i++)
        {
            if (availableBlueprints[i].tileType == tileType)
            {
                return availableBlueprints[i];
            }
        }

        return new Blueprint();
    }
}
