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

    public static void RequestJob(JobType[] jobTypes, Action<Job, bool> cb)
    {
        JobRequest jobRequest = new JobRequest(jobTypes, cb);

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
            Job_Manager.Instance.FindJobs(curJobRequest.jobTypes);
        }
    }

    public void FinishedCheckingJobsAvailable(Job aJob = null)
    {
        if (aJob != null)
        {
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
    public JobType[] jobTypes;
    public Action<Job, bool> callback;

    public JobRequest(JobType[] jTypes, Action<Job, bool> cb)
    {
        jobTypes = jTypes;
        callback = cb;
    }

}
