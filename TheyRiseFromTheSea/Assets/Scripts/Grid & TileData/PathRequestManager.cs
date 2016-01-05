using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

/// <summary>
/// _________ Takes individual find-path requests from units and adds them to a queue that will Find their path, first come first serve. _____________
/// </summary>
public class PathRequestManager : MonoBehaviour
{

    Queue<PathRequest> pathRequestQueue = new Queue<PathRequest>();
    PathRequest currPathRequest;

    static PathRequestManager Instance;

    Pathfinding pathfinding;
    bool isProcessingPath;

    void Awake()
    {
        Instance = this;
        pathfinding = GetComponent<Pathfinding>();
    }

    // The action (callback) stores the method that Receives Path in unit so this Manager can call it once it has calculated that unit's path
    public static void RequestPath(Vector3 pathStart, Vector3 pathEnd, GameObject requesterObj, Action<Vector3[], bool> callback)
    {
        PathRequest newRequest = new PathRequest(pathStart, pathEnd, callback, requesterObj);

        Instance.pathRequestQueue.Enqueue(newRequest);
        //print("Path request " + (Instance.pathRequestQueue.Count) + " added.");
        Instance.TryProcessNext();
    }

    void TryProcessNext()
    {
        // check if we are currently processing a path, if we are NOT it tells us we can process the next one
        if (!isProcessingPath && pathRequestQueue.Count > 0)
        {
            currPathRequest = pathRequestQueue.Dequeue();
            isProcessingPath = true;
            pathfinding.StartFindPath(currPathRequest.pathStart, currPathRequest.pathEnd);
        }
    }

    public void FinishedProcessingPath(Vector3[] path, bool success)
    {
        // Before calling the callback check to make sure that the unit requesting a path is not null or inactive
        if (currPathRequest.unitGObjRequesting != null)
        {
            if (currPathRequest.unitGObjRequesting.activeSelf)
            {
                currPathRequest.callback(path, success);
            }
               
        }
     
        isProcessingPath = false;
        TryProcessNext();
    }

    struct PathRequest
    {
        public Vector3 pathStart;
        public Vector3 pathEnd;
        public Action<Vector3[], bool> callback;
        public GameObject unitGObjRequesting;

        public PathRequest(Vector3 _start, Vector3 _end, Action<Vector3[], bool> _callback, GameObject unitGObj)
        {
            pathStart = _start;
            pathEnd = _end;
            callback = _callback;
            unitGObjRequesting = unitGObj;
        }
    }
}
