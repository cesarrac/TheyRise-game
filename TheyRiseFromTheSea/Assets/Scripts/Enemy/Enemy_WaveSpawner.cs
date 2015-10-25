using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class Enemy_WaveSpawner : MonoBehaviour {

	public enum SpawnState { SPAWNING, WAITING, COUNTING, STOP };

	[System.Serializable]
	public class EnemyUnit
	{
		public string enemyName;
		public int enemyCount;
		public Sprite enemySprite;
		public float spawnRate;
	}

	[System.Serializable]
	public class Wave
	{
		
		public string name;

		[Header("MAXIMUM 3 MEMBERS PER WAVE")]
		public EnemyUnit[] members;
		public int spawnPosIndex;
	
	}



	public Wave[] waves;
	private int nextWave = 0;

	[Header("Total # of Waves in a Group")]
	[SerializeField]
	private int wavesInGroup;
	
	private int groupCount;
	
	private int nextGroup = 1;
	
	private int nextWaveInGroup = 0; // gets reset everytime a group is done spawning
	
	public float timeBetweenWaves = 5f;
	public float peaceTime = 60f;
	public float startingPeaceTime = 120f;

	[SerializeField]
	private float waveCountDown;

	[SerializeField]
	private float peaceCountDown;

	[SerializeField]
	private float startingCountDown;

	public SpawnState state = SpawnState.COUNTING;

	public ObjectPool objPool;

	[SerializeField]
	private Vector3[] spawnPositions;

	Vector3 neighborEnemyPosition = Vector3.zero;

	public ResourceGrid resourceGrid;

	public SpawnPoint_Handler spwnPtHandler;
	
	private int maxWaves;

	private Enemy_MoveHandler lastEnemy;

	public Text displayTime;

	public List< Enemy_MoveHandler> spawnedEnemies = new List<Enemy_MoveHandler>();

	GameObject[] indicators;
	
	private bool indicatorsCreated = false;

	// Camera Shake:
	public CameraShake camShake;

	[HideInInspector]
	public Terraformer_Handler terraformer; // access to the terraformer, to know if spawning should continue

	void Start()
	{
		if (objPool == null)
			objPool = GameObject.FindGameObjectWithTag ("Pool").GetComponent<ObjectPool> ();

		startingCountDown = startingPeaceTime;
		peaceCountDown = peaceTime;
		waveCountDown = timeBetweenWaves;

		displayTime.text = "Next Wave in: ";

		indicators = new GameObject[wavesInGroup];

		// create initial indicators
		CreateSpawnPointIndicators ();

		maxWaves = waves.Length;

		groupCount = maxWaves / wavesInGroup;
//		Debug.Log ("WAVE SPAWNER: Group count = " + groupCount);

		state = SpawnState.COUNTING;
	}

	void Update()
	{
		// Wave Spawner waits to have access to the Terraformer, and the terraformer to be working, before countdown to spawn
		if (terraformer) {
			if (terraformer.curState == Terraformer_Handler.State.WORKING) {

				CountDown();
			}
		}


	}

	void CountDown()
	{
		if (startingCountDown <= 0) {
			
			if (peaceCountDown <= 0) {
				displayTime.color = Color.red;
				
				// start the wave countdown	
				if (waveCountDown <= 0) {
					// check if we are already spawning
					if (state != SpawnState.SPAWNING && state != SpawnState.STOP) {
						
						displayTime.text = waves [nextWave].name + " INCOMING!";
						
						// get rid of the indicators
						for (int i =0; i < indicators.Length; i++){
							objPool.PoolObject(indicators[i]);
							//							indicators[i] = null;
						}
						// reset bool
						indicatorsCreated = false;
						
						// Start spawning a wave
						StartCoroutine (SpawnWave (waves [nextWave]));
					}
				} else {
					waveCountDown -= Time.deltaTime;
					displayTime.text = "Wave Approaching in: " + waveCountDown.ToString ("f1");
				}
			} else {
				peaceCountDown -= Time.deltaTime;
				displayTime.text = "Next Wave in: " + peaceCountDown.ToString ("f1");
			}
			
		} else {
			startingCountDown -= Time.deltaTime;
			displayTime.color = Color.green;
			displayTime.text = "Attack Incoming in: " + startingCountDown.ToString("f1");
		}
		
		// Creates indicators as soon as we are on peace count down
		if (peaceCountDown > 0) {
			if (!indicatorsCreated)
				CreateSpawnPointIndicators();
		}
	}

	public void ForceStartAttack()
	{
		startingCountDown = 0;
		peaceCountDown = 0;
		if (state != SpawnState.SPAWNING) {
			waveCountDown = 0;
		}
	}

	public void CreateSpawnPointIndicators()
	{

		// create spawn point indicators for each wave in a group
		for (int i =0; i < wavesInGroup; i++) {

			// find the wave to get its info
			Wave thisWave = waves[nextWave + i];
			

			// get the indicator from pool
			GameObject spwnIndicator = objPool.GetObjectForType("Spawn Indicator 2", true, spawnPositions[thisWave.spawnPosIndex]);

			// store how many members there are in this wave
			int memberCount = thisWave.members.Length;

//			Debug.Log("WAVE SPAWNER: Member count for wave " + nextWave + " is = " +  memberCount);

			if (spwnIndicator != null){

				// add this indicator to our array so we can eliminate it when it's done
				indicators[i] = spwnIndicator;

				// get the Spawn Indicator Component from it
				Enemy_SpawnIndicator indicator = spwnIndicator.GetComponent<Enemy_SpawnIndicator>();

				// now Initialize & Set the indicator according to the number of members in this wave 
										// NOTE: Maximum allowed members is 3!
				if (memberCount == 1)
				{
					indicator.InitOneTypeIndicator(thisWave.members[0].enemySprite, thisWave.members[0].enemyCount);
					indicator.SetIndicator1();
				}
				else if (memberCount == 2)
				{
//					Debug.Log ("WAVE SPAWNER: Created indicator for " + thisWave.members[0].enemyName 
//					           + " and " + thisWave.members[1].enemyName);

					indicator.InitTwoTypeIndicator(thisWave.members[0].enemySprite, thisWave.members[0].enemyCount,
					                               thisWave.members[1].enemySprite, thisWave.members[1].enemyCount);
					indicator.SetIndicator2();
				}
				else if (memberCount == 3)
				{
//					Debug.Log ("WAVE SPAWNER: Created indicator for " + thisWave.members[0].enemyName 
//					           + " and " + thisWave.members[1].enemyName + " and " + thisWave.members[2].enemyName);

					indicator.InitThreeTypeIndicator(thisWave.members[0].enemySprite, thisWave.members[0].enemyCount,
					                               thisWave.members[1].enemySprite, thisWave.members[1].enemyCount,
					                               thisWave.members[2].enemySprite, thisWave.members[2].enemyCount);
					indicator.SetIndicator3();
				}

				// and give it access to the functions on this script
//				indicator.enemyWaveSpawner = this;
			}else{
				Debug.Log("WAVE SPAWNER: Pool can't find Spawn Indicator!");
			}

		}

		// bool that stops this method from being called after indicator has been created
		indicatorsCreated = true;
	}



	IEnumerator SpawnWave (Wave _wave)
	{
//		Debug.Log ("WAVE: Spawning wave " + nextWave + ", group member: " + nextWaveInGroup);

		state = SpawnState.SPAWNING;

		// Spawn
			// Loop through members in a wave
		for (int x = 0; x < _wave.members.Length; x++) {
			// each member has a name and a count, loop through this count too
			for (int y = 0; y < _wave.members[x].enemyCount; y++){
				// spawn this enemy name
				SpawnEnemy(_wave.members[x].enemyName, _wave.spawnPosIndex);

				yield return new WaitForSeconds (1f / _wave.members[x].spawnRate);
			}
		}

		nextWave ++;
//		Debug.Log ("WAVE SPAWNER: Next Wave = " + nextWave);

		// Check if there are any groups
		if (groupCount > 0) 
		{	
			// There are groups, so check if there are still waves to spawn inside this group
			if (nextWaveInGroup < wavesInGroup - 1)
			{
//				// Get the next wave in this group
				nextWaveInGroup ++;

				// Spawn the next wave in this group
				waveCountDown = timeBetweenWaves;

				// Set state to waiting while wavecountdown counts down and switches this back to Spawning
				state = SpawnState.WAITING;

			} 
			else
			{	// In this case, there ARE groups but no more waves to spawn in this group
			

				// Reset next wave in group
				nextWaveInGroup = 0;

				// This GROUP is done spawning, so we must go to the NEXT group
				nextGroup++;

				// NOTE:  This NEXT GROUP of waves CANNOT spawn until the terraformer has started its cycle again

				// Make sure we haven't already spawned the last group
				if (nextGroup <= groupCount)
				{
//					
					// start peace time and keep nextwave with its current value
					peaceCountDown = peaceTime;
					waveCountDown = timeBetweenWaves;

					// set state to counting to begin countdown
					state = SpawnState.COUNTING;
				}
				else
				{ // The last group has already been spawned

					// Check that there are no more waves left, in case there's any left that are not in group
					if (nextWave <= maxWaves - 1){

						// if there are we spawn
						peaceCountDown = peaceTime;
						waveCountDown = timeBetweenWaves;
						state = SpawnState.COUNTING;

						// we need to tell the spawner we won't be doing groups for this wave
						wavesInGroup = 1;
						groupCount = 0;

					}else{
						// No more groups/waves left, so STOP spawning
						state = SpawnState.STOP;
						
						displayTime.gameObject.SetActive(false);
					}

				}

			}
		} 
		else 
		{					// THERE ARE NO GROUPS
			if (nextWave <= maxWaves - 1) 
			{
				waveCountDown = timeBetweenWaves;

				state = SpawnState.WAITING;
				
			} 
			else 
			{
				// No more Waves left, STOP Spawning
				state = SpawnState.STOP;


				displayTime.gameObject.SetActive(false);
			}
		}

		yield break;
	}

	void SpawnEnemy(string _enemyName, int _spawnIndex)
	{
		// Spawn and fill components
		GameObject _enemy = objPool.GetObjectForType (_enemyName, true, spawnPositions[_spawnIndex]);

		if (_enemy != null) {


			// Get the Attack Handler
			Enemy_AttackHandler _attkHandler = _enemy.GetComponentInChildren<Enemy_AttackHandler>();

			// Get the Move Handler
			Enemy_MoveHandler _moveHandler = _enemy.GetComponent<Enemy_MoveHandler>();

			// Pathfinding variables
			_moveHandler.resourceGrid = resourceGrid;
			_moveHandler.spwnPtIndex = _spawnIndex;
			_moveHandler.spwnPtHandler = spwnPtHandler; 

			// Give it the Object Pool
			_attkHandler.objPool = objPool;

			// Give it the Camera Shake for Special Attack
			_attkHandler._camShake = camShake;

			// Don't Activate GameObject until all components are set
			_enemy.SetActive(false);

			// Feed it the move handler from the enemy spawned before this one so it has a buddy
			if (lastEnemy != null){
				// make sure they belong to the same wave
				if (lastEnemy.gameObject.name == _enemyName){
					_moveHandler.myBuddy = lastEnemy;
				}else{
					// not the same name so this must be the first spawn of a new wave
					lastEnemy = null;
				}
				
			}

			// Store this unit as Last Enemy spawned
			lastEnemy = _moveHandler;
			
			// Add this newly Spawned enemy to the list of spawned enemies
			spawnedEnemies.Add(_moveHandler);

			// Store its position for its neighbors
			neighborEnemyPosition = _enemy.transform.position;


			// IN CASE THIS UNIT WAS ALREADY SPAWNED FROM POOL:
			if (_moveHandler.unitInitialized){

				// Reset their Health Bar 
				_attkHandler.statusIndicator.SetHealth(_attkHandler.stats.curHP, _attkHandler.stats.maxHP);

//				 Reset its starting path position to this new spawn point
				_moveHandler.posX = (int)_enemy.transform.position.x;
				_moveHandler.posY = (int)_enemy.transform.position.y;

				// Reset the current move speed in case this unit was affected by a Speed DeBuffer
				_moveHandler.mStats.curMoveSpeed = _moveHandler.mStats.startMoveSpeed;

				if (_enemy.GetComponentInChildren<Text>() != null){
					Text dmgTxt = _enemy.GetComponentInChildren<Text>();
					objPool.PoolObject(dmgTxt.gameObject);
				}
			}

			// Object Ready to be Activated
			_enemy.SetActive(true);


			if (_enemy.GetComponentInChildren<Text>() != null){
				Text dmgTxt = _enemy.GetComponentInChildren<Text>();
				objPool.PoolObject(dmgTxt.gameObject);
			}

			// Initialize its Stats
			_attkHandler.stats.Init();

			// Initialize their path to Start moving to their destination
			_moveHandler.InitPath();

//			Debug.Log ("WAVE: Spawned " + _enemyName + " with " + _attkHandler.stats.curHP + 
//			           "HP. At position: (x)" + _enemy.transform.position.x + " (y)" + _enemy.transform.position.y); 
		}
	}

	/// <summary>
	/// Gets the spawned enemy and returns type of
	/// Enemy_MoveHandler.
	/// </summary>
	/// <returns>The spawned enemy.</returns>
	/// <param name="count">Count.</param>
	public Enemy_MoveHandler GetSpawnedEnemy ( int count){
		return spawnedEnemies[count];
	}

	// FOR TESTING, This resets the wave spawner
	public void Reset(){

		startingCountDown = startingPeaceTime;

		nextWave = 0;
		nextWaveInGroup = 0;
		nextGroup = 1;

		peaceCountDown = peaceTime;
		waveCountDown = timeBetweenWaves;
		
		displayTime.text = "Next Wave in: ";
		
		indicators = new GameObject[wavesInGroup];
		
		// create initial indicators
		CreateSpawnPointIndicators ();

		state = SpawnState.COUNTING;
		
		maxWaves = waves.Length;
		
		groupCount = maxWaves / wavesInGroup;
	}
}


