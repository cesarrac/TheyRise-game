using UnityEngine;
using System.Collections;
using System;

public class UnitPathHandler : MonoBehaviour {

    // How do I move?
    [Serializable]
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

    public MovementStats mStats = new MovementStats(); // Movement stats initialized by the component spawning this Unit

    public Transform target;
    Vector3 curTargetPosition;

    Vector3[] path; // Path filled by PathRequest manager once a path is requested
    int curPathIndex;

    Func<Vector3, Transform> GetTargetFunc; // This Function will return a target's transform when called. It must be assigned by another component.

    Action TargetReachedActionCB; // Action that is called when this unit reaches its target destination

    Action<Transform> AssignTargetToHandlerCB; // Action called to assign target to an external component such as an Attack Handler

    public bool avoidsPiling = true;

    private float pathDistanceToTravel;

    bool isCorrectingPath = false;

    float startTime;

    Vector3 correctedTargetPos, offsetPos;
    public Vector3 currWayPoint { get; protected set; }

    void OnEnable()
    {
        startTime = Time.time;
    }

    // What's my target?
    // NOTE: calling AssignTarget from the component that spawns this unit
    public void AssignTarget()
    {
        target = GetTargetFunc(transform.position);

        if (target == null)
            return;

        // Do I have to assign the target to a child/partner component (like an Attack Handler)?
        if (AssignTargetToHandlerCB != null)
            AssignTargetToHandlerCB(target);

        pathDistanceToTravel = Vector3.Distance(transform.position, target.position);

        isCorrectingPath = false;

        // Do I have a valid path to the target?
        GetANewPath();
    }

    public void RegisterAssignTargetToHandlerCB(Action<Transform> cb)
    {
        AssignTargetToHandlerCB = cb;
    }

    public void RegisterGetTargetFunc(Func<Vector3, Transform> foo)
    {
        GetTargetFunc = foo;
    }

    public void RegisterDestinationReachedCB(Action cb)
    {
        TargetReachedActionCB = cb;
    }

    public void GetANewPath()
    {
        StopCoroutine("RequestPath");
        StartCoroutine("RequestPath");
    }

    IEnumerator RequestPath()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.2f);

            if (!isCorrectingPath)
            {
                PathRequestManager.RequestPath(transform.position, target.position, gameObject, OnPathFound);
            }
            else
            {
                isCorrectingPath = false;
                PathRequestManager.RequestPath(transform.position, correctedTargetPos, gameObject, OnPathFound);
            }
            yield break;
        }
    }


    // Guarantee this unit has a path! This is called if the path request was unsuccesful.
    IEnumerator GuaranteeLegalPath()
    {
        while (true)
        {
            // Start a Lerp between the target and this unit's transform (as if it was using it to move by Time * speed)
            float fracJourney = ((Time.time - startTime) * 2f) / pathDistanceToTravel;

            // Record the return Vector3 as a potential position...
            Vector3 nextPos = Vector3.Lerp(target.position, transform.position, fracJourney);

            // ... turn it into a valid pathfinding grid position...
            correctedTargetPos = ResourceGrid.Grid.NodeFromWorldPoint(nextPos).worldPosition;

            // ... and Check if that Node is Walkable
            if (ResourceGrid.Grid.NodeFromWorldPoint(correctedTargetPos).isWalkable)
            {

                //Debug.Log("PATH CORRECTION: nextPos = " + nextPos + " correctTargetPos = " + correctedTargetPos +
                //    " from starting pos " + transform.position + " to target +  " + target.position);

                isCorrectingPath = true;

                GetANewPath();
                //PathRequestManager.RequestPath(transform.position, correctedTargetPos, gameObject, OnPathFound);

                yield break;
            }

            // In case nothing was found (the Lerp function has gone all the way back to the starting position)...
            if (nextPos == transform.position)
            {
                Debug.LogError("PATH Correction was unable to find a legal path. Is curr node Walkable? "
                    + ResourceGrid.Grid.NodeFromWorldPoint(transform.position).isWalkable
                    + " is target node Walkable? " + ResourceGrid.Grid.NodeFromWorldPoint(target.position).isWalkable);

                // Stop trying to correct a path
                yield break;
            }

            yield return null;
        }

    }

    public void OnPathFound(Vector3[] newPath, bool pathSuccesful)
    {
        // If path was succesfuly generated, go through it until you reach the target
        if (pathSuccesful)
        {
            // Draw the Path
            if (GetComponentInChildren<Path_Draw>() != null)
                GetComponentInChildren<Path_Draw>().DrawPath(newPath);

            // Stop requesting a path
            StopCoroutine("RequestPath");

            // Stop following path in case this unit was following one
            StopCoroutine("FollowPath");

            // Assign path and reset path index
            path = newPath;
            curPathIndex = 0;

            // Record the target's current position to make sure it does not move
            curTargetPosition = target.position;
            StartCoroutine("VerifyTargetPosition");

            if (path != null)
                StartCoroutine("FollowPath");
        }
        else
        {
            // Stop requesting a path
            StopCoroutine("RequestPath");

            // Stop following path in case this unit was following one
            StopCoroutine("FollowPath");

            // Stop verifying target
            StopCoroutine("VerifyTargetPosition");

            StopCoroutine("GuaranteeLegalPath");
            StartCoroutine("GuaranteeLegalPath");

            startTime = Time.time;
        }
    }

    IEnumerator VerifyTargetPosition()
    {
        while (true)
        {
            // Track the position of my target to make sure it has not changed...
            if (curTargetPosition != target.position)
            {
                // ... if target has moved, break out get a new Path
                GetANewPath();
                yield break;
            }
            yield return new WaitForSeconds(2f);
        }
    }

    IEnumerator FollowPath()
    {
        if (path.Length > 0)
        {
            currWayPoint = path[0];
            while (true)
            {
                // Advance path index when my position is equal to the current node
                if (transform.position == currWayPoint)
                {
                    // Update the path draw
                    if (GetComponentInChildren<Path_Draw>() != null)
                        GetComponentInChildren<Path_Draw>().UpdatePath(path, curPathIndex);

                    curPathIndex++;

                    // Have I arrived at my destination? If so...
                    if (curPathIndex >= path.Length)
                    {
                        // NOTE: A unit that avoids Piling will offset its final position after it has reached the end
                        // of its current path. It should NOT attack until it has arrived at its final offset position.

                        // Stop verifying the target's position because we are already there.
                        StopCoroutine("VerifyTargetPosition");

                        // This will begin a coRoutine that pushes the unit to a final "offset" position to avoid
                        // units piling on top of each other.
                        if (avoidsPiling)
                            OffsetLastPosition();
                        else
                        {
                            if (TargetReachedActionCB != null)
                                TargetReachedActionCB();
                        }

                        curPathIndex = 0;
                        path = null;

                        yield break;
                    }
                    else
                    {
                        if (ResourceGrid.Grid.NodeFromWorldPoint(path[curPathIndex]).isWalkable)
                        {
                            // the next node is walkable, keep going
                            currWayPoint = path[curPathIndex];
                        }
                        else
                        {
                            // next node is NOT walkable
                            yield break;
                        }
                    }
                }

                // Move
                transform.position = Vector2.MoveTowards(transform.position, currWayPoint, mStats.curMoveSpeed * Time.deltaTime);

                yield return null;
            }
        }
        else
        {
            yield break;
        }
    }

    public void StopFollowingPathAndAttack(Transform newTarget)
    {
        StopCoroutine("VerifyTargetPosition");
        StopCoroutine("FollowPath");

        if (AssignTargetToHandlerCB != null)
            AssignTargetToHandlerCB(newTarget);

        OffsetLastPosition();
    }

    void OffsetLastPosition()
    {
        float offset = 0.5f;
        float offsetX = UnityEngine.Random.Range(transform.position.x - offset, transform.position.x + offset);
        float offsetY = UnityEngine.Random.Range(transform.position.y - offset, transform.position.y + offset);
        offsetPos = new Vector3(offsetX, offsetY, 0);

        StartCoroutine("OffsetDestination");
    }

    IEnumerator OffsetDestination()
    {
        while (true)
        {
            if (transform.position != offsetPos)
            {
                transform.position = Vector2.MoveTowards(transform.position, offsetPos, 4 * Time.deltaTime);

            }
            else
            {
                // What do I do when I arrive?
                if (TargetReachedActionCB != null)
                    TargetReachedActionCB();

                yield break;
            }


            yield return null;
        }
    }

    // TODO:
    // What have I covered so far?
    // Getting a target and requesting a path
    // Tracking a moving target and following a path to it
    // Reaching a destination and calling an action
    // Ofsetting the final position to avoid piling units on top of each other
    // Issues:
    // Some units spawn and for some reason don't get a succesful path when other units right beside them do. 
    // Yet when they are waiting and I move the Player they suddenly are able to get a path. 
    // This might be that the target and path have been defined but for some reason the Follow Path coroutine is not kicking in.
    // What is probably making them eventually move is the Verify Target subroutine, that kicks off GetPath and FollowPath. (Bug or feature?)





    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    //
    //                               SPEED CHANGE CODE ( for any external factors that might manipulate this unit's speed stat)
    //
    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    public void ChangeSpeed(float newSpeed, float effectTime)
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
}
