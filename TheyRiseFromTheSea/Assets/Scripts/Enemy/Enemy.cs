using UnityEngine;
using System.Collections;

public class Enemy  {


    public string name { get; protected set; }
    public bool isAggroToBuildings { get; protected set; } // --- gets distracted by towers attacking it and diverts from main path target
    public bool chasesPlayer { get; protected set; } // --- if chases player then its main target is the player
    public Transform alternateTarget { get; protected set; }// -- if it doesnt chase player OR go for the Terraformer, it needs an alternate target
    public UnitStats UnitStats { get; protected set; }
    public Enemy_PathHandler.MovementStats MovementStats { get; protected set; }

    // Constructor for an Enemy with no alternate target (attacks player OR Terraformer)
    public Enemy(string n, UnitStats unitStats, Enemy_PathHandler.MovementStats moveStats, bool chasesP, bool isAggro = false)
    {
        name = n;
        isAggroToBuildings = isAggro;
        UnitStats = unitStats;
        MovementStats = moveStats;
        chasesPlayer = chasesP;

    }

    // This will permit setting an alternate target
    public void SetAltTarget(Transform altTarget)
    {
        alternateTarget = altTarget;
    }


}
