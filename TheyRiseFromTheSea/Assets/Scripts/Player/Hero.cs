using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Hero {
    public List<Weapon> weapons = new List<Weapon>();
    public List<Tool> tools = new List<Tool>();

    public Armor armor { get; protected set; }

    public NanoBuilder nanoBuilder { get; protected set; }

    // Default Constructor (1 Weapon, Armor, and 1 Tool) with Initial NanoBuilder
    public Hero(Weapon weapon, Armor _armor, Tool tool)
    {
        weapons.Add(weapon);
        armor = _armor;
        tools.Add(tool);

        nanoBuilder = new NanoBuilder();
    }

}
