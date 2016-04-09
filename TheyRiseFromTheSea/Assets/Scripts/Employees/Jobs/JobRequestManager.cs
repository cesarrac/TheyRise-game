using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class JobRequestManager : MonoBehaviour {

    public static JobRequestManager instance { get; protected set; }

    Queue<JobRequest> jobQueue = new Queue<JobRequest>();

    JobRequest curJobRequest;

    bool isHandlingJobRequest = false;

    void Awake()
    {
        instance = this;
    }

    public static void RequestJob(TileData.Types[] tileTypes, Action<Job, bool> cb)
    {
        JobRequest jobRequest = new JobRequest(tileTypes, cb);

        instance.jobQueue.Enqueue(jobRequest);

        //Debug.Log("Job Request for " + tileType.ToString() + " was added to queue!");

        instance.TryNext();

    }

    void TryNext()
    {
        if (!isHandlingJobRequest && jobQueue.Count > 0)
        {
            curJobRequest = jobQueue.Dequeue();
            isHandlingJobRequest = true;
            Debug.Log("Finding job...");
            Job_Manager.Instance.FindJobs(curJobRequest.jobTileTypes);
        }
    }

    public void FinishedCheckingJobsAvailable(Job aJob = null)
    {
        if (aJob != null)
        {
            Debug.Log("Job found, calling do action...");

            Job_Manager.Instance.RemoveJob(aJob);
            curJobRequest.callback(aJob, true);
            
        }
        else
        {
            curJobRequest.callback(aJob, false);
        }
        isHandlingJobRequest = false;
        TryNext();
    }
}

struct JobRequest
{
    public TileData.Types[] jobTileTypes;
    public Action<Job, bool> callback;

    public JobRequest(TileData.Types[] tileTypes, Action<Job, bool> cb)
    {
        jobTileTypes = tileTypes;
        callback = cb;
    }

}
