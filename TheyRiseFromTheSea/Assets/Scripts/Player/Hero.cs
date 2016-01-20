using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Hero {
    public List<Weapon> weapons = new List<Weapon>();
    public List<Tool> tools = new List<Tool>();

    public Armor armor { get; protected set; }

    public NanoBuilder nanoBuilder { get; protected set; }

    public UnitStats heroStats { get; protected set; }

    // Default Constructor (1 Weapon, Armor, and 1 Tool) with Initial NanoBuilder
    public Hero(Weapon weapon, Armor _armor, Tool tool)
    {
        weapons.Add(weapon);
        armor = _armor;
        tools.Add(tool);

        heroStats = new UnitStats(100f, 2, armor.armorStats._curDefense, armor.armorStats._curShield);

        nanoBuilder = new NanoBuilder();

    }

    public void AddWeapon(Weapon wpn)
    {
        weapons.Add(wpn);
    }

    public void AddTool(Tool tool)
    {
        tools.Add(tool);
    }

    public void ChangeArmor (Armor _armor)
    {
        armor = _armor;

    }



}
