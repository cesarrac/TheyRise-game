using UnityEngine;
using System.Collections;

public class Ship_Manager : MonoBehaviour {

    // Needs that charge certain amounts of gathered resources of different types every day
    // Oxygen, Food, Water, Energy
    // Food = food resource gathered from the planet and processed on the ship
    // Water = water resource gathered from the planet and filtered on the ship
    // Oxygen = organics resrouce gathered from the planet and processed on the ship

    public static Ship_Manager Instance { get; protected set; }

    public int startingWater = 200, startingFood = 84, startingOxygen = 200, startingEnergy = 60;
    public int startingWaterConsumption = 6, startingFoodConsumption = 3, startingOxygenConsumption = 10;
    int currWaterCons, currFoodCons, currOxygenCons;

    // Maximum amount of days the Ship can go on emergency power before the player dies and it's GAME OVER!
    public int maxDaysInEmergency = 1;
    int currDaysInEmergency = 0;


    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            DestroyImmediate(gameObject);
        }
    }

    // GM can call this when starting a New Game
    public void InitStartingValues()
    {
        Ship_Inventory.Instance.StoreItems(TileData.Types.water, startingWater);
        Ship_Inventory.Instance.StoreItems(TileData.Types.food, startingFood);
        Ship_Inventory.Instance.StoreItems(TileData.Types.oxygen, startingFood);
        Ship_Inventory.Instance.StoreItems(TileData.Types.energy, startingFood);

        currWaterCons = startingWaterConsumption;
        currFoodCons = startingFoodConsumption;
        currOxygenCons = startingOxygenConsumption;
    }

    // GM calls this as it Ends a day (triggered by player pressing End Day button)
    public void ChargeDailyResources(int penalty = 0)
    {
        // Register the Failed consumption and Success consumption callbacks
        Ship_Inventory.Instance.RegisterConsumptionCallbacks(SetToEmergyStatus, DailyChargeSuccessful);

        // Recharge Energy to the current starting energy value
        Ship_Inventory.Instance.RechargeEnergy(startingEnergy);

        // Consume Daily Resources
        Ship_Inventory.Instance.ConsumeDailyResources(currWaterCons + penalty, currFoodCons + penalty, currOxygenCons + penalty);

    }

    // Call this when a daily resource requirement was not met
    public void SetToEmergyStatus()
    {
        if (currDaysInEmergency < maxDaysInEmergency)
        {
            currDaysInEmergency++;
        }
        else
        {
            // TODO: Add Game Over Logic to this!

            Debug.Log("SHIP MANAGER: Ship systems have failed!! It's GAME OVER!");
        }
    }

    // Call this when all daily resource requirements have been met
    public void DailyChargeSuccessful()
    {
        // TODO: Pop up message that tells the player that all is good on the ship and what resources were consumed

        Debug.Log("SHIP MANAGER: All systems are functional.");
    }

}
