using UnityEngine;
using System.Collections;
using System;

public class Enemy_Spawner : MonoBehaviour {

    public static Enemy_Spawner instance;

    int totalToSpawn = 0;
    Enemy curr_Enemy_toSpwn;

    Vector3 spawnPosition;
   // Vector2[] waterSpawnPositions;

    public bool isSpawning { get; protected set; }

    Func<Transform> GetTargetCB;

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
        curr_Enemy_toSpwn = enemyToSpwn;

        // Select a Spawn Position
        //spawnPosition = GetSpawnPosition();
        spawnPosition = spawnPos;

        if (!isSpawning)
        {
            isSpawning = true;
            StartCoroutine("Spawn");
        }

    }

    IEnumerator Spawn()
    {

        while (totalToSpawn > 0)
        {
            if (curr_Enemy_toSpwn != null)
            {
                //// Select a Spawn Position
                //spawnPosition = GetSpawnPosition();

                GameObject e = ObjectPool.instance.GetObjectForType(curr_Enemy_toSpwn.name, true, spawnPosition);

                if (e != null)
                {
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
