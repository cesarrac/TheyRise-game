using UnityEngine;
using System.Collections;

public class TileTexture : MonoBehaviour {

	public Texture2D tileSheet;
	public int tileResolution = 32;

	int levelWidth, levelHeight;

	MeshRenderer mesh_renderer;

	public Map_Generator mapGenScript;

	void Awake()
	{
		if (!mapGenScript) {
			mapGenScript = GameObject.FindGameObjectWithTag("Map").GetComponent<Map_Generator>();
		}else{
			levelWidth = mapGenScript.width;
			levelHeight = mapGenScript.height;
			// Now that Height and Width are set, build texture
			mesh_renderer = GetComponent<MeshRenderer> ();
			BuildTexture ();
		}


	}


	Color[][] SplitTileSheet()
	{
		int tilesPerRow = tileSheet.width / tileResolution;
		int numberOfRows = tileSheet.height / tileResolution;
		
		Color[][] tiles = new Color[tilesPerRow * numberOfRows][];
		tiles [0]=tileSheet.GetPixels(0, 0, tileResolution, tileResolution);

		
		return tiles;
	}

	void BuildTexture(int tileSelectionIndex = 0)
	{
		int textureWidth = levelWidth * tileResolution;
		int textureHeight = levelHeight * tileResolution;
		Texture2D texture = new Texture2D (textureWidth, textureHeight);
		
		// Get tiles by Splitting up the sprite sheet
		Color[][] tiles = SplitTileSheet ();
		
		
		for (int y =0; y < levelHeight; y++){
			for (int x =0; x < levelWidth; x++){
				
				Color[] thisTilePixels = tiles[tileSelectionIndex];
				
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
