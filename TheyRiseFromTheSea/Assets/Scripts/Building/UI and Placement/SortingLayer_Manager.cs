using UnityEngine;
using System.Collections;

public class SortingLayer_Manager : MonoBehaviour {

	private SpriteRenderer spriteRenderer;
    private LineRenderer lineRenderer;
    public bool isLineRenderer;
    string correctSortingLayer = "Foreground";

    public bool isCharacterLimb;

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
		if (spriteRenderer != null && spriteRenderer.isVisible)
        {
            if (spriteRenderer.sortingLayerName != correctSortingLayer)
                spriteRenderer.sortingLayerName = correctSortingLayer;

            if (isCharacterLimb)
            {
                spriteRenderer.sortingOrder = ((int)Camera.main.WorldToScreenPoint(spriteRenderer.bounds.min).y * -1) * (int)transform.position.z;

            }
            else
            {
                spriteRenderer.sortingOrder = ((int)Camera.main.WorldToScreenPoint(spriteRenderer.bounds.min).y * -1);
            }
        }
			

        if (lineRenderer != null && lineRenderer.isVisible)
        {
            if (lineRenderer.sortingLayerName != correctSortingLayer)
                lineRenderer.sortingLayerName = correctSortingLayer;
            

            lineRenderer.sortingOrder = (int)Camera.main.WorldToScreenPoint(lineRenderer.bounds.min).y * -1;
        }

            
	}
	

}
