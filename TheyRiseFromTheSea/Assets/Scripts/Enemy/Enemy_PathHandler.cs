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

    public bool isEscapingObstacle { get; protected set; }

    Vector3 currPathPosition; // < ------ To check against so we don't request a path when our target's position hasn't changed

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
        {
            StartCoroutine("RequestPath");

            // Set my Attack Handler's main target
            GetComponent<Enemy_AttackHandler>().SetMainTarget(target);

            Debug.Log("main target has been set!");
        }
            

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

        if (finishedPath && savedTarget != null)
        {
            if (_state != State.GETTING_PATH)
                SwitchToMoving();
        }

        debugState = _state;

        if (_state == State.MOVING_TO_TARGET && target.gameObject == null)
        {
            if (_state != State.GETTING_PATH)
                SwitchToMoving();
        }

    }


    public void SwitchToMoving()
    {

        // If the savedTarget is null that means we never called SwitchPath, so we are currently chasing the main target set at spawn
        if (savedTarget != null)
        {
            target = savedTarget;
        }
        StopCoroutine("RequestPath");
        StartCoroutine("RequestPath");
        _state = State.GETTING_PATH;
        Debug.Log("Continuing PATH TO Main Target.");


    }

    public void SwitchPathTarget(Transform newTarget)
    {
        savedTarget = target;
        target = newTarget;
        _state = State.MOVING_TO_TARGET;
       // StopCoroutine("FollowPath");
        Debug.Log("SWITCHED TARGET! New Target's position is: " + newTarget.position);
    }

    IEnumerator RequestPath()
    {
        while (true)
        {
           
            yield return new WaitForSeconds(0.7f);

          
            if (currPathPosition != target.position)
            {
                currPathPosition = target.position;

            
            }

            // Need to offset the target position only if the target is sitting on an unwalkable node
            if (!grid.NodeFromWorldPoint(currPathPosition).isWalkable || _state == State.MOVING_TO_TARGET)
            {
                //// check if my target is to my left or right

                //// If the target is to this unit's right, my target position should be to the target's left. Same thing but opposite if they are to my left.
                if (target.position.x > transform.position.x)
                {
                    // they are to my right
                    // move to the target's left
                    currPathPosition.x = currPathPosition.x - 2;
                }
                else
                {
                    // they are to my left
                    // move to the target's right
                    currPathPosition.x = currPathPosition.x + 2;

                }

                // If it is STILL an unwakable tile try pointing to right under it
                if (!grid.NodeFromWorldPoint(currPathPosition).isWalkable)
                {
                    // JUST GO UNDER IT!
                    currPathPosition.y = currPathPosition.y - 2;
                    Debug.Log("Trying to Go UNDER the tower attacking me.");
                }

            }

            if (!grid.NodeFromWorldPoint(transform.position).isWalkable)
            {
                // If the spot this Unit is on is NOT WALKABLE, we need to escape!!
                if (!isEscapingObstacle)
                {
                    if (CheckNeighborsForWalkable() != Vector3.zero)
                        escapePos = CheckNeighborsForWalkable();
                    Debug.Log("ESCAPE POS = " + escapePos);
                 
                    FullStop();
                    isEscapingObstacle = true;
                    StartCoroutine("EscapeObstacle");

                 
                }

               // FullStop();
            }

            PathRequestManager.RequestPath(transform.position, currPathPosition, OnPathFound);
            //print("Path requested.");



        }

    }

    public void OnPathFound(Vector3[] newPath, bool pathSuccesful)
    {
        if (pathSuccesful)
        {
            isEscapingObstacle = false;
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
            print("Could not find a path from this location. This node is walkable: " + currIswalkable + " Target node is walkable: " + grid.NodeFromWorldPoint(currPathPosition).isWalkable);


            if (_state != State.BLOCKED && _state != State.MOVING_TO_TARGET)
            {
                _state = State.BLOCKED;
            }
            

            if (!isEscapingObstacle && grid.NodeFromWorldPoint(currPathPosition).isWalkable)
            {
                Debug.Log("Escaping Obstacle");

                if (CheckNeighborsForWalkable() != Vector3.zero)
                    escapePos = CheckNeighborsForWalkable();
                Debug.Log("ESCAPE POS = " + escapePos);

                FullStop();
                isEscapingObstacle = true;
                StartCoroutine("EscapeObstacle");

            }
   

                

        }
    }


    Vector3 CheckNeighborsForWalkable()
    {
        var closestDistance = 1f;
        var closestNeighbor = Vector3.zero;
        bool isToMyRight = false;

        if (target.position.x > transform.position.x)
        {
            // target is to my right
            closestDistance = (target.position - (transform.position + Vector3.right)).sqrMagnitude;
            isToMyRight = true;
        }
        else
        {
            // target is to my left
            closestDistance = (target.position - (transform.position + Vector3.left)).sqrMagnitude;
            isToMyRight = false;
        }


        for (int y = -1; y <= 1; y++)   
        {
            for (int x = -1; x <= 1; x++)
            {
                Vector3 neighbor = new Vector3(transform.position.x + x, transform.position.y + y, 0);
                if (grid.NodeFromWorldPoint(neighbor).isWalkable)
                {
                    var newDistance = (target.position - neighbor).sqrMagnitude;
                    if ( newDistance <= closestDistance)
                    {
                        closestDistance = newDistance;
                        closestNeighbor = neighbor;
                    }
              
                }
            }
        }

        // To assure we dont get a vector 3 zero position as a return, at least return the left or right of my current position
        if (closestNeighbor == Vector3.zero)
        {
            if (isToMyRight)
            {
                closestNeighbor = transform.position + Vector3.left;
            }
            else
            {
                closestNeighbor = transform.position + Vector3.right;
            }
        }
        return closestNeighbor;
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
            if (Vector3.Distance(transform.position, escapePos) > 0.1f  && isEscapingObstacle)
            {
                transform.position = Vector3.MoveTowards(transform.position, escapePos, mStats.curMoveSpeed * Time.deltaTime);
                yield return null;
            }
            else
            {
                Debug.Log("STOPPED ESCAPING.");
                StartCoroutine("RequestPath");
                isEscapingObstacle = false;
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
        else
        {
            yield break;
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



            if (!isEscapingObstacle)
            {
                FullStop();

                if (CheckNeighborsForWalkable() != Vector3.zero)
                    escapePos = CheckNeighborsForWalkable();

                isEscapingObstacle = true;
                StartCoroutine("EscapeObstacle");
            }


       
        
        }

        //if (_state == State.BLOCKED)
        //{
        //    // A blocked enemy will detect hitting another Enemy to check if they have a way out
        //    if (coll.gameObject.CompareTag("Enemy"))
        //    {
        //        // When a unit hits another unit they should compare target's. 
        //        if (coll.gameObject.GetComponent<Enemy_PathHandler>().currPathPosition == currPathPosition)
        //        {
        //            // If the targets are the same, the unit that is closest to the target will give their current path to the other unit.
        //            var theirDistance = Vector3.Distance(coll.transform.position, currPathPosition);
        //            var myDistance = Vector3.Distance(transform.position, currPathPosition);

        //            if (theirDistance < myDistance)
        //            {
        //                Enemy_PathHandler theirPath = coll.gameObject.GetComponent<Enemy_PathHandler>();
        //                if (theirPath.path != null)
        //                {
        //                    Debug.Log("I'VE COLLIDED AGAINST A BROTHER! They are closer to the target than me!");

        //                    // Stop moving on path
        //                    FullStop();

        //                    // Now set my escape position to be equal to their next path position, so I will try to escape using their path which should be good to go.

        //                    escapePos = theirPath.path[theirPath.targetIndex];

        //                    // Start my escape if I'm not already escaping
        //                    if (!isEscapingObstacle)
        //                    {
        //                        isEscapingObstacle = true;
        //                        StartCoroutine("EscapeObstacle");

        //                    }
        //                }


        //            }
        //        }
        //    }
        //}
 
    }
}
