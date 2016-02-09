using UnityEngine;
using System.Collections;
using System;

public class ExtractorStats
{
    // Seconds it takes to call the extract method and get more of a resource
    public float extractRate { get; protected set; }
    float _extractRate { get { return extractRate; } set { extractRate = Mathf.Clamp(value, 10f, 120f); } } // <------- Use this in a constructor to set extract Rate

    // How much of a resource this building can extract
    public int extractAmmount { get; protected set; }
    int _extractAmmount { get { return extractAmmount; } set { extractAmmount = Mathf.Clamp(value, 1, 1000); } } // <------- Use this in a constructor to set extract ammount

    // How many units of a resource can this building store
    public int personalStorageCapacity { get; protected set; }
    int _personalStorageCap { get { return personalStorageCapacity; } set { personalStorageCapacity = Mathf.Clamp(value, 10, 1000); } }

    // Storage for required materials needed by buildings that PRODUCE a resource
    public int secondStorageCapacity { get; protected set; }
    int _secondStorageCap { get { return secondStorageCapacity; } set { secondStorageCapacity = Mathf.Clamp(value, 5, 500); } }

    // How many units of a material does this building need to produce
    public int materialsConsumed { get; protected set; }
    int _materialsConsumed { get { return materialsConsumed; } set { materialsConsumed = Mathf.Clamp(value, 1, 100); } }

    // Power versus the resources Hardness will result in time (in seconds) it takes for this machine to extract its target resource
    public float extractPower { get; protected set; }
    float _extractPower { get { return extractPower; } set { extractPower = Mathf.Clamp(value, 1, 100); } }

    // Constructor
    public ExtractorStats(float rate, int ammount, float power, int personalStorageCap, int secondStorageCap = 0, int materialConsumed = 0)
    {
        _extractRate = rate;
        _extractAmmount = ammount;
        _personalStorageCap = personalStorageCap;
        _secondStorageCap = secondStorageCap;
        _materialsConsumed = materialConsumed;
        _extractPower = power;
    }

    // Constructor for a Storage Unit
    public ExtractorStats(int storageCap)
    {
        _secondStorageCap = storageCap;
    }

    public void SetCurrentRate(float newRate)
    {
        _extractRate = newRate;
    }
}

public class ExtractionBuilding : MonoBehaviour {
   

    // VARS:
    public LineRenderer lineR; // < --- to display connections from inputs to outputs

    public TileData.Types resourceType { get; protected set; }

    public ExtractorStats extractorStats { get; protected set; }

    public ResourceGrid resource_grid;

    public Building_StatusIndicator b_statusIndicator;

    public string statusMessage;

    public bool statusIndicated { get; protected set; }

    int _currResourcesStored;
    public int currResourceStored { get { return _currResourcesStored; } set { _currResourcesStored = Mathf.Clamp(value, 0, extractorStats.personalStorageCapacity); } }
    int _currMaterialsStored;
    public int currMaterialsStored { get { return _currMaterialsStored; } set { _currMaterialsStored = Mathf.Clamp(value, 0, extractorStats.secondStorageCapacity); } } // < --- stored in secondary storage

    public bool isExtracting { get; protected set; }
    public bool storageIsFull { get; protected set; }
    public bool noMaterialsLeft { get; protected set; }

    // The type of material being sent by an Input (this gets set by the Input when sending to the output by passing its resourceType as an argument)
    public TileData.Types inputType { get; protected set; }

    // This is for checking that the Input being received IS the material that this Producer NEEDS
    public TileData.Types requiredMaterial { get; protected set; }

    // position of the tile containing the resource being extracted
    public int r_PosX { get; protected set; }
    public int r_PosY { get; protected set; }

    public Transform myTransform { get; protected set; }

    // an option to HALT PRODUCTION if needed
    public bool productionHalt { get; protected set; }

    // all Extraction buildings can have an OUTPUT, another building like a storage or self-producer that can RECEIVE & STORE
    public ExtractionBuilding output { get; protected set; }
    public bool isConnectedToOutput { get; protected set; }

    bool isConnectingInput;

    // Callback method ONLY for storage units
    Action<TileData.Types, int> callback;

    public GameObject extractTargetAsGameObj;

    // Another callback method used by rock extractors and such to split the extracted ammount by its type
    public Action<int> inventoryTypeCallback;

    public Action<int> splitShipInventoryCallback;

    public Vector3 resourceWorldPos { get; protected set; }

    // Keep track of the tile I'm on
    public TileData originTile { get; protected set; }

    // Keep track of the tile that is currently being extracted
    public TileData targetTile { get; protected set; }

    public GameObject circleSelection { get; protected set; }

    public bool pickUpSpawned;

    Action rateCalcCallback;

    // STATE MACHINE:
    public enum State
    {
        EXTRACTING,
        SEARCHING,
        NOSTORAGE,
        STARVED,
        PRODUCING,
        HALT, 
        IDLE
    }

    public State _state { get; protected set; }

    // INITIALIZERS:
    public void Init(TileData.Types r_type, float rate, float power, int ammnt, int storageCap, Transform _trans)
    {
        extractorStats = new ExtractorStats(rate, ammnt, power, storageCap);
        resourceType = r_type;
        myTransform = _trans;

        resource_grid = ResourceGrid.Grid;

        b_statusIndicator = GetComponent<Building_ClickHandler>().buildingStatusIndicator;

    }

    // this Initializer works for buildings that extract from another building to produce their OWN resource
    public void InitSelfProducer(TileData.Types productType, float rate, int ammnt, int storageCap, int secondStorageCap, int matConsumed, TileData.Types requiredMat,Transform _trans)
    {
        extractorStats = new ExtractorStats(rate, ammnt, storageCap, secondStorageCap, matConsumed);
        resourceType = productType;
        myTransform = _trans;

        resource_grid = ResourceGrid.Grid;
        b_statusIndicator = GetComponent<Building_ClickHandler>().buildingStatusIndicator;

        requiredMaterial = requiredMat;
    }

    // This is the Initializer for Storage Units
    public void InitStorageUnit(int storageCap, Transform _trans, Action<TileData.Types, int> _callback)
    {
        extractorStats = new ExtractorStats(storageCap);
        myTransform = _trans;
        callback = _callback;

        resource_grid = ResourceGrid.Grid;
        b_statusIndicator = GetComponent<Building_ClickHandler>().buildingStatusIndicator;

        // all storage units require an empty material since they can take any
        requiredMaterial = TileData.Types.empty;
    }



    // METHODS:

    public bool CheckSearchForResource()
    {
        // Get my origin tile
        if (originTile == null)
        {
            originTile = resource_grid.TileFromWorldPoint(myTransform.position);
            Debug.Log("EXTRACTOR origin tile is at: " + originTile.posX + " " + originTile.posY);
        }


        targetTile = SearchForResource();

        if (targetTile != null)
        {
            // Place a circle around the Resource, showing that it is being currently extracted
            Vector3 circlePos = new Vector3();

            if (resource_grid.GetTileGameObjFromIntCoords(targetTile.posX, targetTile.posY) != null)
            {
                circlePos = resource_grid.GetTileGameObjFromIntCoords(targetTile.posX, targetTile.posY).transform.position;
            }
            else
            {
                circlePos = resource_grid.GetTileWorldPos(targetTile.posX, targetTile.posY);
            }

            // Make sure before placing that this Building doesn't already have any circle Selections
            if (circleSelection != null)
            {
                ObjectPool.instance.PoolObject(circleSelection);
            }

            circleSelection = ObjectPool.instance.GetObjectForType("Selection Circle", true, circlePos);

            if (r_PosX != targetTile.posX || r_PosY != targetTile.posY)
            {
                // Define my resource's tile position
                r_PosX = targetTile.posX;
                r_PosY = targetTile.posY;
            }
            return true;
        }
        else
        {
            // if this method returns false then NO tiles of that resource were found
            return false;
        }
    }

    TileData SearchForResource()
    {
        float spriteWidth = GetComponent<SpriteRenderer>().sprite.bounds.size.x;
        float spriteHeight = GetComponent<SpriteRenderer>().sprite.bounds.size.y;


        for (int x =(int) myTransform.position.x - (int)spriteWidth; x <= myTransform.position.x + spriteWidth; x++)
        {
            for (int y = originTile.posY - (int)spriteHeight; y <= originTile.posY + spriteHeight; y++)
            {
                //Debug.Log("EXTRACTION: Checking pos " + x + " " + y);
                if (CheckTileType(x, y) != null)
                {
                    return CheckTileType(x, y); ;
                }
            }
        }

        // If no tiles were found ...
        return null;

    }


    TileData CheckTileType(int x, int y)
    {
        if (resource_grid.GetTileType(x, y) == resourceType)
        {
            resourceWorldPos = new Vector3(x, y, 0);
            return resource_grid.tiles[x, y];

        }
        else
        {
            return null;
        }
       
    }


    public IEnumerator ShowStatusMessage()
    {
        int repetitions = 0;
        statusIndicated = false;

        while (true)
        {
            yield return new WaitForSeconds(2f);
            b_statusIndicator.CreateStatusMessage(statusMessage);
            repetitions++;
            if (repetitions >= 3)
            {
                // Status indicated bool allows this coroutine to be started up again to repeat a message (Like "FULL!!")
                statusIndicated = true;
                yield break;
            }


        }

    }

    void SetExtractRate(float currExtractRate, float power, float hardness)
    {
        extractorStats.SetCurrentRate((hardness / power) + currExtractRate);
        //Debug.Log("EXTRACTOR: Current Power is " + power + " and the Tile Hardness is " + hardness);
        //Debug.Log("EXTRACTOR: Setting Extraction Rate to: " + extractorStats.extractRate);
    }

    public IEnumerator ExtractResource()
    {
        // while true, extract, wait extract rate, check if there's ore left: if no break out and  change state to searching
        while (true)
        {
            // Calculate and Set the true extraction rate considering the current target resource tile
            SetExtractRate(extractorStats.extractRate, extractorStats.extractPower, targetTile.hardness);
           
            yield return new WaitForSeconds(extractorStats.extractRate);

            if (!productionHalt)
            {
                // int extract = resource_grid.ExtractFromTile(r_PosX, r_PosY, extractorStats.extractAmmount);  
                int extract = currResourceStored + extractorStats.extractAmmount;

                if (extract <= extractorStats.personalStorageCapacity)
                {
                    // Extract from the Tile using Resource Grid's method ExtractFromTile
                    currResourceStored += resource_grid.ExtractFromTile(r_PosX, r_PosY, extractorStats.extractAmmount);

                    // If the Extraction Building has a function for splitting the resource into types, call it here
                    if (inventoryTypeCallback != null)
                        inventoryTypeCallback(extractorStats.extractAmmount);

                    // Check again if personal storage is full AFTER adding the ore
                    if (currResourceStored >= extractorStats.personalStorageCapacity)
                    {
                        // STORAGE FULL
                        isExtracting = false;
                        storageIsFull = true;
                        //statusMessage = "Full!";
                        //StopCoroutine("ShowStatusMessage");
                        //StartCoroutine("ShowStatusMessage");
                        //Debug.Log("EXTRACTOR STORAGE FULL!");
                        // _state = State.NOSTORAGE;
                        yield break;
                    }

                }

                else
                {
                    // Check if there's any space left at all in personal storage
                    int spaceLeft = extractorStats.personalStorageCapacity - currResourceStored;
                    if (spaceLeft > 0 && extract >= spaceLeft)
                    {
                        // Fill up the space left
                        currResourceStored += spaceLeft;

                        // Call the inventory split callback if not null to account for the remainder
                        if (inventoryTypeCallback != null)
                            inventoryTypeCallback(spaceLeft);
                    }
    

                    // STORAGE FULL
                    isExtracting = false;
                    storageIsFull = true;
                    //statusMessage = "Full!";
                    //StopCoroutine("ShowStatusMessage");
                    //StartCoroutine("ShowStatusMessage");
                    //Debug.Log("EXTRACTOR STORAGE FULL!");
                    //_state = State.NOSTORAGE;
                    yield break;
                }
            }
            else
            {
                yield break;
            }
        
        }
    }


    // Self producing units need different coroutines since they are NOT extracting from a tile through the Grid.
    // Instead they take a resource from another building's storage if that building has enough for what it needs.
    // Then they go into a coroutine where they produce X ammnt of their product using Y consume ammount.
    // They NEED:
    // 2 storage spaces. One for their Required Resource and another for their Product.
    // A public method that can RECEIVE the Required Resource from another building nearby (maybe automatically detect for the required building around them and check if they have enough every few frames)

    public IEnumerator Produce()
    {
        // while true, extract, wait extract rate, check if there's ore left: if no break out and  change state to searching
        while (true)
        {

            yield return new WaitForSeconds(extractorStats.extractRate);

            if (currMaterialsStored > 0)
            {
                // First check if there is enough Material left to produce
                int consumed = currMaterialsStored - extractorStats.materialsConsumed;

                if (consumed > 0)
                {

                    // create the product if our own storage is NOT full
                    int production = extractorStats.extractAmmount + currResourceStored;
                    if (production <= extractorStats.personalStorageCapacity)
                    {
                        currResourceStored += extractorStats.extractAmmount;
                        // consume material
                        currMaterialsStored -= extractorStats.materialsConsumed;
                    }
                    else
                    {
                        // Check if there's any space left at all in personal storage
                        int extra = production - extractorStats.personalStorageCapacity;
                        int remainder = extractorStats.extractAmmount - extra;
                        if (remainder > 0)
                        {
                            // If there's space left, the resources stored get the remainder
                            currResourceStored += remainder;
                            // materials are consumed fully anyway
                            currMaterialsStored -= extractorStats.materialsConsumed;
                        }


                        // STORAGE FULL
                        isExtracting = false;
                        storageIsFull = true;
                        //statusMessage = "Full!";
                        //StopCoroutine("ShowStatusMessage");
                        //StartCoroutine("ShowStatusMessage");
                        //Debug.Log("EXTRACTOR STORAGE FULL!");
                        //_state = State.NOSTORAGE;
                        yield break;
                    }
                }
                else
                {
                    // NO WATER LEFT
                    noMaterialsLeft = true;
                    productionHalt = true;
                    statusMessage = "No Water!";

                    yield break;
                }
            }
            else
            {
                // NO WATER LEFT
                noMaterialsLeft = true;
                productionHalt = true;
                statusMessage = "No Water!";

                yield break;
            }

        }
    }


    public void ConnectInput()
    {
        if (!isConnectingInput)
        {
            isConnectingInput = true;
            StopCoroutine("WaitToConnect");
            StartCoroutine("WaitToConnect");
        }
   
    }

    IEnumerator WaitToConnect()
    {
        Mouse_Controller mouse_control = Mouse_Controller.MouseController;

        while (true)
        {
          if (Input.GetMouseButtonDown(1))
            {
                TileData tileUnderMouse = mouse_control.GetTileUnderMouse();
                GameObject input = ResourceGrid.Grid.GetTileGameObjFromIntCoords(tileUnderMouse.posX, tileUnderMouse.posY);

                if (input != null)
                {
                    if (input.GetComponent<ExtractionBuilding>() != null)
                    {                     
                        ExtractionBuilding extractor = input.GetComponent<ExtractionBuilding>();

                        // Check that this new input extracts/produces the required material OR that my required material is just empty (meaning im just a storage unit)
                        if (extractor.resourceType == requiredMaterial || requiredMaterial == TileData.Types.empty)  // <----- Storage units require an empty since they can take any type of material
                        {
                            extractor.SetOutput(this);

                            // Now set up the line renderer connection between the buildings
                            extractor.lineR.enabled = true;
                            extractor.lineR.SetWidth(0.05f, 0.5f);
                            extractor.lineR.SetPosition (0, (extractor.myTransform.position) + Vector3.up);
                            extractor.lineR.SetPosition(1, myTransform.position);

                            Debug.Log("Output has been set.");
                        }
                        else
                        {
                            // RESOURCE TYPE DOES NOT MATCH REQUIRED MATERIAL
                            Debug.Log("Output NOT set. Materials don't match!");
                        }
                     
                    }
                    else
                    {
                        Debug.Log("Output COULD NOT be set.");
                    }
                }
                

                isConnectingInput = false;

                yield break;
            }
            else
            {
                yield return null;
            }
        }
    }



    public void SetOutput(ExtractionBuilding receiver)
    {
        output = receiver;
        isConnectedToOutput = true;
    }

    public bool CheckOutputStorage()
    {
        // make sure it's not a null gameobject
        if (output != null)
        {
            // Make sure that my resource Type IS of the same type as my Output's required material
            if (resourceType == output.requiredMaterial || output.requiredMaterial == TileData.Types.empty)
            {
                int ammnt = currResourceStored + output.currMaterialsStored;


                if (ammnt <= output.extractorStats.secondStorageCapacity)
                {
                    OutputResources(currResourceStored);
                    return true;
                }
                else
                {
                    // check if there's at least SOME space in the output's second storage
                    int extra = ammnt - output.extractorStats.secondStorageCapacity;
                    int remainder = currResourceStored - extra;

                    if (remainder > 0)
                        OutputResources(remainder);

                    // We return false anyway because NOW the storage is completely full
                    return false;
                }
            }
            else
            {
                // NOT THE RIGHT TYPE OF MATERIAL OR OUTPUT IS NOT STORAGE UNIT
                isConnectedToOutput = false;
                return false;

            }

          
        }
        else
        {
            // NO OUTPUT FOUND
            isConnectedToOutput = false;
            return false;
        }
        
    }

    public void OutputResources(int ammntToOutput)
    {
        // Tell the output what it's receiving

        // Give to output
        output.ReceiveResources(ammntToOutput, resourceType);

        // Subtract from this building's storage
        currResourceStored -= ammntToOutput;

        // Only set storageIsfull to false, if there's no resources left in this storage
        if (currResourceStored <= 0)
            storageIsFull = false;

    }

    public void ReceiveResources(int ammnt, TileData.Types type)
    {
        // The type of resources being received
        inputType = type;
        // The ammount is stored in the Material storage (Storage units only have materials storage)
        currMaterialsStored += ammnt;

        if (callback != null)
            callback(type, ammnt);
    }

    // This Beams all the contents STORED inside of a building to the ship
    public void BeamAllStoredToShip()
    {
        Ship_Inventory.Instance.ReceiveItems(resourceType, currResourceStored);

        // This will split the current resources before sending them to ship (for example split between common ore and enriched ore)
        if (splitShipInventoryCallback != null)
        {
            splitShipInventoryCallback(currResourceStored);
        }

        currResourceStored = 0;
        if (storageIsFull)
            storageIsFull = false;
    }

    // This beams any Resource Drops the player picks up, to the ship
    public void PickUpAndBeamToShip(int total)
    {
        // This method on Ship Inventory Receives Items and stores them Temporarily. They will actually become part of the inventory once the Player
        // launches from the Transporter.
        Ship_Inventory.Instance.ReceiveItems(resourceType, total);

        // This will split the current resources before sending them to ship (for example split between common ore and enriched ore)
        if (splitShipInventoryCallback != null)
        {
            splitShipInventoryCallback(total);
        }


    }

    public void SpawnPickUp()
    {
        GameObject drop = ObjectPool.instance.GetObjectForType("Resource Drop", true, myTransform.position);
        if (drop != null)
        {
            drop.GetComponent<ResourceDrop>().InitSource(this, currResourceStored);

            currResourceStored = 0;
            if (storageIsFull)
                storageIsFull = false;
        }
    }

    // GRAB ALL THE CONTENTS of this building's storage using THIS method:
    public int GrabAllStoredResource()
    {
        int all = currResourceStored;
        currResourceStored = 0;
        storageIsFull = false;
        return all;
    }


}
