using UnityEngine;
using System.Collections.Generic;
using System.Collections;

[System.Serializable]
public class BuildRequirement
{
    public Dictionary<TileData.Types, int> reqResourcesMap { get; protected set; }
    public Dictionary<Rock.RockProductionType, int> reqRocksMap { get; protected set; }

    public Resource_Required[] buildRequirements;

    // Constructors for requiring 1, 2, or up to 3 resources to build something
    public BuildRequirement(TileData.Types r1, int r1Ammnt)
    {
        reqResourcesMap = new Dictionary<TileData.Types, int>();
        reqResourcesMap.Add(r1, r1Ammnt);
    }

    public BuildRequirement(TileData.Types r1, int r1Ammnt, TileData.Types r2, int r2Ammnt)
    {
        reqResourcesMap = new Dictionary<TileData.Types, int>();
        reqResourcesMap.Add(r1, r1Ammnt);
        reqResourcesMap.Add(r2, r2Ammnt);
    }

    public BuildRequirement(TileData.Types r1, int r1Ammnt, TileData.Types r2, int r2Ammnt, TileData.Types r3, int r3Ammnt)
    {
        reqResourcesMap = new Dictionary<TileData.Types, int>();
        reqResourcesMap.Add(r1, r1Ammnt);
        reqResourcesMap.Add(r2, r2Ammnt);
        reqResourcesMap.Add(r3, r3Ammnt);
    }

    // One Rock Type
    //public BuildRequirement(Rock.RockProductionType rock1, int rock1Ammnt)
    //{
    //    reqRocksMap = new Dictionary<Rock.RockProductionType, int>();
    //    reqRocksMap.Add(rock1, rock1Ammnt);
    //}

    // One raw resource + One rock type
    public BuildRequirement(Rock.RockProductionType rock1, int rock1Ammnt, TileData.Types res1, int res1Ammnt)
    {
        reqRocksMap = new Dictionary<Rock.RockProductionType, int>();
        reqResourcesMap = new Dictionary<TileData.Types, int>();
        reqRocksMap.Add(rock1, rock1Ammnt);
        reqResourcesMap.Add(res1, res1Ammnt);

    }

    public BuildRequirement(Resource_Required[] requirements)
    {
        buildRequirements = requirements;
    }
}

[System.Serializable]
public class Resource_Required
{
    public int ammnt;
    public ResourceType resource;
    public Resource_Required(ResourceType res, int amt)
    {
        ammnt = amt;
        resource = res;
    }

}

// Type of Building: Battle or Utility
public enum BuildingType
{
    BATTLE,
    UTILITY,
    NONE
}


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
[System.Serializable]
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

[System.Serializable]
public class Blueprint
{
    public BuildingType buildingType { get; protected set; }

    public TileData.Types tileType { get; protected set; }

    // Blueprint Memory Cost that can be updated
    public int memoryCost { get; protected set; }

    // Nanobot cost of this building
    //public int nanoBotCost { get; protected set; }

    // Name of the Building
    public string buildingName { get; protected set; }

    // Description of this Blueprint
    public string description { get; protected set; }

    // Tier / Upgrade level of this Blueprint
    public Blueprint_Tier bp_Tier { get; protected set; }

    //public Dictionary<TileData.Types, int> reqResources { get; protected set; }
    public BuildRequirement buildReq { get; protected set; }

    float researchTime; // Time it takes (in days) to research/make available this blueprint
    public float ResearchTime { get { return researchTime; } }

    public Blueprint() { }

    public Blueprint (string Name, int PUCost, int NanoBotCost, TileData.Types _Ttype, BuildingType tType, BuildRequirement bReq, string desc)
    {
        buildingName = Name;
        memoryCost = PUCost;
        //nanoBotCost = NanoBotCost;
        tileType = _Ttype;
        description = desc;

        buildingType = tType;

        buildReq = bReq;

        //reqResources = new Dictionary<TileData.Types, int>();

        //foreach (TileData.Types r in bReq.reqResourcesMap.Keys)
        //{
        //    reqResources.Add(r, bReq.reqResourcesMap[r]);
        //}

        // Initialize this new Blueprint's Tier/Upgrade at 0 (no upgrade)
        bp_Tier = new Blueprint_Tier();
    }

    // For a Required Blueprint (like Terraformer, Generator, etc)
    public Blueprint(string Name, TileData.Types _type, BuildingType tType)
    {
        buildingName = Name;
        memoryCost = 0;
        tileType = _type;
        description = " ";

        buildingType = tType;
    }

    

    public void ChangePUCost(int change)
    {
        memoryCost += change;
    }


    public void ChangeName(string newName)
    {
        buildingName = newName;
    }

    
}
