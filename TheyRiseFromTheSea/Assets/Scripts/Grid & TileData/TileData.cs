using UnityEngine;
using System.Collections;

[System.Serializable]
public class TileStats
{
    float _hp, startingHP, _shield, _defense, _attack;
    int _nanoBotCost;

    public float StartHP { get { return startingHP; } }
    public float HP { get { return _hp; } set { _hp = Mathf.Clamp(value, 0, startingHP); } }
    public float Shield { get { return _shield; } set { _shield = Mathf.Clamp(value, 0, 50000); } }
    public float Defense { get { return _defense; } set { _defense = Mathf.Clamp(value, 0, 50000); } }
    public float Attack { get { return _attack; } set { _attack = Mathf.Clamp(value, 0, 50000); } }
    //public int NanoBotCost { get { return _nanoBotCost; } set { _nanoBotCost = Mathf.Clamp(value, 0, 100); } }

    public TileStats(float hp, float def, float attk, float shield)
    {
        startingHP = hp;

        HP = hp;
  
        Shield = shield;
        Defense = def;
        Attack = attk;
        //NanoBotCost = nCost;
    }

    public TileStats()
    {

    }
}

public class TileData  {
	
	public enum Types{
		rock,
		mineral,
		empty,
		water,
        food,
        oxygen,
        energy,
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
        terraformer,
        wall
	}
	public Types tileType;

	public bool hasBeenSpawned = false;
	
	public int maxResourceQuantity;

	public int movementCost = 1;

//	public GameObject tileAsGObj;
	
	public bool isWalkable = true;

	//public float hp, def, attk, shield; 

    public TileStats tileStats { get; protected set; }

	public string tileName;

   // public int nanoBotCost { get; protected set; }

    // Value for Resources that can be extracted
    public float hardness { get; protected set; }

    public int posX { get; protected set; }
    public int posY { get; protected set; }

	public TileData(int x, int y, string name, Types type, int resourceQuantity, int moveCost, float _hp, float _defence, float _attk, float _shield){
        tileName = name;

        posX = x;
        posY = y;

        tileType = type;
		maxResourceQuantity = resourceQuantity;
		movementCost = moveCost;

        // MAKING ROCK UNWAKABLE

		if (type != Types.empty && type != Types.water) {
			isWalkable = false;
		}

        tileStats = new TileStats(_hp, _defence, _attk, _shield);
        
		//hp = _hp;
		//def = _defence;
		//attk = _attk;
		//shield = _shield;

        //nanoBotCost = nCost;
	}
	public TileData(int x, int y, Types type, int resourceQuantity, int moveCost, int _hardness = 0){
        posX = x;
        posY = y;

        tileType = type;
		maxResourceQuantity = resourceQuantity;

        hardness = _hardness;

		movementCost = moveCost;

        // MAKING ROCK UNWAKABLE
        if (type != Types.empty && type != Types.water) {
			isWalkable = false;
		}
		tileName = type.ToString ();

        tileStats = new TileStats();
	}
}
