using UnityEngine;
using System.Collections;
using System;

public class DiscoverTile : MonoBehaviour {
	/// <summary>
	/// This script is called by Resource Grid when it needs to instantiate a tile.
	/// It gets the actual tile GameObject from Resource grid, spawns a grey tile and starts to dissolve it.
	/// The time it will take to disappear will be determined by mining time. (Which can be upgraded later)
	/// </summary>


	bool fading;

	SpriteRenderer sr;

    GameObject tileToSpawn, rockToSpawn;

	ResourceGrid resourceGrid;

	public ObjectPool objPool;

	public MasterState_Manager master_state;

	public Resource_Sprite_Handler r_sprite_handler;


    void Awake()
	{
        //		if (!r_sprite_handler) {
        //			r_sprite_handler = GameObject.FindGameObjectWithTag("Map").GetComponent<Resource_Sprite_Handler>();
        //		}
        resourceGrid = ResourceGrid.Grid;
	}

	void Start () {
		fading = true;
		sr = GetComponent<SpriteRenderer>();

		if (!r_sprite_handler)
			Debug.Log ("TILE: Dont have the Resource Sprite Handler!");
	}
	
	// Update is called once per frame
	void Update () {
		if (fading) {
			StartCoroutine(FadeOut());
		}
	}

	IEnumerator FadeOut(){
		// stop from calling again
		fading = false;
		//wait
		yield return new WaitForSeconds (0.6f);
		//fade a bit
		Fade ();
		
	}

	void Fade(){
		sr.color = new Color (sr.color.r, sr.color.g, sr.color.b, sr.color.a - 0.6f);
		if (sr.color.a <= 0) {
			Die ();
		} else {
			fading = true;
		}
	}
	void Die(){
        Destroy(gameObject);
	}

	public void TileToDiscover(string newTileName, int mapPosX, int mapPosY, Transform tileHolder, TileData.Types tileType, int spriteWidth = 0, int spriteHeight = 0){      // this is called by Resource grid with the proper tile obj
        if (resourceGrid == null)
            resourceGrid = ResourceGrid.Grid;

        // SPAWN A TILE AS A GAMEOBJECT:
        tileToSpawn = objPool.GetObjectForType (newTileName, false, Vector3.zero);

		if (tileToSpawn != null) {

			tileToSpawn.transform.position = transform.position;
			tileToSpawn.transform.parent = tileHolder;


            // IF TILE IS NOT EMPTY, IT'S A BUILDING,
			// so it will have a Building Click Handler that needs its pos X and pos Y
			if (tileType != TileData.Types.empty && tileType != TileData.Types.rock)
            {
				Building_Handler build_handler = tileToSpawn.GetComponent<Building_Handler> ();
                if (build_handler)
                {
                    build_handler.mapPosX = mapPosX;
                    build_handler.mapPosY = mapPosY;

                    if (tileType != TileData.Types.capital)
                    {
                        // Since we are SURE this is a building we can add a JOB for its construction here:
                        Job_Manager.Instance.AddJob(JobType.Assemble, tileType, tileToSpawn.transform);
                    }
  
                }
			

			} 


			if (tileType == TileData.Types.capital)
            {
                resourceGrid.transporterGObj = tileToSpawn;
				//// IF IT'S THE TERRAFORMER it will need the master state manager
				//Terraformer_Handler terra = tileToSpawn.GetComponent<Terraformer_Handler>();
    //            if (terra)
				//    terra.master_State = master_state;
			}

	

			// ADD this tile to the Grid's spawnedTiles array
            if (spriteWidth > 0 && spriteHeight > 0)
            {
                for (int w = -(spriteWidth - 1); w < spriteWidth; w++)
                {
                    for (int h = 0; h < spriteHeight; h++)
                    {
                        if (resourceGrid.spawnedTiles[mapPosX + w, mapPosY + h] == null)
                        {
                            resourceGrid.spawnedTiles[mapPosX + w, mapPosY + h] = tileToSpawn;
                        }
                      
                    }
                }

            }
            else
            {
			    resourceGrid.spawnedTiles [mapPosX, mapPosY] = tileToSpawn;
            }

		}
	}

    public void DiscoverRock(string newTileName, int mapPosX, int mapPosY, Transform tileHolder, Rock.RockType rockType)
    {
        if (resourceGrid == null)
            resourceGrid = ResourceGrid.Grid;

        rockToSpawn = objPool.GetObjectForType(newTileName, false, Vector3.zero);

        if (rockToSpawn != null)
        {
            rockToSpawn.transform.position = transform.position;
            rockToSpawn.transform.parent = tileHolder;


            SetSpriteForRockOrMineral(rockType);

            resourceGrid.spawnedTiles[mapPosX, mapPosY] = rockToSpawn;
        }
    }

    void SetSpriteForRockOrMineral(Rock.RockType rockType)
	{
       
        Rock_Handler rock_handler = rockToSpawn.GetComponent<Rock_Handler>();
        Rock.RockSize rockSize = GetRockSize();
        rockToSpawn.GetComponent<SpriteRenderer>().sprite = r_sprite_handler.GetRockSprite(rockType, rockSize);
        rock_handler.InitRock(rockType, rockSize);
        rock_handler.res_sprite_handler = r_sprite_handler;

    }

    Rock.RockSize GetRockSize()
    {
        int randomRocksize = UnityEngine.Random.Range(0, 6);

        Rock.RockSize rockSize = Rock.RockSize.single;

        switch (randomRocksize)
        {
            case 0:
                rockSize = Rock.RockSize.single;
                break;
            case 1:
                rockSize = Rock.RockSize.tiny;
                break;
            case 2:
                rockSize = Rock.RockSize.small;
                break;
            case 3:
                rockSize = Rock.RockSize.medium;
                break;
            case 4:
                rockSize = Rock.RockSize.large;
                break;
            case 5:
                rockSize = Rock.RockSize.larger;
                break;
            default:
                rockSize = Rock.RockSize.single;
                break;
        }

        return rockSize;

    }
}
