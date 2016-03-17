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

    Vector3[] path; // Path filled by PathRequest manager once a path is requested
    int curPathIndex;

    Func<Transform> GetTargetFunc; // This Function will return a target's transform when called. It must be assigned by another component.

    Action TargetReachedActionCB; // Action that is called when this unit reaches its target destination

    Action<Transform> AssignTargetToHandlerCB; // Action called to assign target to an external component such as an Attack Handler


    // What's my target?
    // NOTE: calling AssignTarget from the component that spawns this unit

    public void AssignTarget()
    {
        target = GetTargetFunc();

        // Do I have to assign the target to a child/partner component (like an Attack Handler)?
        if (AssignTargetToHandlerCB != null)
            AssignTargetToHandlerCB(target);

        // Do I have a valid path to the target?
        StartCoroutine("RequestPath");
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

    IEnumerator RequestPath()
    {
        while (true)
        {

            yield return new WaitForSeconds(0.7f);


            PathRequestManager.RequestPath(transform.position, target.position, gameObject, OnPathFound);

        }

    }

    public void OnPathFound(Vector3[] newPath, bool pathSuccesful)
    {
        // If path was succesfuly generated, go through it until you reach the target
        if (pathSuccesful)
        {
            // Stop requesting a path
            StopCoroutine("RequestPath");

            // Assign path and reset path index
            path = newPath;
            curPathIndex = 0;

            if (path != null)
                StartCoroutine("FollowPath");
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
                    curPathIndex++;

                    // Have I arrived at my destination? If so...
                    if (curPathIndex >= path.Length)
                    {
                        // What do I do when I arrive?
                        if (TargetReachedActionCB != null)
                            TargetReachedActionCB();

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

    // TODO:
    // How do I handle moving targets? 
    // Check every frame to make sure target is in the same position? and if not, stop follow path routine and start request path again?
    // What if destination has been reached and THEN target moves? Do I continue to check for target's updated position after reaching destination?
    // I would have to make sure the target is still alive and its gameobj is active. But I would also need to stop the action that is being
    // executed when I reached destination (like attacking) before I can start on a new path.


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
