using UnityEngine;
using System.Collections;

public class Terraformer_Handler : MonoBehaviour {

    /// <summary>
    /// This script handles the progress of a terraformer and communicates when final stage is done to a Master script
    /// that will load the victory screen and progress to next level.
    /// </summary>

    private int _maxTerraformerStages = 5;
    public int MaxTerraformerStages { get { return _maxTerraformerStages; } set { _maxTerraformerStages = Mathf.Clamp(value, 5, 11); } }

	private int _maxTerraformCycles = 3; // # of cycles that must that must completed for terraforming to finish a stage
    public int MaxTerraformerCycles { get { return _maxTerraformCycles; } set { _maxTerraformCycles = Mathf.Clamp(value, 2, 11); } }

    private float _maxCycleTime = 20f; // time in seconds it takes for a cycle to finish
	public float currProgressTime { get; private set;} // current elapsed time, resets to 0 when a cycle is completed

    int _currCycleCount = 0; // keeps track of the current cycle terraformer is on
    int _currStageCount = 0;


	public enum State {IDLING, WORKING, DONE};
	private State _state = State.IDLING;
	public State curState { get { return _state; }}

	public Enemy_WAVESpawnerV2 waveSpawner; // control over the spawning of enemy waves

	// Used to activate Mission Succes state, Mission Failed is handled when Resource Grid swaps the capital tile
	public MasterState_Manager master_State;

    bool isWorking = false;
    bool isPlayerNear = false;

    void Awake()
	{

        if (!master_State)
            master_State = MasterState_Manager.Instance;

        //if (GameObject.FindGameObjectWithTag ("Spawner") != null) {
        //	waveSpawner = GameObject.FindGameObjectWithTag ("Spawner").GetComponent<Enemy_WAVESpawnerV2> ();
        //	//waveSpawner.terraformer = this;
        //}

    }

    void Start()
    {
        currProgressTime = 0;
        StopTerraformer();
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
		if (_state != State.WORKING) {
			_state = State.WORKING;
            Debug.Log("TERRAFORMER: Beginning terraforming stage " + _currStageCount);
        }

	}


    IEnumerator Terraform()
    {
        while (true)
        {
            yield return new WaitForSeconds(_maxCycleTime);

            _currCycleCount++;
            Debug.Log("TERRAFORMER: Curr cycle: " + _currCycleCount);

            if (_currCycleCount >= _maxTerraformCycles)
            {
                // Stage completed
                _currStageCount++;
                Debug.Log("TERRAFORMER: Stage " + (_currStageCount - 1) + " completed.");

                // Reset current cycle
                _currCycleCount = 0;
                
                // Check if this was the last stage
                if (_currStageCount >= _maxTerraformerStages)
                {
                    _state = State.DONE;
                }
                else
                {
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
