using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class EnemyIncoming_Indicator : MonoBehaviour {

    // All this needs to do is point from the player to the incoming wave and disappear when the units in that wave are visible to the player

    // This will be something the player can upgrade later but it starts by default at Mk I and upgrades as follows:
    // Mk I - Just points to the incoming wave
    // Mk II - Points and shows the total number of units incoming
    // Mk III - Points, shows total, and defines the threat of the units (low to high)
    // These upgrades will be defined as callbacks that will later be registered to this component by the script tracking Player upgrades.

    Transform enemyToTrack;
    Vector3 enemyPosition;
    Vector3 spawnPosition;

    RectTransform indicator;

    bool enemyHasSpawned = false;
    

    // At first this is tracking the spawn position... 
    public void InitSpawnPos(Vector3 pos)
    {
        Debug.Log("INDICATOR: Spawn position is being tracked!");

        enemyToTrack = null;

        indicator = GetComponent<RectTransform>();

        spawnPosition = pos;

        enemyHasSpawned = false;

        CalculateScreenPosition(spawnPosition);
    }

    public void InitEnemyToTrack(Transform enemy)
    {
        Debug.Log("INDICATOR: Enemy position is being tracked!");
        if (indicator == null)
            indicator = GetComponent<RectTransform>();

        enemyToTrack = enemy;
        enemyPosition = enemyToTrack.position;

        enemyHasSpawned = true;

        CalculateScreenPosition(enemyPosition);
    }


    void Update()
    {
       if (enemyToTrack != null)
        {
            if (enemyToTrack.position != enemyPosition)
            {
                enemyPosition = enemyToTrack.position;
                CalculateScreenPosition(enemyPosition);
            }
        }

    }

    void CalculateScreenPosition(Vector3 pos)
    {
        Image img = GetComponent<Image>();
        Vector3 v3Screen = Camera.main.WorldToViewportPoint(pos);

        if (v3Screen.x > -0.01f && v3Screen.x < 1.01f && v3Screen.y > -0.01f && v3Screen.y < 1.01f && enemyHasSpawned)
        {
            Debug.Log("INDICATOR: Enemy position is on screen! Pooling indicator...");
            // if the Enemy is ON screen, pool this indicator
            ObjectPool.instance.PoolObject(this.gameObject);
        }
        else
        {
            v3Screen.x = Mathf.Clamp(v3Screen.x, 0.10f, 0.90f);
            v3Screen.y = Mathf.Clamp(v3Screen.y, 0.10f, 0.90f);
            indicator.position = Camera.main.ViewportToScreenPoint(v3Screen);
        }
    }

    // USE THIS IF YOU WANT TO ROTATE IT TO POINT TOWARDS AN ENEMY:

    //void PointToEnemy(Vector pos)
    //{
    //    Vector3 dir = Camera.main.WorldToScreenPoint(pos) - incomingWarning.position;
    //    float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90;

    //    incomingWarning.rotation = Quaternion.Euler(0, 0, angle);
    //}
}
