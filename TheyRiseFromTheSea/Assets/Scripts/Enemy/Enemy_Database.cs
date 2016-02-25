using UnityEngine;
using System.Collections.Generic;

public class Enemy_Database : MonoBehaviour {

    public static Enemy_Database Instance { get; protected set; }

    Dictionary<string, Enemy> enemiesAvailable = new Dictionary<string, Enemy>();

    float planetAgressiveness = 0;
    void Awake()
    {
        Instance = this;

        planetAgressiveness = GameTracker.Instance.planetAgressiveness;

        InitAvailableEnemies();
    }

    void InitAvailableEnemies()
    {
        // Have an Enemy class that uses a constructor to fill in its stats and get the correct sprite through other logic maybe using the new enemy's name.
        // That Enemy class' attack stats would need to derive from Unit base class and its movement stats from Move handler

        // To create the enemies I would need to fill out their attack stats, pass it through the new enemy constructor and then give it to the 
        // respective Component (Enemy_AttackHandler and Enemy_PathHandler) when I actually spawn the unit.

        // FIX THIS! For now I'm going to initialize the stats within this script but it would be nice to get these values from an external database!
        UnitStats attk_weak = new UnitStats();
        attk_weak.maxHP = 24f + (24f * planetAgressiveness);
        attk_weak.startAttack = 2;
        attk_weak.startDamage = 5 + planetAgressiveness;
        attk_weak.startDefence = 1 + planetAgressiveness;
        attk_weak.startShield = 0;
        attk_weak.startSpecialDmg = 0;
        attk_weak.startRate = 0.6f;

        UnitStats attk_mid = new UnitStats();
        attk_mid.maxHP = 42f + (42f * planetAgressiveness);
        attk_mid.startAttack = 5;
        attk_mid.startDamage = 8 + planetAgressiveness;
        attk_mid.startDefence = 3 + planetAgressiveness;
        attk_mid.startShield = 0;
        attk_mid.startSpecialDmg = 0;
        attk_mid.startRate = 0.8f;

        UnitStats attk_heavy = new UnitStats();
        attk_heavy.maxHP = 66f + (66f * planetAgressiveness);
        attk_heavy.startAttack = 5;
        attk_heavy.startDamage = 10 + planetAgressiveness;
        attk_heavy.startDefence = 1 + planetAgressiveness;
        attk_heavy.startShield = 0;
        attk_heavy.startSpecialDmg = 0;
        attk_heavy.startRate = 1.0f;

        Enemy_PathHandler.MovementStats move_fast = new Enemy_PathHandler.MovementStats();
        move_fast.startMoveSpeed = 2f;
        move_fast.startChaseSpeed = 4f;

        Enemy_PathHandler.MovementStats move_avg = new Enemy_PathHandler.MovementStats();
        move_avg.startMoveSpeed = 1f;
        move_avg.startChaseSpeed = 2f;

        Enemy_PathHandler.MovementStats move_slow = new Enemy_PathHandler.MovementStats();
        move_slow.startMoveSpeed = 0.5f;
        move_slow.startChaseSpeed = 1f;

        // FIX THIS TOO! Here I'm going to create the enemies hardcoded for testing, but this also would be nice to get from an external database!
        Enemy slimer_weak_noAggro = new Enemy("Slimer_weak", attk_weak, move_avg, true);  // The name of the Enemy class has to match the name of the prefab preloaded unto the Object Pool
        Enemy slimer_weak_Aggro = new Enemy("Slimer_weak", attk_weak, move_avg, true, true);

        Enemy slimer_mid_noAggro = new Enemy("Slimer_weak", attk_mid, move_avg, false);
        Enemy slimer_mid_Aggro = new Enemy("Slimer_weak", attk_mid, move_avg, true, true);

        Enemy slimer_heavy_noAggro = new Enemy("Slimer_weak", attk_heavy, move_avg, false);
        Enemy slimer_heavy_Aggro = new Enemy("Slimer_weak", attk_heavy, move_avg, true, true);

        enemiesAvailable.Add("Slimer_Weak_noAggro", slimer_weak_noAggro);
        enemiesAvailable.Add("Slimer_Mid_noAggro", slimer_mid_noAggro);
        enemiesAvailable.Add("Slimer_Heavy_noAggro", slimer_heavy_noAggro);

        enemiesAvailable.Add("Slimer_Weak_Aggro", slimer_weak_Aggro);
        enemiesAvailable.Add("Slimer_Mid_Aggro", slimer_mid_Aggro);
        enemiesAvailable.Add("Slimer_Heavy_Aggro", slimer_heavy_Aggro);
    }

    public Enemy GetEnemy(string key)
    {
        if (enemiesAvailable.ContainsKey(key))
        {
            return enemiesAvailable[key];
        }
        else
        {
            return null;
        }
    }

}
