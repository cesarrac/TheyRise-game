using UnityEngine;
using System.Collections;

public class Mission{

    public enum Faction
    {
        CORPORATE,
        INDEPENDENT
    }

    public Faction missionFaction;

    public Quota missionQuota;

    public Mission (Faction _faction, Quota.RequiredResource _resource, int _total)
    {
        missionFaction = _faction;
        missionQuota = new Quota(_resource, _total);
    }

    public Mission()
    {

    }

    public class Quota
    {
        public enum RequiredResource
        {
            ENRICHED_ORE,
            COMMON_ORE,
            WATER,
            FOOD,
            RAW_ORE
        }

        public RequiredResource resourceRequired;

        public int totalRequired;

        public Quota(RequiredResource _resource, int _total)
        {
            resourceRequired = _resource;
            totalRequired = _total;
        }
    }
}
