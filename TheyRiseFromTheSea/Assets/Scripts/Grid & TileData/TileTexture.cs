using UnityEngine;
using System.Collections;

public class TileTexture : MonoBehaviour {

	public Texture2D tileSheet;
	public int tileResolution = 32;

    public Texture2D algaeTile;

	int levelWidth, levelHeight;

	MeshRenderer mesh_renderer;

	public Map_Generator mapGenScript;

    public int tilesPerRow, numberOfRows;

  //  Color32[] pixels;

    void Awake()
	{
        /*
		if (!mapGenScript) {
			mapGenScript = GameObject.FindGameObjectWithTag("Map").GetComponent<Map_Generator>();
		}else{
			levelWidth = mapGenScript.width;
			levelHeight = mapGenScript.height;
			// Now that Height and Width are set, build texture
			mesh_renderer = GetComponent<MeshRenderer> ();
			BuildTexture ();
		}
        */
        mesh_renderer = GetComponent<MeshRenderer>();

    }

    void Start()
    {
        //pixels = tileSheet.GetPixels32();
    }


    Color[][] SplitTileSheet()
	{
        /*
        int tilesPerRow = tileSheet.width / tileResolution;
		int numberOfRows = tileSheet.height / tileResolution;
		*/
		Color[][] tiles = new Color[tilesPerRow * numberOfRows][];
       
        //tiles[0] = tileSheet.GetPixels(0, 0, tileResolution, tileResolution);

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

        //int tileSelectionIndex = 0;

        Color[] thisTilePixels;

        /*
        // loop through the array to get the island positions
        for (int x = 0; x < emptyTilesArray.Length; x++)
        {
            // Check tile's neighbors to get the correct tile to render
            thisTilePixels = tiles[CheckTileNeighbors(emptyTilesArray, x)];

            texture.SetPixels((int)emptyTilesArray[x].x * tileResolution, (int)emptyTilesArray[x].y * tileResolution, tileResolution, tileResolution, thisTilePixels);
        }

        */

        Color[] algaePixels = algaeTile.GetPixels(0, 0, 32, 32);

       

        // loop through the mesh to get the positions not rendered as an island tile and make them clear pixels, if it's an island position fill that instead
        for (int mapX = 0; mapX < islandWidth; mapX++)
        {
            for (int mapY = 0; mapY < islandHeight; mapY++)
            {
                Vector2 thisPosition = new Vector2(mapX, mapY);
                if (!CheckIfTileExists(emptyTilesArray, thisPosition))
                {
                    texture.SetPixels((int)thisPosition.x * tileResolution, (int)thisPosition.y * tileResolution, tileResolution, tileResolution, algaePixels);
                }
                else
                {
                    // Check tile's neighbors to get the correct tile to render
                    thisTilePixels = tiles[CheckTileNeighbors(emptyTilesArray, thisPosition)];

                    texture.SetPixels((int)thisPosition.x * tileResolution, (int)thisPosition.y * tileResolution, tileResolution, tileResolution, thisTilePixels);
                }
            }
        }

        texture.filterMode = FilterMode.Bilinear;

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
        Vector2 bottomTile = new Vector2(emptyTilePos.x,emptyTilePos.y - 1);
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

        return GetTileToRender(left, right, top, bottom, leftTop, leftBottom, rightTop, rightBottom);
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

    int GetTileToRender(bool _left, bool _right, bool _top, bool _bottom, bool _topLeft, bool _bottLeft, bool _topRight, bool _bottRight)
    {
        // Check diagonal tiles first:
        if (_top && _right && _left && _bottom && !_bottLeft)
        {
            // Left bottom diagonal
            // select a tile from 0 - 4
            int randomSelect = Random.Range(0, 4);
            return randomSelect;
        }
        else if (_top && _left && _bottom && _right && !_bottRight)
        {
            // Right bottom diagonal
            // select a tile
            int randomSelect = Random.Range(5, 9);
            return randomSelect;
        }
        else if (_top && _right && !_left && !_bottom)
        {
            // bottom left corner
            // select a tile
            int randomSelect = Random.Range(75, 79);
            return randomSelect;
        }
        else if (_top && _left && _right && !_bottom)
        {
            // Bottom
            // select a tile
            int randomSelect = Random.Range(60, 69);
            return randomSelect;
        }
        else if (_top && _left && !_right && !_bottom)
        {
            // Bottom right corner
            // select a tile
            int randomSelect = Random.Range(70, 74);
            return randomSelect;
        }
        else if (_top && _left && _bottom && !_right)
        {
            // Right
            // select a tile
            int randomSelect = Random.Range(50, 54);
            return randomSelect;
        }
        else if (_top && _right && _bottom && !_left)
        {
            // left
            // select a tile
            int randomSelect = Random.Range(55, 59);
            return randomSelect;
        }
        else if (!_top && _right && _left && _bottom)
        {
            // top shore
            // select a tile
            int randomSelect = Random.Range(90, 99);
            return randomSelect;
        }
        /*
        else if (!_top && !_right && _left && _bottom)
        {
            // top right corner
            // select a tile
            int randomSelect = Random.Range(75, 79);
            return randomSelect;
            return 90;
        }
        
        else if (!_top && _right && !_left && _bottom)
        {
            // top left corner
            return 90;
        }
        */
        else
        {
            return 110;
        }

    }










    /*
    public void BuildTexture(Vector2[] emptyTilesArray, int islandWidth, int islandHeight)
    {
        int textureWidth = islandWidth * tileResolution;
        int textureHeight = islandHeight * tileResolution;
        Texture2D texture = new Texture2D(textureWidth, textureHeight);
        texture.name = "Island Texture";

        Debug.Log("TILETEXTURE: Created a texture of width: " + textureWidth + " and height: " + textureHeight);
        Debug.Log("TILETEXTURE: the empty tiles array length is " + emptyTilesArray.Length);

        // Get tiles by Splitting up the sprite sheet
        Color[][] tiles = SplitTileSheet();

        int tileSelectionIndex = 0;

        // loop through the array to get the island positions

        Color[] thisTilePixels = tiles[4];

        texture.SetPixels((int)emptyTilesArray[0].x, (int)emptyTilesArray[0].y, tileResolution, tileResolution, thisTilePixels);

        // Make sure the texture is in Point filter mode (THIS CAN CHANGE DEPENDING ON THE ART!)
        texture.filterMode = FilterMode.Point;


        // Apply texture
        texture.Apply();


        mesh_renderer.sharedMaterial.mainTexture = texture;

    }
    */

    /*
	public void BuildTexture(int width, int height)
	{
		int textureWidth = width * tileResolution;
		int textureHeight = height * tileResolution;
		Texture2D texture = new Texture2D (textureWidth, textureHeight);
		
		// Get tiles by Splitting up the sprite sheet
		Color[][] tiles = SplitTileSheet ();

        int tileSelectionIndex = 0;

        for (int y =0; y < height; y++){
			for (int x =0; x < width; x++){
                
                if (x == 0 && y == 0)
                {
                    tileSelectionIndex = 4;
                }
                else if (x == width - 1 && y == 0)
                {
                    tileSelectionIndex = 3;
                }
                else if (x < width - 1 && x > 0 && y == 0)
                {
                    tileSelectionIndex = 0;
                }
                else {
                    tileSelectionIndex = 5;
                }
               
				Color[] thisTilePixels = tiles[5];
				
				texture.SetPixels(x * tileResolution, y * tileResolution, tileResolution, tileResolution, thisTilePixels);
			}
		}
        

          // Make sure the texture is in Point filter mode (THIS CAN CHANGE DEPENDING ON THE ART!)
         texture.filterMode = FilterMode.Point;

		
		// Apply texture
		texture.Apply ();
		
		
		mesh_renderer.sharedMaterial.mainTexture = texture;
		
	}
	*/

}
