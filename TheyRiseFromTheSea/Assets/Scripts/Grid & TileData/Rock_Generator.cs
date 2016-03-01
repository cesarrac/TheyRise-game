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
                neighborOreTiles = null;
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
  
        for (int i = 0; i < totalRocksOnMap; i++)
        {
            // Here we attempt to get a position for this rock. A legal position means there's no water around it.

            // Start with an empty Vector2, this will be filled if a legal position is found.
            Vector2 pos = new Vector2();

            // We will allow 10 attempts to get a legal position.
            int rockPosAttempts = 10;

            // Loop to check for legal position
            for (int j = 0; j < rockPosAttempts; j++)
            {
                // During each attempt we try to assign a legal position...

                // ... if pos is Null or Vector2.zero, this attempt failed...
                if (pos == null || pos == Vector2.zero)
                {
                    pos = CheckForWater();
                }
                // ... if it's NOT Null or Vector2.zero, this attempt SUCCEDED...
                else
                {
                    // ... so we break out of the loop!
                    break;
                }
            }

            // If the attempt FAILED pos will be equal to null or Vector2.zero
            if (pos == null || pos == Vector2.zero)
            {
                // In this case, give up with this rock and continue to the next Rock in the loop
                continue;
            }

            // If the attempt SUCCEDED a new Ore Patch will be set, using this new position
            SetNewOrePatch((int)pos.x, (int) pos.y, GetRockTypeFromLandType(pos));
        }
    }

    Vector2 GetPosition()
    {
        return TileTexture_3.instance.centerTiles[ResourceGrid.Grid.pseudoRandom.Next(0, TileTexture_3.instance.centerTiles.Length) ];
        
    }

    Vector2 CheckForWater()
    {
        // First get a random position from the Empty Tiles array, using the psedorandom tied to this Map's seed...

        //Vector2 tempPos = ResourceGrid.Grid.emptyTilesArray[ResourceGrid.Grid.pseudoRandom.Next(0, ResourceGrid.Grid.emptyTilesArray.Length - 1)];
        Vector2 tempPos = GetPosition();

        // .. then check for water in a 5 tile radius around this temporary position.

        // If Water is found, this flag will turn true.
        bool waterFound = false;

        for (int x = (int)tempPos.x - 8; x < tempPos.x + 8; x++)
        {
            for (int y = (int)tempPos.y - 8; y < tempPos.y + 8; y++)
            {
                if (ResourceGrid.Grid.CheckForResource(x, y, TileData.Types.water) == true)
                {
                    waterFound = true;

                    // If just ONE tile is water, we can break out of the loop after setting the flag to true
                    break;
                }
            }
        }

        // If the water flag was not set to true, then we return the temporary random position.
        if (!waterFound)
        {
            return tempPos;
        }
        else
        {
            // If Water was found we return Vector2 zero and attempt to get a position again if there are attempts left.
            return Vector2.zero;
        }
    }

    Rock.RockType GetRockTypeFromLandType(Vector2 position)
    {
        GraphicTile gTile = TileTexture_3.instance.GetGraphicTileFromPos(position);

        Rock.RockType type = Rock.RockType.sharp;

        if (gTile != null)
        {
            switch (gTile.MyTileLandType)
            {
                case GraphicTile.TileLandTypes.ASH:
                    // Choose from Tube or Sharp rock
                    int choice = Random.Range(0, 2);

                    if (choice == 0)
                    {
                        type = Rock.RockType.tube;
                    }
                    else
                        type = Rock.RockType.sharp;

                    break;
                case GraphicTile.TileLandTypes.MUD:
                    // Choose Sharp rock
                    type = Rock.RockType.sharp;

                    break;
                case GraphicTile.TileLandTypes.SAND:
                    // Choose Hex rock
                    type = Rock.RockType.hex;

                    break;
                default:
                    type = Rock.RockType.sharp;

                    break;
            }
        }

        return type;

    }



    void SetNewOrePatch(int leadX, int leadY, Rock.RockType rType)
    {
        // FIX THIS! Making density calculation totally random!

        int distance = ResourceGrid.Grid.pseudoRandom.Next(0, 21);

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
       // Debug.Log("ORE PATCH at lead x " + leadX + " lead Y " + leadY);
        patch.SetFormation();
        ResourceGrid.Grid.PlaceOrePatch(patch, rType);
    }

    

}
