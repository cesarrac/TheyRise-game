using UnityEngine;
using System.Collections;

public class ExtractionBuilding : MonoBehaviour {
    
   

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

        // Constructor
        public ExtractorStats(float rate, int ammount, int personalStorageCap, int secondStorageCap = 0, int materialConsumed = 0)
        {
            _extractRate = rate;
            _extractAmmount = ammount;
            _personalStorageCap = personalStorageCap;
            _secondStorageCap = secondStorageCap;
            _materialsConsumed = materialConsumed;
        }


    }


    // VARS:
    public TileData.Types resourceType { get; protected set; }

    public ExtractorStats extractorStats { get; protected set; }

    public ResourceGrid resource_grid;

    public Building_StatusIndicator b_statusIndicator;

    public string statusMessage;

    public bool statusIndicated { get; protected set; }

    public int currResourceStored { get; protected set; }
    public int currMaterialsStored { get; protected set; } // < --- stored in secondary storage

    public bool isExtracting { get; protected set; }
    public bool storageIsFull { get; protected set; }
    public bool noMaterialsLeft { get; protected set; }

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

    // STATE MACHINE:
    public enum State
    {
        EXTRACTING,
        SEARCHING,
        NOSTORAGE,
        STARVED,
        PRODUCING,
        HALT
    }

    public State _state { get; protected set; }

    // INITIALIZERS:
    public void Init(TileData.Types r_type, float rate, int ammnt, int storageCap, Transform _trans)
    {
        extractorStats = new ExtractorStats(rate, ammnt, storageCap);
        resourceType = r_type;
        myTransform = _trans;

        resource_grid = ResourceGrid.Grid;
        b_statusIndicator = GetComponent<Building_ClickHandler>().buildingStatusIndicator;
    }

    // this Initializer works for buildings that extract from another building to produce their OWN resource
    public void InitSelfProducer(float rate, int ammnt, int storageCap, int secondStorageCap, int matConsumed, Transform _trans)
    {
        extractorStats = new ExtractorStats(rate, ammnt, storageCap, secondStorageCap, matConsumed);
        resourceType = TileData.Types.empty;
        myTransform = _trans;

        resource_grid = ResourceGrid.Grid;
        b_statusIndicator = GetComponent<Building_ClickHandler>().buildingStatusIndicator;
    }



    // METHODS:

    public bool CheckSearchForResource()
    {
        TileData resourceTile = SearchForResource();

        if (resourceTile  != null)
        {
            // Define my resource's tile position
            r_PosX = resourceTile.posX;
            r_PosY = resourceTile.posY;

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
       
        Vector3 top = myTransform.position + Vector3.up;
        Vector3 bottom = myTransform.position - Vector3.up;
        Vector3 left = myTransform.position + Vector3.left;
        Vector3 right = myTransform.position + Vector3.right;
        Vector3 topLeft = top + Vector3.left;
        Vector3 topRight = top + Vector3.right;
        Vector3 botLeft = bottom + Vector3.left;
        Vector3 botRight = bottom + Vector3.right;

        //rockTilesDetected.Clear();

        if (CheckTileType(top) != null)
        { // top

            //rockTilesDetected.Add(CheckTileType(top));
            return CheckTileType(top);
        }
        else if (CheckTileType(bottom) != null)
        { // bottom

            //rockTilesDetected.Add(CheckTileType(bottom));
            return CheckTileType(bottom);
        }
        else if (CheckTileType(left) != null)
        { // left
            //rockTilesDetected.Add(CheckTileType(left));
            return CheckTileType(left);
        }
        else if (CheckTileType(right) != null)
        { //right
            //rockTilesDetected.Add(CheckTileType(right));
            return CheckTileType(right);
        }
        else if (CheckTileType(topLeft) != null)
        { // top left
            //rockTilesDetected.Add(CheckTileType(topLeft));
            return CheckTileType(topLeft);
        }
        else if (CheckTileType(topRight) != null)
        { // top right
          // rockTilesDetected.Add(CheckTileType(topRight));
            return CheckTileType(topRight);
        }
        else if (CheckTileType(botLeft) != null)
        { // bottom left
            //rockTilesDetected.Add(CheckTileType(botLeft));
            return CheckTileType(botLeft);
        }
        else if (CheckTileType(botRight) != null)
        { // bottom right
          // rockTilesDetected.Add(CheckTileType(botRight));
            return CheckTileType(botRight);
        }
        else
        {
            return null;
        }

    }

    TileData CheckTileType(Vector3 position)
    {
        if (resource_grid.TileFromWorldPoint(position).tileType == resourceType)
            return resource_grid.TileFromWorldPoint(position);
        else
            return null;
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

    public IEnumerator ExtractResource()
    {
        // while true, extract, wait extract rate, check if there's ore left: if no break out and  change state to searching
        while (true)
        {

            yield return new WaitForSeconds(extractorStats.extractRate);

            if (!productionHalt)
            {
                // int extract = resource_grid.ExtractFromTile(r_PosX, r_PosY, extractorStats.extractAmmount);  
                int extract = currResourceStored + extractorStats.extractAmmount;

                if (extract <= extractorStats.personalStorageCapacity)
                {
                    // Extract from the Tile using Resource Grid's method ExtractFromTile
                    currResourceStored += resource_grid.ExtractFromTile(r_PosX, r_PosY, extractorStats.extractAmmount);
                    // Debug.Log("Extracting. Current ore stored = " + currResourceStored);

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
                    int extra = extract - extractorStats.personalStorageCapacity;
                    int remainder = extract - extra;
                    if (remainder > 0)
                        currResourceStored += remainder;

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
        MouseBuilding_Controller mouse_control = MouseBuilding_Controller.MouseController;

        while (true)
        {
          if (Input.GetMouseButtonDown(1))
            {
                GameObject input = mouse_control.GetTileGameObj();
                if (input != null)
                {
                    if (input.GetComponent<ExtractionBuilding>() != null)
                    {
                        input.GetComponent<ExtractionBuilding>().SetOutput(this);
                        Debug.Log("Output has been set.");
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
                int remainder = ammnt - extra;

                if (remainder > 0)
                {
                    OutputResources(remainder);

                    return true;
                }
                else
                    return false;
            }
        }
        else
        {
            isConnectedToOutput = false;
            return false;
        }
        
    }

    public void OutputResources(int ammntToOutput)
    {
        // Give to output
        output.currMaterialsStored += ammntToOutput;

        // Subtract from this building's storage
        currResourceStored -= ammntToOutput;

        // Only set storageIsfull to false, if there's no resources left in this storage
        if (currResourceStored <= 0)
            storageIsFull = false;

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
