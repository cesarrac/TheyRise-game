using UnityEngine;
using System.Collections;
using System;

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

        public void InitStartingMoveStats(float move_spd, float chase_spd)
        {
            startMoveSpeed = move_spd;
            startChaseSpeed = chase_spd;
        }
    }

    public MovementStats mStats = new MovementStats();


    Transform target, savedTarget;
    public Transform alternateMainTarget;

    Vector3[] path;
    int targetIndex;

    ResourceGrid grid;
    bool finishedPath = false, canWalk = true;
    Vector3 lastTargetPos;

    public enum State { GETTING_PATH, FOLLOWING_PATH, FINISHED_PATH, MOVING_TO_TARGET, BLOCKED, ATTACKING, IDLE, ESCAPING, STOPPED }
    public State _state { get; protected set; }

    public State debugState;

    public TileData blockingTile { get; protected set; }

    Vector3 escapePos;

    public bool isEscapingObstacle { get; protected set; }

    Vector3 currPathPosition; // < ------ To check against so we don't request a path when our target's position hasn't changed

    Rigidbody2D rb;

    Enemy_AttackHandler enemy_AttackHandler;

    float threshold;

    Func<Transform> GetTargetCB;


    bool isFullyStopped = false; // < ---- flag sets to true when Full Stop is called
    bool isInRange = false;
    public bool InRange { get { return isInRange; } set { isInRange = value; } }

    // Steering Behaviors:
    //  seek : normal follow path to the target
    //  Separate: follows and path but always maintains a distance to a certain other object by shifting its speed

    void OnEnable()
    {
        grid = ResourceGrid.Grid;

        ResetFlagsAndTargets();

    }

    void ResetFlagsAndTargets()
    {
        FullStop();
        isInRange = false;
        target = null;
        currPathPosition = new Vector3();
        isFullyStopped = false;

    }

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        enemy_AttackHandler = GetComponent<Enemy_AttackHandler>();
    }

    public void RegisterGetTargetCB(Func<Transform> cb)
    {
        GetTargetCB = cb;
    }

    void Start()
    {
        // Initialize Movement Speed stat
        //mStats.InitMoveStats();

        if (target == null)
        {

            InitTarget();
        }
        _state = State.GETTING_PATH;

        //StartCoroutine("DebugMyStatus");
    }

    public void InitTarget()
    {

        if (GetTargetCB != null)
        {
            target = GetTargetCB();
            // Make one last check to make sure that this new target is not null or pointing to an inactive (pooled) object
            if (target == null || target.gameObject.activeSelf == false)
            {
                // Then just set it to the player
                target = FindPlayerTarget();
            }
        }
        else
        {
            target = FindPlayerTarget();
        }

        isFullyStopped = false;

        StartCoroutine("RequestPath");

        // Set my Attack Handler's main target
        GetComponent<Enemy_AttackHandler>().SetMainTarget(target);
    }

    void SetAltTarget(Transform t)
    {
        target = t;

        isFullyStopped = false;

        StartCoroutine("RequestPath");

        // Set my Attack Handler's main target
        GetComponent<Enemy_AttackHandler>().SetMainTarget(target);
    }


    // This is a backup in case the path handler fails to get a Target from the Enemy Master (this will always return the player as the target!)
    Transform FindPlayerTarget()
    {
        return GameObject.FindGameObjectWithTag("Citizen").transform;
    }

    void Update()
    {

        debugState = _state;

        if (enemy_AttackHandler != null)
        {
            if (enemy_AttackHandler.stats.curHP > 0)
            {
                CheckForNullTarget();

                StopWhenInRange();
            }
        }

    }

    //IEnumerator DebugMyStatus()
    //{
    //    while (true)
    //    {
    //        yield return new WaitForSeconds(5f);
    //        string status = "inRange = " + isInRange + " target = " + target.gameObject + " isFullyStopped = " + isFullyStopped + " finishedPath = " + finishedPath;
    //        Debug.Log("ENEMY PATH STATUS: " + status);
    //    }
    //}

    void CheckForNullTarget()
    {
        // Check if the target's gameobject has been POOLED, if it has make the target null so it starts getting a path again
        if (target == null || target.gameObject.activeSelf == false)
        {
            InitTarget();
        }
    }

    void StopWhenInRange()
    {
        isInRange = CheckIsInRangeOfTarget();

        // Check that the target is in range and that its gameObject is currently active
        if (isInRange)
        {
            // Target is in range!

            // Halt all moving and getting path CoRoutines while we are in range and attacking
            if (!isFullyStopped)
            {
               // Debug.Log("ENEMY: Target in range!");
                FullStop();
            }
        }
        else
        {
            //    // Target NOT in range or NOT currently active

            //    //// If this unit had finished its path and the target has moved (most likely it's a Player), unflag finishedPath
            //    //if (target != null)
            //    //{
            //    //    if (finishedPath)
            //    //    {
            //    //        finishedPath = false;
            //    //        ContinueOnPath();
            //    //        //print("My target's position is: " + target);

            //    //    }
            //    //}


            // Swith Back to main target (if needed) and continue getting a path.
            if (isFullyStopped)
            {
                // This will check if Target needs to be reset to main target and continue on the path towards it
                InitTarget();
            }
        }
    }


    // Always be checking if we are in range of our target

    bool CheckIsInRangeOfTarget()
    {
        threshold = enemy_AttackHandler.AttackRange;
        if (target != null)
        {
            if ((target.position - transform.position).sqrMagnitude <= threshold * threshold)
            {
                return true;
            }
            else
                return false;
        }
        else
            return false;
      
    }


    //public void SwitchTargetBackToMain()
    //{

    //    // If the savedTarget is null that means we never called SwitchPath, so we are currently chasing the main target set at spawn
    //    if (savedTarget != null)
    //    {
    //        target = savedTarget;
    //    }
    //    ContinueOnPath();
    //    //Debug.Log("Continuing PATH TO Main Target.");
    //}

    void ContinueOnPath()
    {
        if (target != null)
        {
            isFullyStopped = false;
            StopCoroutine("RequestPath");
            StartCoroutine("RequestPath");
            _state = State.GETTING_PATH;
        }
        else
            Debug.Log("ENEMY: Can't continue path because my target is null. I've already tried to load the saved target... Is the Main Target set to a non-null object?");
       
    }

    public void SwitchPathTarget(Transform newTarget)
    {
        if (newTarget != target)
        {
            savedTarget = target;
            target = newTarget;
        }
     
        _state = State.GETTING_PATH;
        StopCoroutine("FollowPath");
        StopCoroutine("RequestPath");
        StartCoroutine("RequestPath");
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

            // Case #1 for failed path:
            // - Target node is unwalkable.
            // Solution: offset the target position

            // Offset:
            if (!grid.NodeFromWorldPoint(currPathPosition).isWalkable)
            {
                // Check if my target is to my LEFT or RIGHT

                // If the target is to this unit's right, my target position should be to the target's left. Same thing but opposite if they are to my left.
                if (currPathPosition.x > transform.position.x)
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
                    // Reset the x
                    currPathPosition.x = target.position.x;
                    currPathPosition.y = currPathPosition.y - 3;
                   // Debug.Log("ENEMY: Trying to Go UNDER the target.");

                    // If STILL unwalkable go OVER it
                    if (!grid.NodeFromWorldPoint(currPathPosition).isWalkable)
                    {
                        currPathPosition.y = currPathPosition.y + 6;
                       // Debug.Log("ENEMY: Trying to Go OVER the target.");
                    }
                }

            }

            PathRequestManager.RequestPath(transform.position, currPathPosition, gameObject, OnPathFound);

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

            // If this Unit is not chasing the player, it doesn't need to recheck its path. The target does not move.
            //if (!chasesPlayer && target.gameObject.tag != "Citizen")
            //{
            //    StopCoroutine("RequestPath");
            //}
        }
        else
        {
            bool currIswalkable = grid.NodeFromWorldPoint(transform.position).isWalkable;
            //print("Could not find a path from this location. This node is walkable: " + currIswalkable + " Target node is walkable: " + grid.NodeFromWorldPoint(currPathPosition).isWalkable);


            if (_state != State.BLOCKED && _state != State.MOVING_TO_TARGET)
            {
                _state = State.BLOCKED;
            }

            // In the case of a failed path there's three reasons for why. 
            // 1) A collider is blocking the way. Solution:  Detect the collision, STOP PATH, and bounce away from it. Once out of collision CONTINUE PATH.
            // 2) The current Node is unwalkable. Solution:  Just bounce up everytime this case occurs, until it gets the correct path and continues.
            // 3) The target Node is unwalkable.  Solution:  The actual path request function will offset the target in this case. 

            // Case # 2: "The current Node i'm on is unwalkable."
            if (!currIswalkable)
            {
                // Bounce up away from my current position
                BounceOffObstacle(transform.position + Vector3.left);

            }
        }
    }

    void BounceOffObstacle(Vector3 obstaclePos, bool moveTowards = false)
    {
        if (!moveTowards)
        {
            var heading = -(obstaclePos - transform.position);
            rb.AddForce(heading * 800f);
        }
        else
        {
            var heading = obstaclePos - transform.position;
            rb.AddForce(heading * 200f);
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
                            // if the next node is a wall, change main target to the wall game obj
                            TileData t = ResourceGrid.Grid.TileFromWorldPoint(path[targetIndex]);

                            if (t != null && t.tileType == TileData.Types.wall)
                            {
                                SetAltTarget(ResourceGrid.Grid.GetTileGameObjFromIntCoords(t.posX, t.posY).transform);

                                yield break;
                            }

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


                transform.position = Vector2.MoveTowards(transform.position, currWayPoint, mStats.curMoveSpeed * Time.deltaTime);

                yield return null;
            }
        }
        else
        {
            //Debug.Log("ENEMY: Path is broke because it has 0 positions in it!");
            FullStop();
            // Push towards the current target so we can get there!
            // BounceOffObstacle(target.transform.position, true);
            yield break;
        }


    }

    void GetBlockingTile(Vector3 position)
    {
        blockingTile = ResourceGrid.Grid.TileFromWorldPoint(position);
    }

    bool CheckIfPathIsBlocked(Node nextNode)
    {
        return nextNode.isWalkable;
    }


    public void FullStop()
    {
        _state = State.STOPPED;
        isFullyStopped = true;
        targetIndex = 0;
        StopCoroutine("FollowPath");
        StopCoroutine("RequestPath");
        StopCoroutine("EscapeObstacle");

    }

    public void ChangeSpeed (float newSpeed, float effectTime)
    {
        mStats.curMoveSpeed = newSpeed;
        StartCoroutine(SpeedChange(effectTime));
    }

    IEnumerator SpeedChange(float timeOfEffect)
    {
        while (true)
        {
            yield return new WaitForSeconds(timeOfEffect);
            ResetSpeed();
            yield break;
        }
    }

    public void ResetSpeed()
    {
        mStats.curMoveSpeed = mStats.startMoveSpeed;
    }

    //void OnCollisionEnter2D(Collision2D coll)
    //{
    //    if (coll.gameObject.CompareTag("Rock"))
    //    {
    //        Debug.Log("Colliding with ROCK!");

    //        FullStop();

    //        // Try bouncing off the rock!
    //        BounceOffObstacle(coll.transform.position);
          

    //        // Make the position of my collision with the rock an unwalkable tile so we don't path through it again
    //        grid.NodeFromWorldPoint(coll.transform.position).isWalkable = false;

    //        _state = State.ESCAPING;

    //    }

    //    //if (_state == State.BLOCKED)
    //    //{
    //    //    // A blocked enemy will detect hitting another Enemy to check if they have a way out
    //    //    if (coll.gameObject.CompareTag("Enemy"))
    //    //    {
    //    //        // When a unit hits another unit they should compare target's. 
    //    //        if (coll.gameObject.GetComponent<Enemy_PathHandler>().currPathPosition == currPathPosition)
    //    //        {
    //    //            // If the targets are the same, the unit that is closest to the target will give their current path to the other unit.
    //    //            var theirDistance = Vector3.Distance(coll.transform.position, currPathPosition);
    //    //            var myDistance = Vector3.Distance(transform.position, currPathPosition);

    //    //            if (theirDistance < myDistance)
    //    //            {
    //    //                Enemy_PathHandler theirPath = coll.gameObject.GetComponent<Enemy_PathHandler>();
    //    //                if (theirPath.path != null)
    //    //                {
    //    //                    Debug.Log("I'VE COLLIDED AGAINST A BROTHER! They are closer to the target than me!");

    //    //                    // Stop moving on path
    //    //                    FullStop();

    //    //                    // Now set my escape position to be equal to their next path position, so I will try to escape using their path which should be good to go.

    //    //                    escapePos = theirPath.path[theirPath.targetIndex];

    //    //                    // Start my escape if I'm not already escaping
    //    //                    if (!isEscapingObstacle)
    //    //                    {
    //    //                        isEscapingObstacle = true;
    //    //                        StartCoroutine("EscapeObstacle");

    //    //                    }
    //    //                }


    //    //            }
    //    //        }
    //    //    }
    //    //}
 
    //}

    //void OnCollisionExit2D(Collision2D coll)
    //{
    //    if (coll.gameObject.CompareTag("Rock"))
    //    {
    //        // Go back to getting a path
    //        SwitchTargetBackToMain();

    //    }
    //}
}
