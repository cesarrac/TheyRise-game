using UnityEngine;
using System.Collections;

public class ArmorStats
{
    public float startingDefense;
    public float startingShield;
    float _def;
    float _shield;
    public float _curShield { get { return _shield; } set { _shield = Mathf.Clamp(value, 0, 100); } }
    public float _curDefense { get { return _def; } set { _def = Mathf.Clamp(value, 1, 100); } }

    public ArmorStats(float def, float shield)
    {
        startingDefense = def;
        startingShield = shield;
    }

    public void Init()
    {
        _curShield = startingShield;
        _curDefense = startingDefense;
    }

    public void Upgrade(float def, float shi)
    {
        _curDefense += def;
        _curShield += shi;
    }
}

public class Armor : Item {

	public ArmorStats armorStats { get; protected set; }

    public Armor(string name, float def, float shield)
    {
        itemName = name;
        itemType = ItemType.Armor;

        armorStats = new ArmorStats(def, shield);
        armorStats.Init();
    }

    public void UpgradeArmor(ArmorUpgrade upgrade)
    {
        if (upgrade.upgradeType != ItemUpgradeType.Armor)
        {
            Debug.LogError("The ItemUpgrade being passed into the Upgrade callback is not of the type Armor! Is the Item improperly constructed??");
            return;
        }

        armorStats.Upgrade(upgrade.defense, upgrade.shield);
    }
}
