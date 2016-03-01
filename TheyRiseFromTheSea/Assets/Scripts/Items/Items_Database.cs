using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Items_Database : MonoBehaviour {

    public static Items_Database Instance { get; protected set; }

    Dictionary<string, Weapon> weaponsMap;
    Dictionary<string, Armor> armorMap;
    Dictionary<string, Tool> toolsMap;

    void Awake()
    {
        Instance = this;
        InitWeapons();
        InitArmors();
        InitTools();
    }

    void InitWeapons()
    {
        // FIX THIS! This data should be coming from one giant database of Weapons, Armor and Tools!!!
        weaponsMap = new Dictionary<string, Weapon>
        {
            {"Kinetic Rifle", new Weapon("Kinetic Rifle", 0, 12, 1.5f, 5, 50, "explosive bullet") },
            {"Freeze Gun", new Weapon("Freeze Gun", 0.2f, 100, 3f, 0, 30, "ice bullet")}
        };

    }

    void InitArmors()
    {
        armorMap = new Dictionary<string, Armor>
        {
            {"Vacumn Suit",  new Armor("Vacumn Suit", 5, 0)}
        };
    }

    void InitTools()
    {
        toolsMap = new Dictionary<string, Tool>
        {
            { "Mining Drill", new Tool("Mining Drill") }
        };
    }

    public Weapon GetWeaponfromID(string id)
    {
        if (weaponsMap.ContainsKey(id))
        {
            return weaponsMap[id];
        }
        else
            return null;
    }

    public Armor GetArmorfromID(string id)
    {
        if (armorMap.ContainsKey(id))
        {
            return armorMap[id];
        }
        else
            return null;
    }

    public Tool GetToolfromID(string id)
    {
        if (toolsMap.ContainsKey(id))
        {
            return toolsMap[id];
        }
        else
            return null;
    }


}
