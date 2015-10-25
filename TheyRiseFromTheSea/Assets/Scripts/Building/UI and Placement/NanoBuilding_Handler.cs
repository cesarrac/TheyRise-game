using UnityEngine;
using System.Collections;

public class NanoBuilding_Handler : MonoBehaviour {


	/* This will actually build any of the Available Blueprints in the Player's gear. For Testing purposes and because it's 
	 * A LOT of logic I would have to change, I'm just going to use the Build method in the Building UI script and pass it
	 * a name of a building.
	 * Later on I would have to pass in whatever set options the Player selected as blueprints when selecting their Gear before
	 * loading this level */

	public Building_UIHandler building_handler;

	public int nanoBots = 50;

	void Start () {
	
	}
	

	void Update () {
		if (Input.GetMouseButtonDown (1)) {
			if (nanoBots >= 10){
				building_handler.BuildThis("Machine Gun");
				nanoBots -= 10;
			}else{
				Debug.Log("No more nanobots left!");
			}
		}
	}
}
