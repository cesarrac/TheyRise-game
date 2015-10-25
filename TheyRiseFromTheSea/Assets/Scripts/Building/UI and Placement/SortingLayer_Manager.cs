using UnityEngine;
using System.Collections;

public class SortingLayer_Manager : MonoBehaviour {

	private SpriteRenderer spriteRenderer;

	void Awake(){
		if (spriteRenderer == null) {
			spriteRenderer = GetComponent<SpriteRenderer> ();
		}
	}

	void LateUpdate () 
	{
		if (spriteRenderer != null)
			spriteRenderer.sortingOrder = (int)Camera.main.WorldToScreenPoint (spriteRenderer.bounds.min).y * -1;
	}
	

}
