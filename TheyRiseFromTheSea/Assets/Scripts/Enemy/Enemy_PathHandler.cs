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

    public TileData blockingTile { get; protected set; }

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
           
            yield return new WaitForSeconds(1f);
            Vector3 targetPosition = target.position;
            if (_state == State.MOVING_TO_TARGET || _state == State.BLOCKED)
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
            if (_state != State.BLOCKED)
                _state = State.BLOCKED;

            //GetBlockingTile(transform.position);
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
                            GetBlockingTile(path[targetIndex]);
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
        blockingTile = ResourceGrid.Grid.TileFromWorldPoint(transform.position);
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
}
