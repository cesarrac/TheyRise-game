using UnityEngine;
using System.Collections;

public class Inventory_Unit : Inventory {

    int maxResources = 75;
    int curResources = 0;

    public override void AddResource(ResourceType rType, int ammnt)
    {
        if (curResources < ammnt && (curResources + ammnt) <= maxResources)
        {
            base.AddResource(rType, ammnt);
            curResources += ammnt;
        }
    
    }

}
