using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ItemUpgrade_Database : MonoBehaviour {

    // FIX THIS! This should load externally from a database

    public static ItemUpgrade_Database Instance { get; protected set; }

    Dictionary<int, WeaponUpgrade> weaponUpgradesMap = new Dictionary<int, WeaponUpgrade>();
    Dictionary<int, ArmorUpgrade> armorUpgradesMap = new Dictionary<int, ArmorUpgrade>();
    
    void Awake()
    {
        Instance = this;
        InitUpgrades();
    }

    void InitUpgrades()
    {
        weaponUpgradesMap = new Dictionary<int, WeaponUpgrade>()
        {
            {5, new WeaponUpgrade(0, 2, 0, 0, 5) },
            {10, new WeaponUpgrade(0.3f, 2, 0, 0, 10) }
        };

        armorUpgradesMap = new Dictionary<int, ArmorUpgrade>()
        {
            {5, new ArmorUpgrade(1, 0, 5) },
            {10, new ArmorUpgrade(0, 3, 10) }
        };
    }

    public bool CheckForUpgrades(int coreCount, string upgradeType)
    {
        switch (upgradeType)
        {
            case "Armor":
                if (armorUpgradesMap.ContainsKey(coreCount))
                    return true;
                break;
            case "Weapon":
                if (weaponUpgradesMap.ContainsKey(coreCount))
                    return true;
                break;
            default:
                break;
        }

        return false;
    }

    public ArmorUpgrade GetArmorUpgrade(int coreCount)
    {
        return armorUpgradesMap[coreCount];
    }

    public WeaponUpgrade GetWeaponUpgrade(int coreCount)
    {
        return weaponUpgradesMap[coreCount];
    }
}
