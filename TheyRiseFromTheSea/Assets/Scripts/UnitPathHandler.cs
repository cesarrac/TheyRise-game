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

    Func<Transform> GetTargetFunc; // This Function will return a target's transform when called. It must be assigned by another component.

    Action TargetReachedActionCB; // Action that is called when this unit reaches its target destination

    Action<Transform> AssignTargetToHandlerCB; // Action called to assign target to an external component such as an Attack Handler

    public bool avoidsPiling = true;

    private float pathDistanceToTravel;

    bool isCorrectingPath = false;

    float startTime;

    // What's my target?
    // NOTE: calling AssignTarget from the component that spawns this unit

    public void AssignTarget()
    {
        target = GetTargetFunc();

        // Do I have to assign the target to a child/partner component (like an Attack Handler)?
        if (AssignTargetToHandlerCB != null)
            AssignTargetToHandlerCB(target);

        pathDistanceToTravel = Vector3.Distance(transform.position, target.position);

        Debug.Log("PATH distance to travel = " + pathDistanceToTravel);

        isCorrectingPath = false;

        startTime = Time.time;

        // Do I have a valid path to the target?
        GetANewPath();
    }

    public void RegisterAssignTargetToHandlerCB(Action<Transform> cb)
    {
        AssignTargetToHandlerCB += cb;
    }

    public void RegisterGetTargetFunc(Func<Transform> foo)
    {
        GetTargetFunc += foo;
    }

    public void RegisterDestinationReachedCB(Action cb)
    {
        TargetReachedActionCB += cb;
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

            PathRequestManager.RequestPath(transform.position, target.position, gameObject, OnPathFound);
        }
    }

    // TODO: Have a function that will guarantee this unit has a path!
    void Update()
    {
        if (isCorrectingPath)
        {
            GuaranteeLegalPath();
        }
    }
    void GuaranteeLegalPath()
    {
        float fracJourney = ((Time.time - startTime) * 2f) / pathDistanceToTravel;
        Vector3 nextPos = Vector3.Lerp(target.position, transform.position, fracJourney);

        Vector3 correctedTargetPos = ResourceGrid.Grid.NodeFromWorldPoint(nextPos).worldPosition;

        if (ResourceGrid.Grid.NodeFromWorldPoint(correctedTargetPos).isWalkable)
        {
           
            Debug.Log("PATH CORRECTION: nextPos = " + nextPos + " correctTargetPos = " + correctedTargetPos +
                " from starting pos " + transform.position + " to target +  " + target.position);

            PathRequestManager.RequestPath(transform.position, correctedTargetPos, gameObject, OnPathFound);

            isCorrectingPath = false;
        }

        // In case nothing was found...
        if (nextPos == transform.position)
        {
            Debug.LogError("PATH Correction was unable to find a legal path. Is curr node Walkable? "
                + ResourceGrid.Grid.NodeFromWorldPoint(transform.position).isWalkable
                + " is target node Walkable? " + ResourceGrid.Grid.NodeFromWorldPoint(target.position).isWalkable);

            isCorrectingPath = false;
        }
    }

    public void OnPathFound(Vector3[] newPath, bool pathSuccesful)
    {
        // If path was succesfuly generated, go through it until you reach the target
        if (pathSuccesful)
        {
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
            //Debug.LogError("PATH: " + gameObject.name + " cannot find path to target " + target.gameObject.name + 
            //    " at position: " + target.position + " Current Node walkability is: + " + ResourceGrid.Grid.NodeFromWorldPoint(transform.position).isWalkable);

            // Stop requesting a path
            StopCoroutine("RequestPath");

            // Stop following path in case this unit was following one
            StopCoroutine("FollowPath");

            Debug.Log("PATH: Could not find a path, requesting correction...");
            //StartCoroutine("GuaranteeLegalPath");
            isCorrectingPath = true;
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
                Debug.Log("Target has moved, requesting a new path!");
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
            Vector3 currWayPoint = path[0];
            while (true)
            {

                // Advance path index when my position is equal to the current node
                if (transform.position == currWayPoint)
                {
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
                            StartCoroutine("OffsetDestination");
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


    IEnumerator OffsetDestination()
    {
        float offset = 0.5f;
        float offsetX = UnityEngine.Random.Range(transform.position.x - offset, transform.position.x + offset);
        float offsetY = UnityEngine.Random.Range(transform.position.y - offset, transform.position.y + offset);
        Vector3 offsetPos = new Vector3(offsetX, offsetY, 0);

        while (true)
        {
            if (transform.position != offsetPos)
            {
                transform.position = Vector2.MoveTowards(transform.position, offsetPos, 4 * Time.deltaTime);

                yield return null;
            }
            else
            {
                // What do I do when I arrive?
                if (TargetReachedActionCB != null)
                    TargetReachedActionCB();

                yield break;
            }
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

    // Since units avoid piling they take a little longer to begin their attack, since they are getting into offset position.
    // This might make them too easy to kill and looks very strange when they get to the target then back off to move to their off set pos.
    // I can consider:
    // forcing the direction of the offset to match the heading the unit was on
    // *** Giving them a higher speed when doing the offset does the trick quite nicely



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
