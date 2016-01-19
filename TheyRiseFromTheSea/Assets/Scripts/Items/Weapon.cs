using UnityEngine;
using System.Collections;


[System.Serializable]
public class GunStats
{
    float _fireRate;
    public float curFireRate { get { return _fireRate; } set { _fireRate = Mathf.Clamp(value, 0f, 2f); } }
    public float startingFireRate;

    public int startingChamberAmmo { get; protected set; }
    int _chamberAmmo;
    public int curChamberAmmo { get { return _chamberAmmo; } set { _chamberAmmo = Mathf.Clamp(value, 0, 50000); } }

    float _reloadSpeed;
    public float curReloadSpeed { get { return _reloadSpeed; } set { _reloadSpeed = Mathf.Clamp(value, 0.1f, 3f); } }
    public float startingReloadSpeed;

    public int weaponIndex;
    public string projectileType;

    public float kickAmmt;

    public bool shootsProjectiles;
    
    public GunStats(float rate, float reloadSpd, int ammo)
    {
        startingFireRate = rate;
        startingReloadSpeed = reloadSpd;
        startingChamberAmmo = ammo;
    }

    public void Init()
    {
        curFireRate = startingFireRate;
        curChamberAmmo = startingChamberAmmo;
        curReloadSpeed = startingReloadSpeed;
    }
}

// This weapon class can hold Gun Stats. The PlayerGun Base can hold all the basic shooting and follow mouse funtions of a gun and can sit as a component on a GameObject.
// This class will be constructed by the GameMaster as it keeps track of the guns the Hero purchases and upgrades.
// When the Hero is spawned the PlayerGun Base sitting in its weapon gameobject will ask the GameMaster to fill its corresponding weapon from the Hero's current weapons.

public class Weapon : Item
{

    public GunStats gunStats;

    public Weapon(string name, float fireRate, int ammo, float reloadSpeed)
    {
        itemName = name;
        itemType = ItemType.Weapon;
        gunStats = new GunStats(fireRate, reloadSpeed, ammo);
        gunStats.Init();
    }



    public void Upgrade(ItemUpgradeType upgradeType)
    {
        //switch (U)
        //{
        //    //            case attack rate type:
        //    //            weapon.stats.attackRate = U.attackRate;
        //    //            break;
        //    //        }
        //}
    }
}
