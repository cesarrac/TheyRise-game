using UnityEngine;
using System.Collections;

public class Fauna_Spawner : MonoBehaviour {

    public enum State { IDLING, GET_WATERPOS, GET_NEARESTEMPTY, GET_PATH, COUNTING, SPAWNING, STOP }
    private State _state = State.IDLING;
    public State state { get { return _state; } set { _state = value; } }
    public State debugState;

    public float timeToSpawn, timeToIdle;

    public Vector2 waterSpawnTile = Vector2.zero;
    public Vector2 nearestEmptyTile = Vector2.zero;

    public ResourceGrid resourceGrid;
    public ObjectPool objPool;

    private float spawnCountdown, idleCountDown;
    bool spawning;

    void Awake()
    {
        spawnCountdown = timeToSpawn;
        idleCountDown = timeToIdle;

        if (!resourceGrid)
        {
            resourceGrid = GameObject.FindGameObjectWithTag("Map").GetComponent<ResourceGrid>();
        }

        if (!objPool)
        {
            objPool = GameObject.FindGameObjectWithTag("Pool").GetComponent<ObjectPool>();
        }

    }

    void Update()
    {
        MyStateMachine(_state);
        debugState = _state;
    }

    void MyStateMachine(State curState)
    {
        switch (curState)
        {

            case State.IDLING:
                /*
                if (resourceGrid.pathForEnemy != null)
                {
                    // After giving the Fauna unit its path, clear the path for the next one
                    resourceGrid.pathForEnemy.Clear();
                    resourceGrid.pathForEnemy = null;
                }
                */
                Idle();
                break;

            case State.GET_WATERPOS:
                // Find water spawn and nearest empty tile
                waterSpawnTile = GetWaterSpawnTile();

                if (waterSpawnTile != Vector2.zero)
                    _state = State.GET_NEARESTEMPTY;

                break;
            case State.GET_NEARESTEMPTY:
               
                nearestEmptyTile = FindNearestEmptyTile((int)waterSpawnTile.x, (int)waterSpawnTile.y, waterSpawnTile);
               
                if (waterSpawnTile != Vector2.zero && nearestEmptyTile != Vector2.zero)
                    _state = State.COUNTING;

                break;


            case State.COUNTING:
                // Countdown to spawn
                if (spawnCountdown <= 0)
                {
                    spawnCountdown = timeToSpawn;
                    // Get Path then spawn
                    _state = State.GET_PATH;
                }
                else
                {
                    spawnCountdown -= Time.deltaTime;
                }
                break;

            case State.GET_PATH:
                StartCoroutine(GetPath());
               _state = State.STOP;
                break;

            case State.SPAWNING:
                // Spawn
                if (!spawning)
                {
                    StartCoroutine(Spawn());
                }
                else
                {
                    _state = State.IDLING;
                }
            
                break;

            case State.STOP:
                //STOP!!
                break;

            default:
                _state = State.STOP;
                break;


                     
        }

    }

    void Idle()
    {
        if (idleCountDown <= 0)
        {
            idleCountDown = timeToIdle;
            // Get Position
            _state = State.GET_WATERPOS;
        }
        else
        {
            idleCountDown -= Time.deltaTime;
        }
    }

    Vector2 GetWaterSpawnTile()
    {
        // Get length of grid's water tiles array
        int waterLength = resourceGrid.waterTilesArray.Length;

        // Get a random selection
        int randomWaterSelection = Random.Range(0, waterLength - 1);

        Vector2 waterTile = new Vector2(resourceGrid.waterTilesArray[randomWaterSelection].x, resourceGrid.waterTilesArray[randomWaterSelection].y);

        return waterTile;
    }

    Vector2 FindNearestEmptyTile(int waterX, int waterY, Vector2 _waterPos)
    {
        Vector2 nearestEmpty = Vector2.zero;
        Vector2 firstEmpty = Vector2.zero;
        Debug.Log("WaterX = " + waterX + " WaterY = " + waterY);

        int iX = 0, iY = 0;

        for (int y = waterY; y < resourceGrid.tiles.GetLength(1); y++)
        {
            for (int x =waterX; x < resourceGrid.tiles.GetLength(0); x++)
            {
                if (resourceGrid.tiles[x, y].tileType == TileData.Types.empty)
                {
                    firstEmpty = new Vector2(x, y);
                    iX = x;
                    iY = y;
                }

            }
        }

        var closestDistance = (firstEmpty - _waterPos).sqrMagnitude;

       
        // Find nearest empty tile relative to the selected water spawn tile
        for (int y = 0; y < resourceGrid.tiles.GetLength(1); y++)
        {
            for (int x = 0; x < resourceGrid.tiles.GetLength(0); x++)
            {
                if (resourceGrid.tiles[x,y].tileType == TileData.Types.empty)
                {
                    Vector2 newEmpty = new Vector2(x, y);

                    var thisDistance = (newEmpty - _waterPos).sqrMagnitude;

                    if (thisDistance < closestDistance) {

                        closestDistance = thisDistance;
                        nearestEmpty = newEmpty;
                    }
                }

            }
        }
        
        return nearestEmpty;
    }

    IEnumerator GetPath()
    {
        // Tell resource grid to create an enemy path
        if (resourceGrid.pathForEnemy == null)
        {
            resourceGrid.GenerateWalkPath((int)nearestEmptyTile.x, (int)nearestEmptyTile.y, false, (int)waterSpawnTile.x, (int)waterSpawnTile.y);
        }
        else
        {
            resourceGrid.pathForEnemy.Clear();
            resourceGrid.pathForEnemy = null;
            resourceGrid.GenerateWalkPath((int)nearestEmptyTile.x, (int)nearestEmptyTile.y, false, (int)waterSpawnTile.x, (int)waterSpawnTile.y);

        }
        yield return new WaitForSeconds(0.2f);

        if (resourceGrid.pathForEnemy != null)
        {
            _state = State.SPAWNING;
        }
        yield break;

    }

    IEnumerator Spawn()
    {
        Vector3 spawnPos = new Vector3(waterSpawnTile.x, waterSpawnTile.y, 0.0f);
        GameObject fauna = null;
        if (!spawning)
        {
            // Get from pool
            fauna = objPool.GetObjectForType("Fauna", true, spawnPos);
            spawning = true;

            if (fauna == null) {
                _state = State.STOP;
                spawning = false;
            }


        }

        yield return new WaitForSeconds(0.1f);

        // Fill its movement variables and Path
        if (fauna != null)
        {
            Fauna_MoveHandler faunaMove = fauna.GetComponent<Fauna_MoveHandler>();
            faunaMove.resourceGrid = resourceGrid;
            faunaMove.currentPath = resourceGrid.pathForEnemy;
            faunaMove.posX =(int) spawnPos.x;
            faunaMove.posY = (int) spawnPos.y;

            spawning = false;

            
           
        }

        yield break;

    }

  


}
