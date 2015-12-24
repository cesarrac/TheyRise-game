using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Ship_Inventory : MonoBehaviour {

	public static Ship_Inventory Instance { get; protected set; }

    public int waterStored { get; protected set; }
    public int oreStored { get; protected set; }
    public int foodStored { get; protected set; }

    public int storageTotal { get; protected set; }

    void Awake()
    {
        Instance = this;
    }

    public void ReceiveItems(TileData.Types rType, int ammnt)
    {
        Debug.Log(ammnt + " of " + rType + " beamed to the SHIP!");
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

        // add to the total
        storageTotal += ammnt;
    }

}
