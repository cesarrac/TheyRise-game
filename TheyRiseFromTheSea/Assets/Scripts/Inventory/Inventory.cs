using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class Inventory  {

    Dictionary<ResourceType, int> resources_inventory;

    // TODO: Add other items as potential inventories (like Item, Goods, Weapons, etc.)

    public Inventory()
    {
        resources_inventory = new Dictionary<ResourceType, int>();
    }

    // Copy Constructor
    public Inventory(Inventory other)
    {
        resources_inventory = new Dictionary<ResourceType, int>(other.resources_inventory);
    }

    public void MergeInventories(Inventory other)
    {
        foreach (ResourceType resource in other.resources_inventory.Keys)
        {
            AddResource(resource, other.resources_inventory[resource]);
        }
    }

    // Return ammounts (Getter)
    public int GetResourceAmmnt(ResourceType rType)
    {
        if (resources_inventory.ContainsKey(rType))
        {
            return resources_inventory[rType];
        }
        else
            return 0;
    }

    // Check for ammount (bool)
    public bool CheckForResource(ResourceType rType, int ammnt = 0)
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

    // Subtract Resource
    public void TakeResource(ResourceType rType, int ammnt)
    {
        if (resources_inventory.ContainsKey(rType))
        {
            resources_inventory[rType] -= ammnt;

            if (resources_inventory[rType] <= 0)
                resources_inventory.Remove(rType);
        }

    }

    // Add Resource
    public virtual void AddResource(ResourceType rType, int ammnt)
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
}
