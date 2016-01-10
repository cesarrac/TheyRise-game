using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Ship_Inventory : MonoBehaviour {

	public static Ship_Inventory Instance { get; protected set; }

    public int waterStored { get; protected set; }
    public int oreStored { get; protected set; }
    public int foodStored { get; protected set; }

    public int storageTotal { get; protected set; }

    public int commonOreStored { get; protected set; }
    public int enrichedOreStored { get; protected set; }

    int tempWater, tempOre, tempFood, tempCommorOre, tempEnrichedOre; // ## Use this to create a temporary inventory that gets added to ship inventory only once Player launches (succeeds in Terraforming)
    

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

        commonOreStored += tempCommorOre;
        enrichedOreStored += tempEnrichedOre;

        tempWater = 0;
        tempOre = 0;
        tempFood = 0;
        tempCommorOre = 0;
        tempEnrichedOre = 0;

    }


    public void InitUI()
    {
        if (Application.loadedLevel == 1)
        {
            Player_UIHandler.instance.InitPlanetUIText(tempFood, tempWater, tempOre);
        }
        else if (Application.loadedLevel == 0)
        {
            Player_UIHandler.instance.InitShipUI(foodStored, waterStored, oreStored);
        }
      
        
    }





}
