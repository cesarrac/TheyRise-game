using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System;

public class Ship_Inventory : MonoBehaviour {

	public static Ship_Inventory Instance { get; protected set; }

    Inventory main_Inventory;
    Inventory temp_Inventory;

    public int waterStored { get; protected set; }
    public int steelStored { get; protected set; }
    public int foodStored { get; protected set; }
    public int organicsStored { get; protected set; }

    // Oxygen is NOT gathered on the planet, therefore it has no temporary storage
    public int oxygenStored { get; protected set; }

    // Same case with Energy
    public int energyStored { get; protected set; }

    public int storageTotal { get; protected set; }


    // ## Use this to create a temporary inventory that gets added to ship inventory only once Player launches (succeeds in Terraforming)
    public int tempWater { get; protected set; }
    public int tempFood { get; protected set; }
    public int tempSteel { get; protected set; }
    public int tempVit { get; protected set; }
    public int tempOrganics { get; protected set; }

    // CURRENCY:
    int credits;
    public int Credits { get { return credits; } }


    public Dictionary<Item, int> fabricatedGoodsMap { get; protected set; }
    public Dictionary<TileData.Types, int> rawResourcesMap { get; protected set; }

    List<Weapon> availableWeapons;
    List<Armor> availableArmor;
    List<Tool> availableTools;

    Action missionCompletedCB;
    Action dailyChargeFailCB, dailyChargeSuccessCB;

    public int vitCrystalsSold { get; protected set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            fabricatedGoodsMap = new Dictionary<Item, int>();
            rawResourcesMap = new Dictionary<TileData.Types, int>();

            main_Inventory = new Inventory();

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
        //switch (rType)
        //{
        //    case TileData.Types.water:
        //        tempWater += ammnt;
        //        if (ammnt != 0)
        //            Player_UIHandler.instance.DisplayTransporterStorage(rType, tempWater);
        //        break;
        //    case TileData.Types.food:
        //        tempFood += ammnt;
        //        if (ammnt != 0)
        //            Player_UIHandler.instance.DisplayTransporterStorage(rType, tempFood);
        //        break;
        //    default:
        //        // Cant find that resource
        //        break;
        //}

        // add to the total
        storageTotal += ammnt;

        //TEST:
        temp_Inventory.AddResource(rType, ammnt);

        if (missionCompletedCB != null)
        {
            missionCompletedCB();
        }
    }

    public void ReceiveTempRock(int ammnt, Rock.RockProductionType rockType)
    {
        //TEST:
        temp_Inventory.AddRock(rockType, ammnt);
        Player_UIHandler.instance.DisplayTransporterStorage(TileData.Types.rock, temp_Inventory.GetRockAmmnt(rockType));

        //if (rockType == Rock.RockProductionType.steel)
        //{
        //    tempSteel += ammnt;
        //    Player_UIHandler.instance.DisplayTransporterStorage(TileData.Types.rock, tempSteel);
        //}
        //else if (rockType == Rock.RockProductionType.vit)
        //{
        //    tempVit += ammnt;
        //}
    }

    public void StoreResource(TileData.Types rType, int ammnt)
    {
        //if (rawResourcesMap.ContainsKey(rType))
        //{
        //    rawResourcesMap[rType] += ammnt;
        //}
        //else
        //{
        //    rawResourcesMap.Add(rType, ammnt);
        //}

        main_Inventory.AddResource(rType, ammnt);

        // add to the total
        storageTotal += ammnt;
    }

    public int CheckForSpecificResource(TileData.Types resource, bool checkTemp = false)
    {
        if (checkTemp)
        {
            return temp_Inventory.GetResourceAmmnt(resource);
        }
        else
        {
            return main_Inventory.GetResourceAmmnt(resource);
        }
        //int ammnt = 0;
        //if (!checkTemp)
        //{
        //    if (rawResourcesMap.ContainsKey(resource))
        //    {
        //        ammnt = rawResourcesMap[resource];
        //    }
        //}
        //else
        //{
          
        //    switch (resource)
        //    {
        //        case TileData.Types.water:
        //            ammnt = tempWater;
        //            break;
        //        case TileData.Types.rock:
        //            ammnt = tempSteel;
        //            break;
        //        case TileData.Types.food:
        //            ammnt = tempFood;
        //            break;
        //        default:
        //            // Cant find that resource
        //            break;
        //    }
        //}

        //return ammnt;
    }

    public bool CheckForResourceByAmmnt(TileData.Types resource, int ammnt, bool checkTemp = true)
    {
        if (checkTemp)
        {
            return temp_Inventory.CheckForResource(resource, ammnt);
        }
        else
        {
            return main_Inventory.CheckForResource(resource, ammnt);
        }
        //bool containsResource = false;

        //if (checkTemp)
        //{
        //    switch (resource)
        //    {
        //        case TileData.Types.water:
        //            if (tempWater >= ammnt)
        //            {
        //                containsResource = true;
        //            }
        //            break;
        //        case TileData.Types.rock:
        //            if (tempSteel >= ammnt)
        //            {
        //                containsResource = true;
        //            }
        //            break;
        //        case TileData.Types.food:
        //            if (tempFood >= ammnt)
        //            {
        //                containsResource = true;
        //            }
        //            break;
        //        default:
        //            // Cant find that resource
        //            containsResource = false;
        //            break;
        //    }
        //}
        //else
        //{
        //    if (rawResourcesMap.ContainsKey(resource))
        //    {
        //        if (rawResourcesMap[resource] >= ammnt)
        //        {
        //            containsResource = true;
        //        }
        //    }
        //}


        //return containsResource;
    }

    /// <summary>
    /// Call this from the Launchpad when the Player launches back to the ship. 
    /// At this point Terraformer stages would be complete and the Launchpad should be ready to send items to ship.
    /// </summary>
    public void RegisterTempInventoryToShip()
    {
        //waterStored += tempWater;

        //AutoSellVitCrystals();

        //steelStored += tempSteel;

        //foodStored += tempFood;

        //StoreResource(TileData.Types.water, tempWater);
        //StoreResource(TileData.Types.rock, tempSteel);
        //StoreResource(TileData.Types.food, tempFood);

        // TEST: Merge the current temp inventory with the main inventory
        main_Inventory.MergeInventories(temp_Inventory);
        temp_Inventory = null;

        //UnRegisterCompleteMissionCallback();
    }

    void AutoSellVitCrystals()
    {
        // vitCrystalsSold = tempVit;
        vitCrystalsSold = temp_Inventory.GetRockAmmnt(Rock.RockProductionType.vit);
        Debug.Log("Selling " + vitCrystalsSold + " VIT Crystals!");
        tempVit = 0;
       
    }

    public void ConfirmVitCrystalsSold()
    {
        vitCrystalsSold = 0;
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
       // ResetTempInventory();

        if (SceneManager.GetActiveScene().name == "Level_Planet")
        {
            Player_UIHandler.instance.InitTransporterTempInventory(tempFood, tempWater, tempSteel);
        }

    }

    public void ResetTempInventory()
    {

        //tempWater = 0;
        //tempFood = 0;
        //tempSteel = 0;
        //tempVit = 0;
        //vitCrystalsSold = 0;
        temp_Inventory = new Inventory();
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

    // FOR BUILDING WE USE THE TEMPORARY INVENTORY (on the planet surface inventory)
    public void ChargeResourcesFromTemp(TileData.Types resource, int ammnt)
    {
        // TODO: Instead of using the TileData type "rock", I SHOULD be using the RockType "Steel"
        if (resource == TileData.Types.rock)
        {
            temp_Inventory.TakeRock(Rock.RockProductionType.steel, ammnt);
            Player_UIHandler.instance.DisplayTransporterStorage(resource, temp_Inventory.GetRockAmmnt(Rock.RockProductionType.steel));
        }
        else
        {
            temp_Inventory.TakeResource(resource, ammnt);
            Player_UIHandler.instance.DisplayTransporterStorage(resource, temp_Inventory.GetResourceAmmnt(resource));
        }

        //switch (resource)
        //{
        //    case TileData.Types.water:
        //        tempWater -= ammnt;
        //        Player_UIHandler.instance.DisplayTransporterStorage(resource, tempWater);
        //        break;
        //    case TileData.Types.rock:
        //        tempSteel -= ammnt;
        //        Player_UIHandler.instance.DisplayTransporterStorage(resource, tempSteel);
        //        break;
        //    case TileData.Types.food:
        //        tempFood -= ammnt;
        //        Player_UIHandler.instance.DisplayTransporterStorage(resource, tempFood);
        //        break;
        //    default:
        //        // Cant find that resource
                
        //        break;
        //}
    }

    // TRADE ORDERS:

    // If the above method returns true a button on the Trade Order UI will allow the player to complete the Order by...

    // ... giving their client the required Fabricated Goods ... 
    public void TakeGoods(Item good, int ammnt)
    {
        if (!fabricatedGoodsMap.ContainsKey(good))
            return;

        fabricatedGoodsMap[good] -= ammnt;

        if (fabricatedGoodsMap[good] <= 0)
        {
            fabricatedGoodsMap.Remove(good);
        }
    }
    // ... or required Raw Resource.
    public void TakeResources(TileData.Types resource, int ammnt)
    {
        if (!rawResourcesMap.ContainsKey(resource))
            return;

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
