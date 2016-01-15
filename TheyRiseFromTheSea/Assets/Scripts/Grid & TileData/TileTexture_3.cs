using UnityEngine;
using System.Collections;
using System.Collections.Generic;


[System.Serializable]
public class GraphicTile
{
    // Tile attributes/types:

    public enum TilePositionTypes       // Defines their position using the tile's world coordinates
    {
        BOTTOM, TOP, LEFT, RIGHT,
        BOTTOM_LEFT_CORNER, BOTTOM_RIGHT_CORNER,
        TOP_LEFT_CORNER, TOP_RIGHT_CORNER,
        LEFT_BOTTOM_DIAG, RIGHT_BOTTOM_DIAG,
        LEFT_TOP_DIAG, RIGHT_TOP_DIAG, CENTER, CLEAR
    }

    public enum TileEdgeTypes { CLIFF_SHORE, SHORE_CLIFF, CLIFF, SHORE, CENTER, UNDEFINED }  // Defines Shore/Edge type according to tiles around them

    public enum TileLandTypes { ASH, SAND, MUD }    // Defines types of land

    TilePositionTypes myTilePosType;
    TileEdgeTypes myTileEdgeType = TileEdgeTypes.UNDEFINED;
    TileLandTypes myTileLandType;
    Vector2 myPos;

    public TilePositionTypes MyTilePosType
    {
        get
        {
            return myTilePosType;
        }

        set
        {
            myTilePosType = value;
        }
    }
    public TileEdgeTypes MyTileEdgeType
    {
        get
        {
            return myTileEdgeType;
        }

        set
        {
            myTileEdgeType = value;
        }
    }
    public TileLandTypes MyTileLandType
    {
        get
        {
            return myTileLandType;
        }

        set
        {
            myTileLandType = value;
        }
    }
    public Vector2 MyPos
    {
        get
        {
            return myPos;
        }

        set
        {
            myPos = value;
        }
    }

    // Constructors:

    // Empty
    public GraphicTile() { }

    // One for CLEAR tiles
    public GraphicTile(TilePositionTypes tilePosType, Vector2 myPosition)
    {
        myTilePosType = tilePosType;
        myPos = myPosition;
    }
    // Another for all the LAND tiles
    public GraphicTile(TilePositionTypes tilePosType, TileEdgeTypes tileEdgeType, TileLandTypes tileLandType, Vector2 myPosition)
    {
        myTilePosType = tilePosType;
        myTileEdgeType = tileEdgeType;
        myTileLandType = tileLandType;
        myPos = myPosition;
    }

    public GraphicTile(TileLandTypes myLandType)
    {
        myTileLandType = myLandType;
    }
}

class LandRegion : GraphicTile
{

    // Those regions will need:
    // A starting position (this will be its bottom left corner)
    // A total of tiles in the region
    // a way to decide width and height
    // A 2D array to hold the tile data
    // A way to decide which tiles will be land and which will be clear (to create interesting shapes hopefully!)
    // A way to define all the tiles in the region that are NOT clear (tilePosType, edgeType & landType)
    // A way to determine where to place regions so that they don't all end up right next to each other
    // To cap the width and height, two variables of maxWidth and maxHeight

    // They follow these rules:
    // They have a minimum of 1 tile
    // Their maximum dimensions cannot exceed the dimensions of the island map (width, height)
    // They are not right next to each other.



    GraphicTile[] regionTiles;
    public GraphicTile[] RegionTiles { get { return regionTiles; } set { regionTiles = value; } }

    public List<Vector2> tilePosList;
    Vector2[] tilePositions;
    public Vector2[] TilePositions { get { return tilePositions; } set { tilePositions = value; } }

    Vector2 startingPosition;
    public Vector2 StartingPosition { get { return startingPosition; } set { startingPosition = value; } }

    int maxTilesAllowed;
    public int MaxTiles { get { return maxTilesAllowed; } set { maxTilesAllowed = value; } }

    int width, height;

    // Constructor

    public LandRegion(Vector2 startingPos, int maxTiles)
    {
        startingPosition = startingPos;
        maxTilesAllowed = maxTiles;
        /*
        tilePositions = new Vector2[maxTiles];
        tilePositions[0] = startingPos;
        */

        // Init the tile pos list
        tilePosList = new List<Vector2>();


    }

    public void CleanUpListAndCreateArray()
    {
        foreach (Vector2 v2 in tilePosList)
        {
            if (v2 == null)
            {
                tilePosList.Remove(v2);
            }
        }

        // Once all null vector 2's have been removed, turn this list into an array
        tilePositions = new Vector2[tilePosList.Count];
        tilePositions = tilePosList.ToArray();

        InitRegionTiles(tilePositions.Length);
    }

    void InitRegionTiles(int length)
    {
        regionTiles = new GraphicTile[length];
        for (int x = 0; x < regionTiles.Length; x++)
        {
            regionTiles[x] = new GraphicTile();
        }
    }
}



public class TileTexture_3 : MonoBehaviour
{
    public static TileTexture_3 instance { get; protected set; }

    Texture2D Texture1, Texture2, Texture3;

    [Header("Tile Sheet Settings:")]
    public int tileResolution = 32;

    [System.Serializable]
    public class TileSheet
    {
        public enum LandType { ASH, SAND, MUD, MULTI};
        public LandType landType;
        public Texture2D tileSheetTexture;
        public int tilesPerRow, numberOfRows;
    }

    [Header("Land Type ASH:")]
    public TileSheet ashTileSheet;

    [Header("Land Type SAND & MUD:")]
    public TileSheet sandMudTileSheet;

    [Header("Transparent tile:")]
    public Texture2D clearTile;

    [Header("Water Ripples tilesheet:")]
    public TileSheet rippleSheet;


   
    // A 2D array that will hold all the baseGraphicTiles and their attributes according to the positions of the island map
    GraphicTile[,] baseGraphicTiles, secondGraphicTiles;

    public Dictionary<Vector2, GraphicTile> all_GraphicTiles = new Dictionary<Vector2, GraphicTile>();

    // A List of Graphic Tile that can be considered for land types
    List<GraphicTile> tilesToConsider;

    public Vector2[] centerTiles;

    // A List that holds all the Land Regions created
    List<LandRegion> landRegions;

    // Renderer Components for Base, Second, and Third Layers
    [Header("Mesh Renderers:")]
    public MeshRenderer base_renderer, second_renderer, shore_renderer;

    GraphicTile.TileLandTypes baseLandType;
    GraphicTile.TileLandTypes secondaryLandType;

    Color[] clearPixels;

    [HideInInspector]
    public string seed; // filled by the mapgenerator
    [HideInInspector]
    public int randomFillPercent;

    public System.Random pseudoRandom;

    public Map_Generator mapGenerator;

    int ripplesWidth, ripplesHeight;

    bool isSettingShoreTile = false;
    bool isSettingShoreCorner = false;

    public bool hasTopLayer = false;

    void Awake()
    {
        instance = this;

        if (!base_renderer || !second_renderer)
        {
            Debug.LogError("TILETEXTURE: Renderers are missing!!!");
        }

        // Clear tiles pixels
        clearPixels = clearTile.GetPixels(0, 0, 32, 32);
    }

    Color[][] SplitTileSheet(GraphicTile.TileLandTypes landType)
    {
        TileSheet tileSheet;
        if (landType == GraphicTile.TileLandTypes.ASH)
        {
            tileSheet = ashTileSheet;
        }
        else
        {
            tileSheet = sandMudTileSheet;
        }

        Color[][] tiles = new Color[tileSheet.tilesPerRow * tileSheet.numberOfRows][];

        for (int y = 0; y < tileSheet.numberOfRows; y++)
        {
            for (int x = 0; x < tileSheet.tilesPerRow; x++)
            {
                tiles[y * tileSheet.tilesPerRow + x] = tileSheet.tileSheetTexture.GetPixels(x * tileResolution, y * tileResolution, tileResolution, tileResolution);

            }
        }

        return tiles;
    }

    Color[][] SplitRippleSheet()
    {
        Color[][] tiles = new Color[rippleSheet.tilesPerRow * rippleSheet.numberOfRows][];

        // All i need is the first ripple tile's pixels, since this will be animated
        tiles[0] = rippleSheet.tileSheetTexture.GetPixels(0 * tileResolution, 0, tileResolution, tileResolution);

        return tiles;
    }

    void GetClimateLandTypes()
    {
        baseLandType = Climate_Manager.Instance.curClimateMap.baseLandType;
        if (Climate_Manager.Instance.curClimateMap.hasTopLayer)
        {
            hasTopLayer = true;
            secondaryLandType = Climate_Manager.Instance.curClimateMap.secondLandType;
        }
       
    }

    // THIS IS CALLED BY MAP GENERATOR
    public void DefineTilesAndGenerateBaseTexture(Vector2[] emptyTilesArray, int islandWidth, int islandHeight)
    {
        // Define the Land Types using the Climate Manager
        GetClimateLandTypes();

        ripplesWidth = islandWidth;
        ripplesHeight = islandHeight;


        int texWidth = islandWidth * tileResolution;
        int texHeight = islandHeight * tileResolution;
        Texture2D texture = new Texture2D(texWidth, texHeight);
        texture.name = "Base Land Texture";

        // Split Base land type's tile sheet, based on the already set base land type
        Color[][] tiles = SplitTileSheet(baseLandType);
        // Use this array to Set Pixels
        Color[] thisTilePixels;

        // Initialize the Graphic tiles array to be as big as the island, width * height
        baseGraphicTiles = new GraphicTile[islandWidth, islandHeight];
        // have to initialize all these graphictiles as UNDEFINED EdgeTypes

        // Initialize list of tiles to consider (Center tiles)
        tilesToConsider = new List<GraphicTile>();

        for (int mapX = 0; mapX < islandWidth; mapX++)
        {
            for (int mapY = 0; mapY < islandHeight; mapY++)
            {
                Vector2 thisPosition = new Vector2(mapX, mapY);
                if (!CheckIfTileExists(emptyTilesArray, thisPosition))
                {
                    // Tile is water, not land, so define tileType as clear

                    // baseGraphicTiles[mapX, mapY] = new GraphicTile(GraphicTile.TilePositionTypes.CLEAR, thisPosition);

                    all_GraphicTiles.Add(thisPosition, new GraphicTile(GraphicTile.TilePositionTypes.CLEAR, thisPosition));

                    // set pixels to clear
                    thisTilePixels = clearPixels;

                    texture.SetPixels(mapX * tileResolution, mapY * tileResolution, tileResolution, tileResolution, thisTilePixels);

                }
                else
                {
                    // Tile IS land so define it:

                    // Check Tile's neighbors to define this new tile
                    GraphicTile thisTile = CheckTileNeighbors(emptyTilesArray, thisPosition, baseLandType);
                    //baseGraphicTiles[mapX, mapY] = CheckTileNeighbors(emptyTilesArray, thisPosition, baseLandType);

                    // Set the land type to be of the base land type
                    thisTile.MyTileLandType = baseLandType;

                    all_GraphicTiles.Add(thisPosition, thisTile);

                    //int offset_x = 0;
                    //int offset_y = 0;

                    // Reset is shore tile flag
                    isSettingShoreTile = false;

                    //if (thisTile.MyTilePosType != GraphicTile.TilePositionTypes.CENTER)
                    //{
                    //    isSettingShoreTile = true;
                    //    isSettingShoreCorner = false;

                    //    switch (thisTile.MyTilePosType)
                    //    {
                    //        case GraphicTile.TilePositionTypes.BOTTOM:
                    //            offset_y = -1;
                    //            isSettingShoreTile = false;
                    //            break;
                    //        case GraphicTile.TilePositionTypes.BOTTOM_LEFT_CORNER:
                    //           // offset_y = -1;
                    //            offset_x = -1;
                    //            isSettingShoreCorner = true;
                    //            break;
                    //        //case GraphicTile.TilePositionTypes.BOTTOM_RIGHT_CORNER:
                    //        //    offset_x = 1;
                    //        //    //offset_y = -1;
                    //        //    break;

                    //        case GraphicTile.TilePositionTypes.LEFT_BOTTOM_DIAG:
                    //            offset_x = -1;
                    //            //offset_y = -1;
                    //            break;

                    //        case GraphicTile.TilePositionTypes.LEFT:
                    //            offset_x = -1;
                    //            break;

                    //        //case GraphicTile.TilePositionTypes.TOP_LEFT_CORNER:
                    //        //    offset_x = -1;
                    //        //    //offset_y = 1;
                    //        //    break;
                    //        //case GraphicTile.TilePositionTypes.TOP:
                    //        //    offset_y = 1;
                    //        //    isSettingShoreTile = false;
                    //        //    break;
                    //        //case GraphicTile.TilePositionTypes.TOP_RIGHT_CORNER:
                    //        //    //offset_y = 1;
                    //        //    offset_x = 1;
                    //        //    break;
                    //        //case GraphicTile.TilePositionTypes.RIGHT_BOTTOM_DIAG:
                    //        //    offset_x = 1;
                    //        //    //offset_y = 1;
                    //        //    break;
                    //        //case GraphicTile.TilePositionTypes.LEFT_TOP_DIAG:
                    //        //    offset_x = -1;
                    //        //   // offset_y = 1;
                    //        //    break;
                    //        //case GraphicTile.TilePositionTypes.RIGHT_TOP_DIAG:
                    //        //    offset_x = 1;
                    //        //    //offset_y = 1;
                    //        //    break;

                    //        //case GraphicTile.TilePositionTypes.RIGHT:
                    //        //    offset_x = 1;
                    //        //    break;

                    //        default:
                    //            offset_x = 0;
                    //            offset_y = 0;
                    //            isSettingShoreTile = false;
                    //            break;

                    //    }

                    //    int tileIndex = FindIndex(thisTile.MyTilePosType, thisTile.MyTileEdgeType, baseLandType);

                    //    thisTilePixels = tiles[tileIndex];

                    //    texture.SetPixels((mapX + offset_x) * tileResolution, (mapY + offset_y) * tileResolution, tileResolution, tileResolution, thisTilePixels);

                    //    tilesToConsider.Add(thisTile);

                    //    if (isSettingShoreTile)
                    //    {
                    //        if (isSettingShoreCorner)
                    //        {
                    //            if (thisTile.MyTilePosType == GraphicTile.TilePositionTypes.BOTTOM_LEFT_CORNER || thisTile.MyTilePosType == GraphicTile.TilePositionTypes.BOTTOM_RIGHT_CORNER)
                    //            {
                    //                // Set a bottom shore tile
                    //                thisTilePixels = tiles[FindIndex(GraphicTile.TilePositionTypes.BOTTOM, thisTile.MyTileEdgeType, thisTile.MyTileLandType)];
                    //                SetExtraCentersOnTexture(texture, mapX, mapY, thisTilePixels, tileResolution);
                    //            }
                    //            else if (thisTile.MyTilePosType == GraphicTile.TilePositionTypes.TOP_LEFT_CORNER || thisTile.MyTilePosType == GraphicTile.TilePositionTypes.TOP_RIGHT_CORNER)
                    //            {
                    //                // Set a top shore tile
                    //                thisTilePixels = tiles[FindIndex(GraphicTile.TilePositionTypes.TOP, thisTile.MyTileEdgeType, thisTile.MyTileLandType)];
                    //                SetExtraCentersOnTexture(texture, mapX, mapY, thisTilePixels, tileResolution);
                    //            }
                    //        }
                    //        else
                    //        {
                    //            // Set a Center tile to account for the empty space left after setting the offset
                    //            thisTilePixels = tiles[FindIndex(GraphicTile.TilePositionTypes.CENTER, GraphicTile.TileEdgeTypes.CENTER, thisTile.MyTileLandType)];
                    //            SetExtraCentersOnTexture(texture, mapX, mapY, thisTilePixels, tileResolution);

                    //        }

                    //        //if (thisTile.MyTilePosType == GraphicTile.TilePositionTypes.BOTTOM_LEFT_CORNER || )
                    //        //{
                    //        //    // Set a half center tiles
                    //        //    thisTilePixels = tiles[FindIndex(GraphicTile.TilePositionTypes.CENTER, GraphicTile.TileEdgeTypes.CENTER, thisTile.MyTileLandType)];
                    //        //    SetExtraCentersOnTexture(texture, mapX, mapY, thisTilePixels, 16);
                    //        //}
                    //        //else
                    //        //{
                    //        //    // Set a Center tile to account for the empty space left after setting the offset
                    //        //    thisTilePixels = tiles[FindIndex(GraphicTile.TilePositionTypes.CENTER, GraphicTile.TileEdgeTypes.CENTER, thisTile.MyTileLandType)];
                    //        //    SetExtraCentersOnTexture(texture, mapX, mapY, thisTilePixels, tileResolution);
                    //        //}

                    //    }

                    //}

                    //else
                    //{

                    //}

                    int tileIndex = FindIndex(thisTile.MyTilePosType, thisTile.MyTileEdgeType, baseLandType);

                    thisTilePixels = tiles[tileIndex];

                    texture.SetPixels(mapX * tileResolution, mapY * tileResolution, tileResolution, tileResolution, thisTilePixels);

                    tilesToConsider.Add(thisTile);

                }
            }

            texture.filterMode = FilterMode.Trilinear;
            texture.wrapMode = TextureWrapMode.Clamp;

            // Apply texture
            texture.Apply();

            //base_renderer.sharedMaterial.mainTexture = texture;
            
            Texture1 = texture;
        }


        /* At this Point all tiles are defined  as if we are only using ONE land type, ash.
        This can later be changed to use whatever the BASE land type of this level is. 
        Then we need a method that adds all the tiles that are CENTER tiles to a list of tiles
        to be considered as locations for SECONDARY or THIRD land types. */

        // Split the List of tiles to Consider between those tiles that are edges and those that are center( We are ONLY considering tiles that are NOT clear/water)
        SortGraphicTiles(tilesToConsider);



    }

    void SetExtraCentersOnTexture(Texture2D tex, int x, int y, Color[] tilePixels, int tileRes)
    {
        tex.SetPixels(x * tileRes, y * tileRes, tileResolution, tileResolution, tilePixels);
    }

    public void DefineTilesAndGenerateSecondTexture(Vector2[] emptyTilesArray, int islandWidth, int islandHeight)
    {
        // Make sure this Mesh object shares the same mesh as Base mesh
        Mesh islandMesh = base_renderer.gameObject.GetComponent<MeshFilter>().mesh;
        second_renderer.gameObject.GetComponent<MeshFilter>().mesh = islandMesh;

        int texWidth = islandWidth * tileResolution;
        int texHeight = islandHeight * tileResolution;
        Texture2D texture2 = new Texture2D(texWidth, texHeight);
        texture2.name = "Top Land Texture";

        //GraphicTile.TileLandTypes landType = GetNewLandType(baseLandType);
        // Split Base land type's tile sheet, based on the already set base land type
        Color[][] tiles = SplitTileSheet(secondaryLandType);
        // Use this array to Set Pixels
        Color[] thisTilePixels;

        secondGraphicTiles = new GraphicTile[islandWidth , islandHeight];

        for (int mapX = 0; mapX < islandWidth; mapX++)
        {
            for (int mapY = 0; mapY < islandHeight; mapY++)
            {
                Vector2 thisPosition = new Vector2(mapX, mapY);
                if (!CheckIfTileExists(emptyTilesArray, thisPosition))
                {
                    // Tile is water, not land, so define tileType as clear
                    // secondGraphicTiles[mapX, mapY] = new GraphicTile(GraphicTile.TilePositionTypes.CLEAR, thisPosition);
                    if (!all_GraphicTiles.ContainsKey(thisPosition))
                    {
                        all_GraphicTiles.Add(thisPosition, new GraphicTile(GraphicTile.TilePositionTypes.CLEAR, thisPosition));

                    }
                    else
                    {
                        if (all_GraphicTiles[thisPosition].MyTilePosType != GraphicTile.TilePositionTypes.CLEAR)
                        {
                            all_GraphicTiles[thisPosition].MyTilePosType = GraphicTile.TilePositionTypes.CLEAR;
                        }
                    }

                    // set pixels to clear
                    thisTilePixels = clearPixels;

                    texture2.SetPixels(mapX * tileResolution, mapY * tileResolution, tileResolution, tileResolution, thisTilePixels);

                }
                else
                {
                    // Tile IS land so define it:

                    // Check Tile's neighbors to define this new tile
                    GraphicTile thisTile = CheckTileNeighbors(emptyTilesArray, thisPosition, secondaryLandType, true);

                    if (!all_GraphicTiles.ContainsKey(thisPosition))
                    {
                        all_GraphicTiles.Add(thisPosition, thisTile);
                    }
                    else
                    {
                        all_GraphicTiles.Remove(thisPosition);

                        all_GraphicTiles.Add(thisPosition, thisTile);

                    }




                    thisTilePixels = tiles[FindIndex(thisTile.MyTilePosType, thisTile.MyTileEdgeType, secondaryLandType)];

                    texture2.SetPixels(mapX * tileResolution, mapY * tileResolution, tileResolution, tileResolution, thisTilePixels);

                }
            }

            texture2.filterMode = FilterMode.Trilinear;
            texture2.wrapMode = TextureWrapMode.Clamp;

            // Apply texture
            texture2.Apply();

            // Give the mesh collider the same mesh
            second_renderer.gameObject.GetComponent<MeshCollider>().sharedMesh = islandMesh;

            Texture2 = texture2;

            ApplyTextures();
        }

    }




    void SortGraphicTiles (List<GraphicTile> tiles)
    {
        //tilesForOthers = new List<GraphicTile>();

        // Extracting the Vector 2 positions on the tiles list and keeping only the ones that are of Center pos type
        List<Vector2> secondaryTilePositions = new List<Vector2>();

        List<Vector2> shoreTilePositions = new List<Vector2>();

        foreach(GraphicTile gTile in tiles)
        {
            if (gTile.MyTilePosType == GraphicTile.TilePositionTypes.CENTER)
            {
                //tilesForOthers.Add(gTile);
                secondaryTilePositions.Add(gTile.MyPos);
            }
            else
            {
                shoreTilePositions.Add(gTile.MyPos);
            }
          
        }
        // Pass the list of positions as an array
        // These are ONLY Center tiles, so the top layer can avoid the shores and leave that for the base land type
        centerTiles = secondaryTilePositions.ToArray();

        GenerateShoreRippleTexture(shoreTilePositions.ToArray());

        if (hasTopLayer)
        {
            mapGenerator.GenerateTopLayerMap(centerTiles);
        }
        else
            ApplyTextures();
   

       
    }

    void GenerateShoreRippleTexture(Vector2[] shoreTilePositions)
    {
        int ripTexWidth = ripplesWidth * tileResolution;
        int ripTexHeight = ripplesHeight * tileResolution;

        Texture2D textureRipples = new Texture2D(ripTexWidth, ripTexHeight);
        textureRipples.name = "Ripples Texture";
        Debug.Log("RIPPLES TEXTURE created! width and height: " + ripTexWidth + " " + ripTexHeight);

        // Split Base land type's tile sheet, based on the already set base land type
        Color[][] tiles = SplitRippleSheet();
        // Use this array to Set Pixels
        Color[] thisTilePixels;

        for (int mapX = 0; mapX < ripplesWidth; mapX++)
        {
            for (int mapY = 0; mapY < ripplesHeight; mapY++)
            {
                Vector2 thisPosition = new Vector2(mapX, mapY);
                if (!CheckIfTileExists(shoreTilePositions, thisPosition))
                {
                    // set pixels to clear
                    thisTilePixels = clearPixels;

                    textureRipples.SetPixels(mapX * tileResolution, mapY * tileResolution, tileResolution, tileResolution, thisTilePixels);

                }
                else
                {
                    // Tile IS a ripple tile:
                    
                    thisTilePixels = tiles[0];

                    textureRipples.SetPixels(mapX * tileResolution, mapY * tileResolution, tileResolution, tileResolution, thisTilePixels);

                }
            }

            textureRipples.filterMode = FilterMode.Trilinear;
            textureRipples.wrapMode = TextureWrapMode.Clamp;

            // Apply texture
            textureRipples.Apply();

            Texture3 = textureRipples;

        }
    }

    //void FillSecondLayer(List<Vector2> potentialPositions, int width, int height)
    //{
    //    // Second Texture Variables:
    //    // Make sure this Mesh object shares the same mesh as Base mesh
    //    Mesh islandMesh = base_renderer.gameObject.GetComponent<MeshFilter>().mesh;
    //    second_renderer.gameObject.GetComponent<MeshFilter>().mesh = islandMesh;
        
    //    int textureWidth = width * tileResolution;
    //    int textureHeight = height * tileResolution;
    //    Texture2D texture2 = new Texture2D(textureWidth, textureHeight);
    //    texture2.name = "Secondary Land Texture";

    //    // Turn land regions into an array for faster searching
    //    //LandRegion[] Regions = landRegions.ToArray();

    //    GraphicTile.TileLandTypes currLandType = baseLandType;


    //    Color[][] tiles = SplitTileSheet(currLandType);
    //    // Use this array to Set Pixels
    //    Color[] thisTilePixels;

    //    // Create land regions, limit them to a total of regions, using the otherTiles positions
    //   landRegions = new List<LandRegion>();

       

    //    for (int x = 0; x < width; x++)
    //    {
    //        for (int y = 0; y < height; y++)
    //        {
    //            Vector2 thisPosition = new Vector2(x, y);

    //            //This is a "hacky" way of doing it, if posIndex does not come back with a hit it'll return as 99999
    //            int posIndex = GetIndexOfExistingTile(potentialPositions.ToArray(), thisPosition);

    //            int skipOrNo = Random.Range(0, 2);
    //            if (posIndex < potentialPositions.Count && skipOrNo > 0)
    //            {
    //                // We hit a potential tile position.

    //                // This is the starting position
    //                Vector2 startingPosition = thisPosition;
    //                Debug.Log("New Starting Position: " + startingPosition);

    //                //Decide how many tiles this region will have (MINIMUM = 9, MAX can be the number that looks best
    //                //int maxtiles = Random.Range(9, 28);
    //                int maxtiles = 10;
    //                //Construct a new Land Region
    //                LandRegion newRegion = new LandRegion(startingPosition, maxtiles);

    //                // Loop through the region's max tiles assigning a position
    //                for (int m = 0; m < maxtiles; m++)
    //                {
    //                    // Fill the new Region's positions, including starting pos

    //                    // To make sure this region will be a nice grid we need to force the positions
    //                    // First one is potentialPositions[posIndex]
    //                    if (m == 0)
    //                    {
    //                        newRegion.tilePosList.Add(potentialPositions[posIndex]);
    //                    }
    //                    else if (m >= 1 && m <= 2)
    //                    {
    //                        // Second potentialPositions[posIndex].x, potentialPositions[posIndex].y + 1
    //                        // Third potentialPositions[posIndex].x, potentialPositions[posIndex].y + 2
    //                        Vector2 newPosition = new Vector2(potentialPositions[posIndex].x, potentialPositions[posIndex].y + m);
    //                        newRegion.tilePosList.Add(newPosition);
                            
    //                    }
    //                    // Now second column
    //                    else if (m == 3)
    //                    {
    //                        // Fourth potentialPositions[posIndex].x + 1, potentialPositions[posIndex].y
    //                        Vector2 newPosition = new Vector2(potentialPositions[posIndex].x + 1, potentialPositions[posIndex].y);
    //                        newRegion.tilePosList.Add(newPosition);
    //                    }
    //                    else if (m == 4)
    //                    {
    //                        // Fifth potentialPositions[posIndex].x + 1, potentialPositions[posIndex].y + 1
    //                        Vector2 newPosition = new Vector2(potentialPositions[posIndex].x + 1, potentialPositions[posIndex].y + 1);
    //                        newRegion.tilePosList.Add(newPosition);
                            
    //                    }
    //                    else if (m == 5)
    //                    {
    //                        // Sixth potentialPositions[posIndex].x + 1, potentialPositions[posIndex].y + 2
    //                        Vector2 newPosition = new Vector2(potentialPositions[posIndex].x + 1, potentialPositions[posIndex].y + 2);
    //                        newRegion.tilePosList.Add(newPosition);
    //                    }
    //                    // Third column
    //                    else if (m == 6)
    //                    {
    //                        // Seventh potentialPositions[posIndex].x + 2, potentialPositions[posIndex].y
    //                        Vector2 newPosition = new Vector2(potentialPositions[posIndex].x + 2, potentialPositions[posIndex].y);
    //                        newRegion.tilePosList.Add(newPosition);
    //                    }else if (m == 7)
    //                    {
    //                        // Eigth potentialPositions[posIndex].x + 2, potentialPositions[posIndex].y + 1
    //                        Vector2 newPosition = new Vector2(potentialPositions[posIndex].x + 2, potentialPositions[posIndex].y + 1);
    //                        newRegion.tilePosList.Add(newPosition);
    //                    }
    //                    else
    //                    {
    //                        // Ninth potentialPositions[posIndex].x + 2, potentialPositions[posIndex].y + 2
    //                        Vector2 newPosition = new Vector2(potentialPositions[posIndex].x + 2, potentialPositions[posIndex].y + 2);
    //                        newRegion.tilePosList.Add(newPosition);
    //                    }


    //                    //// Add this position to the region's Tile Positions List but make sure PosIndex + M is < total of potential Positions
    //                    //if (posIndex + m < potentialPositions.Count)
    //                    //{
    //                    //    newRegion.tilePosList.Add(potentialPositions[posIndex + m]);
    //                    //    Debug.Log("New Region Position: " + potentialPositions[posIndex + m]);

    //                    //    // Instead of removing I can make it a null vector 2
    //                    //    potentialPositions[posIndex + m] = Vector2.zero;
                            

                           
    //                    //}


    //                }
    //                // After assigning the positions to the region, we need to clean up the list inside the region and make it an array of V2
    //                newRegion.CleanUpListAndCreateArray();

    //                // Now that this region's positions are defined, add it to the list of regions
    //                landRegions.Add(newRegion);

    //                // remove as a potential position to avoid overlap between regions, this will be a clear tile once the tex is generated
    //                //for (int i = 0; i < maxtiles; i++)
    //                //{
    //                //    potentialPositions.RemoveAt(posIndex + i); // **** This is probably fucking up the index later on since it reduces the list
    //                //}
                    

    //            }
    //            else
    //            {
    //                // Not a potential position, set this to clear tile
    //                // Set clear pixels
    //                thisTilePixels = clearPixels;
    //                texture2.SetPixels(x * tileResolution, y * tileResolution, tileResolution, tileResolution, thisTilePixels);
    //            }
    //        }
    //    }

    //    // Now we define each region's graphic tiles
    //    if (landRegions.Count > 0)
    //        DefineLandRegionTiles(width, height, landRegions.ToArray(), texture2);


    //    //Debug.Log("Region #" + (landRegions.Count - 1) + ", Max Tiles = " + newRegion.MaxTiles + ", TilePosArray = " + newRegion.TilePositions.Length + ", Tiles = " + newRegion.RegionTiles.Length);


    //}
    /*
    // For each region's region tile, decide wether it's a land or clear tile
    int random = Random.Range(0, 100);

                if (random<randomFillPercent)
                {

                // This is a land tile
                // Choose new land type
                 GraphicTile.TileLandTypes newLandType = GetNewLandType(baseLandType);

                // Loop through the region's set Tile Positions and define the Tiles
                for (int i = 0; i<newRegion.TilePositions.Length; i++)
                        {
                            // Define this new region's tile
                            newRegion.RegionTiles[i] = DefineRegionTile(newLandType, newRegion.TilePositions, newRegion.TilePositions[i], newRegion.RegionTiles);

    Debug.Log("Tile defined. " + newRegion.RegionTiles[i].MyTilePosType + " " + newRegion.RegionTiles[i].MyTileEdgeType);

                            // Set Pixels
                            tiles = SplitTileSheet(newLandType);

    thisTilePixels = tiles[FindIndex(newRegion.RegionTiles[i].MyTilePosType,
        newRegion.RegionTiles[i].MyTileEdgeType, newRegion.RegionTiles[i].MyTileLandType)];

                            texture2.SetPixels(x* tileResolution, y * tileResolution, tileResolution, tileResolution, thisTilePixels);
                        }

                  

                    }
                    else
                    {
                        //clear tile
                        
                        // remove this as a potential position
                        potentialPositions.RemoveAt(posIndex);
                        Debug.Log("Potential positions = " + potentialPositions.Count);
                        // Set clear pixels
                        thisTilePixels = clearPixels;
                        texture2.SetPixels(x* tileResolution, y * tileResolution, tileResolution, tileResolution, thisTilePixels);
                    }
                // Define the Graphic Tiles in this region as Clear and Undefined first
                for (int t = 0; t<r.RegionTiles.Length; t++)
                {
                    r.RegionTiles[t] = new GraphicTile(GraphicTile.TilePositionTypes.CLEAR, GraphicTile.TileEdgeTypes.UNDEFINED, baseLandType, r.TilePositions[t]);
                }

                // Loop through the tilePositions array in this region
               // for (int x = 0; x < r.TilePositions.Length; x++)
               // {

                    r.RegionTiles[x] = DefineRegionTile(GetNewLandType(baseLandType), r.TilePositions, r.TilePositions[x], r.RegionTiles);
               // }

    */
   
     void DefineLandRegionTiles(int width, int height, LandRegion[] regions, Texture2D secondTexture)
    {
        GraphicTile.TileLandTypes currLandType = baseLandType;

        Color[][] tiles = SplitTileSheet(currLandType);
        // Use this array to Set Pixels
        Color[] thisTilePixels;

        foreach (LandRegion r in regions)
        {
            // Choose new land type for this region
            GraphicTile.TileLandTypes newLandType = GetNewLandType(baseLandType);

            // Loop through the Region's tiles
            for (int t = 0; t < r.RegionTiles.Length; t++)
            {
                // Decide wether it's land or clear
                //int decision = Random.Range(0, 101);
               // if (1 < 2)
               // {
                    // This is land

                    r.RegionTiles[t] = DefineRegionTile(newLandType, r.TilePositions, r.TilePositions[t], r.RegionTiles);

                    if (newLandType != currLandType)
                    {
                        currLandType = newLandType;
                        tiles = SplitTileSheet(currLandType);

                    }
                    
                    // Set Pixels
                    
                    thisTilePixels = tiles[FindIndex(r.RegionTiles[t].MyTilePosType,
                        r.RegionTiles[t].MyTileEdgeType, r.RegionTiles[t].MyTileLandType)];

                    // Get tile pos as int
                    int x = (int)r.RegionTiles[t].MyPos.x;
                    int y = (int)r.RegionTiles[t].MyPos.y;

                    secondTexture.SetPixels(x * tileResolution, y * tileResolution, tileResolution, tileResolution, thisTilePixels);
               // }
                //else
                //{
                //    // This is a clear tile
                //    r.RegionTiles[t] = new GraphicTile(GraphicTile.TilePositionTypes.CLEAR, r.TilePositions[t]);

                //    // Set clear pixels
                //    thisTilePixels = clearPixels;

                //    // Get tile pos as int
                //    int x = (int)r.RegionTiles[t].MyPos.x;
                //    int y = (int)r.RegionTiles[t].MyPos.y;

                //    secondTexture.SetPixels(x * tileResolution, y * tileResolution, tileResolution, tileResolution, thisTilePixels);
                //}

            }




        }


        // int random = Random.Range(0, regions.Length);
        // int randomTile = Random.Range(0, regions[random].RegionTiles.Length);
        // Debug.Log("Land Region " + random + "'s " + random + "th Tile is defined as " + regions[random].RegionTiles[randomTile].MyTilePosType +
        // " of Edge type " + regions[random].RegionTiles[randomTile].MyTileEdgeType);


        // Once ALL regions are created:
        // Give all their data and respective tile types to a texture.
        //BuildSecondaryTexture(width, height, regions);

        secondTexture.filterMode = FilterMode.Trilinear;

        Texture2 = secondTexture;
        ApplyTextures();
    }
       


    GraphicTile.TileLandTypes GetNewLandType (GraphicTile.TileLandTypes currLandType)
    {
        //make it so there's a low chance of it selecting the same as the Base land type
        int randomLandType = Random.Range(0, 20);
        

        if (randomLandType <= 1)
        {
            return baseLandType;

        }
        else
        {
            

            if (baseLandType == GraphicTile.TileLandTypes.ASH)
            {
                int random = Random.Range(0, 2);
                if (random == 0)
                {
                    return GraphicTile.TileLandTypes.MUD;
                }
                else
                {
                    return GraphicTile.TileLandTypes.SAND;
                }

            }
            else if (baseLandType == GraphicTile.TileLandTypes.MUD)
            {
                int random = Random.Range(0, 2);
                if (random == 0)
                {
                    return GraphicTile.TileLandTypes.ASH;
                }
                else
                {
                    return GraphicTile.TileLandTypes.SAND;
                }
            }
            else 
            {
                int random = Random.Range(0, 2);
                if (random == 0)
                {
                    return GraphicTile.TileLandTypes.ASH;
                }
                else
                {
                    return GraphicTile.TileLandTypes.MUD;
                }
            }

        }

     //   return thisLandType;
    }

    GraphicTile DefineRegionTile(GraphicTile.TileLandTypes myLandType, Vector2[] positions,Vector2 position, GraphicTile[] tiles)
    {
        // Make them all clear tiles at first
        //GraphicTile definedTile = new GraphicTile();

      //  return CheckTileNeighbors(positions, position, myLandType, tiles);

        // First we need to know if this is the First region tile to be defined
        if (position == positions[0])
        {
            // First tile is ALWAYS the bottom left corner. But we still need to find out its Edge Type.
            // Define the tile as a Bottom Left Corner:
            // top && right && not left && not bottom
            return GetTilePositionType(false, true, true, false, false, false, false, false, (int)position.x, (int)position.y, myLandType);
        }
        else if (position == positions[positions.Length - 1])
        {
            // If this is the LAST tile it will ALWAYS be a top right corner
            // !_top && !_right && _left && _bottom && !_topRight
            return GetTilePositionType(_left: true,_right: false, _top: false, _bottom: true,_topLeft: false, _bottLeft: false, _topRight: false,_bottRight: false, posX: (int)position.x, posY: (int)position.y, myLandType: myLandType);

        }
        else
        {
            return CheckTileNeighbors(positions, position, myLandType, tiles);
        }

        //return definedTile;



        /*
        int decideIfClear = Random.Range(0, 6);
        if (decideIfClear == 0)
        {
           definedTile = new GraphicTile(GraphicTile.TilePositionTypes.CLEAR, position);
        }
        else
        {

            // Initialize this new Graphic Tile
            definedTile = CheckTileNeighbors(positions, position, myLandType, tiles);

            // THIS NOW considers clear tiles to be Non-existant and will create edges accordingly

        }
        */
       

        //return definedTile;
   
    }

    //void BuildSecondaryTexture(int width, int height, LandRegion[] Regions)
    //{
    //    // Make sure this Mesh object shares the same mesh as Base mesh
    //    Mesh islandMesh = base_renderer.gameObject.GetComponent<MeshFilter>().mesh;
    //    second_renderer.gameObject.GetComponent<MeshFilter>().mesh = islandMesh;
        
        
    //    int textureWidth = width * tileResolution;
    //    int textureHeight = height * tileResolution;
    //    Texture2D texture2 = new Texture2D(textureWidth, textureHeight);
    //    texture2.name = "Secondary Land Texture";

    //    // Turn land regions into an array for faster searching
    //    //LandRegion[] Regions = landRegions.ToArray();

    //    GraphicTile.TileLandTypes currLandType = Regions[0].RegionTiles[0].MyTileLandType;

        
    //    Color[][] tiles = SplitTileSheet(currLandType);
    //    // Use this array to Set Pixels
    //    Color[] thisTilePixels;

    //    Debug.Log("Regions total = " + Regions.Length);

    //    //NOTE: Try setting the pixels for each region first, since we know their locations

    //    for (int i = 0; i < Regions.Length; i++)
    //    {
    //        for (int t = 0; t < Regions[i].RegionTiles.Length; t++)
    //        {
    //            GraphicTile thisTile = Regions[i].RegionTiles[t];
    //            Debug.Log("This tile " + Regions[i].RegionTiles[t]);
    //            if (thisTile.MyTilePosType != GraphicTile.TilePositionTypes.CLEAR)
    //            {
    //                if (thisTile.MyTileLandType != currLandType)
    //                {
    //                    currLandType = thisTile.MyTileLandType;
    //                    tiles = SplitTileSheet(currLandType);
    //                }

    //                thisTilePixels = tiles[FindIndex(thisTile.MyTilePosType, thisTile.MyTileEdgeType, thisTile.MyTileLandType)];
    //                texture2.SetPixels((int)thisTile.MyPos.x * tileResolution, (int)thisTile.MyPos.y * tileResolution, tileResolution, tileResolution, thisTilePixels);
    //            }
    //            else
    //            {
    //                // set pixels to clear
    //                thisTilePixels = clearPixels;
    //                texture2.SetPixels((int)thisTile.MyPos.x * tileResolution, (int)thisTile.MyPos.y * tileResolution, tileResolution, tileResolution, thisTilePixels);
    //            }

    //        }
    //    }
 
    //    texture2.filterMode = FilterMode.Trilinear;

    //    Texture2 = texture2;
    //    /*
    //    // Apply texture
    //    texture2.Apply();

    //    second_renderer.sharedMaterial.mainTexture = texture2;
    //    */
    //    ApplyTextures();
    //}

    int CheckIfGraphicTileMatches (Vector2[] tilePositions, Vector2 posToCheck)
    {
        int tile = 5000;

        for (int i = 0; i < tilePositions.Length; i++)
        {
            if (tilePositions[i] == posToCheck)
            {
                return tile = i;
            }
        }
        return tile;
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

    int GetIndexOfExistingTile(Vector2[] positions, Vector2 positionToCheck)
    {
        for(int i = 0; i < positions.Length; i++)
        {
            if (positions[i] == positionToCheck)
            {
                return i;
            }
        }

        return 999999;
    }



    // * Tile Data * :


    GraphicTile CheckTileNeighbors(Vector2[] emptyTileArray, Vector2 emptyTilePos, GraphicTile.TileLandTypes myLandType, bool isSecondTex = false)
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

        return GetTilePositionType(left, right, top, bottom, leftTop, leftBottom, rightTop, rightBottom, (int)emptyTilePos.x, (int)emptyTilePos.y, myLandType, isSecondTex);
    }

    GraphicTile CheckTileNeighbors(Vector2[] emptyTileArray, Vector2 emptyTilePos, GraphicTile.TileLandTypes myLandType, GraphicTile[] tiles)
    {
        bool left = false, right = false, bottom = false, top = false;
        bool leftTop = false, leftBottom = false, rightTop = false, rightBottom = false;

        
        Vector2 leftTile = new Vector2(emptyTilePos.x - 1, emptyTilePos.y);
        int leftTileExists = CheckIfGraphicTileMatches(emptyTileArray, leftTile);
        if (leftTileExists < tiles.Length)
        {
            // It does exist, make sure it's NOT clear
            if (tiles[leftTileExists].MyTilePosType != GraphicTile.TilePositionTypes.CLEAR)
                 left = true;
        }

        // Check if a tile exists to my right
        Vector2 rightTile = new Vector2(emptyTilePos.x + 1, emptyTilePos.y);
        int rightTileExists = CheckIfGraphicTileMatches(emptyTileArray, rightTile);
        if (rightTileExists < tiles.Length)
        {
            if (tiles[rightTileExists].MyTilePosType != GraphicTile.TilePositionTypes.CLEAR)
                right = true;
        }

        // Check if a tile exists on top
        Vector2 topTile = new Vector2(emptyTilePos.x, emptyTilePos.y + 1);
        int topTileExists = CheckIfGraphicTileMatches(emptyTileArray, topTile);
        if (topTileExists < tiles.Length)
        {
            if (tiles[topTileExists].MyTilePosType != GraphicTile.TilePositionTypes.CLEAR)
                top = true;
        }

        // Check if a tile exists on bottom
        Vector2 bottomTile = new Vector2(emptyTilePos.x, emptyTilePos.y - 1);
        int bottomTileExists = CheckIfGraphicTileMatches(emptyTileArray, bottomTile);

        if (bottomTileExists < tiles.Length)
        {
            if (tiles[bottomTileExists].MyTilePosType != GraphicTile.TilePositionTypes.CLEAR)
                bottom = true;
        }

        // Check if a tile exists on bottom left
        Vector2 bottomLeftTile = new Vector2(emptyTilePos.x - 1, emptyTilePos.y - 1);
        int bottomLeftTileExists = CheckIfGraphicTileMatches(emptyTileArray, bottomLeftTile);
        if (bottomLeftTileExists < tiles.Length)
        {
            if (tiles[bottomLeftTileExists].MyTilePosType != GraphicTile.TilePositionTypes.CLEAR)
                leftBottom = true;
        }

        // Check if a tile exists on top left
        Vector2 topLeftTile = new Vector2(emptyTilePos.x - 1, emptyTilePos.y + 1);
        int topLeftTileExists = CheckIfGraphicTileMatches(emptyTileArray, topLeftTile);
        if (topLeftTileExists < tiles.Length)
        {
            if (tiles[topLeftTileExists].MyTilePosType != GraphicTile.TilePositionTypes.CLEAR)
                leftTop = true;
        }

        // Check if a tile exists on top right
        Vector2 topRightTile = new Vector2(emptyTilePos.x + 1, emptyTilePos.y + 1);
        int topRightTileExists = CheckIfGraphicTileMatches(emptyTileArray, topRightTile);
        if (topRightTileExists < tiles.Length)
        {
            if (tiles[topRightTileExists].MyTilePosType != GraphicTile.TilePositionTypes.CLEAR)
                rightTop = true;
        }

        // Check if a tile exists on bottom right
        Vector2 bottomRightTile = new Vector2(emptyTilePos.x + 1, emptyTilePos.y - 1);
        int bottomRightTileExists = CheckIfGraphicTileMatches(emptyTileArray, bottomRightTile);
        if (bottomRightTileExists < tiles.Length)
        {
            if (tiles[bottomRightTileExists].MyTilePosType != GraphicTile.TilePositionTypes.CLEAR)
                rightBottom = true;
        }

        return GetTilePositionType(left, right, top, bottom, leftTop, leftBottom, rightTop, rightBottom, (int)emptyTilePos.x, (int)emptyTilePos.y, myLandType);
    }

    GraphicTile GetTilePositionType(bool _left, bool _right, bool _top, bool _bottom, bool _topLeft = false, bool _bottLeft = false, bool _topRight = false, bool _bottRight = false, int posX = 0, int posY = 0, GraphicTile.TileLandTypes myLandType = GraphicTile.TileLandTypes.ASH, bool isSecondTexture = false)
    {
        // Positions to check
        int right = posX + 1;
        int left = posX - 1;
        int top = posY + 1;
        int bottom = posY - 1;

        GraphicTile.TileEdgeTypes myTileEdgeType;
        GraphicTile.TileEdgeTypes neighborTop = (myLandType == GraphicTile.TileLandTypes.ASH) ? GetNeighborEdgeTypes(posX, top, isSecondTexture) : GraphicTile.TileEdgeTypes.SHORE;
        GraphicTile.TileEdgeTypes neighborRight = (myLandType == GraphicTile.TileLandTypes.ASH) ? GetNeighborEdgeTypes(right, posY, isSecondTexture) : GraphicTile.TileEdgeTypes.SHORE;
        GraphicTile.TileEdgeTypes neighborLeft = (myLandType == GraphicTile.TileLandTypes.ASH) ? GetNeighborEdgeTypes(left, posY, isSecondTexture) : GraphicTile.TileEdgeTypes.SHORE;
        GraphicTile.TileEdgeTypes neighborBottom = (myLandType == GraphicTile.TileLandTypes.ASH) ? GetNeighborEdgeTypes(posX, bottom, isSecondTexture) : GraphicTile.TileEdgeTypes.SHORE;

        // Tile to return
        GraphicTile newTile = new GraphicTile();
        

        // Check diagonal tiles first:
        if (_top && _right && _left && _bottom && !_bottLeft)
        {
            // Left bottom diagonal
            if (myLandType == GraphicTile.TileLandTypes.ASH)
            {
                // Neighbor 1 = left, Neighbor 2 = bottom
                myTileEdgeType = DefineEdgeType(neighborLeft, neighborBottom);

                // Define the tile to return
                newTile = new GraphicTile(GraphicTile.TilePositionTypes.LEFT_BOTTOM_DIAG, myTileEdgeType, GraphicTile.TileLandTypes.ASH, new Vector2(posX, posY));
            }
            else
            {
                // Tiles that are NOT edge are all SHORE types
                newTile = new GraphicTile(GraphicTile.TilePositionTypes.LEFT_BOTTOM_DIAG, GraphicTile.TileEdgeTypes.SHORE, myLandType, new Vector2(posX, posY));
            }
         
        }
        else if (_top && _left && _bottom && _right && !_bottRight)
        {
            // Right bottom diagonal
            if (myLandType == GraphicTile.TileLandTypes.ASH)
            {
                // Neighbor 1 = bottom, Neighbor 2 = right
                myTileEdgeType = DefineEdgeType(neighborBottom, neighborRight);

                newTile = new GraphicTile(GraphicTile.TilePositionTypes.RIGHT_BOTTOM_DIAG, myTileEdgeType, GraphicTile.TileLandTypes.ASH, new Vector2(posX, posY));
            }
            else
            {
                newTile = new GraphicTile(GraphicTile.TilePositionTypes.RIGHT_BOTTOM_DIAG, GraphicTile.TileEdgeTypes.SHORE, myLandType, new Vector2(posX, posY));
            }
               

        }
        else if (_top && _left && _bottom && _right && !_topRight)
        {
            // Right top diagonal
            if (myLandType == GraphicTile.TileLandTypes.ASH)
            {
                // Neighbor 1 = bottom, Neighbor 2 = right
                myTileEdgeType = DefineEdgeType(neighborTop, neighborRight);

                newTile = new GraphicTile(GraphicTile.TilePositionTypes.RIGHT_TOP_DIAG, myTileEdgeType, GraphicTile.TileLandTypes.ASH, new Vector2(posX, posY));
            }
            else
            {
                newTile = new GraphicTile(GraphicTile.TilePositionTypes.RIGHT_TOP_DIAG, GraphicTile.TileEdgeTypes.SHORE, myLandType, new Vector2(posX, posY));
            }


        }
        else if (_top && _left && _bottom && _right && !_topLeft)
        {
            // Left top diagonal
            if (myLandType == GraphicTile.TileLandTypes.ASH)
            {
                // Neighbor 1 = bottom, Neighbor 2 = right
                myTileEdgeType = DefineEdgeType(neighborLeft, neighborTop);

                newTile = new GraphicTile(GraphicTile.TilePositionTypes.LEFT_TOP_DIAG, myTileEdgeType, GraphicTile.TileLandTypes.ASH, new Vector2(posX, posY));
            }
            else
            {
                newTile = new GraphicTile(GraphicTile.TilePositionTypes.LEFT_TOP_DIAG, GraphicTile.TileEdgeTypes.SHORE, myLandType, new Vector2(posX, posY));
            }


        }
        else if (_top && _right && !_left && !_bottom)
        {
            // bottom left corner
            if (myLandType == GraphicTile.TileLandTypes.ASH)
            {

                // Neighbor 1 = top, Neighbor 2 = right
                myTileEdgeType = DefineEdgeType(neighborTop, neighborRight, isCorner: true);

                newTile = new GraphicTile(GraphicTile.TilePositionTypes.BOTTOM_LEFT_CORNER, myTileEdgeType, GraphicTile.TileLandTypes.ASH, new Vector2(posX, posY));
            }
            else
            {
                newTile = new GraphicTile(GraphicTile.TilePositionTypes.BOTTOM_LEFT_CORNER, GraphicTile.TileEdgeTypes.SHORE, myLandType, new Vector2(posX, posY));
            }

        }
        else if (_top && _left && _right && !_bottom)
        {
            // Bottom
            if (myLandType == GraphicTile.TileLandTypes.ASH)
            {

                // Neighbor 1 = left, Neighbor 2 = right
                myTileEdgeType = DefineEdgeType(neighborLeft, neighborRight);

                newTile = new GraphicTile(GraphicTile.TilePositionTypes.BOTTOM, myTileEdgeType, GraphicTile.TileLandTypes.ASH, new Vector2(posX, posY));
            }
            else
            {
                newTile = new GraphicTile(GraphicTile.TilePositionTypes.BOTTOM, GraphicTile.TileEdgeTypes.SHORE, myLandType, new Vector2(posX, posY));
            }


        }
        else if (_top && _left && !_right && !_bottom)
        {
            // Bottom right corner
            if (myLandType == GraphicTile.TileLandTypes.ASH)
            {
                // Neighbor 1 = left, Neighbor 2 = top
                myTileEdgeType = DefineEdgeType(neighborLeft, neighborTop, isCorner: true);

                newTile = new GraphicTile(GraphicTile.TilePositionTypes.BOTTOM_RIGHT_CORNER, myTileEdgeType, GraphicTile.TileLandTypes.ASH, new Vector2(posX, posY));
            }
            else
            {
                newTile = new GraphicTile(GraphicTile.TilePositionTypes.BOTTOM_RIGHT_CORNER, GraphicTile.TileEdgeTypes.SHORE, myLandType, new Vector2(posX, posY));
            }

        }
        else if (_top && _left && _bottom && !_right)
        {
            // Right
            if (myLandType == GraphicTile.TileLandTypes.ASH)
            {

                // Neighbor 1 = bottom, Neighbor 2 = top
                myTileEdgeType = DefineEdgeType(neighborBottom, neighborTop);

                newTile = new GraphicTile(GraphicTile.TilePositionTypes.RIGHT, myTileEdgeType, GraphicTile.TileLandTypes.ASH, new Vector2(posX, posY));
            }
            else
            {
                newTile = new GraphicTile(GraphicTile.TilePositionTypes.RIGHT, GraphicTile.TileEdgeTypes.SHORE, myLandType, new Vector2(posX, posY));
            }

        }
        else if (_top && _right && _bottom && !_left)
        {
            // left
            if (myLandType == GraphicTile.TileLandTypes.ASH)
            {// Neighbor 1 = bottom, Neighbor 2 = top
                myTileEdgeType = DefineEdgeType(neighborBottom, neighborTop);

                newTile = new GraphicTile(GraphicTile.TilePositionTypes.LEFT, myTileEdgeType, GraphicTile.TileLandTypes.ASH, new Vector2(posX, posY));
            }
            else
            {
                newTile = new GraphicTile(GraphicTile.TilePositionTypes.LEFT, GraphicTile.TileEdgeTypes.SHORE, myLandType, new Vector2(posX, posY));
            }


        }
        else if (!_top && _right && _left && _bottom)
        {
            // top shore
            if (myLandType == GraphicTile.TileLandTypes.ASH)
            {
                // Neighbor 1 = left, Neighbor 2 = right
                myTileEdgeType = DefineEdgeType(neighborLeft, neighborRight);
                //myTileType = GetMyTileTypeFromNeighbors(neighborLeft, neighborRight);

                newTile = new GraphicTile(GraphicTile.TilePositionTypes.TOP, myTileEdgeType, GraphicTile.TileLandTypes.ASH, new Vector2(posX, posY));
            }
            else
            {
                newTile = new GraphicTile(GraphicTile.TilePositionTypes.TOP, GraphicTile.TileEdgeTypes.SHORE, myLandType, new Vector2(posX, posY));
            }

        }
        else if (!_top && !_right && _left && _bottom && !_topRight)
        {
            // top right corner
            if (myLandType == GraphicTile.TileLandTypes.ASH)
            {

                // Neighbor 1 = left, Neighbor 2 = bottom
                myTileEdgeType = DefineEdgeType(neighborLeft, neighborBottom, isCorner: true);

                newTile = new GraphicTile(GraphicTile.TilePositionTypes.TOP_RIGHT_CORNER, myTileEdgeType, GraphicTile.TileLandTypes.ASH, new Vector2(posX, posY));
            }
            else
            {
                newTile = new GraphicTile(GraphicTile.TilePositionTypes.TOP_RIGHT_CORNER, GraphicTile.TileEdgeTypes.SHORE, myLandType, new Vector2(posX, posY));
            }


        }

        else if (!_top && _right && !_left && _bottom && !_topLeft)
        {
            // top left corner
            if (myLandType == GraphicTile.TileLandTypes.ASH)
            {

                // Neighbor 1 = bottom, Neighbor 2 = right
                myTileEdgeType = DefineEdgeType(neighborBottom, neighborRight, isCorner: true);

                newTile = new GraphicTile(GraphicTile.TilePositionTypes.TOP_LEFT_CORNER, myTileEdgeType, GraphicTile.TileLandTypes.ASH, new Vector2(posX, posY));
            }
            else
            {
                newTile = new GraphicTile(GraphicTile.TilePositionTypes.TOP_LEFT_CORNER, GraphicTile.TileEdgeTypes.SHORE, myLandType, new Vector2(posX, posY));
            }
        }
        //else if (_top && !_left && !)
        else
        {
            if (myLandType == GraphicTile.TileLandTypes.ASH)
            {
                // if top , left , right , !bottom place a bottom left corner
                if (_top &&  !_left && !_right && !_bottom)
                {
                    myTileEdgeType = DefineTypeFromUndefinedNeighbors(false);
                    newTile = new GraphicTile(GraphicTile.TilePositionTypes.BOTTOM, myTileEdgeType, GraphicTile.TileLandTypes.ASH, new Vector2(posX, posY));
                }
                // else if !top, left, right, bottom place a top left corner
                else if (!_top && !_left && !_right && _bottom)
                {
                    myTileEdgeType = DefineTypeFromUndefinedNeighbors(true);
                    newTile = new GraphicTile(GraphicTile.TilePositionTypes.TOP_LEFT_CORNER, myTileEdgeType, GraphicTile.TileLandTypes.ASH, new Vector2(posX, posY));
                }
                else if (_top && !_left && !_right && !_bottom && _topLeft)
                {
                    myTileEdgeType = DefineTypeFromUndefinedNeighbors(true);
                    newTile = new GraphicTile(GraphicTile.TilePositionTypes.BOTTOM_LEFT_CORNER, myTileEdgeType, GraphicTile.TileLandTypes.ASH, new Vector2(posX, posY));
                }
 
                else if (!_top && !_left && !_right && _bottom && _bottRight)
                {
                    myTileEdgeType = DefineTypeFromUndefinedNeighbors(true);
                    newTile = new GraphicTile(GraphicTile.TilePositionTypes.TOP_RIGHT_CORNER, myTileEdgeType, GraphicTile.TileLandTypes.ASH, new Vector2(posX, posY));

                }
                else if (!_top && _left && !_right && !_bottom && _bottLeft)
                {
                    myTileEdgeType = DefineTypeFromUndefinedNeighbors(true);
                    newTile = new GraphicTile(GraphicTile.TilePositionTypes.BOTTOM_RIGHT_CORNER, myTileEdgeType, GraphicTile.TileLandTypes.ASH, new Vector2(posX, posY));
                }
                else if (!_top && !_left && _right && !_bottom && _bottRight)
                {
                    myTileEdgeType = DefineTypeFromUndefinedNeighbors(true);
                    newTile = new GraphicTile(GraphicTile.TilePositionTypes.TOP_LEFT_CORNER, myTileEdgeType, GraphicTile.TileLandTypes.ASH, new Vector2(posX, posY));
                }
                else if (!_top && !_left && _right && _bottom)
                {
                    myTileEdgeType = DefineTypeFromUndefinedNeighbors(true);
                    newTile = new GraphicTile(GraphicTile.TilePositionTypes.LEFT_TOP_DIAG, myTileEdgeType, GraphicTile.TileLandTypes.ASH, new Vector2(posX, posY));
                }
                // else it's a center
                else
                {
                    newTile = new GraphicTile(GraphicTile.TilePositionTypes.CENTER, GraphicTile.TileEdgeTypes.CENTER, GraphicTile.TileLandTypes.ASH, new Vector2(posX, posY));
                }
            }
            else
            {
                // if top , left , right , !bottom place a bottom left corner
                if (_top && _left && _right && !_bottom)
                {
                    myTileEdgeType = DefineEdgeType(neighborLeft, neighborRight, isCorner: true);
                    newTile = new GraphicTile(GraphicTile.TilePositionTypes.BOTTOM_LEFT_CORNER, myTileEdgeType, myLandType, new Vector2(posX, posY));
                }
                // else if !top, left, right, bottom place a top left corner
                else if (!_top && _left && _right && _bottom)
                {
                    myTileEdgeType = DefineEdgeType(neighborLeft, neighborRight, isCorner: true);
                    newTile = new GraphicTile(GraphicTile.TilePositionTypes.TOP_LEFT_CORNER, myTileEdgeType, myLandType, new Vector2(posX, posY));
                }
                else if(_top && !_left && !_right && !_bottom && _topLeft)
                {
                    newTile = new GraphicTile(GraphicTile.TilePositionTypes.BOTTOM_LEFT_CORNER, GraphicTile.TileEdgeTypes.SHORE, myLandType, new Vector2(posX, posY));
                }
                else if (!_top && !_left && !_right && _bottom && _bottRight)
                {
                    newTile = new GraphicTile(GraphicTile.TilePositionTypes.TOP_RIGHT_CORNER, GraphicTile.TileEdgeTypes.SHORE, myLandType, new Vector2(posX, posY));

                }
                else if (!_top && _left && !_right && !_bottom && _bottLeft)
                {
                    newTile = new GraphicTile(GraphicTile.TilePositionTypes.BOTTOM_RIGHT_CORNER, GraphicTile.TileEdgeTypes.SHORE, myLandType, new Vector2(posX, posY));
                }
                else if (!_top && !_left && _right && !_bottom && _bottRight)
                {
                    newTile = new GraphicTile(GraphicTile.TilePositionTypes.TOP_LEFT_CORNER, GraphicTile.TileEdgeTypes.SHORE, myLandType, new Vector2(posX, posY));
                }
                else if (!_top && !_left && _right && _bottom && _bottLeft)
                {
                    newTile = new GraphicTile(GraphicTile.TilePositionTypes.LEFT_TOP_DIAG, GraphicTile.TileEdgeTypes.SHORE, myLandType, new Vector2(posX, posY));
                }
                // else it's a center
                else
                {
                    newTile = new GraphicTile(GraphicTile.TilePositionTypes.CENTER, GraphicTile.TileEdgeTypes.CENTER, myLandType, new Vector2(posX, posY));
                }
            }
        }

        return newTile;
    }

    GraphicTile.TileEdgeTypes GetNeighborEdgeTypes(int x, int y, bool isSecondTexture = false)
    {
        if (!isSecondTexture)
        {
            if (x < baseGraphicTiles.GetLength(0) && y < baseGraphicTiles.GetLength(1))
            {
                if (baseGraphicTiles[x, y] != null)
                {
                    return baseGraphicTiles[x, y].MyTileEdgeType;
                }
                else
                {
                    return GraphicTile.TileEdgeTypes.UNDEFINED;
                }
            }
            else
            {
                return GraphicTile.TileEdgeTypes.UNDEFINED;
            }

        }
        else
        {
            if (x < secondGraphicTiles.GetLength(0) && y < secondGraphicTiles.GetLength(1))
            {
                if (secondGraphicTiles[x, y] != null)
                {
                    return secondGraphicTiles[x, y].MyTileEdgeType;
                }
                else
                {
                    return GraphicTile.TileEdgeTypes.UNDEFINED;
                }
            }
            else
            {
                return GraphicTile.TileEdgeTypes.UNDEFINED;
            }

        }
        
        
    }

    GraphicTile.TileEdgeTypes DefineEdgeType(GraphicTile.TileEdgeTypes neighborOne, GraphicTile.TileEdgeTypes neighborTwo, bool isCorner = false)
    {
        GraphicTile.TileEdgeTypes myTileEdgeType = GraphicTile.TileEdgeTypes.UNDEFINED;
       
        if (neighborOne != GraphicTile.TileEdgeTypes.UNDEFINED || neighborTwo != GraphicTile.TileEdgeTypes.UNDEFINED)
        {
            // Both are DEFINED, so select based on the first and second neighbor
            myTileEdgeType = GetMyTileTypeFromNeighbors(neighborOne, neighborTwo, isCorner);
            

        }
        else
        {

            myTileEdgeType = DefineTypeFromUndefinedNeighbors(isCorner);

        }


        return myTileEdgeType;
    }


    GraphicTile.TileEdgeTypes DefineTypeFromUndefinedNeighbors(bool isCorner = false)
    {
        GraphicTile.TileEdgeTypes thisTileType = GraphicTile.TileEdgeTypes.UNDEFINED;
        int select = Random.Range(0, 6); // Cliff or Shore?
        if (select <= 2)
        {
            thisTileType = GraphicTile.TileEdgeTypes.CLIFF;
        }
        else if (select == 3)
        {
            thisTileType = GraphicTile.TileEdgeTypes.SHORE_CLIFF;
        }
        else if (select == 4)
        {
            thisTileType = GraphicTile.TileEdgeTypes.CLIFF_SHORE;
        }
        else
        {
            thisTileType = GraphicTile.TileEdgeTypes.SHORE;
        }
        return thisTileType;

    }

    GraphicTile.TileEdgeTypes GetMyTileTypeFromNeighbors(GraphicTile.TileEdgeTypes neighbor1Type, GraphicTile.TileEdgeTypes neighbor2Type, bool isCorner = false)
    {
        GraphicTile.TileEdgeTypes myTileType = GraphicTile.TileEdgeTypes.UNDEFINED;

        switch (neighbor1Type)
        {
            case GraphicTile.TileEdgeTypes.SHORE:
                if (neighbor2Type == GraphicTile.TileEdgeTypes.SHORE)
                {
                    myTileType = GraphicTile.TileEdgeTypes.SHORE;
                }
                else if (neighbor2Type == GraphicTile.TileEdgeTypes.CLIFF && !isCorner)
                {
                    myTileType = GraphicTile.TileEdgeTypes.SHORE_CLIFF;
                }
                else if (neighbor2Type == GraphicTile.TileEdgeTypes.SHORE_CLIFF)
                {
                    myTileType = GraphicTile.TileEdgeTypes.SHORE;
                }
                else if (neighbor2Type == GraphicTile.TileEdgeTypes.CLIFF_SHORE && !isCorner)
                {
                    myTileType = GraphicTile.TileEdgeTypes.SHORE_CLIFF;
                }
                else if (neighbor2Type == GraphicTile.TileEdgeTypes.UNDEFINED && !isCorner)
                {
                    myTileType = GraphicTile.TileEdgeTypes.SHORE_CLIFF;
                }
                else
                {
                    myTileType = GraphicTile.TileEdgeTypes.SHORE;
                }

                break;
            case GraphicTile.TileEdgeTypes.CLIFF:
                if (neighbor2Type == GraphicTile.TileEdgeTypes.SHORE || neighbor2Type == GraphicTile.TileEdgeTypes.SHORE_CLIFF && !isCorner)
                {
                    myTileType = GraphicTile.TileEdgeTypes.CLIFF_SHORE;
                }
                else if (neighbor2Type == GraphicTile.TileEdgeTypes.CLIFF || neighbor2Type == GraphicTile.TileEdgeTypes.CLIFF_SHORE)
                {
                    myTileType = GraphicTile.TileEdgeTypes.CLIFF;
                }
                else if (neighbor2Type == GraphicTile.TileEdgeTypes.UNDEFINED && !isCorner)
                {
                    myTileType = GraphicTile.TileEdgeTypes.CLIFF_SHORE;
                }
                else
                {
                    myTileType = GraphicTile.TileEdgeTypes.CLIFF;
                }

                break;
            case GraphicTile.TileEdgeTypes.SHORE_CLIFF:
                if (neighbor2Type == GraphicTile.TileEdgeTypes.SHORE || neighbor2Type == GraphicTile.TileEdgeTypes.SHORE_CLIFF && !isCorner)
                {
                    myTileType = GraphicTile.TileEdgeTypes.CLIFF_SHORE;
                }
                else if (neighbor2Type == GraphicTile.TileEdgeTypes.CLIFF || neighbor2Type == GraphicTile.TileEdgeTypes.CLIFF_SHORE)
                {
                    myTileType = GraphicTile.TileEdgeTypes.CLIFF;
                }
                else
                {
                    myTileType = GraphicTile.TileEdgeTypes.CLIFF;
                }

                break;
            case GraphicTile.TileEdgeTypes.CLIFF_SHORE:
                if (neighbor2Type == GraphicTile.TileEdgeTypes.SHORE || neighbor2Type == GraphicTile.TileEdgeTypes.SHORE_CLIFF)
                {
                    myTileType = GraphicTile.TileEdgeTypes.SHORE;
                }
                else if (neighbor2Type == GraphicTile.TileEdgeTypes.CLIFF || neighbor2Type == GraphicTile.TileEdgeTypes.CLIFF_SHORE && !isCorner)
                {
                    myTileType = GraphicTile.TileEdgeTypes.SHORE_CLIFF;
                }
                else
                {
                    myTileType = GraphicTile.TileEdgeTypes.SHORE;
                }

                break;
            case GraphicTile.TileEdgeTypes.UNDEFINED:
                if (neighbor2Type == GraphicTile.TileEdgeTypes.SHORE || neighbor2Type == GraphicTile.TileEdgeTypes.SHORE_CLIFF || neighbor2Type == GraphicTile.TileEdgeTypes.UNDEFINED)
                {
                    myTileType = GraphicTile.TileEdgeTypes.SHORE;
                }
                else if (neighbor2Type == GraphicTile.TileEdgeTypes.CLIFF || neighbor2Type == GraphicTile.TileEdgeTypes.CLIFF_SHORE && !isCorner)
                {
                    myTileType = GraphicTile.TileEdgeTypes.SHORE_CLIFF;
                }
                else
                {
                    myTileType = GraphicTile.TileEdgeTypes.SHORE;
                }
                break;

            default:
                myTileType = GraphicTile.TileEdgeTypes.SHORE;
                break;
        }


        return myTileType;
    }


    int FindIndex(GraphicTile.TilePositionTypes myPosType, GraphicTile.TileEdgeTypes myEdgeType, GraphicTile.TileLandTypes myLandType)
    {
        int tileIndex = 0;
        
        if (myLandType == GraphicTile.TileLandTypes.ASH)
        {
            // Get the index using the Ash Tile Sheet 
            tileIndex = GetAshSheetIndex(myPosType, myEdgeType);
        }
        else
        {
            // Get the index using the Mud Tile Sheet 
            tileIndex = GetIndexFromSandOrMudSheet(myPosType, myLandType);

        }

        return tileIndex;
    }

    int GetAshSheetIndex(GraphicTile.TilePositionTypes myPosType, GraphicTile.TileEdgeTypes myEdgeType)
    {
        int tileIndex = 0;

        switch (myEdgeType)
        {
            case GraphicTile.TileEdgeTypes.CLIFF:

                // Select a cliff of this edgetype
                if (myPosType == GraphicTile.TilePositionTypes.BOTTOM)
                {
                    tileIndex = pseudoRandom.Next(90, 100);
                }
                if (myPosType == GraphicTile.TilePositionTypes.TOP)
                {
                    tileIndex = pseudoRandom.Next(145, 150);
                }
                if (myPosType == GraphicTile.TilePositionTypes.LEFT)
                {
                    tileIndex = pseudoRandom.Next(80, 90);
                }
                if (myPosType == GraphicTile.TilePositionTypes.RIGHT)
                {
                    tileIndex = pseudoRandom.Next(70, 80);
                }
                if (myPosType == GraphicTile.TilePositionTypes.BOTTOM_LEFT_CORNER)
                {
                    // Pick between large corner or small corner
                    int pick = pseudoRandom.Next(0, 3);
                    if (pick <= 1)
                    {
                        tileIndex = pseudoRandom.Next(161, 165);
                    }
                    else
                    {
                        tileIndex = pseudoRandom.Next(65, 70);
                    }
                }
                if (myPosType == GraphicTile.TilePositionTypes.BOTTOM_RIGHT_CORNER)
                {
                    // Pick between large corner or small corner
                    int pick = pseudoRandom.Next(0, 3);
                    if (pick <= 1)
                    {
                        tileIndex = pseudoRandom.Next(165, 170);
                    }
                    else
                    {
                        tileIndex = pseudoRandom.Next(60, 65);
                    }
                }
                if (myPosType == GraphicTile.TilePositionTypes.TOP_LEFT_CORNER)
                {
                    tileIndex = pseudoRandom.Next(180, 185);
                }
                if (myPosType == GraphicTile.TilePositionTypes.TOP_RIGHT_CORNER)
                {
                    tileIndex = pseudoRandom.Next(185, 190);
                }
                if (myPosType == GraphicTile.TilePositionTypes.LEFT_BOTTOM_DIAG)
                {
                    tileIndex = pseudoRandom.Next(150, 155);
                }
                if (myPosType == GraphicTile.TilePositionTypes.RIGHT_BOTTOM_DIAG)
                {
                    tileIndex = pseudoRandom.Next(155, 160);
                }
                if (myPosType == GraphicTile.TilePositionTypes.LEFT_TOP_DIAG)
                {
                    tileIndex = pseudoRandom.Next(170, 175);
                }
                if (myPosType == GraphicTile.TilePositionTypes.RIGHT_TOP_DIAG)
                {
                    tileIndex = pseudoRandom.Next(175, 180);
                }

                break;
            case GraphicTile.TileEdgeTypes.CLIFF_SHORE:
                // Select a cliff shore of this edgetype
                if (myPosType == GraphicTile.TilePositionTypes.BOTTOM)
                {
                    tileIndex = pseudoRandom.Next(130, 135);
                }
                if (myPosType == GraphicTile.TilePositionTypes.TOP)
                {
                    tileIndex = pseudoRandom.Next(190, 195);
                }
                if (myPosType == GraphicTile.TilePositionTypes.LEFT)
                {
                    tileIndex = pseudoRandom.Next(15, 20);
                }
                if (myPosType == GraphicTile.TilePositionTypes.RIGHT)
                {
                    tileIndex = pseudoRandom.Next(25, 30);
                }
                // USING CLIFF CORNERS AND DIAGONALS HERE 
                if (myPosType == GraphicTile.TilePositionTypes.BOTTOM_LEFT_CORNER)
                {
                    // Pick between large corner or small corner
                    int pick = pseudoRandom.Next(0, 3);
                    if (pick <= 1)
                    {
                        tileIndex = pseudoRandom.Next(161, 165);
                    }
                    else
                    {
                        tileIndex = pseudoRandom.Next(65, 70);
                    }
                }
                if (myPosType == GraphicTile.TilePositionTypes.BOTTOM_RIGHT_CORNER)
                {
                    // Pick between large corner or small corner
                    int pick = pseudoRandom.Next(0, 3);
                    if (pick <= 1)
                    {
                        tileIndex = pseudoRandom.Next(165, 170);
                    }
                    else
                    {
                        tileIndex = pseudoRandom.Next(60, 65);
                    }
                }
                if (myPosType == GraphicTile.TilePositionTypes.TOP_LEFT_CORNER)
                {
                    tileIndex = pseudoRandom.Next(180, 185);
                }
                if (myPosType == GraphicTile.TilePositionTypes.TOP_RIGHT_CORNER)
                {
                    tileIndex = pseudoRandom.Next(185, 190);
                }
                if (myPosType == GraphicTile.TilePositionTypes.LEFT_BOTTOM_DIAG)
                {
                    tileIndex = pseudoRandom.Next(150, 155);
                }
                if (myPosType == GraphicTile.TilePositionTypes.RIGHT_BOTTOM_DIAG)
                {
                    tileIndex = pseudoRandom.Next(155, 160);
                }
                if (myPosType == GraphicTile.TilePositionTypes.LEFT_TOP_DIAG)
                {
                    tileIndex = pseudoRandom.Next(170, 175);
                }
                if (myPosType == GraphicTile.TilePositionTypes.RIGHT_TOP_DIAG)
                {
                    tileIndex = pseudoRandom.Next(175, 180);
                }
                break;
            case GraphicTile.TileEdgeTypes.SHORE_CLIFF:
                // select a shore cliff of this edge type
                if (myPosType == GraphicTile.TilePositionTypes.BOTTOM)
                {
                    tileIndex = pseudoRandom.Next(135, 140);
                }
                if (myPosType == GraphicTile.TilePositionTypes.TOP)
                {
                    tileIndex = pseudoRandom.Next(195, 200);
                }
                if (myPosType == GraphicTile.TilePositionTypes.LEFT)
                {
                    tileIndex = pseudoRandom.Next(10, 15);
                }
                if (myPosType == GraphicTile.TilePositionTypes.RIGHT)
                {
                    tileIndex = pseudoRandom.Next(20, 25);
                }
                // USING SHORE CORNERS AND DIAGONALS HERE (WHERE AVAILABLE)
                if (myPosType == GraphicTile.TilePositionTypes.BOTTOM_LEFT_CORNER)
                {
                    tileIndex = pseudoRandom.Next(125, 130);
                }
                if (myPosType == GraphicTile.TilePositionTypes.BOTTOM_RIGHT_CORNER)
                {
                    tileIndex = pseudoRandom.Next(120, 125);
                }
                if (myPosType == GraphicTile.TilePositionTypes.TOP_LEFT_CORNER)
                {
                    tileIndex = pseudoRandom.Next(30, 35);
                }
                if (myPosType == GraphicTile.TilePositionTypes.TOP_RIGHT_CORNER)
                {
                    tileIndex = pseudoRandom.Next(35, 40);
                }
                if (myPosType == GraphicTile.TilePositionTypes.LEFT_BOTTOM_DIAG)
                {
                    tileIndex = pseudoRandom.Next(50, 55);
                }
                if (myPosType == GraphicTile.TilePositionTypes.RIGHT_BOTTOM_DIAG)
                {
                    tileIndex = pseudoRandom.Next(55, 60);
                }
                if (myPosType == GraphicTile.TilePositionTypes.LEFT_TOP_DIAG)
                {
                    tileIndex = pseudoRandom.Next(40, 45);
                }
                if (myPosType == GraphicTile.TilePositionTypes.RIGHT_TOP_DIAG)
                {
                    tileIndex = pseudoRandom.Next(45, 50);
                }
                break;

            case GraphicTile.TileEdgeTypes.SHORE:
                // select a shore cliff of this edge type
                if (myPosType == GraphicTile.TilePositionTypes.BOTTOM)
                {
                    tileIndex = pseudoRandom.Next(110, 120);
                }
                if (myPosType == GraphicTile.TilePositionTypes.TOP)
                {
                    tileIndex = pseudoRandom.Next(140, 150);
                }
                if (myPosType == GraphicTile.TilePositionTypes.LEFT)
                {
                    tileIndex = pseudoRandom.Next(105, 110);
                }
                if (myPosType == GraphicTile.TilePositionTypes.RIGHT)
                {
                    tileIndex = pseudoRandom.Next(100, 105);
                }

                // USING SHORE CORNERS AND DIAGONALS HERE (WHERE AVAILABLE)
                if (myPosType == GraphicTile.TilePositionTypes.BOTTOM_LEFT_CORNER)
                {
                    tileIndex = pseudoRandom.Next(125, 130);
                }
                if (myPosType == GraphicTile.TilePositionTypes.BOTTOM_RIGHT_CORNER)
                {
                    tileIndex = pseudoRandom.Next(120, 125);
                }
                if (myPosType == GraphicTile.TilePositionTypes.TOP_LEFT_CORNER)
                {
                    tileIndex = pseudoRandom.Next(30, 35);
                }
                if (myPosType == GraphicTile.TilePositionTypes.TOP_RIGHT_CORNER)
                {
                    tileIndex = pseudoRandom.Next(35, 40);
                }
                if (myPosType == GraphicTile.TilePositionTypes.LEFT_BOTTOM_DIAG)
                {
                    tileIndex = pseudoRandom.Next(50, 55);
                }
                if (myPosType == GraphicTile.TilePositionTypes.RIGHT_BOTTOM_DIAG)
                {
                    tileIndex = pseudoRandom.Next(55, 60);
                }
                if (myPosType == GraphicTile.TilePositionTypes.LEFT_TOP_DIAG)
                {
                    tileIndex = pseudoRandom.Next(40, 45);
                }
                if (myPosType == GraphicTile.TilePositionTypes.RIGHT_TOP_DIAG)
                {
                    tileIndex = pseudoRandom.Next(45, 50);
                }
                break;

            default:
                // Select a center tile
                int select = pseudoRandom.Next(0, 20);
                if (select > 9)
                {
                    tileIndex = select - 10;
                }
                else
                {
                    tileIndex = 160;
                }

                break;

        }

        return tileIndex;
    }

    int GetIndexFromSandOrMudSheet(GraphicTile.TilePositionTypes myPosType, GraphicTile.TileLandTypes myLandType)
    {
        int tileIndex = 0;

        switch (myPosType)
        {
            case GraphicTile.TilePositionTypes.BOTTOM:
                if (myLandType == GraphicTile.TileLandTypes.SAND)
                {
                    return Random.Range(61, 70);

                }else if (myLandType == GraphicTile.TileLandTypes.MUD)
                {
                    return Random.Range(131, 140);
                }
                break;
             case GraphicTile.TilePositionTypes.TOP:
                if (myLandType == GraphicTile.TileLandTypes.SAND)
                {
                    return Random.Range(0, 9);
                }
                else if (myLandType == GraphicTile.TileLandTypes.MUD)
                {
                    return Random.Range(91, 95);
                }
                break;


            case GraphicTile.TilePositionTypes.LEFT:
                if (myLandType == GraphicTile.TileLandTypes.SAND)
                {
                    return Random.Range(40, 45);
                }
                else if (myLandType == GraphicTile.TileLandTypes.MUD)
                {
                    return Random.Range(100, 105);
                }
                break;
                
            case GraphicTile.TilePositionTypes.RIGHT:
                if (myLandType == GraphicTile.TileLandTypes.SAND)
                {
                    return Random.Range(45, 50);
                }
                else if (myLandType == GraphicTile.TileLandTypes.MUD)
                {
                    return Random.Range(105, 110);
                }
                break;

            case GraphicTile.TilePositionTypes.BOTTOM_LEFT_CORNER:

                if (myLandType == GraphicTile.TileLandTypes.SAND)
                {
                    return Random.Range(30, 35);
                }
                else if (myLandType == GraphicTile.TileLandTypes.MUD)
                {
                    return Random.Range(120, 125);
                }
                break;

            case GraphicTile.TilePositionTypes.BOTTOM_RIGHT_CORNER:
                if (myLandType == GraphicTile.TileLandTypes.SAND)
                {
                    return Random.Range(35,40);
                }
                else if (myLandType == GraphicTile.TileLandTypes.MUD)
                {
                    return Random.Range(125, 130);
                }
                break;

            case GraphicTile.TilePositionTypes.TOP_LEFT_CORNER:
                if (myLandType == GraphicTile.TileLandTypes.SAND)
                {
                    return Random.Range(20 , 25);
                }
                else if (myLandType == GraphicTile.TileLandTypes.MUD)
                {
                    return Random.Range(110, 115);
                }
                break;

            case GraphicTile.TilePositionTypes.TOP_RIGHT_CORNER:
                if (myLandType == GraphicTile.TileLandTypes.SAND)
                {
                    return Random.Range(25, 30);
                }
                else if (myLandType == GraphicTile.TileLandTypes.MUD)
                {
                    return Random.Range(115, 120);
                }
                break;

            case GraphicTile.TilePositionTypes.LEFT_BOTTOM_DIAG:
                if (myLandType == GraphicTile.TileLandTypes.SAND)
                {
                    return Random.Range(50, 55);
                }
                else if (myLandType == GraphicTile.TileLandTypes.MUD)
                {
                    return Random.Range(80, 85);
                }
                break;

            case GraphicTile.TilePositionTypes.RIGHT_BOTTOM_DIAG:
                if (myLandType == GraphicTile.TileLandTypes.SAND)
                {
                    return Random.Range(55, 60);
                }
                else if (myLandType == GraphicTile.TileLandTypes.MUD)
                {
                    return Random.Range(85, 90);
                }
                break;
            case GraphicTile.TilePositionTypes.LEFT_TOP_DIAG:
                if (myLandType == GraphicTile.TileLandTypes.SAND)
                {
                    return Random.Range(10, 15);
                }
                else if (myLandType == GraphicTile.TileLandTypes.MUD)
                {
                    return Random.Range(80, 85);
                }
                break;
            case GraphicTile.TilePositionTypes.RIGHT_TOP_DIAG:
                if (myLandType == GraphicTile.TileLandTypes.SAND)
                {
                    return Random.Range(15, 20);
                }
                else if (myLandType == GraphicTile.TileLandTypes.MUD)
                {
                    return Random.Range(95, 100);
                }
                break;

            default:
                // Center tile
                if (myLandType == GraphicTile.TileLandTypes.SAND)
                {
                    return 60;
                }
                else if (myLandType == GraphicTile.TileLandTypes.MUD)
                {
                    return 130;
                }
                break;

        }

        return tileIndex;
    }

    void ApplyTextures()
    {
        //Texture1.Apply();
        //Texture2.Apply();

        if (hasTopLayer)
        {
            base_renderer.sharedMaterial.mainTexture = Texture1;
            second_renderer.sharedMaterial.mainTexture = Texture2;
            shore_renderer.sharedMaterial.mainTexture = Texture3;

        }
        else
        {
            base_renderer.sharedMaterial.mainTexture = Texture1;
            shore_renderer.sharedMaterial.mainTexture = Texture3;
        }
            

    }

    public GraphicTile GetGraphicTileFromPos(Vector3 position)
    {
        Vector2 pos = new Vector2(Mathf.Round(position.x), Mathf.Round(position.y));
        if (all_GraphicTiles.ContainsKey(pos))
        {
            return all_GraphicTiles[pos];
        }
        else
            return null;
    }

    public GraphicTile GetGraphicTileFromPos(Vector2 position)
    {
        if (all_GraphicTiles.ContainsKey(position))
        {
            return all_GraphicTiles[position];
        }
        else
            return null;
    }
}
