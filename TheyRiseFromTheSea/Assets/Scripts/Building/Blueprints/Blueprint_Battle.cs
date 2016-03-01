using UnityEngine;
using System.Collections;
using System;

[System.Serializable]
public class TowerGunStats
{
    private int _ammo;
    public int startingAmmo;
    public int currAmmo { get { return _ammo; } set { _ammo = Mathf.Clamp(value, 1, 20); } }

    private float _reloadTime;
    public float startingReloadTime;
    public float currReloadTime { get { return _reloadTime; } set { _reloadTime = Mathf.Clamp(value, 1f, 10f); } }

    public TowerGunStats(int ammo, float r_time)
    {
        startingAmmo = ammo;
        startingReloadTime = r_time;

        Init();
    }

    void Init()
    {
        currAmmo = startingAmmo;
        currReloadTime = startingReloadTime;
    }
}


public class Blueprint_Battle : Blueprint {

    public TowerGunStats battleStats { get; protected set; }
    public UnitStats unitStats { get; protected set; }

    // FIX THIS! Battle Stats, unit stats?! Tile STats!?! TOO MANY STATS!!!!!!!!!!!!!!!!!!!!!!!
    // Tile Stats (HP, Defnse, Attk, Shield, Nanobot Cost)
    public TileStats tileStats { get; protected set; }

    public Blueprint_Battle(int ammo, float reload_time, float rate, float damage, float hp, float attk, float defense, float shield)
    {
        battleStats = new TowerGunStats(ammo, reload_time);
        unitStats = new UnitStats();
        unitStats.InitStartingStats(hp, defense, attk, shield, rate, damage, 0);
        unitStats.Init();
        tileStats = new TileStats(hp, defense, attk, shield, nanoBotCost);
    }

    public void UpgradeAmmo(int newAmmo)
    {
        battleStats.startingAmmo = newAmmo;
    }

    public void UpgradeReloadSpeed(float newReload)
    {
        battleStats.startingReloadTime = newReload;
    }

    public void UpgradeUnitStat(string id, float newAmmnt)
    {
        switch (id)
        {
            case "HP":
                unitStats.maxHP = newAmmnt;
                break;
            case "Defense":
                unitStats.startDefence = newAmmnt;
                break;
            case "Attack":
                unitStats.startAttack = newAmmnt;
                break;
            case "Shield":
                unitStats.startShield = newAmmnt;
                break;
            case "Rate":
                unitStats.startRate = newAmmnt;
                break;
            case "Damage":
                unitStats.startDamage = newAmmnt;
                break;
            default:
                // Change nothing
                break;
        }
    }
	
}
