using UnityEngine;
using System.Collections;

public enum ItemType
{
    Weapon,
    Armor,
    Consumable,
    Goods
}

public enum ItemUpgradeType
{
    Attack,
    AttackRate,
    ReloadSpeed,
    HitPoints,
    Defense,
    Shield,
    ExtractRate,
    ExtractAmmnt
}

public class ItemUprade
{
    public float attackRate { get; protected set; }
    public float reloadSpeed { get; protected set; }
    // hp, def, attk, shield; 
    public float hitPoints { get; protected set; }
    public float defense { get; protected set; }
    public float attack { get; protected set; }
    public float shield { get; protected set; }

    // For extraction buildings:
    public float extractionRate { get; protected set; }
    public int extractAmmnt { get; protected set; }

}

public class Item {
    public string itemName { get; protected set; }
    public ItemType itemType { get; protected set; }
	
}
