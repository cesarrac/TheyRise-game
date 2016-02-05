using UnityEngine;
using System.Collections;

public class RigSorting_Manager : MonoBehaviour {
    private SpriteRenderer spriteRenderer;
    string correctSortingLayer = "Foreground";

    void Awake()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
    }

    void LateUpdate()
    {
        if (spriteRenderer != null && spriteRenderer.isVisible)
        {
            if (spriteRenderer.sortingLayerName != correctSortingLayer)
                spriteRenderer.sortingLayerName = correctSortingLayer;

            spriteRenderer.sortingOrder = (int)Camera.main.WorldToScreenPoint(spriteRenderer.bounds.min).z * -1;
        }
    }
}
