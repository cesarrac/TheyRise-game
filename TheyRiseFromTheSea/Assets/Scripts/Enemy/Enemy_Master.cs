using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Enemy_Master : MonoBehaviour {
    public static Enemy_Master instance;

    Unit_Base.Stats attk_weak, attk_mid, attk_heavy;

    Enemy_PathHandler.MovementStats move_avg, move_fast, move_slow;

    Dictionary<string, Enemy> enemiesAvailable = new Dictionary<string, Enemy>();
    Enemy_SquadSpawner enemy_Squad_Spawner;

    bool canSpawn = false;

    float timeToKeepSpawning = 10f; // this will later be set according to this level's difficulty. The longer this is the more time this script has to spawn more enemies.

    int _currUnitsOnField;
    int CurrUnitsOnField { get { return _currUnitsOnField; } set { _currUnitsOnField = Mathf.Clamp(value, 0, 20); } }

    Transform target_killer; // a Transform containing the position of the most recent "killer" responsible for any enemy unit deaths (can be player or towers)

    int decisionCount; // total decisions made by this instance

    void Awake()
    {
        instance = this;

        decisionCount = Mathf.Clamp(decisionCount, 0, 500);

        InitAvailableEnemies();
    }

    void InitAvailableEnemies()
    {
        // Have an Enemy class that uses a constructor to fill in its stats and get the correct sprite through other logic maybe using the new enemy's name.
        // That Enemy class' attack stats would need to derive from Unit base class and its movement stats from Move handler

        // To create the enemies I would need to fill out their attack stats, pass it through the new enemy constructor and then give it to the 
        // respective Component (Enemy_AttackHandler and Enemy_PathHandler) when I actually spawn the unit.

        // FIX THIS! For now I'm going to initialize the stats within this script but it would be nice to get these values from an external database!
        attk_weak = new Unit_Base.Stats();
        attk_weak.maxHP = 24f;
        attk_weak.startAttack = 2;
        attk_weak.startDamage = 5;
        attk_weak.startDefence = 1;
        attk_weak.startShield = 0;
        attk_weak.startSpecialDmg = 0;
        attk_weak.startRate = 0.8f;

        attk_mid = new Unit_Base.Stats();
        attk_mid.maxHP = 42f;
        attk_mid.startAttack = 5;
        attk_mid.startDamage = 8;
        attk_mid.startDefence = 3;
        attk_mid.startShield = 0;
        attk_mid.startSpecialDmg = 0;
        attk_mid.startRate = 1f;

        attk_heavy = new Unit_Base.Stats();
        attk_heavy.maxHP = 66f;
        attk_heavy.startAttack = 5;
        attk_heavy.startDamage = 10;
        attk_heavy.startDefence = 1;
        attk_heavy.startShield = 0;
        attk_heavy.startSpecialDmg = 0;
        attk_heavy.startRate = 1.2f;

        move_fast = new Enemy_PathHandler.MovementStats();
        move_fast.startMoveSpeed = 6f;
        move_fast.startChaseSpeed = 12f;
        move_avg = new Enemy_PathHandler.MovementStats();
        move_avg.startMoveSpeed = 2f;
        move_avg.startChaseSpeed = 4f;
        move_slow = new Enemy_PathHandler.MovementStats();
        move_slow.startMoveSpeed = 0.5f;
        move_slow.startChaseSpeed = 2f;

        // FIX THIS TOO! Here I'm going to create the enemies hardcoded for testing, but this also would be nice to get from an external database!
        Enemy slimer_weak_noAggro = new Enemy("Slimer_weak", attk_weak, move_avg, true);  // The name of the Enemy class has to match the name of the prefab preloaded unto the Object Pool
        Enemy slimer_weak_Aggro = new Enemy("Slimer_weak", attk_weak, move_avg, true, true);
        Enemy slimer_mid_noAggro = new Enemy("Slimer_weak", attk_mid, move_avg, true);
        Enemy slimer_heavy_noAggro = new Enemy("Slimer_weak", attk_heavy, move_avg, true);

        enemiesAvailable.Add("Slimer_Weak_noAggro", slimer_weak_noAggro);
        enemiesAvailable.Add("Slimer_Mid_noAggro", slimer_mid_noAggro);
        enemiesAvailable.Add("Slimer_Heavy_noAggro", slimer_heavy_noAggro);
        enemiesAvailable.Add("Slimer_Weak_Aggro", slimer_weak_Aggro);
    }

    
    public void SetCanSpawn()
    {
        canSpawn = true;
        // Later I want this to allow the Decision Making logic to be able to issue commands. 
        // The Decision Maker should be running regardless so that it is always ready to spawn when it can.
        // This can spawn should not set to false until a pre-set number of seconds have passed depending on the current level difficulty or
        // the current terrformer stage. That way the higher the difficulty or terra stage the more time this Master will have to keep spawning,
        // potentially making the difficulty progressively harder.
        if (decisionCount > 0)
        {
            // Decide if I need to be agressive!
        }
        else
        {
            MakeADecision();
        }


    }



    void MakeADecision(bool beAgressive = false)
    {
        if (!beAgressive) // Decides to spawn non Aggro units
        {
            // Decision based on the Terraformer's current stage. The higher the stage number, the stronger the enemies should be.
            int currentTerraformerStage = Terraformer_Handler.instance._currStageCount;

            if (currentTerraformerStage <= 1)
            {
                // Decide to spawn weak enemies 
                if (canSpawn)
                {
                    CurrUnitsOnField = 8;
                    IssueSpawnCommand(CurrUnitsOnField, "Slimer_Weak_noAggro");
                    decisionCount++;
                }
            }
            else if (currentTerraformerStage > 1 && currentTerraformerStage <= 3)
            {
                if (canSpawn)
                {
                    CurrUnitsOnField = 6;
                    IssueSpawnCommand(CurrUnitsOnField, "Slimer_Mid_noAggro");
                    decisionCount++;

                }
            }
            else
            {
                if (canSpawn)
                {
                    CurrUnitsOnField = 6;
                    IssueSpawnCommand(CurrUnitsOnField, "Slimer_Heavy_noAggro");
                    decisionCount++;

                }
            }
        }
     
    }

    void IssueSpawnCommand(int total, string key)
    {

        // Now once Enemies are contructed we would pass them to a Spawner script that would have them ready to spawn them when necessary.
        // The Spawner would just spawn a default enemy prefab with the right components attached. Then it would take this Enemy class to find
        // its correct Sprite, assign it, then give its corresponding components the Attack Stats and Movement Stats.
        if (enemiesAvailable.ContainsKey(key))
        {
            Enemy_Spawner.instance.ReceiveSpawnCommand(total, enemiesAvailable[key]);
            canSpawn = false;
        }
        else
        {
            Debug.LogError("ENEMY_MASTER: Dictionary enemiesAvailable does not contain key " + key);
            return;
        }
       

    }

    public void RegisterDeath(Transform killer)
    {
        CurrUnitsOnField--;

        if (killer != target_killer)
        {
            target_killer = killer;
        }

    }
}
