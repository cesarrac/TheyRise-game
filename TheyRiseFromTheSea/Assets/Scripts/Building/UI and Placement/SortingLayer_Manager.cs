using UnityEngine;
using System.Collections;

public class SortingLayer_Manager : MonoBehaviour {

	private SpriteRenderer spriteRenderer;
    private LineRenderer lineRenderer;
    public bool isLineRenderer;

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
			spriteRenderer.sortingOrder = (int)Camera.main.WorldToScreenPoint (spriteRenderer.bounds.min).y * -1;

        if (lineRenderer != null)
            lineRenderer.sortingOrder = (int)Camera.main.WorldToScreenPoint(lineRenderer.bounds.min).y * -1;
	}
	

}
