using UnityEngine;
using System.Collections;

public class NanoBuilding_Handler : MonoBehaviour {


	/* This will actually build any of the Available Blueprints in the Player's gear. When player right clicks anywhere to build,
    check what type of building the player might want to build and place that icon on the mouse to left click and build.*/

	public Building_UIHandler building_handler;

    ResourceGrid resource_grid;


	public int nanoBots = 50;

    private AudioSource audio_source;
    public AudioClip buildSound, nanoBotReturnSound;

	void Awake ()
    {

        resource_grid = GetComponent<Player_MoveHandler>().resourceGrid;
        building_handler = resource_grid.buildingUIHandler;


        audio_source = GetComponent<AudioSource>();
     
	}
	void Start()
    {
        // if for some reason the grid object is null, try getting it again
        if (!resource_grid)
        {
            resource_grid = GetComponent<Player_MoveHandler>().resourceGrid;
            building_handler = resource_grid.buildingUIHandler;
        }
    }
     

	void Update () {
        ListenFourRightClick();
    }

    void ListenFourRightClick()
    {
        if (Input.GetMouseButtonDown(1) && !building_handler.currentlyBuilding)
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            int mX = Mathf.RoundToInt(mousePosition.x);
            int mY = Mathf.RoundToInt(mousePosition.y);

            if (resource_grid.CheckIsInMapBounds(mX, mY))
            {
;               GetBuildingFromType(resource_grid.GetTileType(mX, mY));

                // Play build sound
                audio_source.PlayOneShot(buildSound, 0.5f);
            }

        }
    }

    void GetBuildingFromType(TileData.Types _tileType)
    {
        switch (_tileType)
        {
            case TileData.Types.rock:
                building_handler.BuildThis("Extractor");
                break;
            case TileData.Types.mineral:
                building_handler.BuildThis("Extractor");
                break;
            case TileData.Types.empty:
                building_handler.BuildThis("Machine Gun");
                //TODO: Add an option here to select from the different battle towers the player has loaded as blueprints
                break;
            case TileData.Types.water:
                building_handler.BuildThis("Desalination Pump");
                break;
            default:
                Debug.Log("Cant build on that type of tile!");
                break;
        }
    }
}
