using UnityEngine;
using System.Collections;

public class DiscoverTile : MonoBehaviour {
	/// <summary>
	/// This script is called by Resource Grid when it needs to instantiate a tile.
	/// It gets the actual tile GameObject from Resource grid, spawns a grey tile and starts to dissolve it.
	/// The time it will take to disappear will be determined by mining time. (Which can be upgraded later)
	/// </summary>


	bool fading;

	SpriteRenderer sr;

	GameObject tileToSpawn;

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
		// before dying make sure to turn on the tileToSpawn's box collider so the player can interact with it
		if (tileToSpawn != null) {
//			BoxCollider2D coll = tileToSpawn.GetComponent<BoxCollider2D> ();
//			coll.enabled = true;
			Destroy (this.gameObject);
		} else {
			Destroy (this.gameObject);
		}
	}

	public void TileToDiscover(string newTileName, int mapPosX, int mapPosY, Transform tileHolder, TileData.Types tileType, int spriteWidth = 0, int spriteHeight = 0){      // this is called by Resource grid with the proper tile obj
        if (resourceGrid == null)
            resourceGrid = ResourceGrid.Grid;

        tileToSpawn = objPool.GetObjectForType (newTileName, false, Vector3.zero);
		if (tileToSpawn != null) {
			tileToSpawn.transform.position = transform.position;
			tileToSpawn.transform.parent = tileHolder;
			// Give the Tile position relative to the grid map
//		TileClick_Handler tc = tileToSpawn.GetComponent<TileClick_Handler> ();
//		tc.mapPosX = mapPosX;
//		tc.mapPosY = mapPosY;
//		tc.resourceGrid = grid;
//		tc.playerUnit = selectedUnit;

//		// IF TILE IS NOT A ROCK OR EMPTY, IT'S A BUILDING,
			// so it will have a Building Click Handler that needs its pos X and pos Y
			if (tileType != TileData.Types.rock && tileType != TileData.Types.empty && tileType != TileData.Types.mineral) {
				Building_ClickHandler bClickHandler = tileToSpawn.GetComponent<Building_ClickHandler> ();
                if (bClickHandler)
                {
                    bClickHandler.mapPosX = mapPosX;
                    bClickHandler.mapPosY = mapPosY;
                    //bClickHandler.resourceGrid = resourceGrid;
                    //bClickHandler.objPool = objPool;
                }
			

			} else if (tileType == TileData.Types.rock || tileType == TileData.Types.mineral){
				GetSpriteForRockOrMineral(tileType);
			}
			if (tileType == TileData.Types.extractor) {
				// IF IT'S AN EXTRACTOR it will ALSO need the extractor variables
				Extractor extra = tileToSpawn.GetComponent<Extractor> ();
                if (extra)
                {
                    extra.mapPosX = mapPosX;
                    extra.mapPosY = mapPosY;
                    extra.resourceGrid = resourceGrid;
                    extra.playerResources = resourceGrid.playerCapital.GetComponent<Player_ResourceManager>();
                }
				
			} 
			if (tileType == TileData.Types.capital){
                resourceGrid.playerCapital = tileToSpawn;
				// IF IT'S THE TERRAFORMER it will need the master state manager
				Terraformer_Handler terra = tileToSpawn.GetComponent<Terraformer_Handler>();
                if (terra)
				    terra.master_State = master_state;
			}

	

			// ADD this tile to the Grid's spawnedTiles array
            if (spriteWidth > 0 && spriteHeight > 0)
            {
                for (int w = 0; w < spriteWidth; w++)
                {
                    for (int h = 0; h < spriteHeight; h++)
                    {
                        resourceGrid.spawnedTiles[mapPosX + w, mapPosY + h] = tileToSpawn;
                    }
                }
            }
            else
            {
			resourceGrid.spawnedTiles [mapPosX, mapPosY] = tileToSpawn;
            }

		}
	}

	void GetSpriteForRockOrMineral(TileData.Types _tileType)
	{
        SpriteRenderer sr = tileToSpawn.GetComponent<SpriteRenderer>();
        Rock_Handler rock_handler = tileToSpawn.GetComponent<Rock_Handler>();
        if (_tileType == TileData.Types.rock) {
			// Is this a single, tiny, small, medium, large or larger rock?
			int randomRocksize = Random.Range (0, 6);
            switch (randomRocksize)
            {
                case 0:
                    Rock singleRock = new Rock(Rock.RockType.rock, Rock.RockSize.single);
                    sr.sprite = r_sprite_handler.GetRockSprite(singleRock._rockType, singleRock._rockSize);
                    // Initialize this rock from its Rock_Handler script
                    rock_handler.InitRock(singleRock._rockType, singleRock._rockSize);
                    rock_handler.res_sprite_handler = r_sprite_handler;
                    break;
                case 1:
                    Rock tinyRock = new Rock(Rock.RockType.rock, Rock.RockSize.tiny);
                    sr.sprite = r_sprite_handler.GetRockSprite(tinyRock._rockType, tinyRock._rockSize);
                    // Initialize this rock from its Rock_Handler script
                    rock_handler.InitRock(tinyRock._rockType, tinyRock._rockSize);
                    rock_handler.res_sprite_handler = r_sprite_handler;
                    break;
                   
                case 2:
                    Rock smallRock = new Rock(Rock.RockType.rock, Rock.RockSize.small);
                    sr.sprite = r_sprite_handler.GetRockSprite(smallRock._rockType, smallRock._rockSize);
                    rock_handler.InitRock(smallRock._rockType, smallRock._rockSize);
                    rock_handler.res_sprite_handler = r_sprite_handler;
                    break;
                   
                case 3:
                    Rock medRock = new Rock(Rock.RockType.rock, Rock.RockSize.medium);
                    sr.sprite = r_sprite_handler.GetRockSprite(medRock._rockType, medRock._rockSize);
                    rock_handler.InitRock(medRock._rockType, medRock._rockSize);
                    rock_handler.res_sprite_handler = r_sprite_handler;
                    break;
                    
                case 4:
                    Rock largeRock = new Rock(Rock.RockType.rock, Rock.RockSize.large);
                    sr.sprite = r_sprite_handler.GetRockSprite(largeRock._rockType, largeRock._rockSize);
                    rock_handler.res_sprite_handler = r_sprite_handler;
                    rock_handler.InitRock(largeRock._rockType, largeRock._rockSize);

                    break;
                    
                case 5:
                    Rock largerRock = new Rock(Rock.RockType.rock, Rock.RockSize.larger);
                     sr.sprite = r_sprite_handler.GetRockSprite(largerRock._rockType, largerRock._rockSize);               
                    rock_handler.res_sprite_handler = r_sprite_handler;
                    rock_handler.InitRock(largerRock._rockType, largerRock._rockSize);

                    break;

            }
			
		} else if (_tileType == TileData.Types.mineral) {
            // Is this a single, tiny, small, medium, large or larger mineral?
            int randomRocksize = Random.Range(0, 6);
            switch (randomRocksize)
            {
                case 0:
                    Rock singleRock = new Rock(Rock.RockType.mineral, Rock.RockSize.single);
                    sr.sprite = r_sprite_handler.GetRockSprite(singleRock._rockType, singleRock._rockSize);
                    rock_handler.InitRock(singleRock._rockType, singleRock._rockSize);
                    rock_handler.res_sprite_handler = r_sprite_handler;
                    break;
                case 1:
                    Rock tinyRock = new Rock(Rock.RockType.mineral, Rock.RockSize.tiny);
                    sr.sprite = r_sprite_handler.GetRockSprite(tinyRock._rockType, tinyRock._rockSize);
                    rock_handler.InitRock(tinyRock._rockType, tinyRock._rockSize);
                    rock_handler.res_sprite_handler = r_sprite_handler;
                    break;

                case 2:
                    Rock smallRock = new Rock(Rock.RockType.mineral, Rock.RockSize.small);
                    sr.sprite = r_sprite_handler.GetRockSprite(smallRock._rockType, smallRock._rockSize);
                    rock_handler.InitRock(smallRock._rockType, smallRock._rockSize);
                    rock_handler.res_sprite_handler = r_sprite_handler;
                    break;

                case 3:
                    Rock medRock = new Rock(Rock.RockType.mineral, Rock.RockSize.medium);
                    sr.sprite = r_sprite_handler.GetRockSprite(medRock._rockType, medRock._rockSize);
                    rock_handler.InitRock(medRock._rockType, medRock._rockSize);
                    rock_handler.res_sprite_handler = r_sprite_handler;
                    break;

                case 4:
                    Rock largeRock = new Rock(Rock.RockType.mineral, Rock.RockSize.large);
                    sr.sprite = r_sprite_handler.GetRockSprite(largeRock._rockType, largeRock._rockSize);
                    rock_handler.InitRock(largeRock._rockType, largeRock._rockSize);
                    rock_handler.res_sprite_handler = r_sprite_handler;
                    break;

                case 5:
                    Rock largerRock = new Rock(Rock.RockType.mineral, Rock.RockSize.larger);
                    sr.sprite = r_sprite_handler.GetRockSprite(largerRock._rockType, largerRock._rockSize);
                    rock_handler.InitRock(largerRock._rockType, largerRock._rockSize);
                    rock_handler.res_sprite_handler = r_sprite_handler;
                    break;

            }
        }
	}
}
