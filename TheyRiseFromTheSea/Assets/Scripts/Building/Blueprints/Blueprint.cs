using UnityEngine;
using System.Collections.Generic;
using System.Collections;

// Blueprint Upgrade/Tier:
// Tier 0 = no Upgrade
// NEEDS: 
// - The BuildingType
// - According to the Building Type it would know to affect attack stats or extraction stats
// - If Building Type = Battle, it would affect:
    // - Attack stats: Rate of Fire, Damage, or Ammount of Targets (for AoE) and
    // Tile stats like Shield and HP.
    // - Extraction stats: Rate of Extraction or Extraction Ammount

// - Battle Buildings AND Extraction Buildings can get their Tile Stats upgraded (HitPoints, Shield, Defence, Attack)
// - Only Extraction Buildings can get Extraction Ammnt and Rate of Extraction upgraded
// - Only Battle Buildings can get Rate of Attack, Damage, Reload time, and ammnt of Targets upgraded.

// Each Blueprint, by TileData Type, has their own set of named upgrades (from Tier I to Tier V ). For example,
// Machine Gun's Tier I: Reaction Time - upgrades Reloading mechanisms to load cartridges faster. Reload Time -25%
public class Blueprint_Tier
{
    int _tier = 0;
    public int Tier { get { return _tier; } set { _tier = Mathf.Clamp(value, 0, 6); } }
    Dictionary<TileData.Types, Blueprint_Upgrade[]> upgrades = new Dictionary<TileData.Types, Blueprint_Upgrade[]>();

    public Blueprint_Upgrade GetUpgrade(int tier, TileData.Types bp_type)
    {
        if (upgrades.ContainsKey(bp_type))
        {
            UpgradeTier();
            return upgrades[bp_type][tier];
        }
        else
        {
            return null;
        }
    }

    void UpgradeTier()
    {
        Tier += 1;
    }

    public Blueprint_Tier()
    {
        Tier = 0;
        Init();
    }


    void Init()
    {
        Blueprint_Upgrade[] macGun_upgrades = new Blueprint_Upgrade[5];
        upgrades.Add(TileData.Types.machine_gun, macGun_upgrades);
    }
}

public class Blueprint_Upgrade
{
    public enum UpgradeStatType
    {
        AttackStats, TileStats, ExtractionStats 
    }

    public UpgradeStatType upgradeOne, upgradeTwo, upgradeThree;
    public int totalUpgrades { get; protected set; }

    public Blueprint_Upgrade(UpgradeStatType up_one)
    {
        upgradeOne = up_one;
        totalUpgrades = 1;
    }
    public Blueprint_Upgrade(UpgradeStatType up_one, UpgradeStatType up_two)
    {
        upgradeOne = up_one;
        upgradeTwo = up_two;
        totalUpgrades = 2;

    }
    public Blueprint_Upgrade(UpgradeStatType up_one, UpgradeStatType up_two, UpgradeStatType up_three)
    {
        upgradeOne = up_one;
        upgradeTwo = up_two;
        upgradeThree = up_three;
        totalUpgrades = 3;

    }


    // An Upgrade Manager would Get the corresponding Upgrade and know from the UpgradeStatType what to update when spawning the building
}

public class Blueprint
{
    // Type of Building: Battle or Utility
    public enum BuildingType
    {
        BATTLE,
        UTILITY
    }

    public TileData.Types tileType { get; protected set; }

    // Blueprint PU Cost that can be updated
    public int processingCost { get; protected set; }

    // Nanobot cost of this building
    public int nanoBotCost { get; protected set; }

    // Name of the Building
    public string buildingName { get; protected set; }

    // Description of this Blueprint
    public string description { get; protected set; }

    // Tier / Upgrade level of this Blueprint
    public Blueprint_Tier bp_Tier { get; protected set; }


    public Blueprint() { }

    public Blueprint (string Name, int PUCost, int NanoBotCost, TileData.Types _Ttype, string desc)
    {
        buildingName = Name;
        processingCost = PUCost;
        nanoBotCost = NanoBotCost;
        tileType = _Ttype;
        description = desc;

        // Initialize this new Blueprint's Tier/Upgrade at 0 (no upgrade)
        bp_Tier = new Blueprint_Tier();
    }

    public void ChangePUCost(int change)
    {
        processingCost += change;
    }

    public void ChangeNanoCost(int change)
    {
        nanoBotCost += change;
    }

    public void ChangeName(string newName)
    {
        buildingName = newName;
    }

    
}
