using UnityEngine;
using System.Collections;

public class TileTexture : MonoBehaviour {

	public Texture2D tileSheet;
	public int tileResolution = 32;

	int levelWidth, levelHeight;

	MeshRenderer mesh_renderer;

	public Map_Generator mapGenScript;

    public int tilesPerRow, numberOfRows;
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
                /*
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
               */
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
	

}
