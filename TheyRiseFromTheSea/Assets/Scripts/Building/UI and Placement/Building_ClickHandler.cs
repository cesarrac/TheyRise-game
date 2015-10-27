using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Building_ClickHandler : MonoBehaviour {

	public int mapPosX;
	public int mapPosY;
	public Building_UIHandler buildingUIhandler;
	public ResourceGrid resourceGrid;

	// UI Handler feeds this when this is a new building so it may Swap Tiles
	[HideInInspector]
	public TileData.Types tileType, myTileType;

	// get the bounds of this collider to know where to place the options panel
	BoxCollider2D myCollider;
	float vertExtents;

	[SerializeField]
	private Canvas buildingCanvas;

	[SerializeField]
	private GameObject buildingPanel;


	public Building_StatusIndicator buildingStatusIndicator;

	// Adding this object Pool here so we can feed it to the buildings as they are built
	public ObjectPool objPool;

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

	public enum State { ASSEMBLING, READY, DISSASEMBLING, RECYCLE_NANOBOTS, CREATING }
	private State _state;
	public State state { get { return _state; } set { _state = value; } }

	NanoBuilding_Handler nano_builder; // this will allow the building to give back the nanobots when sold, getting it from resourceGrid
	int nanoBotsCreated = 0;
	int nanoBotsNeeded = 10; // CHANGE THIS TO COST !!

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


	}
	void Awake()
	{
		dissasemblyCountDown = dissasemblyTime;

		_state = State.ASSEMBLING;
		s_renderer = GetComponent<SpriteRenderer> ();
		s_renderer.color = B;
		FadeIn ();
	}

	void FadeIn()
	{
		s_renderer.color = Color.Lerp (B, A, colorTime);
		
		if (colorTime < 1){ 
			// increment colorTime it at the desired rate every update:
			colorTime += Time.deltaTime/colorChangeDuration;
		}

		if (s_renderer.color == A){
			colorTime = 0;
			_state = State.READY;
		}
	}

	void Start () {

		if (!resourceGrid) {
			resourceGrid = GameObject.FindGameObjectWithTag ("Map").GetComponent<ResourceGrid> ();
            if (myTileType != TileData.Types.capital)
			    nano_builder = resourceGrid.Hero.GetComponent<NanoBuilding_Handler> ();
		} else {
            if (myTileType != TileData.Types.capital)
                nano_builder = resourceGrid.Hero.GetComponent<NanoBuilding_Handler> ();
		}


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

					// IF THIS BUILDING is spawned by the UI Handler, it won't need to make this search
		if (buildingUIhandler == null) {
			buildingUIhandler = GameObject.FindGameObjectWithTag ("UI").GetComponent<Building_UIHandler> ();
		}

		myCollider = GetComponent<BoxCollider2D> ();
		vertExtents = myCollider.bounds.extents.y;

		// get my tiletype
		myTileType = CheckTileType ((int)transform.position.x,(int) transform.position.y);


	}

	void Update()
	{			
		MyStateMachine (_state);
	}

	void MyStateMachine(State _curState)
	{
		switch (_curState) {
		case State.ASSEMBLING:
			FadeIn();
			break;
		case State.READY:

			DissasemblyControl();

			if (Input.GetKeyDown (KeyCode.F) && playerIsNear) {
				if (!buildingUIhandler.currentlyBuilding){
					if (!buildingPanel.gameObject.activeSelf) {
						ActivateBuildingUI ();
						
					}else{
						ClosePanel();
					}
					
				}
			}
			break;
		case State.RECYCLE_NANOBOTS:
//			FadeOutControl();
			StartCoroutine(CreateNanoBotsEachSecond());
			break;

		case State.DISSASEMBLING:
			// While we are creating the dissasembled nanobots, fade out the building
			FadeOutControl();
			break;
		default:
			break;
		}
	}

	void DissasemblyControl()
	{
		// NOTE: Affecting ONLY the Battle towers
		if (myTileType == TileData.Types.machine_gun || myTileType == TileData.Types.sniper || 
		    myTileType == TileData.Types.cannons || myTileType == TileData.Types.seaWitch) {
			if (!isDissasembling){
				Dissasemble ();
			}
		} 

	}

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
//		if (!buildingUIhandler.currentlyBuilding)
//			buildingUIhandler.CreateOptionsButtons (offset, CheckTileType(mapPosX, mapPosY), mapPosX, mapPosY, buildingPanel, buildingCanvas);

		if (!buildingPanel.gameObject.activeSelf) {
			buildingPanel.gameObject.SetActive(true);
		}

	}

	public void ClosePanel(){
		if (buildingPanel.gameObject.activeSelf) {
			buildingPanel.gameObject.SetActive(false);
		}
	}

	public void Sell(){

		nanoBotsCreated = 0;

		if (resourceGrid != null) {
			resourceGrid.SwapTileType(mapPosX, mapPosY, TileData.Types.empty);
		}
	}


	void Dissasemble()
	{
		if (dissasemblyCountDown <= 0) {
			dissasemblyCountDown = dissasemblyTime;
			isDissasembling = true;
			isFading = true;
			// Start making the nanobots for disassembly
			_state = State.RECYCLE_NANOBOTS;

		} else {

			dissasemblyCountDown -= Time.deltaTime;
		}

	}

	// Once this building is dissasembled it will return the bots to the Hero
	void CreateNanoBot()
	{
		// TODO: Change the hardcoded value of nanobots to the building nanobot cost
		GameObject nanobot = objPool.GetObjectForType("NanoBot", true, transform.position);
		if (nanobot){
			nanobot.GetComponent<NanoBot_MoveHandler>().player = resourceGrid.Hero.transform;
			nanobot.GetComponent<NanoBot_MoveHandler>().objPool = objPool;
		}

		nanoBotsCreated++;

		if (nanoBotsCreated >= 10) {
			// After nanobots are created now Sell to swap the tile
			Sell ();
		} else {
			// keep making them
			_state = State.RECYCLE_NANOBOTS;
		}

	}

	IEnumerator CreateNanoBotsEachSecond()
	{
		_state = State.DISSASEMBLING;
		yield return new WaitForSeconds (0.1f);
		CreateNanoBot ();
	}

	TileData.Types CheckTileType(int x, int y){
		TileData.Types type = resourceGrid.tiles [x, y].tileType;
		return type;
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
		if (coll.gameObject.CompareTag ("Citizen")) {
			playerIsNear = true;
		}
	}
	
	void OnTriggerExit2D(Collider2D coll)
	{
		if (coll.gameObject.CompareTag ("Citizen")) {
			playerIsNear = false;
		}
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
