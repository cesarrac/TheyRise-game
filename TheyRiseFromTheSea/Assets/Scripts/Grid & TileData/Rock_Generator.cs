using UnityEngine;
using System.Collections;


public class OrePatch
{
    public int leadPositionX;
    public int leadPositionY;
    int pDensity;
    public int density { get { return pDensity; } set { pDensity = Mathf.Clamp(value, 1, 5); } }
    public int totalInPatch;
    public OreTile[] neighborOreTiles;
    public Rock.RockType rockType;


    public OrePatch(int xPos, int yPos, int _density, Rock.RockType rockT)
    {
        leadPositionX = xPos;
        leadPositionY = yPos;
        density = _density;
        totalInPatch = _density;
        rockType = rockT;
    }

    public void SetFormation()
    {
        /* formation offset,  indicating how far the neighbor ore tile is from its lead tile.
         * Depending on their density they will have a minor offset(more density) or major offset (less density) */
        int minorOffset = Random.Range(1, 4);
        int majorOffset = Random.Range(4, 6);

        switch (density)
        {
            case 1:
                // This patch only has one rock or mineral
                break;
            case 2:
                // This patch contains two, so neighbor ore array = 1
                neighborOreTiles = new OreTile[1];
                // This is the position the neighbor ore can be placed on
                neighborOreTiles[0] = new OreTile(leadPositionX + majorOffset, leadPositionY - majorOffset);
                break;
            case 3:
                neighborOreTiles = new OreTile[2];
                neighborOreTiles[0] = new OreTile(leadPositionX + minorOffset, leadPositionY - majorOffset);
                neighborOreTiles[1] = new OreTile(leadPositionX - minorOffset, leadPositionY - majorOffset);
                break;
            case 4:
                neighborOreTiles = new OreTile[3];
                neighborOreTiles[0] = new OreTile(leadPositionX, leadPositionY + minorOffset);
                neighborOreTiles[1] = new OreTile(leadPositionX, leadPositionY - minorOffset);
                neighborOreTiles[2] = new OreTile(leadPositionX - minorOffset, leadPositionY);
                break;
            case 5:
                neighborOreTiles = new OreTile[4];
                neighborOreTiles[0] = new OreTile(leadPositionX - minorOffset, leadPositionY);
                neighborOreTiles[1] = new OreTile(leadPositionX + minorOffset, leadPositionY);
                neighborOreTiles[2] = new OreTile(leadPositionX + minorOffset, leadPositionY + minorOffset);
                neighborOreTiles[3] = new OreTile(leadPositionX, leadPositionY + minorOffset);
                break;
            default:
                neighborOreTiles = new OreTile[1];
                neighborOreTiles[0] = new OreTile(leadPositionX + minorOffset, leadPositionY - minorOffset);
                break;

        }
    }

    public class OreTile
    {
        public int posX;
        public int posY;

        public OreTile(int x, int y)
        {
            posX = x;
            posY = y;
        }
    }
}


public class Rock_Generator : MonoBehaviour {

    public static Rock_Generator Instance { get; protected set; }

    [Range(5, 55)]
    public int totalRocksOnMap;
    [Range(6, 28)]
    public int totalMineralsOnMap;


    void Awake()
    {
        Instance = this;
    }

    /*
        NEEDS:
        - A way to select positions for a lead rock tile
        - From that lead rock tile a way to spawn rocks around it depending on its formation density
        - Get its formation density depending on how far it is from the shore
        - A way to get what rock type to spawn by finding the land type of the GraphicTile




        - Loop through total rocks on map
        - On each loop iteration we'll need a position to place a rock on
        - This tile position will always be of empty land type. But get its Graphic Tile landtype to know which type of rock to generate for this pos.
        - This first placed rock can be a lead position to a formation
        - But, before placing we'll need to check if there isn't already a rock there.
        - To do this above mentioned check we could create a dictionary of <position, rock> that we can check if it contains the pos before placing
        - After placing this first rock we can place its neighbors according to its formation
        - We would loop through its neighbor rocks count ( this would still be within the the total rocks on map loop)
        - Check that this neighbor position is legal, it's on an empty tile



    */

    public void GenerateRocks()
    {
  
        for (int i = 0; i <= totalRocksOnMap; i++)
        {
            // Get a position
            Vector2 pos = GetPosition();
            int posX = Mathf.RoundToInt(pos.x);
            int posY = Mathf.RoundToInt(pos.y);
            SetNewOrePatch(posX, posY, GetRockTypeFromLandType(pos));
        }
    }

    Vector2 GetPosition()
    {
        return TileTexture_3.instance.centerTiles[ Random.Range(0, TileTexture_3.instance.centerTiles.Length) ];
        
    }

    Rock.RockType GetRockTypeFromLandType(Vector2 position)
    {
        GraphicTile gTile = TileTexture_3.instance.GetGraphicTileFromPos(position);

        if (gTile != null)
        {
            switch (gTile.MyTileLandType)
            {
                case GraphicTile.TileLandTypes.ASH:
                    // Choose from Tube or Sharp rock
                    int choice = Random.Range(0, 2);

                    if (choice == 0)
                    {
                        return Rock.RockType.tube;
                    }
                    else
                        return Rock.RockType.sharp;

                    break;
                case GraphicTile.TileLandTypes.MUD:
                    // Choose Sharp rock
                    return Rock.RockType.sharp;

                    break;
                case GraphicTile.TileLandTypes.SAND:
                    // Choose Hex rock
                    return Rock.RockType.hex;

                    break;
                default:
                    return Rock.RockType.sharp;

                    break;
            }
        }
        else
        {
            // Default:
            return Rock.RockType.sharp;
        }

    }



    void SetNewOrePatch(int leadX, int leadY, Rock.RockType rType)
    {
        // FIX THIS! Making density calculation totally random
        int distance = Random.Range(0, 21);

        int density = 0;
        if (distance >= 15)
        {
            // pick a 1 or 2 density
            int pick = Random.Range(0, 2);
            density = pick;
        }
        else if (distance < 15 && distance > 8)
        {
            // pick between 4 or 5 density
            int pick = Random.Range(2, 4);
            density = pick;
        }
        else
        {
            density = 5;
        }
        OrePatch patch = new OrePatch(leadX, leadY, density, rType);
        patch.SetFormation();
        ResourceGrid.Grid.PlaceOrePatch(patch, rType);
    }

    

}
