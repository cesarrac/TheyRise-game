using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class MasterState_Manager : MonoBehaviour {

	/// <summary>
	/// Controls the Master State of the Game:
	/// - Load Levels / Scenes
	/// - Track Player Resources across levels
	/// - Save Game
	/// - Manipulate Time to Pause game
	/// - Pause CoRoutines in other scripts by changing Master State
	/// </summary>

	public enum MasterState { WAITING, LOADING, START, PAUSED, MISSION_FAILED, PLAYER_DEAD,  MISSION_SUCCESS, ONSHIP,CONTINUE, ON_EQUIP, ON_BLUEPRINTS, QUIT }

	private MasterState _mState = MasterState.WAITING;

	[HideInInspector]
	public MasterState mState { get { return _mState; } set { _mState = value; }}

	GameMaster game_master;

	public GameObject missionFailedPanel;

    public static MasterState_Manager Instance { get; protected set; }

	void Awake () 
	{
        Instance = this;



        // ON PLANET:
        if (SceneManager.GetActiveScene().name == "Level_Planet")
        {
            // If for some reason the mission failed panel is still active, deactivate it
            if (missionFailedPanel)
            {
                if (missionFailedPanel.activeSelf)
                    missionFailedPanel.SetActive(false);
            }
            mState = MasterState.START;
        }
        else if (SceneManager.GetActiveScene().name == "Level_CENTRAL")
        {
            _mState = MasterState.ONSHIP;
        }
	}
	
    void Start()
    {
        if (!game_master) game_master = GameMaster.Instance;
    }

	void Update ()
	{
		// QUIT:
		if (Input.GetKey ("escape"))
			mState = MasterState.QUIT;

		MasterStateMachine (mState);
	}

	void MasterStateMachine(MasterState _curState)
	{
		switch (_curState) {
		case MasterState.WAITING:
			// Waiting to Load level
			break;
		case MasterState.LOADING:
			// Loading a Level
			// If level has been loaded, state is Start
			break;
		case MasterState.START:
			// Initialize a level
			// If all units and map have been initialized, state is Continue
			break;
		case MasterState.PAUSED:
			// Pause the game
			break;
		case MasterState.CONTINUE:
			// Level has been initialized, run Time as normal
			break;
		case MasterState.MISSION_FAILED:
			// Pause game and tell GameMaster to load level or go back to ship level
			MissionFailed();
			break;
        case MasterState.PLAYER_DEAD:
            // Pause game and tell GameMaster to load level or go back to ship level
            MissionFailed();
            break;
        case MasterState.MISSION_SUCCESS:
                // Load a progress scene where the player sees what items they got and what they have left
                MissionSuccess();
			break;
        case MasterState.ONSHIP:
                if (Time.timeScale == 0)
                {
                    Time.timeScale = 1;
                }
            break;
            case MasterState.QUIT:
			//TODO: Here we would begin the save progress function and then quit the application
			Application.Quit();
			break;
		default:
			_mState = MasterState.WAITING;
			break;
		}
	}

	void MissionFailed()
	{
		if (Time.timeScale != 0) {
			Time.timeScale = 0;
			Debug.Log("MasterState: Mission FAILED. Stopping game");
		}
		if (!missionFailedPanel.activeSelf) {
			missionFailedPanel.SetActive(true);
		}
	}

    void MissionSuccess()
    {
        if (Time.timeScale != 0)
        {
            UI_Manager.Instance.DisplayVictoryPanel();
           
            StopAllCoroutines();

            Time.timeScale = 0;
        }
      
    }

	// BUTTONS:
	public void PauseButton()
	{
		_mState = MasterState.PAUSED;
		Debug.Log ("Master State is: " + _mState.ToString() +" Pausing game! ");
		Time.timeScale = 0;
	}

	public void UnPause()
	{
		_mState = MasterState.CONTINUE;
		Debug.Log ("Master State is: " + _mState.ToString() +" Continuing game! ");
		Time.timeScale = 1;
	}

	public void RestartLevel()
	{
		game_master.MissionRestart ();
	}


	public void ReturnToShip()
	{
        _mState = MasterState.ONSHIP;
		game_master.GoBackToShip ();
	}

    public void NewCharacter()
    {
        _mState = MasterState.ONSHIP;
        game_master.NewCharacterScreen();
    }

}
