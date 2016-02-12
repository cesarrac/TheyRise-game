using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;


public class Hero {
    public List<Weapon> weapons = new List<Weapon>();
    public List<Tool> tools = new List<Tool>();

    public Armor armor { get; protected set; }

    public NanoBuilder nanoBuilder { get; protected set; }

    public UnitStats heroStats { get; protected set; }

    public string heroName { get; protected set; }

    // Default Constructor (1 Weapon, Armor, and 1 Tool) with Initial NanoBuilder
    public Hero(string name, Weapon weapon, Armor _armor, Tool tool, float maxHP, float curHP, float attk, NanoBuilder builder = null)
    {
        heroName = name;
        weapons.Add(weapon);
        armor = _armor;
        tools.Add(tool);

        heroStats = new UnitStats(maxHP, attk, armor.armorStats._curDefense, armor.armorStats._curShield);

        heroStats.curHP = curHP;

        if (builder == null)
            nanoBuilder = new NanoBuilder();
        else
            nanoBuilder = builder;

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

[Serializable]
public class HeroData
{
    public float curHP { get; protected set; }
    public float maxHP { get; protected set; }
    public float Attack { get; protected set; }

    public HeroData(float maxHitPoints, float curHitpoints, float attk)
    {
        maxHP = maxHitPoints;
        curHP = curHitpoints;
        Attack = attk;
    }
}
