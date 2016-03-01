using UnityEngine;
using System.Collections;

public class Loader : MonoBehaviour {

	/// <summary>
	/// Finds the GM object at the start of the scene and connects buttons to the GM's load level method 
	/// </summary>

	GameMaster gm;

	// Use this for initialization
	void Start () {
		gm = GameObject.FindGameObjectWithTag ("GM").GetComponent<GameMaster> ();
	}

	//public void LoadLevelFromGM()
	//{
	//	if (gm) {
	//		gm.LoadLevel ();
	//	} else {
	//		Debug.Log("Can't load next level! GM script not found!");
	//	}
	//}

}
