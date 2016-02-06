using UnityEngine;
using System.Collections;

public class Weapon_SortingLayer : MonoBehaviour {

    // This script needs to know what its parent's sorting layer and sorting order are when it is attached to a new transform.

    string correctSortingLayer = "Foreground";

    SpriteRenderer sr, parent_sr;

    int currSortingOrder;

    int offset = 1;

    void OnEnable()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    public void CheckSortingLayer(SpriteRenderer parentSR)
    {
        if (sr.sortingLayerName != correctSortingLayer)
        {
            sr.sortingLayerName = correctSortingLayer;
        }

        parent_sr = parentSR;

        currSortingOrder = parent_sr.sortingOrder;

        // Always go one layer over the weapon holder
        sr.sortingOrder = currSortingOrder + offset;

    }

    void LateUpdate()
    {
        if (sr != null && parent_sr != null)
        {
            if (parent_sr.sortingOrder != currSortingOrder)
            {
                currSortingOrder = parent_sr.sortingOrder;

                // Always go one layer over the weapon holder
                sr.sortingOrder = currSortingOrder + offset;
            }
        }
    }
}
