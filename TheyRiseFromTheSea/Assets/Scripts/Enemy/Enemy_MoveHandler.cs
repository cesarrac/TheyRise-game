﻿using UnityEngine;
using System.Collections.Generic;

public class Enemy_MoveHandler : MonoBehaviour {

	[System.Serializable]
	public class MovementStats {
		public float startMoveSpeed, startChaseSpeed;

		private float _moveSpeed, _chaseSpeed;

		public float curMoveSpeed{ get { return _moveSpeed; } set { _moveSpeed = Mathf.Clamp(value, 0, startMoveSpeed); } }
		public float curChaseSpeed{ get { return _chaseSpeed; } set { _chaseSpeed = Mathf.Clamp(value, 0, startChaseSpeed); } }

		public void InitMoveStats(){
			curMoveSpeed = startMoveSpeed;
			curChaseSpeed = startChaseSpeed;
		}
	}

	public MovementStats mStats = new MovementStats();

	public ResourceGrid resourceGrid;
	
	public int posX, targetPosX;
	public int posY, targetPosY;
	
	// This stores this unit's path
	public List<Node>currentPath = null;
	
	private Vector3 velocity = Vector3.zero;

	public bool isAttacking = false; // this turns true when this object enters a player unit's collider OR a blocking tile

	public Enemy_AttackHandler enemyAttkHandler;

//	public float movementSpeed = 1.0F;



	public int spwnPtIndex;
	public SpawnPoint_Handler spwnPtHandler;

	public float stoppingDistance;
	CircleCollider2D collider;
	public Vector3 destination;


	public bool movingBackToPath = false, movingToFormation = false;
	Vector2 formationPos;

	// THE BUDDY SYSTEM: each individual spawned enemy will know the Move Handler of the enemy before them (given to them by the SpawnHandler)
	public Enemy_MoveHandler myBuddy;

	public Animator anim;

	private Vector2 _capitalPosition;

	public bool unitInitialized { get; private set;} 

	public enum State { IDLING, MOVING, MOVING_BACK, DISPERSING, AVOIDING, ATTACKING, FROZEN, FOLLOW_PLAYER, ATTACKING_PLAYER };

	private State _state = State.IDLING;

	[HideInInspector]
	public State state { get { return _state; } set { _state = value; }}

	private Vector2 lastKnownNode, myLastPosition, disperseDirection = Vector2.zero, avoidDirection = Vector2.zero;

	public bool isKamikaze;

	[SerializeField]
	private bool isAvoider; // if unit is an Avoider, they won't attack tiles, just go around them

	public State debugState;

	[HideInInspector]
	public float frozenTime;

	[HideInInspector]
	public GameObject targetPlayer;
	private bool followingPlayer;
	public bool isPlayerAttacker;

	Rigidbody2D rb;

	void Start () 
	{
		rb = GetComponent<Rigidbody2D> ();

		// Initialize Movement Speed stat
		mStats.InitMoveStats ();

		// Get the Animator
		if (anim == null) {
			anim = GetComponentInChildren<Animator>();
		}

		// Store initial position for Grid as an int
		posX = (int)transform.position.x;
		posY = (int)transform.position.y;
		Debug.Log ("X=" + posX + "Y=" + posY);
		// Store the Attack Handler to interact with its state
		enemyAttkHandler = GetComponent<Enemy_AttackHandler> ();

		// Get First Path, pass in True argument only if this is a Kamikaze unit
		if (!isKamikaze) {

			GetFirstPath (false);

		} else {

			GetFirstPath(true);
		}

		// Store the Capital position from the resource grid
		if (resourceGrid != null)
			_capitalPosition = new Vector2 (resourceGrid.capitalSpawnX, resourceGrid.capitalSpawnY);

		// This unit has been initialized (meaning it's already been spawned once)
		// In order to know which units already spawned from pool and need to reset stats
		unitInitialized = true;
	}


	// Called by scripts spawning this unit to make sure it gets the right path
	public void InitPath()
	{
		if (!isKamikaze) {
			GetFirstPath (false);
		} else {
			GetFirstPath(true);
		}
	}


	void GetFirstPath(bool isKamikaze)
	{
		// Just in case this unit had a path already (it's a recycled unit) make it null
		if (currentPath != null)
			currentPath.Clear ();

		// Get the path from Spawn Point Handler
		if (spwnPtHandler != null) {

			// Regular Units path:
			if (!isKamikaze){

				// Initialize Current Path
				currentPath = new List<Node>();

				// Loop through each Node of the corresponding path and Add Node to Current Path
				for (int x = 0; x < spwnPtHandler.paths[spwnPtIndex].Count; x++){
					currentPath.Add(spwnPtHandler.paths[spwnPtIndex][x]);

					// When the Loop reaches the last Node store the value as a Vector3 destination
					if (x == spwnPtHandler.paths[spwnPtIndex].Count - 1){

						destination = new Vector3( currentPath[x].x, currentPath[x].y, 0.0f);
					}
				}

				// Now that all Nodes in Current Path have been set, start moving
				_state = State.MOVING;

			}else{// Kamikaze Path:

				// Init Current Path
				currentPath = new List<Node>();

				// Loop through each Node in Kamikaze path and add Nodes to Current Path
				for (int x = 0; x < spwnPtHandler.kamikazePaths[spwnPtIndex].Count; x++){
					currentPath.Add(spwnPtHandler.kamikazePaths[spwnPtIndex][x]);

					// As above, last Node is stored in Vector3 destination
					if (x == spwnPtHandler.kamikazePaths[spwnPtIndex].Count - 1){
						destination = new Vector3( currentPath[x].x, currentPath[x].y, 0.0f);
					}
				}

				// All Nodes in Current Path are set, start moving
//				
				// Change state:
				_state = State.MOVING;
			}
		
		}
	}

	Vector2 Disperse(Vector2 obstaclePos)
	{
		Vector2 disperseDirection = Vector2.zero;

		// First check if Obstacle is on the same Y coordinate as this unit
		if (obstaclePos.y == transform.position.y) {
			// If it IS the same Y, disperse position will be to ABOVE or BELOW the obstacle

			// Randomly decide wether to move to tile above or below it
			int decision = Random.Range (0, 3);

			// Then apply the decision to the direction to return
			if (decision == 1) {
				
				// move above
				disperseDirection = new Vector2 (obstaclePos.x, obstaclePos.y + 1);
				
			} else {
				// move below
				disperseDirection = new Vector2 (obstaclePos.x, obstaclePos.y - 1);
			}
		}

		// IF NOT the same Y, then check if Obstacle is on the same X coordinate
		else if (obstaclePos.x == transform.position.x) {
		
			// Decide to move to the tile right or left of it
			int decision2 = Random.Range (0, 3);
			
			// Then apply the decision to the direction to return
			if (decision2 == 1) {
				// move up & left
				disperseDirection = new Vector2 (obstaclePos.x + 1, obstaclePos.y);
			} else {
				// move up & right right
				disperseDirection = new Vector2 (obstaclePos.x - 1, obstaclePos.y);
			}
			
		} else {
			// Obstacle is neither the same Y nor X coordinate as this unit, it must be in a diagonal position relative to this unit
				
			//Check if it's to the LEFT or RIGHT

				
			if (obstaclePos.x > transform.position.x){
				// If it's to the RIGHT then disperse to the tile immediately to the LEFT of the obstacle
				disperseDirection = new Vector2(obstaclePos.x - 1, obstaclePos.y);

			}else if (obstaclePos.x < transform.position.x){
				// If it's to the LEFT, disperse to its RIGHT
				disperseDirection = new Vector2(obstaclePos.x + 1, obstaclePos.y);
			}
		}

		return disperseDirection;
	}

	Vector2 Avoid(Vector2 myLastPos, Vector2 obstaclePos)
	{
		Vector2 avoidDirection = Vector2.zero;

		// Was the obstacle on same Y coordinate as this unit's last position?
		if (obstaclePos.y == myLastPos.y) {

			// Was it to this unit's LEFT or RIGHT?
			if (obstaclePos.x > myLastPos.x) {

				// Record the avoid tile direction as two tiles to the RIGHT of last Position
				avoidDirection = new Vector2 (myLastPos.x + 2, myLastPos.y);

			} else if (obstaclePos.x < myLastPos.x) {

				// Record the avoid tile direction as two tiles to the LEFT of last Position
				avoidDirection = new Vector2 (myLastPos.x - 2, myLastPos.y);
			}
		} 
		// Was it on the same X coordinate?
		else if (obstaclePos.x == myLastPos.x) {

			// Was it ABOVE or BELOW this unit?
			if (obstaclePos.y > myLastPos.y) {

				// Record the avoid tile direction as two tiles ABOVE last Position
				avoidDirection = new Vector2 (myLastPos.x, myLastPos.y + 2);
			} else if (obstaclePos.y < myLastPos.y) {

				// Record the avoid tile direction as two tiles BELOW last Position
				avoidDirection = new Vector2 (myLastPos.x, myLastPos.y - 2);
			}
		} 
		// The obstacle is not on the same Y or X coordinate as this unit, must be diagonal 
		else {

			// Check if obstacle was ABOVE or BELOW
			if (obstaclePos.y > myLastPos.y){


				// IF obstacle was to this unit's RIGHT or LEFT and ABOVE it dispersed to the obstacle's LEFT or RIGHT
				// so Avoid by moving to tile above obstacle
				avoidDirection = new Vector2(obstaclePos.x, obstaclePos.y + 1);
				
				
			}else if (obstaclePos.y < myLastPos.y){
				// Check if obstacle was to the LEFT or RIGHT of this unit's last position

				// IF obstacle was to this unit's BELOW, it dispersed to the obstacle's LEFT or RIGHT
				// so Avoid by moving to tile below
				avoidDirection = new Vector2(obstaclePos.x, obstaclePos.y - 1);
					

			}
		
		}
		
		
		return avoidDirection;
	}


	// Move Back to Path after dispersing
	void MoveBackToPath(Vector2 _nodePos)
	{
		// Alter the speed slightly as they move back to keep them from bunching up
		float randomSpeed = Random.Range (0.1f, mStats.startMoveSpeed);
		mStats.curMoveSpeed = randomSpeed;

		if (Vector2.Distance (transform.position, _nodePos) == 0) {
			GetPath ();
		} else {
			
			transform.position = Vector2.MoveTowards(transform.position, _nodePos ,mStats.curMoveSpeed * Time.deltaTime);
		}
	}

	// To find path after moving away from it
	public void GetPath()
	{
		posX = (int)transform.position.x;
		posY = (int)transform.position.y;

		int myPosIndexInPath = 0;

		if (spwnPtHandler != null) {

			currentPath = new List<Node>();
			for (int x = 0; x < spwnPtHandler.paths[spwnPtIndex].Count; x++){
				// find my position on the path
				if(spwnPtHandler.paths[spwnPtIndex][x].x == posX && spwnPtHandler.paths[spwnPtIndex][x].y == posY){
					Debug.Log("Found my node at pathtoCapital" + x);
					myPosIndexInPath = x;
					break;
				}
			}
			if (myPosIndexInPath > 0){ // At this point Index should never be 0 if we found our position in the path
				for (int i = myPosIndexInPath; i < spwnPtHandler.paths[spwnPtIndex].Count; i++){
					currentPath.Add(spwnPtHandler.paths[spwnPtIndex][i]);
				}

				// Change speed back to original speed
				mStats.curMoveSpeed = mStats.startMoveSpeed;

				// Start moving on path
				_state = State.MOVING;



			}else{
				_state = State.IDLING;
				Debug.Log("Couldn't find my node :(");
			}


		}
	}
	


	void Update () {

		if (isPlayerAttacker && targetPlayer != null && !followingPlayer) {
			_state = State.FOLLOW_PLAYER;
		}

		DrawLine ();
	
		MyStateMachine (_state);
	
		debugState = _state;

		// NOTE: Turning ON the buddy system forces the entire wave to stop when the "leader" is blocked by a building. This makes
		// it look quite static and robotic. The other alternative is NOT activating it, which causes all the units to move up to
		// the building. This makes them all pile together and when they destroy the building they move in a jumbled mess.

		// NOTE: Disperse works! Now they pile together when they go back to their path though
		// TODO: Consider altering their speed slightly while they are moving back to path, that should cause them to NOT look like a mess

//		if (myBuddy != null) {
//			// NOTE: Buddy System is only called by units who were not the first to spawn in their Wave
//			// Buddy System links units of the same wave to allow them to attack a Tile or Unit in unison
//			BuddySystem();
//		}


	}

	void MyStateMachine(State _curState)
	{
		switch (_curState) {
		case State.IDLING:
			// just spawned, not moving & not attacking

			break;
		case State.MOVING:
			// Move
			ActualMove();
			break;

		case State.ATTACKING:
			// Attack
			// While they are attacking, units disperse around the tile under attack
			if (disperseDirection != Vector2.zero)
				transform.position = Vector2.MoveTowards(transform.position, disperseDirection, mStats.curMoveSpeed * Time.deltaTime);

			break;

		case State.MOVING_BACK:
			// Move Back to Path, once found this will call Moving state again

			MoveBackToPath(lastKnownNode);
			

			break;

		case State.DISPERSING:
			// Disperse, then once on the disperse tile move to avoid direction
			if (disperseDirection != Vector2.zero){
				if (Vector2.Distance(transform.position, disperseDirection) > 0){
					transform.position = Vector2.MoveTowards(transform.position, disperseDirection, mStats.curMoveSpeed * Time.deltaTime);
				}else{
					// Once on the disperse tile, state is avoiding
					_state = State.AVOIDING;
				}
			}
			break;
		case State.AVOIDING:
			// Once on the avoid tile, Move Back to Path
			if (avoidDirection != Vector2.zero){
				if (Vector2.Distance(transform.position, avoidDirection) > 0){
					transform.position = Vector2.MoveTowards(transform.position, avoidDirection, mStats.curMoveSpeed * Time.deltaTime);
				}else{
					posX = Mathf.RoundToInt(transform.position.x);
					posY = Mathf.RoundToInt(transform.position.y);
					_state = State.MOVING;
				}
			}
			break;

		case State.FROZEN:
			/* If the Player freezes this unit it can't move for an ammount of time = to the frozenTime variable
			 * If this Unit has a Buddy, if it's close than half a tile freeze it. 
			 To freeze it, change its state to frozen and make its frozentime = 5 seconds*/
			if (frozenTime <= 0){

				if (!followingPlayer){
					// go back to moving
					_state = State.MOVING;
				}else{
					// go back to following the player target
					_state = State.FOLLOW_PLAYER;
				}
			}else{
				frozenTime -= Time.deltaTime;
				/* This makes the Freeze effect spread to this Unit's buddy
				if (myBuddy){
					if (Vector2.Distance(transform.position, new Vector2(myBuddy.transform.position.x, myBuddy.transform.position.y))
					    <= 0.5f){
						myBuddy.frozenTime = 5;
						myBuddy.state = State.FROZEN;
					}
				}
				*/
			}
			break;
		case State.FOLLOW_PLAYER:
			// Stop moving on path, check if target Player is assigned and start following
			if (!followingPlayer)
				followingPlayer = true;
			_state = State.ATTACKING_PLAYER;

			break;
		case State.ATTACKING_PLAYER:
			// Continue to follow and attack on contact

			FollowPlayer(targetPlayer);
			Debug.Log("ENEMY_MOVE: Following Player!");
			
			break;
		default:
			_state = State.IDLING;
			break;
		}

	}

	void FollowPlayer(GameObject target)
	{
//		transform.position = Vector3.Lerp (transform.position, new Vector3 (target.position.x - 1, target.position.y, 0.0f), 
//		                                   mStats.curMoveSpeed * Time.deltaTime);
		transform.position = Vector2.MoveTowards (transform.position, target.transform.position, mStats.curChaseSpeed * Time.deltaTime);
		// Moving using Rigidbody 2D
//		Vector2 targetPos = new Vector2 (target.transform.position.x, target.transform.position.y);
//		rb.MovePosition(rb.position + targetPos * mStats.curMoveSpeed * Time.deltaTime);
	}


	// DEBUG NOTE: Using this to draw line to show path. Only works in Preview mode
	void DrawLine()
	{
		// Debug Draw line for visual reference
		if (currentPath != null) {
			int currNode = 0;
			while (currNode < currentPath.Count -1) {
				Vector3 start = resourceGrid.TileCoordToWorldCoord (currentPath [currNode].x, currentPath [currNode].y);
				Vector3 end = resourceGrid.TileCoordToWorldCoord (currentPath [currNode + 1].x, currentPath [currNode + 1].y);
				;
				Debug.DrawLine (start, end, Color.blue);
				currNode++;
			}
		} 
	}

	// Physically moves the unit through world space
	void ActualMove()
	{
		// Movement:

		// Have we moved close enough to the target tile that we can move to next tile in current path?
		if (Vector2.Distance (transform.position, resourceGrid.TileCoordToWorldCoord (posX, posY)) < (0.1f)) {
			MoveToNextTile ();
		}
		transform.position = Vector2.MoveTowards(transform.position, 
		                                         resourceGrid.TileCoordToWorldCoord (posX, posY), 
		                                         mStats.curMoveSpeed * Time.deltaTime);
		// ANIMATION CONTROLS:
		if (posX > transform.position.x){
			anim.SetTrigger ("movingRight");
			anim.ResetTrigger("movingLeft");
		}else if (posX < transform.position.x){
			anim.SetTrigger ("movingLeft");
			anim.ResetTrigger("movingRight");
		}
		
	}

	// Move through Path:
	void MoveToNextTile(){
		if (currentPath == null) {
			return;
		}
		// Remove the old first node from the path
		currentPath.RemoveAt (0);
		
		// Check if the next tile is a UNWAKABLE tile OR if it is clear path
		if (resourceGrid.UnitCanEnterTile (currentPath [1].x, currentPath [1].y) == false) {

			// Since Path is blocked set the state to Idling until this unit knows if it must attack
			_state = State.IDLING;

			if (CheckForTileAttack (currentPath [1].x, currentPath [1].y)) {

				// Start its attack on the tile:

				// If it's the Destination tile, not just any tile, then it needs to do a special attack
				if (currentPath[1].x == destination.x && currentPath[1].y == destination.y)
				{
					// we are at the destination! Do special!
					enemyAttkHandler.SpecialAttack(currentPath[1].x, currentPath[1].y);
				}
				else
				{
					// Check if this unit is NOT an Avoider 
					if (!isAvoider){

						// the Target Tile is not our destination, so do Normal Attack
						targetPosX = currentPath [1].x;
						targetPosY = currentPath [1].y;
						enemyAttkHandler.targetTilePosX = currentPath [1].x;
						enemyAttkHandler.targetTilePosY = currentPath [1].y;
						enemyAttkHandler.resourceGrid = resourceGrid;

						// Change attack handler state to Attacking Tile
						enemyAttkHandler.state = Enemy_AttackHandler.State.ATTACK_TILE;

						// Change my state to Attack to stop movement
						_state = State.ATTACKING;

						// Record the Node location that contains the obstacle
						lastKnownNode = new Vector2(currentPath[1].x, currentPath[1].y);

						// Record the direction Units must use to disperse
						disperseDirection = Disperse(lastKnownNode);
					}else{
						// This IS an Avoider, instead of attacking they move around the tile

						// Record the Node location that contains the obstacle
						lastKnownNode = new Vector2(currentPath[1].x, currentPath[1].y);
						
						// Record the direction Units must use to disperse
						disperseDirection = Disperse(lastKnownNode);

						// Avoiders need to record the position they are currently on to decide which direction to continue on
						myLastPosition = new Vector2(transform.position.x, transform.position.y);

						// Record the avoid direction
						avoidDirection = Avoid(myLastPosition, lastKnownNode);

						// Change state to Disperse first, that in turn will change to Avoiding
						_state = State.DISPERSING;

					}

				}
			
			} 

		} else {
			if (_state == State.ATTACKING){ // at this point if this is true it means this unit is engaging a Player Unit
				currentPath = null;
				return;
			}

			// this check if for KAMIKAZE UNITS ONLY
			if (isKamikaze){
				// if the next tile on the Path is the destination
				if (currentPath[1].x == destination.x && currentPath[1].y == destination.y){
					// reached the destination, do Special Attack!
					enemyAttkHandler.SpecialAttack(currentPath[1].x, currentPath[1].y);
				}
			}
		} 	
		// Move to the next Node position in path
		posX = currentPath [1].x;
		posY = currentPath [1].y;

		// We are on the tile that is our DESTINATION, 
		// CLEAR PATH
		if (currentPath.Count == 1) {
			currentPath = null;
		}
	}

	bool CheckForTileAttack(int x, int y){
		if (resourceGrid.tiles [x, y].tileType != TileData.Types.empty && 
		    resourceGrid.tiles [x, y].tileType != TileData.Types.rock && 
		    resourceGrid.tiles [x, y].tileType != TileData.Types.mineral &&
		    resourceGrid.tiles [x, y].tileType != TileData.Types.water) {
			return true;
		} else {
			return false;
		}
	}

	/*
	/// <summary>
	/// Checks if MY BUDDY has:
	/// -Reached the destination OR
	/// - is Attacking
	/// </summary>
	void BuddySystem(){
//		if (!destinationReached) {
//			if (myBuddy.destinationReached == true){
//				destinationReached = true;
////				moving = false;
////				Debug.Log ("stopping.");
//				//TODO: Instead of stopping here, when they reach their destination, I want them to surround the building
//			}
//		}
		if (_state != State.ATTACKING) {
			if (myBuddy.state == State.ATTACKING){
				if (CheckForTileAttack(myBuddy.targetPosX, myBuddy.targetPosY)){

					// We found a tile to attack, change state to attacking to stop movement
					_state = State.ATTACKING;

//					isAttacking = true;
//					moving = false;

					targetPosX = myBuddy.targetPosX;
					targetPosY = myBuddy.targetPosY;
					enemyAttkHandler.targetTilePosX = myBuddy.targetPosX;
					enemyAttkHandler.targetTilePosY = myBuddy.targetPosY;
					enemyAttkHandler.resourceGrid = resourceGrid;

					// Change attack handler state to Attacking Tile
					enemyAttkHandler.state = Enemy_AttackHandler.State.ATTACK_TILE;
					Debug.Log ("Also attacking tile!");
				}
			}
		}
	}
	*/
}
