using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System;

public class Ship_Inventory : MonoBehaviour {

	public static Ship_Inventory Instance { get; protected set; }

    public int waterStored { get; protected set; }
    public int oreStored { get; protected set; }
    public int foodStored { get; protected set; }
    public int organicsStored { get; protected set; }

    // Oxygen is NOT gathered on the planet, therefore it has no temporary storage
    public int oxygenStored { get; protected set; }

    // Same case with Energy
    public int energyStored { get; protected set; }

    public int storageTotal { get; protected set; }

    public int commonOreStored { get; protected set; }
    public int enrichedOreStored { get; protected set; }

    // ## Use this to create a temporary inventory that gets added to ship inventory only once Player launches (succeeds in Terraforming)
    public int tempWater { get; protected set; }
    public int tempOre { get; protected set; }
    public int tempFood { get; protected set; }
    public int tempCommorOre { get; protected set; }
    public int tempEnrichedOre { get; protected set; }
    public int tempOrganics { get; protected set; }




    public int credits { get; protected set; }

    public Dictionary<Item, int> fabricatedGoodsMap { get; protected set; }
    public Dictionary<TileData.Types, int> rawResourcesMap { get; protected set; }

    List<Weapon> availableWeapons;
    List<Armor> availableArmor;
    List<Tool> availableTools;

    Action missionCompletedCB;
    Action dailyChargeFailCB, dailyChargeSuccessCB;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            fabricatedGoodsMap = new Dictionary<Item, int>();
            rawResourcesMap = new Dictionary<TileData.Types, int>();
        }
        else
        {
            DestroyImmediate(gameObject);
        }
    }

    public void RegisterCompleteMissionCallback(Action cb)
    {
        missionCompletedCB = cb;
    }

    public void UnRegisterCompleteMissionCallback()
    {
        missionCompletedCB = null;
    }

    /// <summary>
    /// Stores items in the temporary Ship Inventory. They will not be registered
    /// to the actual inventory until the Player launches back to the ship.
    /// </summary>
    /// <param name="rType"></param>
    /// <param name="ammnt"></param>
    public void ReceiveTemporaryResources(TileData.Types rType, int ammnt)
    {
       // Debug.Log(ammnt + " of " + rType + " beamed to the SHIP!");
        switch (rType)
        {
            case TileData.Types.water:
                tempWater += ammnt;
                if (ammnt != 0)
                    Player_UIHandler.instance.DisplayTransporterStorage(rType, tempWater);
                break;
            case TileData.Types.rock:
                tempOre += ammnt;
                if (ammnt != 0)
                    Player_UIHandler.instance.DisplayTransporterStorage(rType, tempOre);
                break;
            case TileData.Types.food:
                tempFood += ammnt;
                if (ammnt != 0)
                    Player_UIHandler.instance.DisplayTransporterStorage(rType, tempFood);
                break;
            default:
                // Cant find that resource
                break;
        }

        // add to the total
        storageTotal += ammnt;

        if (missionCompletedCB != null)
        {
            missionCompletedCB();
        }
    }

    public void StoreItems(TileData.Types rType, int ammnt)
    {
        if (rawResourcesMap.ContainsKey(rType))
        {
            rawResourcesMap[rType] += ammnt;
        }
        else
        {
            rawResourcesMap.Add(rType, ammnt);
        }

        // add to the total
        storageTotal += ammnt;
    }

    public void SplitOre(int common, int enriched)
    {
        commonOreStored += common;
        enrichedOreStored += enriched;
       // Debug.Log("Ore has been split! Common = " + commonOreStored + " Enriched = " + enrichedOreStored);
    }

    public int CheckForSpecificResource(TileData.Types resource, bool checkTemp = false)
    {
        int ammnt = 0;
        if (!checkTemp)
        {
            if (rawResourcesMap.ContainsKey(resource))
            {
                ammnt = rawResourcesMap[resource];
            }
        }
        else
        {
          
            switch (resource)
            {
                case TileData.Types.water:
                    ammnt = tempWater;
                    break;
                case TileData.Types.rock:
                    ammnt = tempOre;
                    break;
                case TileData.Types.food:
                    ammnt = tempFood;
                    break;
                default:
                    // Cant find that resource
                    break;
            }
        }

        return ammnt;
    }

    /// <summary>
    /// Call this from the Launchpad when the Player launches back to the ship. 
    /// At this point Terraformer stages would be complete and the Launchpad should be ready to send items to ship.
    /// </summary>
    public void RegisterTempInventoryToShip()
    {
        waterStored += tempWater;

        oreStored += tempOre;

        foodStored += tempFood;

        StoreItems(TileData.Types.water, tempWater);
        StoreItems(TileData.Types.rock, tempOre);
        StoreItems(TileData.Types.food, tempFood);

        commonOreStored += tempCommorOre;
        enrichedOreStored += tempEnrichedOre;

        UnRegisterCompleteMissionCallback();
    }



    public int DisplayResourceAmount(TileData.Types key)
    {
        if (rawResourcesMap.ContainsKey(key))
        {
            return rawResourcesMap[key];
        }
        else
            return 0;
    }



    public void InitUI()
    {
        ResetTempInventory();

        if (SceneManager.GetActiveScene().name == "Level_Planet")
        {
            Player_UIHandler.instance.InitTransporterTempInventory(tempFood, tempWater, tempOre);
        }

    }

    public void ResetTempInventory()
    {

        tempWater = 0;
        tempOre = 0;
        tempFood = 0;
        tempCommorOre = 0;
        tempEnrichedOre = 0;
    }


    // FABRICATOR:
    public void AddFabricatedGood(Item good, int ammnt)
    {
        if (fabricatedGoodsMap.ContainsKey(good))
        {
            fabricatedGoodsMap[good] += ammnt;
        }
        else
        {
            fabricatedGoodsMap.Add(good, ammnt);
        }
    }

    
    // Whenever the player opens the info of a Current trade order, call this method below to check if the required good are available
    public bool CheckForGoods(Item good, int ammnt)
    {
        if (fabricatedGoodsMap.ContainsKey(good))
        {
            if (fabricatedGoodsMap[good] >= ammnt)
            {
                return true;
            }
        }

        return false;
    }

    public bool CheckForRawResources(TileData.Types resource, int ammnt)
    {
        if (rawResourcesMap.ContainsKey(resource))
        {
            if (rawResourcesMap[resource] >= ammnt)
            {
                return true;
            }
        }

        return false;
    }


    // TRADE ORDERS:

    // If the above method returns true a button on the Trade Order UI will allow the player to complete the Order by...

    // ... giving their client the required Fabricated Goods ... 
    public void TakeGoods(Item good, int ammnt)
    {
        fabricatedGoodsMap[good] -= ammnt;

        if (fabricatedGoodsMap[good] <= 0)
        {
            fabricatedGoodsMap.Remove(good);
        }
    }
    // ... or required Raw Resource.
    public void TakeResources(TileData.Types resource, int ammnt)
    {
        rawResourcesMap[resource] -= ammnt;

        if (rawResourcesMap[resource] <= 0)
        {
            rawResourcesMap.Remove(resource);
        }
    }

    public void RegisterConsumptionCallbacks(Action cbFail, Action cbSuccess)
    {
        if (dailyChargeSuccessCB == null)
            dailyChargeSuccessCB = cbSuccess;

        if (dailyChargeFailCB == null)
            dailyChargeFailCB = cbFail;
    }

    // used by Ship Manager

    public void ConsumeDailyResources(int waterAmmnt, int foodAmmnt, int oxygenAmmnt)
    {
        // if ALL 3 return true call the success callback,
        // if NOT all 3 return true call fail callback

        if (   
                ConsumeDailyResource(TileData.Types.water, waterAmmnt) &&
                ConsumeDailyResource(TileData.Types.water, foodAmmnt) &&
                ConsumeDailyResource(TileData.Types.water, oxygenAmmnt)
           )
        {

            // Call the succesful callback to tell the Ship Manager all is good
            if (dailyChargeSuccessCB != null)
                dailyChargeSuccessCB();
        }
        else
        {
            // Call the fail callback to tell the Ship Manager to go into Emergency state 
            if (dailyChargeFailCB != null)
                dailyChargeFailCB();
        }

    }
    
    // TODO: Consider making this a function return a bool so that we know if it succeeded or not
    public bool ConsumeDailyResource(TileData.Types resource, int ammnt)
    {
        // If inventory contains this resource...
        if (rawResourcesMap.ContainsKey(resource))
        {
            // ... and more than or equal the ammnt...
            if (rawResourcesMap[resource] >= ammnt)
            {
                // subtract the ammnt from it.
                rawResourcesMap[resource] -= ammnt;

                CheckIfResourceAtZero(resource);

                return true;
            }
            else
            {
                // Doesn't have enough of it in store!


                return false;

            }

        }
        else
        {
            // It has none of that resource in store!
            // Call the fail callback to tell the Ship Manager to go into Emergency state 
            if (dailyChargeFailCB != null)
                dailyChargeFailCB();

            return false;
        }

    }

    /// <summary>
    /// Consumes Energy if stored energy is more than or equal to cost. Returns true if it was succesful.
    /// </summary>
    /// <param name="cost"></param>
    /// <returns></returns>
    public bool ConsumeEnergy(int cost)
    {
        if (rawResourcesMap.ContainsKey(TileData.Types.energy))
        {
            if (rawResourcesMap[TileData.Types.energy] >= cost)
            {
                rawResourcesMap[TileData.Types.energy] -= cost;
                return true;
            }
            else
                return false;
        }
        else
            return false;
    }

    public void RechargeEnergy(int startingEnergy)
    {
        if (rawResourcesMap.ContainsKey(TileData.Types.energy))
        {
            rawResourcesMap[TileData.Types.energy] = startingEnergy;
        }
    }

    void CheckIfResourceAtZero(TileData.Types resource)
    {
        if (rawResourcesMap[resource] <= 0)
        {
            rawResourcesMap.Remove(resource);
        }
    }

}
