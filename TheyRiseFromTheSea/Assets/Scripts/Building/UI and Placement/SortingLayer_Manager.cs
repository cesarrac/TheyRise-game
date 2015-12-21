using UnityEngine;
using System.Collections;

public class SortingLayer_Manager : MonoBehaviour {

	private SpriteRenderer spriteRenderer;
    private LineRenderer lineRenderer;
    public bool isLineRenderer;
    string correctSortingLayer = "Foreground";

	void Awake(){
        if (!isLineRenderer)
        {
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }
        }
        else
        {
            if (lineRenderer == null)
            {
                lineRenderer = GetComponent<LineRenderer>();
            }
        }
		
	}

	void LateUpdate () 
	{
		if (spriteRenderer != null)
        {
            if (spriteRenderer.sortingLayerName != correctSortingLayer)
                spriteRenderer.sortingLayerName = correctSortingLayer;

            spriteRenderer.sortingOrder = (int)Camera.main.WorldToScreenPoint(spriteRenderer.bounds.min).y * -1;
        }
			

        if (lineRenderer != null)
        {
            if (lineRenderer.sortingLayerName != correctSortingLayer)
                lineRenderer.sortingLayerName = correctSortingLayer;
            

            lineRenderer.sortingOrder = (int)Camera.main.WorldToScreenPoint(lineRenderer.bounds.min).y * -1;
        }

            
	}
	

}
