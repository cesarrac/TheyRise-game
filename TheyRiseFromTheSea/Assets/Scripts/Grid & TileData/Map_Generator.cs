using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class Map_Generator : MonoBehaviour {

	public int width;
	public int height;
	
	public string seed;
	public bool useRandomSeed;
	
	[Range(0, 100)]
	public int randomFillPercent;
	
	int[,] map;
    int[,] topLayerMap;

	ResourceGrid grid;

    public GameObject _Floor;

    System.Random pseudoRandom;

    void Awake()
    {
        // For the Resource Grid to work, this MAP Gameobject needs to sit on X = 1/2 map width Y = 1/2 map height Z = 0
        float _x = ((float)width) / 2;
        float _y = ((float)height) / 2;
        Vector3 correctPosition = new Vector3(_x, _y, 0.0f);
        if (transform.position != correctPosition)
        {
            transform.position = correctPosition;
            // and move the floor as well, except don't touch its z position
            //_Floor.transform.position = new Vector3(_x, _y, _Floor.transform.position.z);
        }

        if (useRandomSeed)
        {
            // Grabs a random seed using the value of Time
            //seed = Time.time.ToString();
            int randomSeed = UnityEngine.Random.Range(100, 100992112);
            seed = randomSeed.ToString();
        }

        pseudoRandom  = new System.Random(seed.GetHashCode());
    }
	
	void Start()
	{


		GenerateMap ();
		
	}
	
	void Update()
	{
//		if (Input.GetMouseButtonDown (0)) {
//			GenerateMap();
//		}
	}
	
	
	void GenerateMap()
	{
		map = new int[width, height];
		
		RandomFillMap ();
		
		for (int i =0; i < 10; i++) {
			SmoothMap();
		}
		
		ProcessMap ();

        // BORDERS:
        int borderSize = 64;
		int [,] borderedMap = new int[width + borderSize * 2, height + borderSize * 2];

        int[,] islandMap = new int[width, height];

		for (int x = 0; x < borderedMap.GetLength(0); x++) {
			for (int y = 0; y < borderedMap.GetLength(1); y++) {
				
				if (x>= borderSize && x < width + borderSize && y >= borderSize && y < height + borderSize){
					borderedMap[x,y] = map [x - borderSize, y - borderSize];

				}else{
					borderedMap[x,y] = 1;

				}

              
			}
		}

        for (int x = 0; x < islandMap.GetLength(0); x++)
        {
            for (int y = 0; y < islandMap.GetLength(1); y++)
            {
                if (map[x,y] == 0)
                {
                    islandMap[x, y] = 1;

                }

            }
        }




        Mesh_Generator meshGen = GetComponent<Mesh_Generator> ();
		meshGen.GenerateMesh (borderedMap, 1);

        meshGen.GenerateIslandMesh(islandMap);
        //meshGen.GenerateShoreWaterMesh(borderedMap.GetLength(0), borderedMap.GetLength(1));
        // Build island Texture
       // TileTexture tileTexture = _Floor.GetComponent<TileTexture>();
        // tileTexture.BuildTexture(islandMap.GetLength(0), islandMap.GetLength(1));

		GiveMapToResourceGrid (islandMap);
	}

	void GiveMapToResourceGrid(int[,] iMap)
	{
		// Loop through the map array and if it is equal to 0 make it empty if it's equal to 1 make it water
		ResourceGrid grid = GetComponent<ResourceGrid> ();
        grid.mapSizeX = width;
        grid.mapSizeY = height;
		grid.tiles = new TileData[width, height];
        

		int countWaterTiles = 0;
        // Initialize the grid's water tile positions list
        grid.waterTilePositions = new List<Vector2>();

        // Init empty tiles positions list
        grid.emptyTilePositions = new List<Vector2>();

		for (int x =0; x < width; x++) {
			for (int y =0; y < height; y ++){
				if (map[x,y] == 0){
					grid.tiles[x,y] = new TileData(x, y, TileData.Types.empty, 0, 1);

                    // fill empty tiles list
                    grid.emptyTilePositions.Add(new Vector2(x, y));

				}else {
					grid.tiles[x,y] = new TileData(x, y, TileData.Types.water, 2000000, 2);

                    // fill a list of water tiles positions as vector 2 to be used as potential spawn positions for enemies
                    grid.waterTilePositions.Add(new Vector2(x, y));

                    countWaterTiles++;
				}
			}
		}
		grid.spawnedTiles = new GameObject[width, height];

		// let the grid know how many of these tiles are water so it can get spawn positions
		grid.totalTilesThatAreWater = countWaterTiles;
       
        // turn water tiles list into an array for faster searching
        grid.waterTilesArray = grid.waterTilePositions.ToArray();

        // do the same to the empty tiles list
        grid.emptyTilesArray = grid.emptyTilePositions.ToArray();

//        // Build island Texture
		TileTexture_3 tileTexture = _Floor.GetComponent<TileTexture_3>();
        tileTexture.seed = seed;
        tileTexture.randomFillPercent = randomFillPercent;

        // Generate the proper texture:
		tileTexture.DefineTilesAndGenerateBaseTexture(grid.emptyTilesArray, iMap.GetLength(0), iMap.GetLength(1));

        // After texture is rendered generate rocks
        grid.InitializeRockandMinerals();

    }

    public void GenerateTopLayerMap(Vector2[] centerPositions)
    {
        // Init top layer map
        topLayerMap = new int[width, height];
        
        // Second tile positions will be filled by a positions that is topLayerMask = 0 & map = 0 & is NOT an edgeposition
        List<Vector2> secondTilePositions = new List<Vector2>();

        // Loop through map and define each int[,]
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (map[x,y] == 0)
                {
                    // If Top Layer Map = 0 it will be part of the second texture
                    topLayerMap[x, y] = (pseudoRandom.Next(0, 100) < randomFillPercent) ? 1 : 0;

                }
                else
                {
                    topLayerMap[x, y] = 1;
                }
            }
        }

        // Smooth Map:
        for (int i = 0; i < 10; i++)
        {
            SmoothTopMap();
        }

        // Process Map to connect passages and rooms:
        ProcessTopMap();

        // Loop again to fill second Tile positions:
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (topLayerMap[x, y] == 0 && map[x, y] == 0 && IsCenterTile(centerPositions, new Vector2(x, y)))
                {
                    secondTilePositions.Add(new Vector2(x, y));                
                }
                
               
            }
        }


        //BuildTexture(tWidth, tHeight, centerTilePositions);
        TileTexture_3 tileTexture = _Floor.GetComponent<TileTexture_3>();
        tileTexture.DefineTilesAndGenerateSecondTexture(secondTilePositions.ToArray(), width, height);
    }

    bool IsCenterTile(Vector2[] edges, Vector2 posToCheck)
    {
        foreach (Vector2 pos in edges)
        {
            if (pos == posToCheck)
            {
                return true;
            }
        }

        return false;
    }


    bool IsThisPartOfCenterTiles(Vector2[] positions, Vector2 posTocheck)
    {
        foreach(Vector2 pos in positions)
        {
            if (pos == posTocheck)
            {
                return true;
            }
        }

        return false;
        
    }


    // Gets rid of smaller patches of land (small defined by threshold)
    void ProcessMap()
	{
		List<List<Coord>> wallRegions = GetRegions (1, false);
		
        
		int wallThresholdSize =16;
		foreach (List<Coord> wallRegion in wallRegions) {
			if (wallRegion.Count < wallThresholdSize){
				foreach(Coord tile in wallRegion){
					map[tile.tileX, tile.tileY] = 0;
				}
			}
		}
      
		
		List<List<Coord>> roomRegions = GetRegions (0, false);
		int roomThresholdSize =50;
		List<Room> survivingRooms = new List<Room> ();
		
		foreach (List<Coord> roomRegion in roomRegions) {
			if (roomRegion.Count < roomThresholdSize){
				foreach(Coord tile in roomRegion){
					map[tile.tileX, tile.tileY] = 1;
				}
			}
			else{
				survivingRooms.Add(new Room(roomRegion, map));
			}
		}
		
		survivingRooms.Sort ();
		survivingRooms [0].isMainRoom = true;
		survivingRooms [0].isAccessibleFromMainRoom = true;
		
		
		ConnectClosestRooms (survivingRooms, false);
       
	}

    void ProcessTopMap()
    {
        List<List<Coord>> wallRegions = GetRegions(1, true);


        int wallThresholdSize = 200; // <------ TODO: still needs adjustment
        foreach (List<Coord> wallRegion in wallRegions)
        {
            if (wallRegion.Count < wallThresholdSize)
            {
                foreach (Coord tile in wallRegion)
                {
                    topLayerMap[tile.tileX, tile.tileY] = 0;
                }
            }
        }


        List<List<Coord>> roomRegions = GetRegions(0, true);
        int roomThresholdSize = 64; // <------ TODO: still needs adjustment
        List<Room> survivingRooms = new List<Room>();

        foreach (List<Coord> roomRegion in roomRegions)
        {
            if (roomRegion.Count < roomThresholdSize)
            {
                foreach (Coord tile in roomRegion)
                {
                    topLayerMap[tile.tileX, tile.tileY] = 1;
                }
            }
            else
            {
                survivingRooms.Add(new Room(roomRegion, topLayerMap));
            }
        }

        survivingRooms.Sort();
        survivingRooms[0].isMainRoom = true;
        survivingRooms[0].isAccessibleFromMainRoom = true;


        ConnectClosestRooms(survivingRooms, true);

    }

    // Connects all "rooms" with passages so NO room is disconnected
    void ConnectClosestRooms (List<Room> allRooms, bool isTopMap,  bool forceAccessibilityFromMainRoom = false)
	{
		List<Room> roomListA = new List<Room> ();
		List<Room> roomListB = new List<Room> ();
		
		if (forceAccessibilityFromMainRoom) {
			foreach (Room room in allRooms) {
				if (room.isAccessibleFromMainRoom) {
					roomListB.Add (room);
				} else {
					roomListA.Add (room);
				}
			}
		} else {
			roomListA = allRooms;
			roomListB = allRooms;
		}
		
		int bestDistance = 0;
		Coord bestTileA = new Coord ();
		Coord bestTileB = new Coord ();
		Room bestRoomA = new Room ();
		Room bestRoomB = new Room ();
		bool possibleConnectionFound = false;
		
		foreach (Room roomA in roomListA) {
			
			if (!forceAccessibilityFromMainRoom){
				possibleConnectionFound = false;
				if (roomA.connectedRooms.Count > 0){
					continue;
				}
			}
			
			
			foreach(Room roomB in roomListB){
				if (roomA == roomB || roomA.IsConnected(roomB)){
					continue;
				}
				
				
				for (int tileIndexA = 0; tileIndexA < roomA.edgeTiles.Count; tileIndexA ++){
					for (int tileIndexB = 0; tileIndexB < roomB.edgeTiles.Count; tileIndexB ++){
						Coord tileA = roomA.edgeTiles[tileIndexA];
						Coord tileB = roomB.edgeTiles[tileIndexB];
						int distanceBetweenRooms =(int) (Mathf.Pow(tileA.tileX - tileB.tileX, 2) + Mathf.Pow(tileA.tileY - tileB.tileY, 2)); 
						
						if (distanceBetweenRooms < bestDistance || !possibleConnectionFound){
							bestDistance = distanceBetweenRooms;
							possibleConnectionFound = true;
							bestTileA = tileA;
							bestTileB = tileB;
							bestRoomA = roomA;
							bestRoomB = roomB;
						}
						
					}
				}
			}
			
			if (possibleConnectionFound && !forceAccessibilityFromMainRoom){
				CreatePassage(bestRoomA, bestRoomB, bestTileA, bestTileB, isTopMap);
			}
		}
		
		if (possibleConnectionFound && forceAccessibilityFromMainRoom){
			CreatePassage(bestRoomA, bestRoomB, bestTileA, bestTileB, isTopMap);
			ConnectClosestRooms(allRooms, isTopMap);
		} 
		
		if (!forceAccessibilityFromMainRoom) {
			// Any room still not connected will be forced to connect
			ConnectClosestRooms(allRooms, isTopMap, true);
		}
	}
	
	void CreatePassage (Room roomA, Room roomB, Coord tileA, Coord tileB, bool isTopMap)
	{
		Room.ConnectRooms (roomA, roomB);
		//		Debug.DrawLine (CoordToWorldPoint (tileA), CoordToWorldPoint (tileB), Color.green, 100);
		
		List<Coord> line = GetLine (tileA, tileB);
		foreach (Coord c in line) {
			DrawCircle(c, 4, isTopMap);
		}
		
		
	}
	
	void DrawCircle(Coord c, int r, bool isTopMap)
	{
		for (int x = -r; x <= r; x++){
			for (int y = -r; y <= r; y++){
				if (x*x + y*y <= r*r){
					int drawX = c.tileX + x;
					int drawY = c.tileY + y;

                    if (IsInMapRange(drawX, drawY))
                    {
                        if (!isTopMap)
                        {
                            map[drawX, drawY] = 0;
                        }
                        else
                        {
                            
                            topLayerMap[drawX, drawY] = 0;
                            
                        }
                    }
                   
					
				}
			}
		}
	}
	
	List<Coord> GetLine (Coord from, Coord to)
	{
		List <Coord> line = new List<Coord> ();
		
		int x = from.tileX;
		int y = from.tileY;
		
		int dx = to.tileX - from.tileX;
		int dy = to.tileY - from.tileY;
		
		bool inverted = false;
		int step = Math.Sign (dx);
		int gradientStep = Math.Sign (dy);
		
		int longest = Mathf.Abs (dx);
		int shortest = Mathf.Abs (dy);
		
		if (longest < shortest) {
			inverted = true;
			longest = Mathf.Abs(dy);
			shortest = Mathf.Abs(dx);
			
			step = Math.Sign (dy);
			gradientStep = Math.Sign (dx);
		}
		
		int gradienAccumulation = longest / 2;
		for (int i =0; i < longest; i ++) {
			line.Add (new Coord(x,y));
			
			if (inverted){
				y += step;
			}else{
				x += step;
			}
			
			gradienAccumulation += shortest;
			if (gradienAccumulation >= longest){
				if (inverted){
					x += gradientStep;
				}else{
					y += gradientStep;
				}
				gradienAccumulation -= longest;
			}
		}
		return line;
	}
	
	Vector3 CoordToWorldPoint(Coord tile)
	{
		return new Vector3 (-width / 2 + 0.5f + tile.tileX, 2, -height / 2 + 0.5f + tile.tileY);
	}
	
	
	List<List<Coord>> GetRegions (int tileType, bool isTopMap)
	{
		List<List<Coord>> regions = new List<List<Coord>> ();
		int[,] mapFlags = new int[width,height];
		
		for (int x = 0; x < width; x++) {
			for (int y = 0; y < height; y++) {
				if (!isTopMap)
                {
                    if (mapFlags[x, y] == 0 && map[x, y] == tileType)
                    {
                        List<Coord> newRegion = GetRegionTiles(x, y, isTopMap);
                        regions.Add(newRegion);

                        // marked as looked at
                        foreach (Coord tile in newRegion)
                        {
                            mapFlags[tile.tileX, tile.tileY] = 1;
                        }
                    }
                }
                else
                {
                    if (mapFlags[x, y] == 0 && topLayerMap[x, y] == tileType)
                    {
                        List<Coord> newRegion = GetRegionTiles(x, y, isTopMap);
                        regions.Add(newRegion);

                        // marked as looked at
                        foreach (Coord tile in newRegion)
                        {
                            mapFlags[tile.tileX, tile.tileY] = 1;
                        }
                    }
                }
				
			}
		}
		return regions;
	}
	
	
	
	List<Coord> GetRegionTiles(int startX, int startY, bool isTopMap)
	{
		List<Coord> tiles = new List<Coord> ();
		int[,] mapFlags = new int[width, height];
        int tileType = map[startX, startY];
        if (isTopMap)
        {
            tileType = topLayerMap[startX, startY];
        }
		
		
		Queue<Coord> queue = new Queue<Coord> ();
		queue.Enqueue (new Coord (startX, startY));
		mapFlags [startX, startY] = 1;
		
		while (queue.Count > 0) {
			Coord tile = queue.Dequeue();
			tiles.Add (tile);
			
			// Look at adjacent tiles
			for (int x = tile.tileX - 1; x <= tile.tileX + 1; x++){
				for (int y = tile.tileY - 1; y <= tile.tileY + 1; y++){
					if (IsInMapRange(x, y) && (y == tile.tileY || x == tile.tileX)){
                        if (!isTopMap)
                        {
                            if (mapFlags[x, y] == 0 && map[x, y] == tileType)
                            {
                                mapFlags[x, y] = 1;
                                queue.Enqueue(new Coord(x, y));
                            }
                        }
                        else
                        {
                            if (mapFlags[x, y] == 0 && topLayerMap[x, y] == tileType)
                            {
                                mapFlags[x, y] = 1;
                                queue.Enqueue(new Coord(x, y));
                            }
                        }
						
					}
				}
			}
		}
		
		return tiles;
	}
	
	bool IsInMapRange (int x, int y)
	{
		return x >= 0 && x < width && y >= 0 && y < height;
	}




    void RandomFillMap()
	{
		//if (useRandomSeed) {
  //          // Grabs a random seed using the value of Time
  //          //seed = Time.time.ToString();
  //          int randomSeed = UnityEngine.Random.Range(100, 100992112);
  //          seed = randomSeed.ToString();
  //      }
		
		//System.Random pseudoRandom = new System.Random (seed.GetHashCode ());
		
		for (int x = 0; x < width; x++){
			for (int y = 0; y < height; y++){
               // map[x, y] = (pseudoRandom.Next(0, 100) < randomFillPercent) ? 1 : 0;
                // Borders
                
                if (x == 0 || x == width -1 || y == 0 || y == height -1){
					
					map [x, y] = 1;
				}else{
                    map[x,y] = (pseudoRandom.Next(0,100) < randomFillPercent) ? 1: 0;
                    
                }
               
				
			}
		}
	}

    void RandomFillTopMap()
    {
        //if (useRandomSeed)
        //{
        //    // Grabs a random seed using the value of Time
        //    //seed = Time.time.ToString();
        //    int randomSeed = UnityEngine.Random.Range(100, 100992112);
        //    seed = randomSeed.ToString();
        //}

       // System.Random pseudoRandom = new System.Random(seed.GetHashCode());

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // map[x, y] = (pseudoRandom.Next(0, 100) < randomFillPercent) ? 1 : 0;
                // Borders

                if (x == 0 || x == width - 1 || y == 0 || y == height - 1)
                {

                    topLayerMap[x, y] = 0;
                }
                else
                {
                    topLayerMap[x, y] = (pseudoRandom.Next(0, 100) < randomFillPercent) ? 1 : 0;
                  
                }


            }
        }
    }

    void SmoothMap()
	{
		for (int x = 0; x < width; x++) {
			for (int y = 0; y < height; y++) {
				
				int neighbourWallTiles = GetSurroundingWallCount(x,y);
				
				if (neighbourWallTiles > 4){
					map[x,y] = 1;
				}else if (neighbourWallTiles < 4){
					map[x,y] = 0;
				}
			}
		}
	}


    void SmoothTopMap()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {

                int neighbourWallTiles = GetTopSurroundingWallCount(x, y);

                if (neighbourWallTiles > 4)
                {
                    topLayerMap[x, y] = 1;
                }
                else if (neighbourWallTiles < 4)
                {
                    topLayerMap[x, y] = 0;
                }
            }
        }
    }

    int GetSurroundingWallCount(int gridX, int gridY)
	{
		int wallCount = 0;
		
		for (int neighbourX = gridX - 1; neighbourX <= gridX + 1; neighbourX ++) {
			for (int neighbourY = gridY - 1; neighbourY <= gridY + 1; neighbourY ++) {
				
				if (IsInMapRange(neighbourX, neighbourY)){
					// We ignore the tile passed in as an argument
					if (neighbourX != gridX || neighbourY != gridY){
						wallCount += map[neighbourX, neighbourY];
					}
				}
                else
                {
                    wallCount++;
                }
            }
        }
		
		return wallCount;
	}

    int GetTopSurroundingWallCount(int gridX, int gridY)
    {
        int wallCount = 0;

        for (int neighbourX = gridX - 1; neighbourX <= gridX + 1; neighbourX++)
        {
            for (int neighbourY = gridY - 1; neighbourY <= gridY + 1; neighbourY++)
            {

                if (IsInMapRange(neighbourX, neighbourY))
                {
                    // We ignore the tile passed in as an argument
                    if (neighbourX != gridX || neighbourY != gridY)
                    {
                        wallCount += topLayerMap[neighbourX, neighbourY];
                    }
                }
                else
                {
                    wallCount++;
                }
            }
        }

        return wallCount;
    }

    struct Coord{
		public int tileX;
		public int tileY;
		
		public Coord (int x, int y)
		{
			tileX = x;
			tileY = y;
		}
	}
	
	
	class Room : IComparable<Room>{
		public List<Coord> tiles;
		public List<Coord> edgeTiles;
		public List<Room> connectedRooms;
		public int roomSize;
		public bool isAccessibleFromMainRoom;
		public bool isMainRoom;
		
		// The largest room will be the MAIN room, where every room must get to directly or indirectly
		
		
		public Room ()
		{
			// empty constructor for empty rooms
		}
		
		public Room(List<Coord> roomTiles, int [,] map)
		{
			tiles = roomTiles;
			roomSize = tiles.Count;
			connectedRooms = new List<Room>();
			
			edgeTiles = new List<Coord>();
			foreach (Coord tile in tiles){
				for (int x = tile.tileX -1; x <= tile.tileX + 1; x++){
					for (int y = tile.tileY -1; y <= tile.tileY + 1; y++){
						if (x == tile.tileX || y == tile.tileY){
							// This excludes diagonals and just checks the four adjacent tiles
							if (map[x,y] == 1){
								edgeTiles.Add(tile);
							}
						}
					}
				}
			}
		}
		
		public void SetAccessibleFromMainRoom()
		{
			if (!isAccessibleFromMainRoom) {
				isAccessibleFromMainRoom = true;
				foreach(Room connectedRoom in connectedRooms){
					connectedRoom.SetAccessibleFromMainRoom();
				}
			}
		}
		
		
		public static void ConnectRooms (Room roomA, Room roomB)
		{
			if (roomA.isAccessibleFromMainRoom) {
				roomB.SetAccessibleFromMainRoom ();
			} else if (roomB.isAccessibleFromMainRoom) {
				roomA.SetAccessibleFromMainRoom();
			}
			
			roomA.connectedRooms.Add (roomB);
			roomB.connectedRooms.Add (roomA);
		}
		
		public bool IsConnected (Room otherRoom)
		{
			return connectedRooms.Contains (otherRoom);
		}
		
		
		public int CompareTo(Room otherRoom)
		{
			return otherRoom.roomSize.CompareTo(roomSize);
		}
		
		
	}

}
