using UnityEngine;
using System.Collections;

public class ResourceDrop : MonoBehaviour {

    public ExtractionBuilding extractionSource { get; protected set; }
    int totalAmmntOfResource;

    Rock.RockProductionType rockProdType;

    Rigidbody2D rb;
    float forceAmmt = 10;
    int randomForceDirection;

    TileData.Types resourceType;

    public bool goesToPlayer = false, goesToTransporter = true;

    Vector3 targetPosition;

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

    public void InitRock(Rock.RockProductionType rock, int ammnt)
    {
        totalAmmntOfResource = ammnt;
        rockProdType = rock;
        resourceType = TileData.Types.rock;
    }

    void Start()
    {
        if (goesToPlayer)
            targetPosition = ResourceGrid.Grid.Hero.transform.position;

        if (goesToTransporter)
            targetPosition = Transporter_Handler.instance.GetTransporterPosition();
         
        if (extractionSource != null)
        {
            // Once spawned, if on Water bob in the waves...
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
            // If not on water just Push in a random direction
            else
            {
                randomForceDirection = ResourceGrid.Grid.pseudoRandom.Next(0, 3);
                Push(randomForceDirection);
            }
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
        // By default it will go to the Transporter, unless goesToPlayer is true
        if (targetPosition != null)
        {
            transform.position = Vector2.MoveTowards(transform.position, targetPosition, 5 * Time.deltaTime);

            // Are we there yet?
            ListenForTarget();
        }
    }

    void ListenForTarget()
    {
        if (Vector2.Distance(transform.position, targetPosition) <= 0.1f)
        {
            PickUp();
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
        else
        {
    
            
            if (resourceType == TileData.Types.rock)
            {
                Ship_Inventory.Instance.ReceiveTempRock(totalAmmntOfResource, rockProdType);
            }
            else
            {
                Ship_Inventory.Instance.ReceiveTemporaryResources(resourceType, totalAmmntOfResource);
            }
           

            StopCoroutine("Wave");

            ObjectPool.instance.PoolObject(gameObject);
        }
    }

}
