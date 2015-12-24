using UnityEngine;
using System.Collections;

public class FoodProduction_Manager : ExtractionBuilding {
	/// <summary>
	/// Extracts x # of food each production cycle and adds it to the Player's resources. Will not start or continue
	/// to produce food if the player has no water in storage.
	/// </summary>

	public int waterConsumed; // water Consumed every farming cycle in order to produce a harvest

    public float ProductionRate;

    public int ProductionAmmnt;

    public int startingStorageCap, startingSecondStorageCap;
    public int PersonalStorageCap { get; protected set; }
    public int SecondStorageCap { get; protected set; }

    public int currWaterStored { get; protected set; }
    public int WaterStorageCap { get; protected set; }


    //	bool farming;

    //public Player_ResourceManager resourceManager;

    //bool foodStatsInitialized = false;

    //public enum State { HARVESTING, NOWATER }

    //private State _state = State.HARVESTING;

    //[HideInInspector]
    //public State state { get { return _state; } set { _state = value; } }


    void OnEnable()
    {
        currResourceStored = 0;
    }

    void Awake()
    {
        PersonalStorageCap = startingStorageCap;
        SecondStorageCap = startingSecondStorageCap;

        InitSelfProducer(ProductionRate, ProductionAmmnt, PersonalStorageCap, SecondStorageCap, waterConsumed, transform);

        _state = State.SEARCHING;
    }

	void Update () 
	{

        MyStateManager(_state);
	}

	void MyStateManager(State curState)
	{
        switch (curState)
        {

            case State.PRODUCING:
                //CountDownToExtract();
                if (!isExtracting && !productionHalt)
                {
                    if (!storageIsFull)
                    {
                        StopCoroutine("Produce");
                        StartCoroutine("Produce");
                        isExtracting = true;

                        StopCoroutine("ShowStatusMessage");
                        StartCoroutine("ShowStatusMessage");
                        statusMessage = "Producing!";
                    }
                    else
                    {
                        statusMessage = "Full!";
                        StopCoroutine("ShowStatusMessage");
                        StartCoroutine("ShowStatusMessage");
                        _state = State.NOSTORAGE;
                    }



                }
                else if (productionHalt)
                {
                    // Halt production is a state that handles what happens when this Food Producer runs out of Water
                    _state = State.HALT;
                }
                break;

            case State.NOSTORAGE:
                if (!storageIsFull)
                {

                    _state = State.SEARCHING;
                }
                else if (statusIndicated && storageIsFull)
                {
                    // repeating full status message for player to see!!
                    statusMessage = "Full!";
                    StopCoroutine("ShowStatusMessage");
                    StartCoroutine("ShowStatusMessage");
                }
                else
                {
                    if (output != null && isConnectedToOutput)
                    {
                        if (CheckOutputStorage())
                        {
                            statusMessage = "Sending!";
                            StopCoroutine("ShowStatusMessage");
                            StartCoroutine("ShowStatusMessage");
                        }
                        else
                        {
                            // OUTPUT STORAGE FULL:
                            statusMessage = "Output Full!";
                            StopCoroutine("ShowStatusMessage");
                            StartCoroutine("ShowStatusMessage");
                        }
                    }
                }

                break;

            case State.SEARCHING:

                statusMessage = "Callibrating...";
                StopCoroutine("ShowStatusMessage");
                StartCoroutine("ShowStatusMessage");

                if (currMaterialsStored > 0)
                {
                    _state = State.PRODUCING;
                }
                else
                {
                    Debug.Log("NO water.");

                    noMaterialsLeft = true;
                    productionHalt = true;
                    _state = State.HALT;

                }
                break;

            case State.HALT:

                statusMessage = "No Water!";
                StopCoroutine("ShowStatusMessage");
                StartCoroutine("ShowStatusMessage");

                if (noMaterialsLeft && statusIndicated)
                {
                    statusMessage = "No Water!";
                    StopCoroutine("ShowStatusMessage");
                    StartCoroutine("ShowStatusMessage");
                }
                else if (currMaterialsStored > 0)
                {
                    noMaterialsLeft = false;
                    productionHalt = false;
                    _state = State.SEARCHING;
                }

                break;

            default:
                // starved / no power
                break;
        }
    }






}
