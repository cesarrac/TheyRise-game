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

    float _damage;
    public float damage { get { return _damage; } set { _damage = Mathf.Clamp(value, 1, 500); } }

    public int weaponIndex;
    public string projectileType;

    public float kickAmmt { get; protected set; }

    public bool shootsProjectiles { get; protected set; }
    
    // Projectile Shooters
    public GunStats(float rate, float reloadSpd, int ammo, float dmg, float kick, string projectileName)
    {
        startingFireRate = rate;
        startingReloadSpeed = reloadSpd;
        startingChamberAmmo = ammo;

        damage = dmg;

        kickAmmt = kick;
        shootsProjectiles = true;
        projectileType = projectileName;
    }

    // Non-Projectile Shooters
    public GunStats(float rate, float reloadSpd, int ammo, float dmg, float kick)
    {
        startingFireRate = rate;
        startingReloadSpeed = reloadSpd;
        startingChamberAmmo = ammo;

        damage = dmg;

        kickAmmt = kick;
        shootsProjectiles = false;
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

    // Guns with no projectiles!
    public Weapon(string name, float fireRate, int ammo, float reloadSpeed, float dmg, float kick)
    {
        itemName = name;
        itemType = ItemType.Weapon;
        gunStats = new GunStats(fireRate, reloadSpeed, ammo, dmg, kick);
        gunStats.Init();
    }

    // Guns with projectiles!
    public Weapon(string name, float fireRate, int ammo, float reloadSpeed, float dmg, float kick, string projType)
    {
        itemName = name;
        itemType = ItemType.Weapon;
        gunStats = new GunStats(fireRate, reloadSpeed, ammo, dmg, kick, projType);
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
