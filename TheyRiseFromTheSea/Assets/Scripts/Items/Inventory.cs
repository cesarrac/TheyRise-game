using UnityEngine;
using System.Collections.Generic;

public class Inventory  {

    Dictionary<TileData.Types, int> resources_inventory; // For Resources like Water, Organics, etc. (NOT any rocks/ore)

    Dictionary<Rock.RockProductionType, int> ore_inventory; // Contains all types of rock that can be found on the planet

    // TODO: Add other items as potential inventories (like Item, Goods, Weapons, etc.)

    public Inventory()
    {
        resources_inventory = new Dictionary<TileData.Types, int>();
        ore_inventory = new Dictionary<Rock.RockProductionType, int>();
    }

    public void AddResource(TileData.Types rType, int ammnt)
    {
        if (resources_inventory.ContainsKey(rType))
        {
            resources_inventory[rType] += ammnt;
        }
        else
        {
            resources_inventory.Add(rType, ammnt);
        }
    }

    public void AddRock(Rock.RockProductionType rType, int ammnt)
    {
        if (ore_inventory.ContainsKey(rType))
        {
            ore_inventory[rType] += ammnt;
        }
        else
        {
            ore_inventory.Add(rType, ammnt);
        }
    }

    public void TakeResource(TileData.Types rType, int ammnt)
    {
        if (resources_inventory.ContainsKey(rType))
        {
            resources_inventory[rType] -= ammnt;
        }

    }
    public void TakeRock(Rock.RockProductionType rType, int ammnt)
    {
        if (ore_inventory.ContainsKey(rType))
        {
            ore_inventory[rType] -= ammnt;
        }

    }

    public bool CheckForResource(TileData.Types rType, int ammnt = 0)
    {
        if (resources_inventory.ContainsKey(rType))
        {
            if (resources_inventory[rType] >= ammnt)
                return true;
            else
                return false;
        }
        else
            return false;
    }

    public bool CheckForRock(Rock.RockProductionType rType, int ammnt = 0)
    {
        if (ore_inventory.ContainsKey(rType))
        {
            if (ore_inventory[rType] >= ammnt)
                return true;
            else
                return false;
        }
        else
            return false;
    }

    public int GetRockAmmnt(Rock.RockProductionType rType)
    {
        if (ore_inventory.ContainsKey(rType))
        {
            return ore_inventory[rType];
        }
        else
            return 0;
    }

    public int GetResourceAmmnt(TileData.Types rType)
    {
        if (resources_inventory.ContainsKey(rType))
        {
            return resources_inventory[rType];
        }
        else
            return 0;
    }

    public void MergeInventories(Inventory other)
    {
        foreach(TileData.Types resource in other.resources_inventory.Keys)
        {
            AddResource(resource, other.resources_inventory[resource]);
        }

        foreach (Rock.RockProductionType rock in other.ore_inventory.Keys)
        {
            AddRock(rock, other.ore_inventory[rock]);
        }
    }

}
