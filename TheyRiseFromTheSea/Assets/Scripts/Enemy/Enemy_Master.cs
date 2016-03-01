using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

public enum EnemyTaskType { PLAYER, BATTLE, UTILITY }
public class EnemyTask : IComparable<EnemyTask>
{
    float generalPriority;
    public float GeneralPriority { get { return generalPriority; } set { generalPriority = Mathf.Clamp(value, 1, 5); } }

    float assignmentScore;
    public float AssignmentScore { get { return assignmentScore; } set { assignmentScore = value; } }

    public EnemyTaskType taskType { get; protected set; }

    public EnemyTask(EnemyTaskType type, float genPriority)
    {
        taskType = type;
        GeneralPriority = genPriority;
    }

    public void CalcAssignment (float modifier, float maxTasks, float distance)
    {
        this.assignmentScore = (maxTasks - (this.generalPriority - modifier)) / distance;
    }

    public int CompareTo(EnemyTask taskToCompare)
    {
        return this.AssignmentScore.CompareTo(taskToCompare.AssignmentScore);
    }
}

public class Enemy_Master : MonoBehaviour {
    public static Enemy_Master instance;

    Enemy_SquadSpawner enemy_Squad_Spawner;

    bool canSpawn = false;

    float timeToKeepSpawning = 10f; // this will later be set according to this level's difficulty. The longer this is the more time this script has to spawn more enemies.

    int _currUnitsOnField;
    int CurrUnitsOnField { get { return _currUnitsOnField; } set { _currUnitsOnField = Mathf.Clamp(value, 0, 20); } }

    Transform target_killer; // a Transform containing the position of the most recent "killer" responsible for any enemy unit deaths (can be player or towers)

    int decisionCount; // total decisions made by this instance

    // Decision based on the Terraformer's current stage. The higher the stage number, the stronger the enemies should be.
    int currentTerraformerStage;

    // Towers the player has built so far. With these lists the enemy knows how many and where they are.
    List<Transform> battleTowersBuilt = new List<Transform>();
    List<Transform> utilityTowersBuilt = new List<Transform>();

    // Tasks the Master can choose from
    EnemyTask[] tasks;
    EnemyTask activeTask;

    float utilityModifier = 0, battleModifier = 0, playerModifier = 0;
    int battleTowerCount, utilityTowerCount;

    Transform nearestUtilityTower, nearestBattleTower;

    // Spawn Points: currency the Enemy Master uses to buy/spawn units
    int maxSpawnPoints = 200;
    int spawnPoints;
    public int SpawnPoints { get { return spawnPoints; } set { spawnPoints = Mathf.Clamp(value, 0, 1000); } }

    float timeAtStartofDecision = 0, timeAtStartOfLevel = 0;

    Vector3 spawnPosition = new Vector3();

    int maxUnitsCap = 10; // max units the Enemy Master can spawn on one action

    int spawnPointRegenRate = 50;

    void Awake()
    {
        utilityModifier = Mathf.Clamp(utilityModifier, 0, 4);
        battleModifier =  Mathf.Clamp(utilityModifier, 0, 4);
        playerModifier =  Mathf.Clamp(utilityModifier, 0, 4);

        utilityModifier = 0;
        battleModifier = 0;
        playerModifier = 0;

        instance = this;
        spawnPoints = maxSpawnPoints;
        InitTasks();
    }

    void InitTasks()
    {
        tasks = new EnemyTask[] { new EnemyTask(EnemyTaskType.PLAYER, 1), new EnemyTask(EnemyTaskType.UTILITY, 2), new EnemyTask(EnemyTaskType.BATTLE, 3) };
    }

    void Start()
    {
        ResourceGrid.Grid.RegisterTowerBuildCB(AddBattleTowerBuilt, AddUtilityTowerBuilt, RemoveBattleTowerBuilt, RemoveUtilityTowerBuilt);

        timeAtStartOfLevel = Time.time;

        // Register Get Target callback with The Enemy Spawner
        Enemy_Spawner.instance.RegisterGetTargetCB(GetCurrentTarget);
    }

    // This gets called by the Building_Handler every time the player builds a Battle Tower (Machine Gun, Sniper, etc.)
    void AddBattleTowerBuilt(Transform towerTransform)
    {
        if (!battleTowersBuilt.Contains(towerTransform))
        {
            Debug.Log("MASTER : Adding battle tower! ");
            battleTowersBuilt.Add(towerTransform);
        }
          
    }

    void RemoveBattleTowerBuilt(Transform towerTransform)
    {
        if (battleTowersBuilt.Contains(towerTransform))
            battleTowersBuilt.Remove(towerTransform);
    }

    void AddUtilityTowerBuilt(Transform towerTransform)
    {
        if (!utilityTowersBuilt.Contains(towerTransform))
        {
            Debug.Log("MASTER : Adding utility tower! ");
            utilityTowersBuilt.Add(towerTransform);
        }
         
    }

    void RemoveUtilityTowerBuilt(Transform towerTransform)
    {
        if (utilityTowersBuilt.Contains(towerTransform))
            utilityTowersBuilt.Remove(towerTransform);
    }



    // With these 2 functions above this Master can know how many towers the Player has built.
    // TODO: Consider also allowing the Master to know how much of a Resource the Player has gathered, maybe
    // what type of mission they are on, to know what to target specifically to STOP them from being able to complete the mission.

    // Possible Tasks sorted by General Priority score 
    // Kill the Player = 1
    // Destroy Battle Tower = 2
    // Destroy Utility Tower = 3
    // When deciding a Task each priority gets a modifier ( +0 to +3 )

    // To sort tasks by score they each need to be applied to this formula:
    // Assignment Priority Score = (4 - (general score + modifier)) / distance to target

    public void Initialize()
    {
        // At init wait 5 seconds before starting to indicate first enemies
        StartCoroutine(WaitToAct(5, true));
    }

    void StartWaitToAct()
    {
        // Get a spawn position for this next wave
        spawnPosition = GetSpawnPosition();

        // ONLY CREATE A SPAWN INDICATOR WHEN THERE ARE SOME SPAWN POINTS LEFT! (To avoid creating one when Economic Strategy will be implemented)
        if (spawnPoints > 0)
        {
            // Notify the incoming indicator of the position the enemy will be coming from
            Enemy_Spawner.instance.CreateIndicator(spawnPosition);
        }


        StartCoroutine(WaitToAct(15));
    }

    IEnumerator WaitToAct(float time, bool first = false)
    {
        while (true)
        {
            yield return new WaitForSeconds(time);
            if (!first)
            {
                StartDecisionMakingProcess();
            }
            else
            {
                StartWaitToAct();
            }
    
            yield break;
        }
    }

    void StartDecisionMakingProcess()
    {
       // Debug.Log("Starting to make a decision!");
        timeAtStartofDecision = Time.time;
        CalcModifiers();

    }

    // Before we can score each task we need to get a Spawn Position (where this next wave will be spawning from is a random water tile position)
    void CalcModifiers()
    {
        //TODO: How to handle the player's modifier?? Could raise modifier by health, the lower their health the higher modifier they get?


        // First, calculate the modifiers.
        // Each Utility tower built gives the Destroy Utility task a + 1 modifier
        if (utilityTowersBuilt.Count != utilityTowerCount)
        {
            utilityTowerCount = utilityTowersBuilt.Count;
            utilityModifier = utilityTowerCount;
        }

        // Every 3 Battle Towers built gives the Destroy Battle task a + 1 modifier
        if (battleTowersBuilt.Count != battleTowerCount)
        {
            // ... set the new battle count from the list...
            battleTowerCount = battleTowersBuilt.Count;

            // ... and check if that new count is divisible by 3 by making sure the remainder is 0
            if (battleTowerCount % 3 == 0 || battleTowerCount > 3)
            {
                // ... if it's true then the result of the division is the modifier or just +1 if its only a count of 3
                if (battleTowerCount == 3)
                {
                    battleModifier = 1;
                }
                else
                {
                    battleModifier = battleTowerCount / 3;
                }

            }

            // If the count is not divisible by 3 or it results in less than 1, no modifier is added yet.
        }

        //// Get a spawn position for this next wave
        //spawnPosition = GetSpawnPosition();

        // Calculate distances

        if (utilityTowerCount > 0)
        {
            nearestUtilityTower = GetNearestTower(utilityTowersBuilt, spawnPosition);
        }

        if (battleTowerCount > 0)
        {
            nearestBattleTower = GetNearestTower(battleTowersBuilt, spawnPosition);
        }

        // Calculate Assignment Scores: (0 Kill Player, 1 Destroy Utility, 2 Destroy Battle Tower)

        // We need the distance to the Player...
        float distanceToPlayer = (ResourceGrid.Grid.Hero.transform.position - spawnPosition).magnitude;

        // ... then Calculate assignment score for player.
        tasks[0].CalcAssignment(playerModifier, 3, distanceToPlayer);

        // If there is a nearest utility tower...
        if (nearestUtilityTower != null)
        {
            // ... use the distance to it to calculate its assignment
            tasks[1].CalcAssignment(utilityModifier, 3, (nearestUtilityTower.position - spawnPosition).magnitude);
        }
        else
        {
            // ... if there is no nearest, just hardcode it to 100 to make sure it won't be assigned as an active task
            tasks[1].CalcAssignment(utilityModifier, 3, 100f);
        }

        // Same thing as above, but for the Battle Tower...
        if (nearestBattleTower != null)
        {
            tasks[2].CalcAssignment(battleModifier, 3, (nearestBattleTower.position - spawnPosition).magnitude);
        }
        else
        {
            tasks[2].CalcAssignment(battleModifier, 3, 100f);
        }

        SortTasks();
    }

    void SortTasks()
    {

        EnemyTask[] orderedTasks = new EnemyTask[] { tasks[0], tasks[1], tasks[2] };

        // Now all tasks must be sorted by their Assignment Score. Ordering them by descending so the highest score goes first.
        orderedTasks.OrderByDescending(x => x.AssignmentScore);

        // Set the active task
        activeTask = orderedTasks[0];

        //Debug.Log("ENEMY MASTER Assignment scores: ");
        for (int i = 0; i < tasks.Length; i++)
        {
            Debug.Log(orderedTasks[i].taskType.ToString() + " score = " + orderedTasks[i].AssignmentScore);
        }

        Debug.Log("ENEMY MASTER: Active task is " + activeTask.taskType.ToString());

        SelectStrategy();
    }

    // Tasks now sorted and active task set, now decide what wave to spawn
    void SelectStrategy()
    {
        // The Enemy Master uses Spawn Points as currency to purchase/spawn waves. If it currently has none, go straight to an Economic strategy
        if (spawnPoints <= 0)
        {
            EconomicStrategy();
            //StartWaitToAct();
            return;
        }

        // To select what Wave to spawn we are first considering what the active Task is, each task has a different strategy
        if (activeTask != null)
        {
            if (activeTask.taskType == EnemyTaskType.PLAYER)
            {
                // Use Conservative or Rush strategies if criteria is met, if not go to Economic strategy
                if (timeAtStartofDecision - timeAtStartOfLevel > 120)
                {
                    // more than a minute has passed, consider Rushing
                    if (spawnPoints >= (maxSpawnPoints / 2))
                    {
                        // Currently have half of max Spawn points or more, Rush Aggressively!
                        RushAggressiveStrategy();

                    }
                    else if (spawnPoints > (maxSpawnPoints / 4))
                    {
                        // Currently have more than or equal 1/4th of max spawn points, Rush Conservatively!
                        RushConservativeStrategy();
                    }
                    else if (spawnPoints >= 20)
                    {
                        ConservativeStrategy();
                    }
                    else
                    {
                        EconomicStrategy();
                    }
                }
                else
                {
                    // During the 1st minute, if Kill Player is active task, always go Conservative
                    ConservativeStrategy();
                }
            }
            else if (activeTask.taskType == EnemyTaskType.UTILITY)
            {
                // Use Agressive strategy only if the player is more than half way done with their mission goal, else go Conservative
            }
            else if (activeTask.taskType == EnemyTaskType.BATTLE)
            {
                // Use Agressive or Rush Agressive if possible, or attempt Rush Conservative if not able. Else go Conservative.
            }
        }

        // After a strategy has been executed, wait to decide what to do next
       // StartWaitToAct();
    }

    // **************************************************************   BASIC STRATEGIES:
    void EconomicStrategy()
    {
        Debug.Log("MASTER: Implementing Economic Strategy!");
        SpawnPoints += spawnPointRegenRate;

        // Check if any indicators were created so we can pool them here
        //if (enemy_indicator != null)
        //{
        //    if (enemy_indicator.gameObject.activeSelf == true)
        //    {
        //        ObjectPool.instance.PoolObject(enemy_indicator.gameObject);
        //    }
        //}

        StartWaitToAct();
    }

    void ConservativeStrategy()
    {
        Debug.Log("MASTER: Implementing Conservative Strategy!");

        int midCost = Enemy_Database.Instance.GetEnemy("Slimer_Mid_noAggro").spawnCost;
        int weakCost = Enemy_Database.Instance.GetEnemy("Slimer_Weak_noAggro").spawnCost;
        if (spawnPoints < midCost && spawnPoints < weakCost)
            return;

        if (spawnPoints > maxSpawnPoints / 2)
        {
            // Only purchase higher than low cost when spawn points are higher than half of max

            // So only spend on Mid units until spawnpoints would be at half
            int moreThanHalf = spawnPoints - (maxSpawnPoints / 2);

   
            if (moreThanHalf >= midCost)
            {
                if (moreThanHalf / midCost > maxUnitsCap)
                {
                    IssueSpawnCommand(maxUnitsCap, "Slimer_Mid_noAggro");
                }
                else
                {
                    IssueSpawnCommand(moreThanHalf / midCost, "Slimer_Mid_noAggro");
                }
            }
            else
            {
                if (spawnPoints / weakCost > maxUnitsCap)
                {
                    IssueSpawnCommand(maxUnitsCap, "Slimer_Weak_noAggro");
                }
                else
                {
                    IssueSpawnCommand(spawnPoints / weakCost, "Slimer_Weak_noAggro");
                }
            }

        }
        else
        {
            // If spawnpoints are less than or equal to half, spawn as many Weak units as you can
            if (spawnPoints / weakCost > maxUnitsCap)
            {
                IssueSpawnCommand(maxUnitsCap, "Slimer_Weak_noAggro");
            }
            else
            {
                IssueSpawnCommand(spawnPoints / weakCost, "Slimer_Weak_noAggro");
            }
        }

        StartWaitToAct();
    }

    void RushAggressiveStrategy()
    {
        Debug.Log("MASTER: Implementing RushAggressiveStrategy Strategy!");
        // Buy as many Heavy units as you can
        int heavyCost = Enemy_Database.Instance.GetEnemy("Slimer_Heavy_noAggro").spawnCost;
        if (spawnPoints < heavyCost)
            return;

        if ((spawnPoints / heavyCost) > maxUnitsCap)
        {
            IssueSpawnCommand(maxUnitsCap, "Slimer_Heavy_noAggro");
        }
        else
        {
            IssueSpawnCommand(spawnPoints / heavyCost, "Slimer_Heavy_noAggro");
        }

        // Then as a second spawn command, buy as many Mid units as you can without exceeding max Units
        int midCost = Enemy_Database.Instance.GetEnemy("Slimer_Mid_noAggro").spawnCost;
        if (spawnPoints < midCost)
        {
            return;
        }
        else
        {
            if (spawnPoints / midCost > maxUnitsCap - CurrUnitsOnField)
            {
                Debug.Log("MASTER: Implementing RushAggressiveStrategy Second Spawn Command!");
                StartCoroutine(WaitForSecondSpawnCommand(maxUnitsCap - CurrUnitsOnField, "Slimer_Mid_noAggro"));
            }
            else
            {
                Debug.Log("MASTER: Implementing RushAggressiveStrategy Second Spawn Command!");
                StartCoroutine(WaitForSecondSpawnCommand(spawnPoints / midCost - CurrUnitsOnField, "Slimer_Mid_noAggro"));
            }
        }

        StartWaitToAct();
    }

    void RushConservativeStrategy()
    {
        Debug.Log("MASTER: Implementing RushConservativeStrategy Strategy!");

        // Buy as many Weak units as you can
        int weakCost = Enemy_Database.Instance.GetEnemy("Slimer_Weak_noAggro").spawnCost;
        if (spawnPoints < weakCost)
            return;

        if ((spawnPoints / weakCost) > maxUnitsCap)
        {
            IssueSpawnCommand(maxUnitsCap, "Slimer_Weak_noAggro");
        }
        else
        {
            IssueSpawnCommand(spawnPoints / weakCost, "Slimer_Weak_noAggro");
        }

        StartWaitToAct();

    }

    IEnumerator WaitForSecondSpawnCommand(int total, string id)
    {
        while (true)
        {
            if (!Enemy_Spawner.instance.isSpawning)
            {
                IssueSpawnCommand(total, id);
                yield break;
            }

            yield return null;
        }
    }

    // This is a callback for each unit to get its target from the active task
    Transform GetCurrentTarget()
    {
        if (activeTask.taskType == EnemyTaskType.PLAYER)
        {
            return ResourceGrid.Grid.Hero.transform;
        }
        else if (activeTask.taskType == EnemyTaskType.BATTLE)
        {
            if (nearestBattleTower != null)
            {
                return nearestBattleTower;
            }
        }
        else if (activeTask.taskType == EnemyTaskType.UTILITY)
        {
            if (nearestUtilityTower != null)
            {
                return nearestUtilityTower;
            }
        }

        // If nothing has returned try returning the player
        return ResourceGrid.Grid.Hero.transform; 
    }

    Transform GetNearestTower(List<Transform> towers, Vector3 spawnPos)
    {
        float nearestDistance = 0;

        foreach (Transform trans in towers)
        {
            float newDistance = (trans.position - spawnPos).magnitude;
            if (nearestDistance == 0)
            {
                nearestDistance = newDistance;
                return trans;
            }
            else if (newDistance < nearestDistance)
            {
                nearestDistance = newDistance;
                return trans;
            }
        }

        return null;
    }


    Vector3 GetSpawnPosition()
    {
        int index = UnityEngine.Random.Range(0, ResourceGrid.Grid.waterTilesArray.Length);

        return new Vector3(ResourceGrid.Grid.waterTilesArray[index].x, ResourceGrid.Grid.waterTilesArray[index].y, 0);
    }

    public bool CheckSpawnPointsHasCost(int cost)
    {
        if (spawnPoints >= cost)
            return true;
        else
            return false;
    }

    void ChargeSpawnPoints(int cost)
    {
        spawnPoints -= cost;
    }

    //public void StartMakingDecisions()
    //{
    //    canSpawn = true;
    //    // Later I want this to allow the Decision Making logic to be able to issue commands. 
    //    // The Decision Maker should be running regardless so that it is always ready to spawn when it can.
    //    // This can spawn should not set to false until a pre-set number of seconds have passed depending on the current level difficulty or
    //    // the current terrformer stage. That way the higher the difficulty or terra stage the more time this Master will have to keep spawning,
    //    // potentially making the difficulty progressively harder.

    //    MakeADecision();


    //}


    //// This allows the Master to make a decision dynamically (in between stages)
    //IEnumerator DecideWhatToSpawnNext()
    //{
    //    while (true)
    //    {

    //        yield return new WaitForSeconds(10f);

    //        // If all units that were on field have registered deaths, make a kill squad to avenge them
    //        if (CurrUnitsOnField == 0)
    //        {

    //            MakeAKillSquad();
    //            yield break;
    //        }
    //    }
    //}


    //void MakeADecision(bool beAgressive = false)
    //{
    //    currentTerraformerStage = Terraformer_Handler.instance._currStageCount;

    //    if (!beAgressive) // Decides to spawn non Aggro units
    //    {
    //        if (currentTerraformerStage <= 1)
    //        {
    //            // Decide to spawn weak enemies 
    //            if (canSpawn)
    //            {
    //                CurrUnitsOnField = 5;
    //                IssueSpawnCommand(CurrUnitsOnField, "Slimer_Weak_noAggro");
    //                decisionCount++;
    //                StartCoroutine("DecideWhatToSpawnNext");
    //            }
    //        }
    //        else if (currentTerraformerStage > 1 && currentTerraformerStage <= 3)
    //        {
    //            if (canSpawn)
    //            {
    //                CurrUnitsOnField = 3;
    //                IssueSpawnCommand(CurrUnitsOnField, "Slimer_Mid_noAggro");
    //                decisionCount++;
    //                StartCoroutine("DecideWhatToSpawnNext");
    //            }
    //        }
    //        else
    //        {
    //            if (canSpawn)
    //            {
    //                CurrUnitsOnField = 2;
    //                IssueSpawnCommand(CurrUnitsOnField, "Slimer_Heavy_noAggro");
    //                decisionCount++;
    //                StartCoroutine("DecideWhatToSpawnNext");
    //            }
    //        }
    //    }
    //    else
    //    {
    //        // Be aggressive!
    //        // Decide how aggresive to be depending on the terraformer stage:
    //        if (currentTerraformerStage <= 1)
    //        {
    //            // Decide to spawn weak enemies 
    //            if (canSpawn)
    //            {
    //                CurrUnitsOnField = 5;

    //                IssueSpawnCommand(CurrUnitsOnField, "Slimer_Weak_Aggro");

    //                decisionCount++;

    //                StartCoroutine("DecideWhatToSpawnNext");
    //            }
    //        }
    //        else if (currentTerraformerStage > 1 && currentTerraformerStage <= 3)
    //        {
    //            if (canSpawn)
    //            {
    //                CurrUnitsOnField = 4;

    //                IssueSpawnCommand(CurrUnitsOnField, "Slimer_Mid_Aggro");

    //                decisionCount++;

    //                StartCoroutine("DecideWhatToSpawnNext");

    //            }
    //        }
    //        else
    //        {
    //            if (canSpawn)
    //            {
    //                CurrUnitsOnField = 3;

    //                IssueSpawnCommand(CurrUnitsOnField, "Slimer_Heavy_Aggro");

    //                decisionCount++;

    //                StartCoroutine("DecideWhatToSpawnNext");

    //            }
    //        }

    //    }

    //}


    //void MakeAKillSquad()
    //{
    //    currentTerraformerStage = Terraformer_Handler.instance._currStageCount;

    //    if (target_killer != null)
    //    {
    //        Debug.Log("ENEMY MASTER: Spawning a kill squad to target " + target_killer.name);
    //        if (currentTerraformerStage <= 1)
    //        {
    //            // Decide to spawn weak enemies 
    //            if (canSpawn)
    //            {
    //                CurrUnitsOnField = 3;

    //                // Set an alternate target to this Aggro unit, so that its mission will be to destroy the Killer responsible for its comrades deaths
    //               // enemiesAvailable["Slimer_Weak_noAggro"].SetAltTarget(target_killer);

    //                IssueSpawnCommand(CurrUnitsOnField, "Slimer_Weak_noAggro");

    //                decisionCount++;
    //            }
    //        }
    //        else if (currentTerraformerStage > 1 && currentTerraformerStage <= 3)
    //        {
    //            if (canSpawn)
    //            {
    //                CurrUnitsOnField = 2;

    //                //enemiesAvailable["Slimer_Mid_noAggro"].SetAltTarget(target_killer);

    //                IssueSpawnCommand(CurrUnitsOnField, "Slimer_Mid_noAggro");
    //                decisionCount++;

    //            }
    //        }
    //        else
    //        {
    //            if (canSpawn)
    //            {
    //                CurrUnitsOnField = 1;

    //               // enemiesAvailable["Slimer_Heavy_noAggro"].SetAltTarget(target_killer);

    //                IssueSpawnCommand(CurrUnitsOnField, "Slimer_Heavy_noAggro");
    //                decisionCount++;

    //            }
    //        }
    //    }
    //}

    void IssueSpawnCommand(int total, string key)
    {
        // Now once Enemies are contructed we would pass them to a Spawner script that would have them ready to spawn them when necessary.
        // The Spawner would just spawn a default enemy prefab with the right components attached. Then it would take this Enemy class to find
        // its correct Sprite, assign it, then give its corresponding components the Attack Stats and Movement Stats.

        // Get Enemy from Database using string key...
        Enemy e = Enemy_Database.Instance.GetEnemy(key);

        if (e != null)
        {
            // ... an enemy was returned. Now Send a Spawn command with this enemy and the total number we want to spawn.
            Enemy_Spawner.instance.ReceiveSpawnCommand(total, e, spawnPosition);

            // charge the spawn points the cost to spawn that type of enemy
            ChargeSpawnPoints(e.spawnCost * total);

            CurrUnitsOnField += total;
        }
        else
        {
            Debug.LogError("ENEMY_MASTER: Database enemiesAvailable does not contain key " + key);
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

        Debug.Log("ENEMY MASTER: Registered death from " + killer.name);
    }
}
