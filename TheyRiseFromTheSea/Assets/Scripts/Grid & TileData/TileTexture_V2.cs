using UnityEngine;
using System.Collections;

public class TileTexture_V2 : MonoBehaviour {
	public Texture2D tileSheet;
	public int tileResolution = 32;
	
	public Texture2D algaeTile;
	
	int levelWidth, levelHeight;
	
	MeshRenderer mesh_renderer;
	
	public Map_Generator mapGenScript;
	
	public int tilesPerRow, numberOfRows;
	
	public enum TileType { CLIFF_SHORE, SHORE_CLIFF, CLIFF, SHORE, CENTER, CLEAR, UNDEFINED }
	public enum EdgeType { BOTTOM, TOP, LEFT, RIGHT, BOTTOM_LEFT_CORNER, BOTTOM_RIGHT_CORNER, TOP_LEFT_CORNER, TOP_RIGHT_CORNER, LEFT_BOTTOM_DIAG, RIGHT_BOTTOM_DIAG,
		LEFT_TOP_DIAG, RIGHT_TOP_DIAG}
	public enum LandType {ASH, SAND, MUD }
	
	TileType[,] tileTypes;
	
	
	void Awake()
	{
		
		mesh_renderer = GetComponent<MeshRenderer>();
		
	}
	
	
	
	Color[][] SplitTileSheet()
	{
		Color[][] tiles = new Color[tilesPerRow * numberOfRows][];
		
		for (int y = 0; y < numberOfRows; y++)
		{
			for (int x = 0; x < tilesPerRow; x++)
			{
				tiles[y * tilesPerRow + x] = tileSheet.GetPixels(x * tileResolution, y * tileResolution, tileResolution, tileResolution);
				
			}
		}
		
		return tiles;
	}
	
	public void BuildTexture(Vector2[] emptyTilesArray, int islandWidth, int islandHeight)
	{
		/*
		 * When looping through the map figure out its neighbors. 
		 * Do I have another tile to the top, left or right of me?
		 * if I have a tile on top and right but none to the left and bottom, I'm a bottom left corner
		 * and so on...
		*/
		
		
		int textureWidth = islandWidth * tileResolution;
		int textureHeight = islandHeight * tileResolution;
		Texture2D texture = new Texture2D(textureWidth, textureHeight);
		texture.name = "Island Texture";
		
		
		Debug.Log("TILETEXTURE: Created a texture of width: " + textureWidth + " and height: " + textureHeight);
		Debug.Log("TILETEXTURE: the empty tiles array length is " + emptyTilesArray.Length);
		
		// Get tiles by Splitting up the sprite sheet
		Color[][] tiles = SplitTileSheet();
		// Use this array to Set Pixels
		Color[] thisTilePixels;
		// Clear tiles pixels
		Color[] algaePixels = algaeTile.GetPixels(0, 0, 32, 32);
		
		
		// Initialize tileTypes array by making them all undefined for now so we have something to check against
		tileTypes = new TileType[islandWidth, islandHeight];
		for (int x = 0; x < islandWidth; x++)
		{
			for (int y = 0; y < islandHeight; y++)
			{
				tileTypes[x, y] = TileType.UNDEFINED;
			}
		}
		
		
		// loop through the mesh to get the positions not rendered as an island tile and make them clear pixels, if it's an island position fill that instead
		for (int mapX = 0; mapX < islandWidth; mapX++)
		{
			for (int mapY = 0; mapY < islandHeight; mapY++)
			{
				Vector2 thisPosition = new Vector2(mapX, mapY);
				if (!CheckIfTileExists(emptyTilesArray, thisPosition))
				{
					// Tile does NOT exist, so set pixels as clear algae pixels
					texture.SetPixels((int)thisPosition.x * tileResolution, (int)thisPosition.y * tileResolution, tileResolution, tileResolution, algaePixels);
					
					// define tileType as clear
					tileTypes[mapX, mapY] = TileType.CLEAR;
				}
				else
				{   // Tile DOES exist:
					// Check tile's neighbors to get the correct tile to render
					int tileSelected = CheckTileNeighbors(emptyTilesArray, thisPosition);
					
					thisTilePixels = tiles[tileSelected];
					
					texture.SetPixels((int)thisPosition.x * tileResolution, (int)thisPosition.y * tileResolution, tileResolution, tileResolution, thisTilePixels);
				}
			}
		}
		
		texture.filterMode = FilterMode.Trilinear;
		
		// Apply texture
		texture.Apply();
		
		mesh_renderer.sharedMaterial.mainTexture = texture;
		
	}
	
	int CheckTileNeighbors(Vector2[] emptyTileArray, Vector2 emptyTilePos)
	{
		bool left = false, right = false, bottom = false, top = false;
		bool leftTop = false, leftBottom = false, rightTop = false, rightBottom = false;
		
		// Check if a tile exists to my left
		// Vector2 leftTile = new Vector2(emptyTileArray[index].x - 1, emptyTileArray[index].y);
		Vector2 leftTile = new Vector2(emptyTilePos.x - 1, emptyTilePos.y);
		if (CheckIfTileExists(emptyTileArray, leftTile))
		{
			left = true;
		}
		// Check if a tile exists to my right
		Vector2 rightTile = new Vector2(emptyTilePos.x + 1, emptyTilePos.y);
		
		//Vector2 rightTile = new Vector2(emptyTileArray[index].x + 1, emptyTileArray[index].y);
		if (CheckIfTileExists(emptyTileArray, rightTile))
		{
			right = true;
		}
		// Check if a tile exists on top
		Vector2 topTile = new Vector2(emptyTilePos.x, emptyTilePos.y + 1);
		//Vector2 topTile = new Vector2(emptyTileArray[index].x, emptyTileArray[index].y + 1);
		
		if (CheckIfTileExists(emptyTileArray, topTile))
		{
			top = true;
		}
		// Check if a tile exists on bottom
		Vector2 bottomTile = new Vector2(emptyTilePos.x, emptyTilePos.y - 1);
		//Vector2 bottomTile = new Vector2(emptyTileArray[index].x, emptyTileArray[index].y - 1);
		if (CheckIfTileExists(emptyTileArray, bottomTile))
		{
			bottom = true;
		}
		// Check if a tile exists on bottom left
		Vector2 bottomLeftTile = new Vector2(emptyTilePos.x - 1, emptyTilePos.y - 1);
		//Vector2 bottomLeftTile = new Vector2(emptyTileArray[index].x - 1, emptyTileArray[index].y - 1);
		if (CheckIfTileExists(emptyTileArray, bottomLeftTile))
		{
			leftBottom = true;
		}
		// Check if a tile exists on top left
		Vector2 topLeftTile = new Vector2(emptyTilePos.x - 1, emptyTilePos.y + 1);
		//Vector2 topLeftTile = new Vector2(emptyTileArray[index].x - 1, emptyTileArray[index].y + 1);
		if (CheckIfTileExists(emptyTileArray, topLeftTile))
		{
			leftTop = true;
		}
		// Check if a tile exists on top right
		Vector2 topRightTile = new Vector2(emptyTilePos.x + 1, emptyTilePos.y + 1);
		//Vector2 topRightTile = new Vector2(emptyTileArray[index].x + 1, emptyTileArray[index].y + 1);
		if (CheckIfTileExists(emptyTileArray, topRightTile))
		{
			rightTop = true;
		}
		// Check if a tile exists on bottom right
		Vector2 bottomRightTile = new Vector2(emptyTilePos.x + 1, emptyTilePos.y - 1);
		//Vector2 bottomRightTile = new Vector2(emptyTileArray[index].x + 1, emptyTileArray[index].y - 1);
		if (CheckIfTileExists(emptyTileArray, bottomRightTile))
		{
			rightBottom = true;
		}
		
		return GetTileToRender(left, right, top, bottom, leftTop, leftBottom, rightTop, rightBottom, (int)emptyTilePos.x, (int)emptyTilePos.y);
	}
	
	bool CheckIfTileExists(Vector2[] emptyTileArray, Vector2 positionToCheck)
	{
		bool tileExists = false;
		
		foreach (Vector2 vector in emptyTileArray)
		{
			if (vector == positionToCheck)
			{
				tileExists = true;
				break;
			}
		}
		return tileExists;
	}
	
	int GetTileToRender(bool _left, bool _right, bool _top, bool _bottom, bool _topLeft, bool _bottLeft, bool _topRight, bool _bottRight, int posX, int posY)
	{
		// Positions to check
		int right = posX + 1;
		int left = posX - 1;
		int top = posY + 1;
		int bottom = posY - 1;
		TileType myTileType;
		TileType neighborTop = GetNeighborTileTypes(posX, top);
		TileType neighborRight = GetNeighborTileTypes(right, posY);
		TileType neighborLeft = GetNeighborTileTypes(left, posY);
		TileType neighborBottom = GetNeighborTileTypes(posX, bottom);
		
		// Check diagonal tiles first:
		if (_top && _right && _left && _bottom && !_bottLeft)
		{
			// Left bottom diagonal
			EdgeType myEdgeType = EdgeType.LEFT_BOTTOM_DIAG;
			// Neighbor 1 = left, Neighbor 2 = bottom
			myTileType = DefineAll(neighborLeft, neighborBottom, posX, posY, left, posY, posX, bottom);
			
			//int randomSelect = Random.Range(0, 4);
			return GetTileSheetIndex(myTileType, myEdgeType);
		}
		else if (_top && _left && _bottom && _right && !_bottRight)
		{
			// Right bottom diagonal
			EdgeType myEdgeType = EdgeType.RIGHT_BOTTOM_DIAG;
			// Neighbor 1 = bottom, Neighbor 2 = right
			myTileType = DefineAll(neighborBottom, neighborRight, posX, posY, posX, bottom,right, posY);
			
			return GetTileSheetIndex(myTileType, myEdgeType);
			
		}
		else if (_top && _left && _bottom && _right && !_topRight)
		{
			// Right top diagonal
			EdgeType myEdgeType = EdgeType.RIGHT_TOP_DIAG;
			// Neighbor 1 = bottom, Neighbor 2 = right
			myTileType = DefineAll(neighborTop, neighborRight, posX, posY, posX, bottom,right, posY);
			
			return GetTileSheetIndex(myTileType, myEdgeType);
			
		}
		else if (_top && _left && _bottom && _right && !_topLeft)
		{
			// Left top diagonal
			EdgeType myEdgeType = EdgeType.LEFT_TOP_DIAG;
			// Neighbor 1 = bottom, Neighbor 2 = right
			myTileType = DefineAll(neighborLeft, neighborTop, posX, posY, posX, bottom,right, posY);
			
			return GetTileSheetIndex(myTileType, myEdgeType);
			
		}
		else if (_top && _right && !_left && !_bottom)
		{
			// bottom left corner
			EdgeType myEdgeType = EdgeType.BOTTOM_LEFT_CORNER;
			
			// Neighbor 1 = top, Neighbor 2 = right
			myTileType = DefineAll(neighborTop, neighborRight, posX, posY, posX, top, right, posY, isCorner: true);
			
			return GetTileSheetIndex(myTileType, myEdgeType);
		}
		else if (_top && _left && _right && !_bottom)
		{
			// Bottom
			EdgeType myEdgeType = EdgeType.BOTTOM;
			
			// Neighbor 1 = left, Neighbor 2 = right
			myTileType = DefineAll(neighborLeft, neighborRight, posX, posY, left, posY, right, posY);
			
			return GetTileSheetIndex(myTileType, myEdgeType);
		}
		else if (_top && _left && !_right && !_bottom)
		{
			// Bottom right corner
			EdgeType myEdgeType = EdgeType.BOTTOM_RIGHT_CORNER;
			
			// Neighbor 1 = left, Neighbor 2 = top
			myTileType = DefineAll(neighborLeft, neighborTop, posX, posY, left, posY, posX, top, isCorner: true);
			
			return GetTileSheetIndex(myTileType, myEdgeType);
		}
		else if (_top && _left && _bottom && !_right)
		{
			// Right
			EdgeType myEdgeType = EdgeType.RIGHT;
			
			// Neighbor 1 = bottom, Neighbor 2 = top
			myTileType = DefineAll(neighborBottom, neighborTop, posX, posY, posX, bottom, posX, top);
			
			return GetTileSheetIndex(myTileType, myEdgeType);
		}
		else if (_top && _right && _bottom && !_left)
		{
			// left
			EdgeType myEdgeType = EdgeType.LEFT;
			
			// Neighbor 1 = bottom, Neighbor 2 = top
			myTileType = DefineAll(neighborBottom, neighborTop, posX, posY, posX, bottom, posX, top);
			
			return GetTileSheetIndex(myTileType, myEdgeType);
		}
		else if (!_top && _right && _left && _bottom)
		{
			// top shore
			EdgeType myEdgeType = EdgeType.TOP;
			
			// Neighbor 1 = left, Neighbor 2 = right
			myTileType = DefineAll(neighborBottom, neighborTop, posX, posY, left, posY, right, posY);
			//myTileType = GetMyTileTypeFromNeighbors(neighborLeft, neighborRight);
			
			return GetTileSheetIndex(myTileType, myEdgeType);
		}
		else if (!_top && !_right && _left && _bottom && !_topRight)
		{
			// top right corner
			EdgeType myEdgeType = EdgeType.TOP_RIGHT_CORNER;
			
			// Neighbor 1 = left, Neighbor 2 = bottom
			myTileType = DefineAll(neighborLeft, neighborBottom, posX, posY, left, posY, posX, bottom, isCorner: true);
			
			return GetTileSheetIndex(myTileType, myEdgeType);
		}
		
		else if (!_top && _right && !_left && _bottom && !_topLeft)
		{
			// top left corner
			EdgeType myEdgeType = EdgeType.TOP_LEFT_CORNER;
			
			// Neighbor 1 = bottom, Neighbor 2 = right
			myTileType = DefineAll(neighborBottom, neighborRight, posX, posY, posX, bottom, right, posY, isCorner: true);
			
			return GetTileSheetIndex(myTileType, myEdgeType);
		}
		else
		{
			// assign to array if undefined
			if (tileTypes[posX, posY] == TileType.UNDEFINED)
				tileTypes[posX, posY] = TileType.CENTER;
			
			// Center tile
			LandType myLandType = SelectLandType();
			if (myLandType == LandType.ASH){
				return 260;
			}else if (myLandType == LandType.SAND){
				return 30;
			}else {
				return 100;
			}
		}
		
		
	}
	
	TileType DefineAll(TileType neighborOne, TileType neighborTwo, int myX, int myY, int firstPosX, int firstPosY, int secondPosX, int secondPosY, bool isCorner = false){
		TileType myTileType = TileType.UNDEFINED;
		
		if (neighborOne != TileType.UNDEFINED || neighborTwo != TileType.UNDEFINED)
		{
			// Both are DEFINED, so select based on the first and second neighbor
			myTileType = GetMyTileTypeFromNeighbors(neighborOne, neighborTwo, isCorner);
			print("One neighbor is defined. Neighbors of : " + myX + " " + myY + " are: " + neighborOne.ToString() + " and " + neighborTwo.ToString() + ", isCorner = " + isCorner.ToString());
			
		}
		else
		{
			
			myTileType = DefineTypeFromUndefinedNeighbors(isCorner);
			
		}
		
		
		// define this tile on the types array
		tileTypes[myX, myY] = myTileType;
		
		print ("Tile at: " + myX + " " + myY + " is of type: " + myTileType.ToString ()); 
		
		return myTileType;
	}
	
	TileType DefineTypeFromUndefinedNeighbors(bool isCorner = false){
		TileType thisTileType = TileType.UNDEFINED;
		int select = Random.Range (0, 6); // Cliff or Shore?
		if (select <= 2) {
			thisTileType = TileType.CLIFF;
		} 
		else if (select == 3) {
			thisTileType = TileType.SHORE_CLIFF;
		} 
		else if (select == 4) {
			thisTileType = TileType.CLIFF_SHORE;
		}
		else {
			thisTileType = TileType.SHORE;
		}
		return thisTileType;
	}

	
	TileType GetNeighborTileTypes(int x, int y)
	{
		return tileTypes[x, y];
	}
	
	/* IT'S VERY IMPORTANT that CORNERS AND DIAGONALS are NEVER defined as: Shore_Cliff or Cliff_Shore.
	 If a Diagonal is place first, CORNERS need to know if the DIAGONALS that are their neigbors are CLIFF or SHORE,
	 If a Corner is placed first, DIAGONALS need to know if the CORNERS neighbors are CLIFF or SHORE*/
	TileType GetMyTileTypeFromNeighbors(TileType neighbor1Type, TileType neighbor2Type, bool isCorner = false)
	{
		TileType myTileType = TileType.UNDEFINED;
		
		switch (neighbor1Type)
		{
		case TileType.SHORE:
			if (neighbor2Type == TileType.SHORE)
			{
				myTileType = TileType.SHORE;
			}
			else if (neighbor2Type == TileType.CLIFF && !isCorner)
			{
				myTileType = TileType.SHORE_CLIFF;
			}
			else if (neighbor2Type == TileType.SHORE_CLIFF)
			{
				myTileType = TileType.SHORE;
			}
			else if (neighbor2Type == TileType.CLIFF_SHORE && !isCorner)
			{
				myTileType = TileType.SHORE_CLIFF;
			}
			else if (neighbor2Type == TileType.UNDEFINED && !isCorner){
				myTileType = TileType.SHORE_CLIFF;	
			}
			else{
				myTileType = TileType.SHORE;	
			}
			
			break;
		case TileType.CLIFF:
			if (neighbor2Type == TileType.SHORE || neighbor2Type == TileType.SHORE_CLIFF  && !isCorner)
			{
				myTileType = TileType.CLIFF_SHORE;
			}
			else if (neighbor2Type == TileType.CLIFF || neighbor2Type == TileType.CLIFF_SHORE)
			{
				myTileType = TileType.CLIFF;
			}
			else if (neighbor2Type == TileType.UNDEFINED && !isCorner){
				myTileType = TileType.CLIFF_SHORE;	
			}
			else{
				myTileType = TileType.CLIFF;
			}
			
			break;
		case TileType.SHORE_CLIFF:
			if (neighbor2Type == TileType.SHORE || neighbor2Type == TileType.SHORE_CLIFF && !isCorner)
			{
				myTileType = TileType.CLIFF_SHORE;
			}
			else if (neighbor2Type == TileType.CLIFF || neighbor2Type == TileType.CLIFF_SHORE )
			{
				myTileType = TileType.CLIFF;
			}
			else{
				myTileType = TileType.CLIFF;
			}
			
			break;
		case TileType.CLIFF_SHORE:
			if (neighbor2Type == TileType.SHORE || neighbor2Type == TileType.SHORE_CLIFF)
			{
				myTileType = TileType.SHORE;
			}
			else if (neighbor2Type == TileType.CLIFF || neighbor2Type == TileType.CLIFF_SHORE && !isCorner)
			{
				myTileType = TileType.SHORE_CLIFF;
			}
			else{
				myTileType = TileType.SHORE;
			}
			
			break;
		case TileType.UNDEFINED:
			if (neighbor2Type == TileType.SHORE || neighbor2Type == TileType.SHORE_CLIFF || neighbor2Type == TileType.UNDEFINED)
			{
				myTileType = TileType.SHORE;
			}
			else if (neighbor2Type == TileType.CLIFF || neighbor2Type == TileType.CLIFF_SHORE && !isCorner)
			{
				myTileType = TileType.SHORE_CLIFF;
			} else{
				myTileType = TileType.SHORE;
			}    
			break;
			
		default:
			myTileType = TileType.SHORE;
			break;
		}
		
		
		return myTileType;
	}
	
	int GetTileSheetIndex (TileType myTileType, EdgeType myEdgeType)
	{
		int tileIndex = 0;

		LandType myLandType = SelectLandType();
		
		switch (myTileType)
		{
			/* Any tiles that are Cliffs or Cliff transitions can only be BLUE SAND, anything else it will randomly pick between blue sand, yellow sand or mud */
		case TileType.CLIFF: 
			
			// Select a cliff of this edgetype
			if (myEdgeType == EdgeType.BOTTOM)
			{
				tileIndex = Random.Range(190, 199);
			}
			if (myEdgeType == EdgeType.TOP)
			{
				tileIndex = Random.Range(245, 249);
			}
			if (myEdgeType == EdgeType.LEFT)
			{
				tileIndex = Random.Range(180, 189);
			}
			if (myEdgeType == EdgeType.RIGHT)
			{
				tileIndex = Random.Range(170, 179);
			}
			if (myEdgeType == EdgeType.BOTTOM_LEFT_CORNER)
			{
				// Pick between large corner or small corner
				int pick = Random.Range(0, 3);
				if (pick <= 1)
				{
					tileIndex = Random.Range(165, 169);
				}
				else
				{
					tileIndex = Random.Range(261, 264);
				}
			}
			if (myEdgeType == EdgeType.BOTTOM_RIGHT_CORNER)
			{
				// Pick between large corner or small corner
				int pick = Random.Range(0, 3);
				if (pick <= 1)
				{
					tileIndex = Random.Range(160, 164);
				}
				else
				{
					tileIndex = Random.Range(265, 269);
				}
			}
			if (myEdgeType == EdgeType.TOP_LEFT_CORNER)
			{
				tileIndex = Random.Range(280, 284);
			}
			if (myEdgeType == EdgeType.TOP_RIGHT_CORNER)
			{
				tileIndex = Random.Range(285, 289);
			}
			if (myEdgeType == EdgeType.LEFT_BOTTOM_DIAG)
			{
				tileIndex = Random.Range(140, 144);
			}
			if (myEdgeType == EdgeType.RIGHT_BOTTOM_DIAG)
			{
				tileIndex = Random.Range(145, 149);
			}
			if (myEdgeType == EdgeType.LEFT_TOP_DIAG)
			{
				tileIndex = Random.Range(250, 254);
			}
			if (myEdgeType == EdgeType.RIGHT_TOP_DIAG)
			{
				tileIndex = Random.Range(255, 259);
			}
			break;
		case TileType.CLIFF_SHORE: 
			// Select a cliff shore of this edgetype
			if (myEdgeType == EdgeType.BOTTOM)
			{
				tileIndex = Random.Range(230, 234);
			}
			if (myEdgeType == EdgeType.TOP)
			{
				tileIndex = Random.Range(290, 294);
			}
			if (myEdgeType == EdgeType.LEFT)
			{
				tileIndex = Random.Range(115, 119);
			}
			if (myEdgeType == EdgeType.RIGHT)
			{
				tileIndex = Random.Range(125, 129);
			}
			// USING CLIFF CORNERS AND DIAGONALS HERE 
			if (myEdgeType == EdgeType.BOTTOM_LEFT_CORNER)
			{
				// Pick between large corner or small corner
				int pick = Random.Range(0, 3);
				if (pick <= 1)
				{
					tileIndex = Random.Range(165, 169);
				}
				else
				{
					tileIndex = Random.Range(261, 264);
				}
			}
			if (myEdgeType == EdgeType.BOTTOM_RIGHT_CORNER)
			{
				// Pick between large corner or small corner
				int pick = Random.Range(0, 3);
				if (pick <= 1)
				{
					tileIndex = Random.Range(160, 164);
				}
				else
				{
					tileIndex = Random.Range(265, 269);
				}
			}
			if (myEdgeType == EdgeType.TOP_LEFT_CORNER)
			{
				tileIndex = Random.Range(280, 284);
			}
			if (myEdgeType == EdgeType.TOP_RIGHT_CORNER)
			{
				tileIndex = Random.Range(285, 289);
			}
			if (myEdgeType == EdgeType.LEFT_BOTTOM_DIAG)
			{
				tileIndex = Random.Range(140, 144);
			}
			if (myEdgeType == EdgeType.RIGHT_BOTTOM_DIAG)
			{
				tileIndex = Random.Range(145, 149);
			}
			if (myEdgeType == EdgeType.LEFT_TOP_DIAG)
			{
				tileIndex = Random.Range(250, 254);
			}
			if (myEdgeType == EdgeType.RIGHT_TOP_DIAG)
			{
				tileIndex = Random.Range(255, 259);
			}
			break;
		case TileType.SHORE_CLIFF:
			// select a shore cliff of this edge type
			if (myEdgeType == EdgeType.BOTTOM)
			{
				tileIndex = Random.Range(235, 239);
			}
			else if (myEdgeType == EdgeType.TOP)
			{
				tileIndex = Random.Range(294, 299);
			}
			else if (myEdgeType == EdgeType.LEFT)
			{
				tileIndex = Random.Range(110, 114);
			}
			else if (myEdgeType == EdgeType.RIGHT)
			{
				tileIndex = Random.Range(120, 124);
			}
			// USING SHORE CORNERS AND DIAGONALS HERE, so different land types are allowed
			else{
				tileIndex = GetLandTypeIndex(myEdgeType, myLandType);

			}

			break;
			
		case TileType.SHORE:
			// select a shore, of any land type
			tileIndex = GetLandTypeIndex(myEdgeType, myLandType);

			break;
			
		default:

			if (myLandType == LandType.ASH){
				tileIndex = 260;
			}else if (myLandType == LandType.SAND){
				tileIndex = 30;
			}else {
				tileIndex = 100;
			}
			break;
			
		}
		
		return tileIndex;
	}




	LandType SelectLandType (){
		LandType landType = LandType.ASH;
		int select = Random.Range (0, 3);
		if (select == 0) {
			landType = LandType.ASH;
		} else if (select == 1) {
			landType = LandType.SAND;
		} else {
			landType = LandType.MUD;
		}
		return landType;
	}

	int GetLandTypeIndex(EdgeType edgeType, LandType landType) // ONLY WORKS WITH SHORE TILE TYPES(NO CLIFFS!)
	{
		int tileIndex = 0;
		switch (edgeType) {
		case EdgeType.BOTTOM:
			// index
			if (landType == LandType.ASH){
				tileIndex = Random.Range(210, 219);
			}else if (landType == LandType.SAND){
				tileIndex = Random.Range(31, 39);
			}else if (landType == LandType.MUD){
				tileIndex = Random.Range(101, 109);
			}
			break;
		case EdgeType.BOTTOM_LEFT_CORNER:
			//index
			if (landType == LandType.ASH){
				tileIndex = Random.Range(225, 229);
			}else if (landType == LandType.SAND){
				tileIndex = Random.Range(20, 24);
			}else if (landType == LandType.MUD){
				tileIndex = Random.Range(90, 94);
			}
			break;
		case EdgeType.BOTTOM_RIGHT_CORNER:
			//index
			if (landType == LandType.ASH){
				tileIndex = Random.Range(220, 224);
			}else if (landType == LandType.SAND){
				tileIndex = Random.Range(25, 29);
			}else if (landType == LandType.MUD){
				tileIndex = Random.Range(95, 99);
			}
			break;
		case EdgeType.LEFT_BOTTOM_DIAG:
			// index
			if (landType == LandType.ASH){
				tileIndex = Random.Range(150, 154);
			}else if (landType == LandType.SAND){
				tileIndex = Random.Range(10, 14);
			}else if (landType == LandType.MUD){
				tileIndex = Random.Range(30, 34);
			}
			break;
		case EdgeType.RIGHT_BOTTOM_DIAG:
			// index
			if (landType == LandType.ASH){
				tileIndex = Random.Range(154, 159);
			}else if (landType == LandType.SAND){
				tileIndex = Random.Range(15, 19);
			}else if (landType == LandType.MUD){
				tileIndex = Random.Range(35, 39);
			}
			break;
		case EdgeType.LEFT:
			if (landType == LandType.ASH){
				tileIndex = Random.Range(205, 209);
			}else if (landType == LandType.SAND){
				tileIndex = Random.Range(0, 4);
			}else if (landType == LandType.MUD){
				tileIndex = Random.Range(70, 74);
			}
			break;
		case EdgeType.TOP:
			if (landType == LandType.ASH){
				tileIndex = Random.Range(240, 249);
			}else{
				tileIndex = Random.Range(60,64); // there are no top sand tiles, have to add them
			}
			break;
		case EdgeType.TOP_LEFT_CORNER:
			if (landType == LandType.ASH){
				tileIndex = Random.Range(130, 134);
			}else{
				tileIndex = Random.Range(80, 84);
			}
			break;
		case EdgeType.TOP_RIGHT_CORNER:
			if (landType == LandType.ASH){
				tileIndex = Random.Range(135, 139);
			}else{
				tileIndex = Random.Range(85, 89);
			}
			break;
		case EdgeType.LEFT_TOP_DIAG:
			if (landType == LandType.ASH){
				tileIndex = Random.Range(140, 144);
			}else{
				tileIndex = Random.Range(40, 44);
			}
			break;
		case EdgeType.RIGHT_TOP_DIAG:
			if (landType == LandType.ASH){
				tileIndex = Random.Range(145, 149);
			}else{
				tileIndex = Random.Range(65, 69);
			}
			break;
		case EdgeType.RIGHT:
			if (landType == LandType.ASH){
				tileIndex = Random.Range(200, 204);
			}else if (landType == LandType.SAND){
				tileIndex = Random.Range(5, 9);
			}else if (landType == LandType.MUD){
				tileIndex = Random.Range(75, 79);
			}
			break;
		default:
			// center
			if (landType == LandType.ASH){
				tileIndex = 260;
			}else if (landType == LandType.SAND){
				tileIndex = 30;
			}else if (landType == LandType.MUD){
				tileIndex = 100;
			}
			break;


		}

		return tileIndex;
	}
}
