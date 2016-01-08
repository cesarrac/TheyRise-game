using UnityEngine;
using System.Collections;

[System.Serializable]
public class TileData  {
	
	public enum Types{
		rock,
		mineral,
		empty,
		water,
        food,
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
		generator,
        terraformer
	}
	public Types tileType;

	public bool hasBeenSpawned = false;
	
	public int maxResourceQuantity;

	public int movementCost = 1;

//	public GameObject tileAsGObj;
	
	public bool isWalkable = true;

	public float hp, def, attk, shield; 

	public string tileName;

	//public int energyCost, oreCost;
    public int nanoBotCost { get; protected set; }

    public int posX { get; protected set; }
    public int posY { get; protected set; }

	public TileData(int x, int y, string name, Types type, int resourceQuantity, int moveCost, float _hp, float _defence, float _attk, float _shield, int nCost){
        posX = x;
        posY = y;

        tileType = type;
		maxResourceQuantity = resourceQuantity;
		movementCost = moveCost;

        // MAKING ROCK UNWAKABLE

		if (type != Types.empty && type != Types.water) {
			isWalkable = false;
		}
		hp = _hp;
		def = _defence;
		attk = _attk;
		shield = _shield;
		tileName = name;
        nanoBotCost = nCost;
	}
	public TileData(int x, int y, Types type, int resourceQuantity, int moveCost){
        posX = x;
        posY = y;

        tileType = type;
		maxResourceQuantity = resourceQuantity;
		movementCost = moveCost;

		if (type != Types.empty && type != Types.water) {
			isWalkable = false;
		}
		tileName = type.ToString ();
	}
}
