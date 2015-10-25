using UnityEngine;
using System.Collections;

public class PixelPerfectScale2 : MonoBehaviour {

	public float zoomScale = 4f;
	Camera myCam;

	void Start () {
		myCam = GetComponent<Camera> ();
	}
	
	void Update () {
		myCam.orthographicSize = (Screen.height / 100f) / zoomScale;
	}
}
