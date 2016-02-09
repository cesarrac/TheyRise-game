using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class Ship_Inventory : MonoBehaviour {

	public static Ship_Inventory Instance { get; protected set; }

    public int waterStored { get; protected set; }
    public int oreStored { get; protected set; }
    public int foodStored { get; protected set; }

    public int storageTotal { get; protected set; }

    public int commonOreStored { get; protected set; }
    public int enrichedOreStored { get; protected set; }

    // ## Use this to create a temporary inventory that gets added to ship inventory only once Player launches (succeeds in Terraforming)
    public int tempWater { get; protected set; }
    public int tempOre { get; protected set; }
    public int tempFood { get; protected set; }
    public int tempCommorOre { get; protected set; }
    public int tempEnrichedOre { get; protected set; }




    public int credits { get; protected set; }

    public Dictionary<Item, int> fabricatedGoodsMap { get; protected set; }
    public Dictionary<TileData.Types, int> rawResourcesMap { get; protected set; }

    List<Weapon> availableWeapons;
    List<Armor> availableArmor;
    List<Tool> availableTools;

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

    void Start()
    {

    }

    /// <summary>
    /// Stores items in the temporary Ship Inventory. They will not be registered
    /// to the actual inventory until the Player launches back to the ship.
    /// </summary>
    /// <param name="rType"></param>
    /// <param name="ammnt"></param>
    public void ReceiveItems(TileData.Types rType, int ammnt)
    {
        Debug.Log(ammnt + " of " + rType + " beamed to the SHIP!");
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
        Debug.Log("Ore has been split! Common = " + commonOreStored + " Enriched = " + enrichedOreStored);
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


    // MISSIONS:

    // If the above method returns true a button on the Trade Order UI will allow the player to complete the Order by...

    // ... giving their client the required Fabricated Goods ... 
    public void DeliverGoods(Item good, int ammnt)
    {
        fabricatedGoodsMap[good] -= ammnt;

        if (fabricatedGoodsMap[good] <= 0)
        {
            fabricatedGoodsMap.Remove(good);
        }
    }
    // ... or required Raw Resource.
    public void DeliverResources(TileData.Types resource, int ammnt)
    {
        rawResourcesMap[resource] -= ammnt;

        if (rawResourcesMap[resource] <= 0)
        {
            rawResourcesMap.Remove(resource);
        }
    }



}
