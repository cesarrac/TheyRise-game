using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Building_Handler : MonoBehaviour {

	public int mapPosX;
	public int mapPosY;
	Build_MainController buildMainController;
	ResourceGrid resourceGrid;

	// UI Handler feeds this when this is a new building so it may Swap Tiles
	[HideInInspector]
	public TileData.Types tileType, myTileType;

    // Set this to my vertical Sprite size to calculate in what position to render a GUI menu on top of this building
	float vertExtents;

	[SerializeField]
	private Canvas buildingCanvas;

	[SerializeField]
	private GameObject buildingPanel;


	public Building_StatusIndicator buildingStatusIndicator;

	// Adding this object Pool here so we can feed it to the buildings as they are built
	ObjectPool objPool;

	[Header ("For Gun Towers Only:")]
	public Tower_TargettingHandler tower;

	// Storing the building's energy cost here to access it from other scripts
	public int energyCost {  get; private set; }

	public float dissasemblyTime = 20f; // seconds
	private float dissasemblyCountDown;
	private bool isDissasembling = false, isFading = false;
	SpriteRenderer s_renderer;

	Color A = Color.white;
	Color B = Color.clear;
	public float colorChangeDuration = 2;
	private float colorTime;

	private bool playerIsNear = false;// Only turns true if the player walks up to the building

	public enum State { ASSEMBLING, READY, DISSASEMBLING, RECYCLE_NANOBOTS, CREATING, DISSASEMBLED}
	private State _state;
	public State state { get { return _state; } set { _state = value; } }
    public State debugState;

	NanoBuilding_Handler nano_builder; // this will allow the building to give back the nanobots when sold, getting it from resourceGrid
    public int nanoBotCost;
	int nanoBotsCreated = 0;
	int nanoBotsNeeded = 10; // CHANGE THIS TO COST !!

    TileData myTile;

    Transform playerTransform; // < --------------- can get it when player collides with this building. if not it can just find it.


    void OnEnable()
	{
		// Make sure to reset the color
		s_renderer = GetComponent<SpriteRenderer> ();

		FadeIn ();
		// reset timer variables
		isDissasembling = false;
		isFading = false;

		// Assemble
		_state = State.ASSEMBLING;

        // set cost
        //nanoBotsNeeded = nanoBotCost / 10;
        nanoBotsCreated = 0;

    }
	void Awake()
	{
		dissasemblyCountDown = dissasemblyTime;

		_state = State.ASSEMBLING;
		s_renderer = GetComponent<SpriteRenderer> ();
		s_renderer.color = B;
		FadeIn ();

        buildMainController = Build_MainController.Instance;
        objPool = ObjectPool.instance;
        resourceGrid = ResourceGrid.Grid;
    }

	void FadeIn()
	{
		s_renderer.color = Color.Lerp (B, A, colorTime);
		
		if (colorTime < 1){ 
			// increment colorTime it at the desired rate every update:
			colorTime += Time.deltaTime/colorChangeDuration;
		}

        // FINISHED BUILDING:
		if (s_renderer.color == A){
			colorTime = 0;

			_state = State.READY;

            Sound_Manager.Instance.PlaySound("Build Finish");
		}
	}

	void Start () {

        // get my tiletype
        Debug.Log("CLICK HANDLER: pos = " + transform.position);
        myTileType = resourceGrid.TileFromWorldPoint(transform.position).tileType;

        // Register as Built for the Enemy Master to know
        resourceGrid.AddTowerBuiltForEnemyMaster(transform);


        if (myTileType != TileData.Types.capital)
            nano_builder = resourceGrid.Hero.GetComponent<NanoBuilding_Handler>();


        if (buildingCanvas == null) {
			Debug.Log("CLICK HANDLER: Building Canvas not set!");
		} 
		if (buildingPanel == null) {
			Debug.Log("CLICK HANDLER: Building Panel not set!");
		}

		if (buildingStatusIndicator == null) {
			Debug.Log("CLICK HANDLER: Building Status Indicator not set!");
		}

		if (buildingCanvas != null) {
			buildingCanvas.worldCamera = Camera.main;
		}
        
  

        vertExtents = GetComponent<SpriteRenderer>().sprite.bounds.size.y;


        //if (resourceGrid.worldGridInitialized)
        //{
        //    myTile = resourceGrid.TileFromWorldPoint(transform.position);
        //}

    }

	void Update()
	{
        MyStateMachine(_state);
        debugState = _state;
    }

	void MyStateMachine(State _curState)
	{
		switch (_curState) {
		case State.ASSEMBLING:
			FadeIn();
			break;
		case State.READY:

                if (myTileType != TileData.Types.terraformer)
                {
                    if (Input.GetButtonDown("Interact") && playerIsNear)
                    {
                        if (!buildMainController.currentlyBuilding)
                        {
                            if (buildingPanel != null && !buildingPanel.gameObject.activeSelf)
                            {
                                ActivateBuildingUI();

                            }
                            else
                            {
                                ClosePanel();
                            }

                        }
                    }
                }

                if (Input.GetButtonDown("Beam") && playerIsNear)
                {
                    if (GetComponent<ExtractionBuilding>() != null)
                    {
                        GetComponent<ExtractionBuilding>().BeamAllStoredToShip(); // <------------ BEAMS ALL CONTENTS (if any) STORED ON THIS BUILDING TO THE SHIP
                    }
                }

			break;
		case State.RECYCLE_NANOBOTS:
//			FadeOutControl();
			StartCoroutine("CreateNanoBotsEachSecond");
            state = State.DISSASEMBLING;
			break;

		case State.DISSASEMBLING:
			// While we are creating the dissasembled nanobots, fade out the building
			FadeOutControl();
			break;
		default:
			break;
		}
	}

	//void DissasemblyControl()
	//{
	//	// NOTE: Affecting ONLY the Battle towers
	//	if (myTileType == TileData.Types.machine_gun || myTileType == TileData.Types.sniper || 
	//	    myTileType == TileData.Types.cannons || myTileType == TileData.Types.seaWitch) {
	//		if (!isDissasembling){
	//			Dissasemble ();
	//		}
	//	} 

	//}

	void FadeOutControl()
	{
		if (isFading) {
			s_renderer.color = Color.Lerp(A, B, colorTime);
			
			if (colorTime < 1){ 
				// increment colorTime it at the desired rate every update:
				colorTime += Time.deltaTime/colorChangeDuration;
			}
			
			if (s_renderer.color == B){
				colorTime = 0;
				isFading = false;
				isDissasembling = false;

			}
		}
	}

	public void ActivateBuildingUI(){
		Vector3 offset = new Vector3 (transform.position.x, transform.position.y + vertExtents);

		if (!buildingPanel.gameObject.activeSelf) {
			buildingPanel.gameObject.SetActive(true);
		}

	}

	public void ClosePanel(){
		if (buildingPanel.gameObject.activeSelf) {
			buildingPanel.gameObject.SetActive(false);
		}
	}

	public void SwapBuildingTile()
    {
        // Define the tile again in case this was a pooled object and for some reason it didn't re-define its tile. This might make it unnecessary to do it on start.
        myTile = ResourceGrid.Grid.TileFromWorldPoint(transform.position);
        Debug.Log("My tile pos X " + myTile.posX + " pos Y " + myTile.posY);
        ResourceGrid.Grid.SwapTileType(myTile.posX, myTile.posY, TileData.Types.empty, "Empty", myTile.tileStats.NanoBotCost);

        Sound_Manager.Instance.PlaySound("Build Break");

        // Pool any circle selections this building has, only if it IS an extraction building
        if (GetComponent<ExtractionBuilding>() != null)
        {
            if (GetComponent<ExtractionBuilding>().circleSelection != null)
            {
                ObjectPool.instance.PoolObject(GetComponent<ExtractionBuilding>().circleSelection);
            }
        }

    }


	//void Dissasemble()
	//{
	//	if (dissasemblyCountDown <= 0) {
	//		dissasemblyCountDown = dissasemblyTime;
	//		isDissasembling = true;
	//		isFading = true;
	//		// Start making the nanobots for disassembly
	//		_state = State.RECYCLE_NANOBOTS;

	//	} else {

	//		dissasemblyCountDown -= Time.deltaTime;
	//	}

	//}

	// Once this building is dissasembled it will return the bots to the Hero
	void CreateNanoBot()
	{
		// TODO: Change the hardcoded value of nanobots to the building nanobot cost
		GameObject nanobot = objPool.GetObjectForType("NanoBot", true, transform.position);
		if (nanobot){
            if (playerTransform != null)
                nanobot.GetComponent<NanoBot_MoveHandler>().player = playerTransform;
            else
                nanobot.GetComponent<NanoBot_MoveHandler>().player = resourceGrid.Hero.transform;
        }

		//nanoBotsCreated++;

		//if (nanoBotsCreated >= 10) {
		//	// After nanobots are created now Sell to swap the tile
		//	Sell ();
		//} else {
		//	// keep making them
		//	_state = State.RECYCLE_NANOBOTS;
		//}

	}

	IEnumerator CreateNanoBotsEachSecond()
	{
	

        while(true)
        {
            CreateNanoBot();
            nanoBotsCreated++;
            yield return new WaitForSeconds(0.1f);

            if (nanoBotsCreated >= 10)
            {
                nanoBotsCreated = 0;
                SwapBuildingTile();
                yield break;
            }

        }
	}



	public void ChangeBuildingStatus(string change){

		switch (change) {
		case "Starve":

			// starve
			buildingStatusIndicator.CreateStatusMessage("Power Down!");

			break;
		case "Unstarve":

			// unstarve
			buildingStatusIndicator.CreateStatusMessage("Powering Up!");

			break;
		case "Reload":
			// show reloading message
			buildingStatusIndicator.CreateStatusMessage("Reloading...");
			break;
		case "Acquired":
			// show target acquired
			buildingStatusIndicator.CreateStatusMessage("Target acquired!");
			break;
		case "Siege":
			// under siege
			buildingStatusIndicator.CreateStatusMessage("Under Attack!");
			break;
		default:
			// building name initialized
			break;
		}

	}


	void OnTriggerEnter2D(Collider2D coll)
	{
		if (coll.gameObject.CompareTag ("Citizen") && !playerIsNear) {
           
            playerIsNear = true;

            if (playerTransform == null)
                playerTransform = coll.transform;
		}
	}
	
	void OnTriggerExit2D(Collider2D coll)
	{
		if (coll.gameObject.CompareTag ("Citizen") && playerIsNear) {
			playerIsNear = false;
		}
	}


    public void BreakBuilding(int _nanoBotCost)
    {
        nanoBotCost = _nanoBotCost;
        // nanoBotsNeeded = nanoBotCost / 10;

        if (_state == State.READY)
        {
            if (!isDissasembling)
            {
                isDissasembling = true;
                isFading = true;
                _state = State.RECYCLE_NANOBOTS;

            }
        }
        //Sell();
    }


//	void OnMouseOver()
//	{
//
//		// Turn ON Manual Control for Gun Towers:
//		if (tower != null) {
//			if (tower.state != Tower_TargettingHandler.State.MANUAL_CONTROL && 
//			    tower.state != Tower_TargettingHandler.State.MANUAL_SHOOTING && 
//			    tower.state != Tower_TargettingHandler.State.STARVED){
//
//				if (Input.GetMouseButtonDown(1)){
//
//					tower.state = Tower_TargettingHandler.State.MANUAL_CONTROL;
//
//					// Also turn off the Building Menus so they don't get in the way
//					buildingUIhandler.currentlyBuilding = true;
//
//				}
//
//			}else if (tower.state == Tower_TargettingHandler.State.MANUAL_CONTROL || 
//			          tower.state == Tower_TargettingHandler.State.MANUAL_SHOOTING){
//				
//				if (Input.GetMouseButtonDown(1)){
//					
//					tower.state = Tower_TargettingHandler.State.SEEKING;
//					
//					// Turn Building Menus back on
//					buildingUIhandler.currentlyBuilding = false;
//					
//				}
//			}
//
//		}
//
//	}
	
//	void OnMouseExit()
//	{
//		// Turn OFF Manual Control
//		if (tower.state == Tower_TargettingHandler.State.MANUAL_CONTROL || 
//		    tower.state == Tower_TargettingHandler.State.MANUAL_SHOOTING){
//			
//			if (Input.GetMouseButtonDown(1)){
//				
//				tower.state = Tower_TargettingHandler.State.SEEKING;
//				
//				// Turn Building Menus back on
//				buildingUIhandler.currentlyBuilding = false;
//				
//			}
//		}
//	}
}
