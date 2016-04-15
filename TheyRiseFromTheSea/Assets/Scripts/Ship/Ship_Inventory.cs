using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System;

public class Ship_Inventory : MonoBehaviour {

	public static Ship_Inventory Instance { get; protected set; }

    public Inventory main_Inventory;
    Inventory temp_Inventory;

    //public int waterStored { get; protected set; }
    //public int steelStored { get; protected set; }
    //public int foodStored { get; protected set; }
    //public int organicsStored { get; protected set; }

    //// Oxygen is NOT gathered on the planet, therefore it has no temporary storage
    //public int oxygenStored { get; protected set; }

    //// Same case with Energy
    //public int energyStored { get; protected set; }

    public int storageTotal { get; protected set; }


    // ## Use this to create a temporary inventory that gets added to ship inventory only once Player launches (succeeds in Terraforming)
    //public int tempWater { get; protected set; }
    //public int tempFood { get; protected set; }
    //public int tempSteel { get; protected set; }
    //public int tempVit { get; protected set; }
    //public int tempOrganics { get; protected set; }

    // CURRENCY:
    int credits;
    public int Credits { get { return credits; } }


    public Dictionary<Item, int> fabricatedGoodsMap { get; protected set; }
    //public Dictionary<TileData.Types, int> rawResourcesMap { get; protected set; }

    List<Weapon> availableWeapons;
    List<Armor> availableArmor;
    List<Tool> availableTools;

   // Action missionCompletedCB;
    Action dailyChargeFailCB, dailyChargeSuccessCB;

    public int vitCrystalsSold { get; protected set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            //fabricatedGoodsMap = new Dictionary<Item, int>();
            //rawResourcesMap = new Dictionary<TileData.Types, int>();
            temp_Inventory = new Inventory();
            main_Inventory = new Inventory();

        }
        else
        {
            DestroyImmediate(gameObject);
        }
    }

    public void LoadSavedInventory(Inventory savedInventory)
    {
        main_Inventory = new Inventory(savedInventory);
    }


    //public void RegisterCompleteMissionCallback(Action cb)
    //{
    //    missionCompletedCB = cb;
    //}

    //public void UnRegisterCompleteMissionCallback()
    //{
    //    missionCompletedCB = null;
    //}


    //************************************ USING RESOURCE TYPE

    public bool CheckForResource(ResourceType res, int ammnt, bool checkTemp = true)
    {
        if (checkTemp)
        {
            return temp_Inventory.CheckForResource(res, ammnt);
        }
        else
        {
            return main_Inventory.CheckForResource(res, ammnt);
        }
    }

    public int GetResourceAmmntFromMainInventory(ResourceType resource)
    {
        return main_Inventory.GetResourceAmmnt(resource);
    }

    public int GetResourceAmmntFromTempInventory(ResourceType resource)
    {
        return temp_Inventory.GetResourceAmmnt(resource);
    }

    public void AddTempResource(ResourceType rType, int ammnt)
    {
        temp_Inventory.AddResource(rType, ammnt);
        Player_UIHandler.instance.DisplayTransporterStorage(rType, temp_Inventory.GetResourceAmmnt(rType));
        //    if (missionCompletedCB != null)
        //    {
        //        missionCompletedCB();
        //    }
    }

    public void ChargeResourceFromTemp(ResourceType res, int ammnt)
    {
        temp_Inventory.TakeResource(res, ammnt);
    }

    public void AddToMainInventory(ResourceType rType, int ammnt)
    {
        main_Inventory.AddResource(rType, ammnt);
    }

    public void StoreTempInMainInventory()
    {
        main_Inventory.MergeInventories(temp_Inventory);

       // UnRegisterCompleteMissionCallback();
    }

    void AutoSellVitCrystals()
    {
        // vitCrystalsSold = tempVit;
        vitCrystalsSold = temp_Inventory.GetResourceAmmnt(ResourceType.Vit);
        Debug.Log("Selling " + vitCrystalsSold + " VIT Crystals!");
        //tempVit = 0;
       
    }

    public void ConfirmVitCrystalsSold()
    {
        vitCrystalsSold = 0;
    }

    public void InitUI()
    {
       // ResetTempInventory();

        if (SceneManager.GetActiveScene().name == "Level_Planet")
        {
          //  Player_UIHandler.instance.InitTransporterTempInventory(tempFood, tempWater, tempSteel);
            Player_UIHandler.instance.InitTransporterTempInventory(0, 0, 0);
        }

    }

    public void ResetTempInventory()
    {
        vitCrystalsSold = 0;
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

    // TRADE ORDERS:

    // If the above method returns true a button on the Trade Order UI will allow the player to complete the Order by...

    // ... giving their client the required Fabricated Goods ... 
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
                ConsumeDailyResource(ResourceType.Water, waterAmmnt) &&
                ConsumeDailyResource(ResourceType.Food, foodAmmnt)
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
    public bool ConsumeDailyResource(ResourceType resource, int ammnt)
    {
        // If inventory contains this resource...
        if (main_Inventory.CheckForResource(resource, ammnt))
        {
            main_Inventory.TakeResource(resource, ammnt);
            return true;
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
        if (main_Inventory.CheckForResource(ResourceType.Energy, cost))
        {
            main_Inventory.TakeResource(ResourceType.Energy, cost);
            return true;
        }
        else
        {
            return false;
        }
    }

    public void RechargeEnergy(int startingEnergy)
    {
        main_Inventory.AddResource(ResourceType.Energy, startingEnergy);
    }


}
