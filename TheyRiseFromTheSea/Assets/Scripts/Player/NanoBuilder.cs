using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class NanoBuilder  {

    public int memoryBank { get; protected set; }
    public int cur_memory { get; protected set; }
    
    public int nanoBots { get; protected set; }

    int _wpnSlots, _toolSlots;
    public int weaponSlots { get { return _wpnSlots; } set { _wpnSlots = Mathf.Clamp(value, 1, 5); } }
    public int toolSlots { get { return _toolSlots; } set { _toolSlots = Mathf.Clamp(value, 1, 3); } }

    public List<TileData.Types> bpTypes { get; protected set; }

    public Dictionary<TileData.Types, Blueprint> blueprintsMap
    {
        get; protected set;
    }

    // Default or Starting Nanobuilder
    public NanoBuilder()
    {
        memoryBank = 50;
        cur_memory = memoryBank;
        nanoBots = 50;
        weaponSlots = 1;
        toolSlots = 1;

        blueprintsMap = new Dictionary<TileData.Types, Blueprint>();
        bpTypes = new List<TileData.Types>();
    }

    public bool CheckForBlueprint(TileData.Types bpType)
    {
        if (blueprintsMap.ContainsKey(bpType))
        {
            return true;
        }
        else
            return false;
    }

    public void AddBluePrint(TileData.Types bpType, Blueprint bp)
    {
        if (!blueprintsMap.ContainsKey(bpType))
        {
            blueprintsMap.Add(bpType, bp);
        }

        if (!bpTypes.Contains(bpType))
        {
            bpTypes.Add(bpType);

            if (bpType != TileData.Types.terraformer)
            {
                cur_memory -= bp.memoryCost;
            }
        }
           
    }

    public void RemoveBlueprint(TileData.Types bpType)
    {
        if (blueprintsMap.ContainsKey(bpType))
        {
            cur_memory += blueprintsMap[bpType].memoryCost;
            blueprintsMap.Remove(bpType);
        }
   

        if (bpTypes.Contains(bpType))
            bpTypes.Remove(bpType);

    }

    public void RemoveAllLoadedBlueprints()
    {
        blueprintsMap.Clear();
        bpTypes.Clear();
    }
}
