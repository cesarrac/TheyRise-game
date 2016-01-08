using UnityEngine;
using System.Collections;

public class Enemy  {


    public string name { get; protected set; }
    public bool isAggroToBuildings { get; protected set; } // --- gets distracted by towers attacking it and diverts from main path target
    public bool chasesPlayer { get; protected set; } // --- if chases player then its main target is the player
    public Transform nonHeroPathTarget { get; protected set; }// -- if it doesnt chase player it needs an alternate path target
    public Unit_Base.Stats AttackStats { get; protected set; }
    public Enemy_PathHandler.MovementStats MovementStats { get; protected set; }

    // Constructor for NON - AGGRO units
    public Enemy(string n, Unit_Base.Stats attkStats, Enemy_PathHandler.MovementStats moveStats, bool chasesP, bool isAggro = false)
    {
        name = n;
        isAggroToBuildings = isAggro;
        AttackStats = attkStats;
        MovementStats = moveStats;
        chasesPlayer = chasesP;

    }


}
