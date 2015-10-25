using UnityEngine;
using System.Collections;

public class Energy_Generator : MonoBehaviour {

	Building_UIHandler buildingUI;
	
	public Player_ResourceManager playerResources;
	
	bool statsInitialized;
	
	SpriteRenderer sr;
	
	public enum State { GENERATING, STARVED }

	private State _state = State.GENERATING;
	
	[HideInInspector]
	public State state { get { return _state; } set { _state = value; } }

	public int energyUnitsGenerated = 5;

	private bool energyInitialized = false;

	private bool statusIndicated = false;

	private Building_StatusIndicator buildingStatusIndicator;

	
	void Start () {

		if (buildingStatusIndicator == null)
			buildingStatusIndicator = GetComponent<Building_ClickHandler> ().buildingStatusIndicator;


		// In case Building UI is null
		if (buildingUI == null) {
			buildingUI = GameObject.FindGameObjectWithTag ("UI").GetComponent<Building_UIHandler> ();
		}
		
		// In case Player Resources is null
		if (playerResources == null) {
			playerResources = GameObject.FindGameObjectWithTag("Capital").GetComponent<Player_ResourceManager>();
		}
		
		// Store the Sprite Renderer for layer management
		sr = GetComponent<SpriteRenderer> ();

	}
	
	
	void Update () 
	{

		// Give the Player Resource Manager our stats to show on Food Production panel
//		if (!statsInitialized){
//			playerResources.CalculateWaterProduction(waterPumped, genRate, false);
//			statsInitialized = true;
//		}
	
		
		MyStateMachine (_state);
	}
	
	
	void MyStateMachine(State curState)
	{
		switch (curState) {
			
		case State.GENERATING:
			if (!energyInitialized){
				GenerateEnergy();
			}

			if (!statusIndicated)
				IndicateStatus("Online!");

			break;
			
		default:
			// starved
			// make energy initialized false so when it's unstarved it brings back power
			energyInitialized = false;
			// take away energy
			playerResources.ChangeResource("Energy", -energyUnitsGenerated);
			break;
		}
	}

	void IndicateStatus(string status)
	{
		if (buildingStatusIndicator != null) {
			buildingStatusIndicator.CreateStatusMessage (status);
			
			statusIndicated = true;
		} else {
			Debug.Log("GENERATOR: Building Status Indicator not set!");
		}
	}



	void GenerateEnergy()
	{
		playerResources.ChangeResource ("Energy", energyUnitsGenerated);
		energyInitialized = true;

	}

}
