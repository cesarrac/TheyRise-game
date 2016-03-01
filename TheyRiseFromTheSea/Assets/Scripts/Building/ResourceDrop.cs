using UnityEngine;
using System.Collections;

public class ResourceDrop : MonoBehaviour {

    public ExtractionBuilding extractionSource { get; protected set; }
    int totalAmmntOfResource;

    Rock.RockProductionType rockProdType;

    Rigidbody2D rb;
    float forceAmmt = 10;
    int randomForceDirection;

    ResourceDrop_PlayerDetect player_detect;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void InitSource(ExtractionBuilding e, int ammnt)
    {
        extractionSource = e;
        totalAmmntOfResource = ammnt;

        if (e.resourceType == TileData.Types.rock)
        {
            rockProdType = ResourceGrid.Grid.GetTileGameObjFromIntCoords(e.targetTile.posX, e.targetTile.posY).GetComponent<Rock_Handler>().myRock._rockProductionType;
        }
    }

    void Start()
    {
        player_detect = GetComponentInChildren<ResourceDrop_PlayerDetect>();
          
        if (extractionSource.resourceType == TileData.Types.water)
        {
            // For water we always need to make sure to push away from the water
            var origin = ResourceGrid.Grid.GetTileWorldPos(extractionSource.originTile.posX, extractionSource.originTile.posY);
            var target = ResourceGrid.Grid.GetTileWorldPos(extractionSource.targetTile.posX, extractionSource.targetTile.posY);
            var heading = -(target - origin);
            rb.AddForce(heading * forceAmmt, ForceMode2D.Impulse);

            // FIX THIS! This below should cause the R Drop to "bob"in the waves if it is on water... but for some reason it keeps sending it to Vector3.zero position...
           //StartCoroutine("WaitToLand");

        }
        else
        {
            randomForceDirection = ResourceGrid.Grid.pseudoRandom.Next(0, 3);
            Push(randomForceDirection);
        }
      
    }

    void Push(int randomForceDirection)
    {
        switch (randomForceDirection)
        {
            case 0:
                rb.AddForce(Vector2.down * forceAmmt, ForceMode2D.Impulse);
                break;
            case 1:
                rb.AddForce(Vector2.left * forceAmmt, ForceMode2D.Impulse);
                break;
            case 2:
                rb.AddForce(Vector2.right * forceAmmt, ForceMode2D.Impulse);
                break;
            default:
                rb.AddForce(Vector2.left * forceAmmt, ForceMode2D.Impulse);
                break;
        }
    }

    IEnumerator WaitToLand()
    {
        while (true)
        {
            // Wait a second, to give time for the animation to fully play
            yield return new WaitForSeconds(1f);

            // Now check the tile we are on is water 
            //if (ResourceGrid.Grid.TileFromWorldPoint(transform.position).tileType == TileData.Types.water)
            //{
            //    // If it is, allow the Wave Bobbing script to start bobbing
            //    GetComponent<Wave_Bobbing>().CanBob();
            //}

            if (ResourceGrid.Grid.TileFromWorldPoint(transform.position) != null)
            {
                TileData tile = ResourceGrid.Grid.TileFromWorldPoint(transform.position);

                for (int x = tile.posX - 1; x <= tile.posX + 1; x++)
                {
                    for (int y = tile.posY - 1; y <= tile.posY + 1; y++)
                    {
                        if (ResourceGrid.Grid.CheckForResource(x, y, TileData.Types.water))
                        {
                            // If it is, allow the Wave Bobbing script to start bobbing
                            GetComponent<Wave_Bobbing>().CanBob();
                        }
                    }
                }
            }
       

            yield break;
        }
    }

    void Update()
    {
        if (player_detect != null)
        {
            if (player_detect.isPlayerDetected)
            {
                transform.position = Vector2.MoveTowards(transform.position, player_detect.playerPos, 5 * Time.deltaTime);
            }
        }
    }

    public void PickUp()
    {
        if (extractionSource != null)
        {
            extractionSource.PickUpAndBeamToShip(totalAmmntOfResource);

            StopCoroutine("Wave");

            ObjectPool.instance.PoolObject(gameObject);
        }
    }

}
