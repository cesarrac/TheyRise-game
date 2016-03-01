using UnityEngine;
using System.Collections;
using System;

public class Enemy_Spawner : MonoBehaviour {

    public static Enemy_Spawner instance;

    int totalToSpawn = 0, curSpawnCount = 0;
    Enemy curr_Enemy_toSpwn;

    Vector3 spawnPosition;
   // Vector2[] waterSpawnPositions;

    public bool isSpawning { get; protected set; }

    Func<Transform> GetTargetCB;

    EnemyIncoming_Indicator curIndicator;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        totalToSpawn = 0;
    }

    //public void InitSpawnPositions(Vector2[] waterTilePositions)
    //{
    //    waterSpawnPositions = waterTilePositions;
    //}

    public void RegisterGetTargetCB(Func<Transform> cb)
    {
        GetTargetCB = cb;
    }

    public void ReceiveSpawnCommand(int spawnCount, Enemy enemyToSpwn, Vector3 spawnPos)
    {
        totalToSpawn = spawnCount;
        curSpawnCount = 0;
        curr_Enemy_toSpwn = enemyToSpwn;

        spawnPosition = spawnPos;

        if (!isSpawning)
        {
            isSpawning = true;
            //IndicateIncomingEnemy(curr_Enemy_toSpwn, totalToSpawn);
            StartCoroutine("Spawn");
        }

    }

    IEnumerator Spawn()
    {

        while (totalToSpawn > 0)
        {
            if (curr_Enemy_toSpwn != null)
            {
                GameObject e = ObjectPool.instance.GetObjectForType(curr_Enemy_toSpwn.name, true, spawnPosition);

                if (e != null)
                {
                    // Pass the first enemy spawned as the Enemy Transform that the Indicator will track
                    if (curSpawnCount == 0 && curIndicator != null)
                    {
                        curIndicator.InitEnemyToTrack(e.transform);
                    }


                    // Assign the unit's attack stats
                    if (e.GetComponent<Unit_Base>() != null)
                    {
                        Unit_Base enemyUnitBase = e.GetComponent<Unit_Base>();
                        enemyUnitBase.stats = new UnitStats();
                        enemyUnitBase.stats.InitStartingStats(curr_Enemy_toSpwn.UnitStats.maxHP, curr_Enemy_toSpwn.UnitStats.startDefence,
                            curr_Enemy_toSpwn.UnitStats.startAttack, curr_Enemy_toSpwn.UnitStats.startShield, curr_Enemy_toSpwn.UnitStats.startRate, curr_Enemy_toSpwn.UnitStats.startDamage, curr_Enemy_toSpwn.UnitStats.startSpecialDmg);
                        enemyUnitBase.stats.Init();
                        enemyUnitBase.isAggroToBuildings = curr_Enemy_toSpwn.isAggroToBuildings;
                    }

                    // Assign the unit's Path/Movement stats
                    if (e.GetComponent<Enemy_PathHandler>() != null)
                    {
                        Enemy_PathHandler ePathHandler = e.GetComponent<Enemy_PathHandler>();
                        ePathHandler.mStats = new Enemy_PathHandler.MovementStats();
                        ePathHandler.mStats.InitStartingMoveStats(curr_Enemy_toSpwn.MovementStats.startMoveSpeed, curr_Enemy_toSpwn.MovementStats.startChaseSpeed);
                        ePathHandler.mStats.InitMoveStats();

                        //// If this isn't a player chaser (meaning the player is its main path target) then it needs an alternate path target
                        //if (!curr_Enemy_toSpwn.chasesPlayer)
                        //{
                        //    ePathHandler.chasesPlayer = false;
                        //    ePathHandler.alternateMainTarget = curr_Enemy_toSpwn.alternateTarget;
                        //}
                        //else
                        //{
                        //    ePathHandler.chasesPlayer = true;
                        //}

                        if (GetTargetCB != null)
                            ePathHandler.RegisterGetTargetCB(GetTargetCB);

                        ePathHandler.InitTarget();
                    }
                }
          

                totalToSpawn--;
                curSpawnCount++;

                yield return new WaitForSeconds(0.5f);
            }
            else
            {
                isSpawning = false;
                yield break;
            }
        }

        isSpawning = false;
        curr_Enemy_toSpwn = null;
        yield break;
    }


    public void CreateIndicator(Vector3 spawnPos)
    {
        GameObject indicator = ObjectPool.instance.GetObjectForType("Enemy Indicator", true, spawnPos);
        if (indicator != null)
        {
            indicator.transform.SetParent(GameObject.FindGameObjectWithTag("Canvas").transform);
            if (indicator.GetComponent<EnemyIncoming_Indicator>() != null)
            {
                curIndicator = indicator.GetComponent<EnemyIncoming_Indicator>();
                curIndicator.InitSpawnPos(spawnPos);

            }
        }
    }

    void IndicateIncomingEnemy(Enemy currEnemy, int total)
    {
        // TODO: Use the currEnemy to assess threat and total units incoming

        // Display the arrow
        //UI_Manager.Instance.DisplayEnemyIndicator(spawnPosition);

    }

    //Vector3 GetSpawnPosition()
    //{
    //    int index = Random.Range(0, ResourceGrid.Grid.waterTilesArray.Length);

    //    return new Vector3(ResourceGrid.Grid.waterTilesArray[index].x, ResourceGrid.Grid.waterTilesArray[index].y, 0);
    //}


    public void StopSpawning()
    {
        StopCoroutine("Spawn");
        totalToSpawn = 0;
        curr_Enemy_toSpwn = null;
        isSpawning = false;
    }
}
