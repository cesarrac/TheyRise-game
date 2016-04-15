using UnityEngine;
using System.Collections;
using System;

/// <summary>
///                             ITEMS:
///                                     Objects that can be Equipped, Used, Consumed, and
///                                     are represented by a graphic.
///                                     They can be bought, created or found as loot.
/// </summary>

public enum ItemType
{
    Weapon,
    Armor,
    Consumable,
    Goods
}

public enum ItemUpgradeType
{
    Armor,
    Weapon,
    Tool,
    Battle_Tower,
    Extraction_Tower
}

public class ItemUprade
{
    //public float attackRate { get; protected set; }
    //public float reloadSpeed { get; protected set; }
    //// hp, def, attk, shield; 
    //public float hitPoints { get; protected set; }
    //public float defense { get; protected set; }
    //public float attack { get; protected set; }
    //public float shield { get; protected set; }

    //// For extraction buildings:
    //public float extractionRate { get; protected set; }
    //public int extractAmmnt { get; protected set; }

    public ItemUpgradeType upgradeType { get; protected set; }

}

public class ArmorUpgrade : ItemUprade
{
    public float defense { get; protected set; }
    public float shield { get; protected set; }

    public int coreCost { get; protected set; } // How many Armor Cores are needed to unlock this upgrade

    public ArmorUpgrade(float def, float shi, int coreCost)
    {
        defense = def;
        shield = shi;

        upgradeType = ItemUpgradeType.Armor;

        this.coreCost = coreCost;
    }
}

public class WeaponUpgrade : ItemUprade
{
    public float attackRate { get; protected set; }
    public float damage { get; protected set; }
    public float reload_speed { get; protected set; }
    public int ammo { get; protected set; }

    public int coreCost { get; protected set; } // How many Weapon Cores are needed to unlock this upgrade

    public WeaponUpgrade(float att_rate, float dmg, float reload_spd, int ammo, int coreCost)
    {
        attackRate = att_rate;
        damage = dmg;
        reload_speed = reload_spd;
        this.ammo = ammo;

        upgradeType = ItemUpgradeType.Armor;

        this.coreCost = coreCost;
    }
}

[System.Serializable]
public class Item {
    public string itemName { get; protected set; }
    public ItemType itemType { get; protected set; }
	
}

public class Core
{
    public ItemUpgradeType coreType { get; protected set; }

    public Core(ItemUpgradeType type)
    {
        coreType = type;
    }
}


    /*
    UPGRADE SYSTEM:
    There will be a database of upgrades with Dictionaries for WeaponUpgrades, ArmorUpgrades, and ToolUpgrades, each with keys based on their corresponding Core costs.
    So if the Player crafts 1 new WeaponCore a function would check if the WeaponUpgrades dictionary has an upgrade with a key that is equal to the current total of 
    Weapon cores the player has crafted.
    If it contains an upgrade with that key we would call upgrade on the current selected Weapon passing in the corresponding WeaponUpgrade.
    

    */
