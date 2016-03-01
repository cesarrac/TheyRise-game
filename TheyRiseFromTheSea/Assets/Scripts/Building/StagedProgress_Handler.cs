using UnityEngine;
using System.Collections;
using System;

public class StagedProgress_Handler : MonoBehaviour {

    public static StagedProgress_Handler instance;

    private int _maxStages = 1;
    public int MaxStages { get { return _maxStages; } set { _maxStages = Mathf.Clamp(value, 3, 11); } }

    private int _maxCycles = 3; // # of cycles that must that must completed for terraforming to finish a stage
    public int MaxCycles { get { return _maxCycles; } set { _maxCycles = Mathf.Clamp(value, 3, 11); } }

    private float _maxCycleTime = 20f; // time in seconds it takes for a cycle to finish
    public float currProgressTime { get; private set; } // current elapsed time, resets to 0 when a cycle is completed

    int _currCycleCount = 0; // keeps track of the current cycle terraformer is on
    public int _currStageCount { get; protected set; }


    public enum State { IDLING, WORKING, DONE };
    private State _state = State.IDLING;
    public State curState { get { return _state; } }


    bool isWorking = false;
    bool isPlayerNear = false;

    Building_StatusIndicator build_statusIndicator;

    Action<StagedProgress_Handler> MissionStagesCB;
    Action<int> MissionCompletedCheckCB;

    void Awake()
    {
        instance = this;

        build_statusIndicator = GetComponent<Building_Handler>().buildingStatusIndicator;

    }



    void Start()
    {
        CheckForMissionCallbacks();

        if (MissionStagesCB != null)
        {
            MissionStagesCB(this);
        }

        currProgressTime = 0;
        _currStageCount = 0;

        StopProgress();

        build_statusIndicator.CreateStatusMessage("Press 'Interact' to Start.");
    }

    void CheckForMissionCallbacks()
    {
        if (Mission_Manager.Instance.ActiveMission.MissionType == MissionType.SCIENCE)
        {
            RegisterMissionCompletedCB(Mission_Manager.Instance.CheckScienceMissionCompleted);
            RegisterMissionStagesCallback(Mission_Manager.Instance.SetMissionStages);
        }
    }

    // This callback allows the Mission Manager to set this component's Max Stages value depending on the current Active Mission
    void RegisterMissionStagesCallback(Action<StagedProgress_Handler> cb)
    {
        MissionStagesCB = cb;
    }

    void RegisterMissionCompletedCB(Action<int> cb)
    {
        MissionCompletedCheckCB = cb;
    }

    void Update()
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
            StartProgress();
        }
    }

    void MyStateMachine(State _curState)
    {
        switch (_curState)
        {
            case State.WORKING:

                if (!isWorking)
                {
                    isWorking = true;
                    StartCoroutine("Progress");
                }
                break;
            case State.DONE:
                if (isWorking)
                {
                    StopProgress();
                    isWorking = false;
                }

                _state = State.IDLING;

                break;

            default:
                // Terraformer is IDLING
                break;
        }
    }


    public void StartProgress()
    {
        if (_state != State.WORKING && _currStageCount < _maxStages)
        {
            _state = State.WORKING;
            build_statusIndicator.CreateStatusMessage("Beginning stage " + _currStageCount, Color.black);
        }

    }


    IEnumerator Progress()
    {
        while (true)
        {
            yield return new WaitForSeconds(_maxCycleTime);

            _currCycleCount++;
            build_statusIndicator.CreateStatusMessage("Processing cycle " + _currCycleCount, Color.black);


            if (_currCycleCount >= _maxCycles)
            {
                // Stage completed
                _currStageCount++;
               // build_statusIndicator.CreateStatusMessage(_currStageCount + " stages completed.", Color.black);

                // Reset current cycle
                _currCycleCount = 0;

                // Check if this was the last stage
                if (_currStageCount >= _maxStages)
                {
                    build_statusIndicator.CreateStatusMessage("ALL stages completed.", Color.black);

                    // If the current active Mission requires this building to complete its stages, notify the Mission Manager that stages are completed
                    if (MissionCompletedCheckCB != null)
                    {
                        MissionCompletedCheckCB(_currStageCount);
                    }

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

    void StopProgress()
    {
        isWorking = false;
        currProgressTime = 0;
        _currCycleCount = 0;
        StopCoroutine("Progress");
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
