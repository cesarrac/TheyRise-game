using UnityEngine;
using System.Collections;

[System.Serializable]
public class TileData  {
	
	public enum Types{
		rock,
		mineral,
		empty,
		water,
		building,
		capital,
		farm_s,
		nutrient,
		extractor,
		desalt_s,
		desalt_m,
		desalt_l,
		house,
		seaWitch,
		cannons,
		machine_gun,
		harpoonHall,
		storage,
		sniper,
		generator
	}
	public Types tileType;

	public bool hasBeenSpawned = false;
	
	public int maxResourceQuantity;

	public int movementCost = 1;

//	public GameObject tileAsGObj;
	
	public bool isWalkable = true;

	public float hp, def, attk, shield; 

	public string tileName;

	public int energyCost, oreCost;

	public TileData(string name, Types type, int resourceQuantity, int moveCost, float _hp, float _defence, float _attk, float _shield, int eCost, int oCost){
		tileType = type;
		maxResourceQuantity = resourceQuantity;
		movementCost = moveCost;

        // MAKING ROCK UNWAKABLE

		if (type != Types.empty && type != Types.mineral && type != Types.water) {
			isWalkable = false;
		}
		hp = _hp;
		def = _defence;
		attk = _attk;
		shield = _shield;
		tileName = name;
		energyCost = eCost;
		oreCost = oCost;
	}
	public TileData(Types type, int resourceQuantity, int moveCost){
		tileType = type;
		maxResourceQuantity = resourceQuantity;
		movementCost = moveCost;

		if (type != Types.empty && type != Types.mineral && type != Types.water) {
			isWalkable = false;
		}
		tileName = type.ToString ();
	}
}
