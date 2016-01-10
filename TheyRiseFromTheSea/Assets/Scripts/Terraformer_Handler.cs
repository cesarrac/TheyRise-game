using UnityEngine;
using System.Collections;

public class Terraformer_Handler : MonoBehaviour {

    /// <summary>
    /// This script handles the progress of a terraformer and communicates when final stage is done to a Master script
    /// that will load the victory screen and progress to next level.
    /// </summary>

    public static Terraformer_Handler instance;
    
    private int _maxTerraformerStages = 5;
    public int MaxTerraformerStages { get { return _maxTerraformerStages; } set { _maxTerraformerStages = Mathf.Clamp(value, 5, 11); } }

	private int _maxTerraformCycles = 3; // # of cycles that must that must completed for terraforming to finish a stage
    public int MaxTerraformerCycles { get { return _maxTerraformCycles; } set { _maxTerraformCycles = Mathf.Clamp(value, 2, 11); } }

    private float _maxCycleTime = 20f; // time in seconds it takes for a cycle to finish
	public float currProgressTime { get; private set;} // current elapsed time, resets to 0 when a cycle is completed

    int _currCycleCount = 0; // keeps track of the current cycle terraformer is on
    public int _currStageCount { get; protected set; }


    public enum State {IDLING, WORKING, DONE};
	private State _state = State.IDLING;
	public State curState { get { return _state; }}

	public Enemy_WAVESpawnerV2 waveSpawner; // control over the spawning of enemy waves

	// Used to activate Mission Succes state, Mission Failed is handled when Resource Grid swaps the capital tile
	public MasterState_Manager master_State;

    bool isWorking = false;
    bool isPlayerNear = false;

    Building_StatusIndicator build_statusIndicator;

    void Awake()
	{
        instance = this;

        if (!master_State)
            master_State = MasterState_Manager.Instance;

        //if (GameObject.FindGameObjectWithTag ("Spawner") != null) {
        //	waveSpawner = GameObject.FindGameObjectWithTag ("Spawner").GetComponent<Enemy_WAVESpawnerV2> ();
        //	//waveSpawner.terraformer = this;
        //}

        build_statusIndicator = GetComponent<Building_ClickHandler>().buildingStatusIndicator;

    }

    void Start()
    {
        currProgressTime = 0;
        _currStageCount = 0;

        StopTerraformer();

        build_statusIndicator.CreateStatusMessage("Press 'Interact' to Start.");
    }
	
	void Update () 
	{
        if (!isWorking && isPlayerNear)
        {
            ListenForInteractButton();
        }
      

        MyStateMachine(_state);
	}

    void ListenForInteractButton()
    {
        if (Input.GetButtonDown("Interact"))
        {
            StartTerraformer();
        }
    }

    void MyStateMachine(State _curState)
	{
		switch (_curState) {
		case State.WORKING:
			
			    if (!isWorking)
                {
                    isWorking = true;
                    StartCoroutine("Terraform");
                }
			break;
		case State.DONE:
                StopTerraformer();
                Debug.Log("Congratulations! Terraformer has succesfully completed its final stage!");

                // Notify the Launchpad/Transporter that it can now launch safely by unlocking its controls.
                Transporter_Handler.instance.LockControls(false);

			    //Notify Master State that this level was succesfully terraformed, Game Master will load ship level after UI screen pops up
			   // master_State.mState = MasterState_Manager.MasterState.MISSION_SUCCESS;
			break;
   
		default:
			// Terraformer is IDLING
			break;
		}
	}


	public void StartTerraformer()
	{
		if (_state != State.WORKING && _currStageCount < _maxTerraformerStages) {
			_state = State.WORKING;
            build_statusIndicator.CreateStatusMessage("Beginning terraforming stage " + _currStageCount, Color.black);

            // Tell the enemy spawner it can spawn now that the Terraformer is working
            Enemy_Master.instance.SetCanSpawn();
        }

	}


    IEnumerator Terraform()
    {
        while (true)
        {
            yield return new WaitForSeconds(_maxCycleTime);

            _currCycleCount++;
            build_statusIndicator.CreateStatusMessage("Processing cycle " + _currCycleCount, Color.black);


            if (_currCycleCount >= _maxTerraformCycles)
            {
                // Stage completed
                _currStageCount++;
                build_statusIndicator.CreateStatusMessage(_currStageCount + " stages completed.", Color.black);

                // Reset current cycle
                _currCycleCount = 0;
                
                // Check if this was the last stage
                if (_currStageCount >= _maxTerraformerStages)
                {
                    build_statusIndicator.CreateStatusMessage("ALL stages completed.", Color.black);
                    _state = State.DONE;
                }
                else
                {
                    build_statusIndicator.CreateStatusMessage("Waiting for manual activation...", Color.black);
                    // Idle until player starts it up again
                    _state = State.IDLING;
                }

                isWorking = false;

                yield break;
            }

        }
    }

    void StopTerraformer()
    {
        isWorking = false;
        currProgressTime = 0;
        _currCycleCount = 0;
        StopCoroutine("Terraform");
    }


	void OnTriggerEnter2D(Collider2D coll)
    {
		if (coll.gameObject.tag == "Citizen")
        {
            isPlayerNear = true;
        }
	}

    void OnTriggerExit2D(Collider2D coll)
    {
        if (coll.gameObject.tag == "Citizen")
        {
            isPlayerNear = false;
        }
    }
}
