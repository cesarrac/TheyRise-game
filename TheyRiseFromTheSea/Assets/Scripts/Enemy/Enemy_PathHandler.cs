using UnityEngine;
using System.Collections;

public class Enemy_PathHandler : MonoBehaviour
{
    public bool chasesPlayer;

    [System.Serializable]
    public class MovementStats
    {
        public float startMoveSpeed, startChaseSpeed;

        private float _moveSpeed, _chaseSpeed;

        public float curMoveSpeed { get { return _moveSpeed; } set { _moveSpeed = Mathf.Clamp(value, 0, startMoveSpeed); } }
        public float curChaseSpeed { get { return _chaseSpeed; } set { _chaseSpeed = Mathf.Clamp(value, 0, startChaseSpeed); } }

        public void InitMoveStats()
        {
            curMoveSpeed = startMoveSpeed;
            curChaseSpeed = startChaseSpeed;
        }
    }

    public MovementStats mStats = new MovementStats();


    Transform target, savedTarget;

    Vector3[] path;
    int targetIndex;

    ResourceGrid grid;
    bool finishedPath = false, canWalk = true;
    Vector3 lastTargetPos;

    public enum State { GETTING_PATH, FOLLOWING_PATH, FINISHED_PATH, MOVING_TO_TARGET, BLOCKED, ATTACKING, IDLE }
    public State _state { get; protected set; }

    public State debugState;

    public TileData blockingTile { get; protected set; }

    Vector3 escapePos;

    // Steering Behaviors:
    //  seek : normal follow path to the target
    //  Separate: follows and path but always maintains a distance to a certain other object by shifting its speed

    void OnEnable()
    {
       
        grid = ResourceGrid.Grid;
        if (target == null)
        {
            if (chasesPlayer)
            {   
                if (GameObject.FindGameObjectWithTag("Citizen") != null)
                    target = GameObject.FindGameObjectWithTag("Citizen").transform;
            }
            else
            {
                // it goes for the capital/launchpad
                target = grid.playerCapital.transform;
            }


        }

        // Initialize Movement Speed stat
        mStats.InitMoveStats ();

    }

    void Start()
    {
        if (target != null)
            StartCoroutine("RequestPath");

        _state = State.GETTING_PATH;
      
        print("My target's position is: " + target.position);
    }

    void Update()
    {
        
        if (finishedPath && target.position != lastTargetPos)
        {
            finishedPath = false;
            print("My target's position is: " + target);
            StartCoroutine("RequestPath");
        }

        if (_state == State.MOVING_TO_TARGET && finishedPath)
            SwitchToAttacking();

        debugState = _state;
    }

    public void SwitchToAttacking()
    {
        StopCoroutine("RequestPath");
        StopCoroutine("FollowPath");
        _state = State.ATTACKING;
    }

    public void SwitchToMoving()
    {
        target = savedTarget;
        StartCoroutine("RequestPath");
        _state = State.GETTING_PATH;
    }

    public void SwitchPathTarget(Transform newTarget)
    {
        savedTarget = target;
        target = newTarget;
        _state = State.MOVING_TO_TARGET;
        StopCoroutine("FollowPath");
        Debug.Log("SWITCHED TARGET! New Target's position is: " + newTarget.position);
    }

    IEnumerator RequestPath()
    {
        while (true)
        {
           
            yield return new WaitForSeconds(0.7f);
            Vector3 targetPosition = target.position;

            if (_state == State.MOVING_TO_TARGET)
            {
                // Since this New Target is NOT walkable, choose to move to the left or right of it
                int leftOrRight = Random.Range(0, 2);
                if (leftOrRight == 0)
                    // left
                    targetPosition.x = target.position.x - 1;
                else
                    //right
                    targetPosition.x = target.position.x + 1;
            }

            PathRequestManager.RequestPath(transform.position, targetPosition, OnPathFound);
            print("Path requested.");
        }

    }

    public void OnPathFound(Vector3[] newPath, bool pathSuccesful)
    {
        if (pathSuccesful)
        {
            path = newPath;
            targetIndex = 0;
            StopCoroutine("FollowPath");
            StartCoroutine("FollowPath");
            if (_state != State.FOLLOWING_PATH && _state != State.MOVING_TO_TARGET)
                _state = State.FOLLOWING_PATH;
        }
        else
        {
            bool currIswalkable = grid.NodeFromWorldPoint(transform.position).isWalkable;
            print("Could not find a path from this location. This node is walkable: " + currIswalkable);

         

            if (!currIswalkable && grid.NodeFromWorldPoint(escapePos).isWalkable)
            {
                var direction = -(target.position - transform.position).normalized;
                escapePos = transform.position + direction;
                //StopCoroutine("RequestPath");
                FullStop();
                StartCoroutine("EscapeObstacle");
            }
                

        }
    }

    //void StartEscaping(Vector3 blockedPosition)
    //{
    //    // Reset the Follow Path coroutine & targetIndex
    //    targetIndex = 0;
    //    StopCoroutine("FollowPath");

    //    // State becomes Blocked
    //    if (_state != State.BLOCKED)
    //        _state = State.BLOCKED;

    //    // Define what direction this unit was walking on by comparing the current position to the target's position
    //    Vector3 direction = -(target.position - blockedPosition).normalized;

    //    // So we DONT try to get a path from this tile again make this tile unwakable
    //    //GetBlockingTile(blockedPosition);

    //    // Vector3 blockingTilePos = new Vector3(blockingTile.posX, blockingTile.posY, 0);

    //    escapePos = blockedPosition + direction;

    //    Debug.Log("Escape Position " + escapePos);

    //    // Stop requesting path while we Escape Obstacle
    //    StopCoroutine("RequestPath");
    //    StopCoroutine("EscapeObstacle");
    //    StartCoroutine("EscapeObstacle");
    //}

    IEnumerator EscapeObstacle()
    {
        while (true)
        {
            if ((transform.position - escapePos).sqrMagnitude > 0.5f )
            {
                transform.position = Vector3.MoveTowards(transform.position, escapePos, mStats.curMoveSpeed * Time.deltaTime);
                yield return null;
            }
            else
            {
                StartCoroutine("RequestPath");
                yield break;
            }
        }
    }

    IEnumerator FollowPath()
    {
        if (path.Length > 0)
        {
            Vector3 currWayPoint = path[0];
            while (true)
            {
                if (transform.position == currWayPoint)
                {
                    targetIndex++;
                    if (targetIndex >= path.Length)
                    {
                        finishedPath = true;
                        lastTargetPos = target.position;
                        targetIndex = 0;
                        path = null;
                        StopCoroutine("RequestPath");
                        if (_state != State.FINISHED_PATH)
                            _state = State.FINISHED_PATH;

                        yield break;
                    }
                    else
                    {
                        if (ResourceGrid.Grid.NodeFromWorldPoint(path[targetIndex]).isWalkable)
                        {
                            // the next node is walkable, keep going
                            currWayPoint = path[targetIndex];

                        }
                        else
                        {
                            // next node is NOT walkable
                            _state = State.BLOCKED;
                            Debug.Log("DETECTED next position in path is blocked");
                            //StartEscaping(path[targetIndex]);

                            yield break;
                        }


                    }
                }

                //if(canWalk)
                transform.position = Vector3.MoveTowards(transform.position, currWayPoint, mStats.curMoveSpeed * Time.deltaTime);

                yield return null;
            }
        }


    }

    void GetBlockingTile(Vector3 position)
    {
        blockingTile = ResourceGrid.Grid.TileFromWorldPoint(position);

       // ResourceGrid.Grid.SwitchTileWalkability(blockingTile.posX, blockingTile.posY, false);
    }

    //public void OnDrawGizmos()
    //{
    //    if (path != null)
    //    {
    //        for (int i = targetIndex; i < path.Length; i++)
    //        {
    //            Gizmos.color = Color.black;
    //            Gizmos.DrawCube(path[i], Vector3.one);

    //            if (i == targetIndex)
    //            {
    //                Gizmos.DrawLine(transform.position, path[i]);
    //            }
    //            else
    //            {
    //                Gizmos.DrawLine(path[i - 1], path[i]);
    //            }
    //        }
    //    }
    //}

    bool CheckIfPathIsBlocked(Node nextNode)
    {
        return nextNode.isWalkable;
    }


    public void FullStop()
    {
        targetIndex = 0;
        StopCoroutine("FollowPath");
        StopCoroutine("RequestPath");
        StopCoroutine("EscapeObstacle");

    }

    void OnCollisionEnter2D(Collision2D coll)
    {
        if (coll.gameObject.CompareTag("Rock"))
        {
            //Debug.Log("Colliding with ROCK!");
            // In case the escaping coroutine is currently going
            //StopCoroutine("EscapeObstacle");
            // StartEscaping(coll.transform.position);
            FullStop();

            var direction = -(coll.transform.position - transform.position).normalized;

            escapePos = transform.position + direction;

            StartCoroutine("EscapeObstacle");

            Debug.Log("Direction from me to rock " + direction);
        
        }
    }
}
