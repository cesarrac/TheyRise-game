using UnityEngine;
using System.Collections;

public class Storage : ExtractionBuilding {


    // One storage to store them all. This is the TOTAL CAPACITY of this storage.
    public int PersonalStorageCap { get; protected set; }
    public int startingStorageCap;

    // How much water is stored in this building
	public int waterStored { get; private set; }

    // How much ore is stored
    public int oreStored { get; private set; }

    // How much food is stored
    public int foodStored { get; private set; }


    void OnEnable()
    {
        EmptyAll();
    }

    void Awake()
    {
        PersonalStorageCap = startingStorageCap;

        InitStorageUnit(PersonalStorageCap, transform, StoreSpecificResource);

        _state = State.IDLE;
    }

    void StoreSpecificResource(TileData.Types rType, int ammnt)
    {
        Debug.Log("STORAGE receiving " + ammnt + " of " + rType);
        switch (rType)
        {
            case TileData.Types.water:
                waterStored += ammnt;
                break;
            case TileData.Types.rock:
                oreStored += ammnt;
                break;
            case TileData.Types.food:
                foodStored += ammnt;
                break;
            default:
                // Cant find that resource
                break;
        }

        if ((waterStored + oreStored + foodStored) >= extractorStats.secondStorageCapacity)
        {
            storageIsFull = true;
        }
    }

    void Update()
    {
        MyStateMachine(_state);
    }

    void MyStateMachine(State _curState)
    {
        switch (_curState)
        {
            case State.IDLE:
                if (currMaterialsStored >= extractorStats.secondStorageCapacity)
                {
                    storageIsFull = true;
                    _state = State.NOSTORAGE;
                }
                break;
            case State.NOSTORAGE:

                if (storageIsFull)
                {
                    statusMessage = "Full!";
                    StopCoroutine("ShowStatusMessage");
                    StartCoroutine("ShowStatusMessage");

                    // Send all items to ship
                    SendToShip();
                }
                else
                {
                    _state = State.IDLE;
                }

                if (statusIndicated)
                {
                    statusMessage = "Full!";
                    StopCoroutine("ShowStatusMessage");
                    StartCoroutine("ShowStatusMessage");
                }

                break;

            default:
                break;

        }
    }
	

    void SendToShip()
    {
        Ship_Inventory shipInventory = Ship_Inventory.Instance;

        if (waterStored > 0)
        {
            shipInventory.ReceiveTemporaryResources(TileData.Types.water, waterStored);

        }

        if (oreStored > 0)
        {
            shipInventory.ReceiveTemporaryResources(TileData.Types.rock, oreStored);

        }

        if (foodStored > 0)
        {
            shipInventory.ReceiveTemporaryResources(TileData.Types.food, foodStored);

        }

        EmptyAll();
    }


    void EmptyAll()
    {
        waterStored = 0;
        oreStored = 0;
        foodStored = 0;

        currMaterialsStored = 0;
        storageIsFull = false;
    }

//	void Update () {
//		if (oreStored >= 100 || waterStored >= 5)
//			WithdrawResources ();
//	}


//	public void AddOreOrWater(int ammt, bool trueIfWater)
//	{
//		if (trueIfWater) {
//			int calcW = waterStored + ammt;
//			if (calcW <= waterCapacity) {
//				waterStored = calcW;
//				waterCapacityLeft = waterCapacity - waterStored;

//				// ADD TO THE PLAYER RESOURCES TO DISPLAY ON UI
////				playerResources.ChangeResource("Water", ammt);

//			} else {
//				// cant store more water
//			}
//			Debug.Log ("STORAGE: Total water = " + waterStored);
//		} else {
//			int calcW = oreStored + ammt;
//			if (calcW <= oreCapacity) {
//				oreStored = calcW;
//				oreCapacityLeft = oreCapacity - oreStored;

//				// ADD TO THE PLAYER RESOURCES TO DISPLAY ON UI
////				playerResources.ChangeResource("Ore", ammt);

//			} else {
//				// cant store more water
//			}
//			Debug.Log ("STORAGE: Total ore = " + oreStored);
//		}
//	}

//	public bool CheckIfFull(int ammntToStore, bool trueIfWater){
//		if (trueIfWater) {
//			if (ammntToStore > waterCapacityLeft) {
//				return true;
//			} else {
//				return false;
//			}
//		} else {
//			if (ammntToStore > oreCapacityLeft){
//				return true;
//			}else{
//				return false;
//			}
//		}
//	}

//	/// <summary>
//	/// Charges the resource from this storage
//	/// </summary>
//	/// <param name="ammnt">Ammnt.</param>
//	/// <param name="id">Identifier.</param>
//	public void ChargeResource(int ammnt, string id){
//		if (id == "Ore") {
//			oreStored = oreStored + ammnt;
//			Debug.Log ("STORAGE: Charging stored ore for " + ammnt);
//			playerResources.ChangeResource (id, ammnt);
//		} else if (id == "Water") {
//			waterStored = waterStored + ammnt;
//			playerResources.ChangeResource (id, ammnt);
//			Debug.Log ("STORAGE: Charging stored water for " + ammnt);

//		} else {
//			Debug.Log ("STORAGE: Can't Find that resource ID!");
//		}
//	}

//	public void WithdrawResources(){
//		Debug.Log ("STORAGE: Capital withdrawing " + waterStored + " WATER and " + oreStored + " ORE.");

//		playerResources.ChangeResource ("Ore", oreStored);
//		playerResources.ChangeResource ("Water", waterStored);

//		ResetStorageAmmnts ();
//	}

//	void ResetStorageAmmnts(){
//		oreStored = 0;
//		waterStored = 0;
//		waterCapacityLeft = waterCapacity;
//		oreCapacityLeft = oreCapacity;
//		Debug.Log ("STORAGE: Storage now empty!");
//	}
}
