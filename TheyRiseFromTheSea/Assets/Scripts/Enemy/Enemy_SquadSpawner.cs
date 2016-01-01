using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Enemy_SquadSpawner : MonoBehaviour {

    public int totalMembers = 5;
    public GameObject enemyPrefab;
    Vector3[] spawnPositions;
    public Vector3 firstPos;
    List<GameObject> spawnedEnemies;

    void Start()
    {
        ColumnFormation();
    }

    void ColumnFormation()
    {
        spawnedEnemies = new List<GameObject>();

        spawnPositions = new Vector3[totalMembers];
        spawnPositions[0] = firstPos;

        for (int i = 1; i < spawnPositions.Length; i++)
        {
            spawnPositions[i] = firstPos + Vector3.up;
        }
    }

    public void Spawn()
    {
        StartCoroutine("SpawnSquad");
    }

    IEnumerator SpawnSquad()
    {
        spawnedEnemies.Clear();

        while (totalMembers > 0)
        {
            GameObject e = ObjectPool.instance.GetObjectForType("Default Enemy", true, spawnPositions[totalMembers - 1]);
            totalMembers--;
            spawnedEnemies.Add(e);
            yield return new WaitForSeconds(0.5f);

        }

        yield break;
    }

    public void Reset()
    {
        ColumnFormation();
        KillAll();
    }

    public void KillAll()
    {
       
        foreach (GameObject enemy in spawnedEnemies)
        {
            ObjectPool.instance.PoolObject(enemy);
            spawnedEnemies.Remove(enemy);
        }

    }
}
