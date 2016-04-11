using UnityEngine;
using System.Collections.Generic;

public class Job_Manager : MonoBehaviour {

	public static Job_Manager Instance { get; protected set; }

    List<Job> jobs_available;

    void Awake()
    {
        Instance = this;

        jobs_available = new List<Job>();
    }

    public void AddJob(JobType jType, TileData.Types tileType, Transform target, bool hasStarted = false)
    {
        // Make the job's hardness less than the default 0.5 for jobs like construction/assembly
        if (tileType != TileData.Types.rock)
            jobs_available.Add(new Job(jType, tileType, target, hasStarted, 0.1f));
        else
        {
            jobs_available.Add(new Job(jType, tileType, target, hasStarted));
        }

        Debug.Log("Job Added for " + tileType.ToString());
    }

    public void FindJobs(JobType[] jobTypes)
    {
        foreach (Job job in jobs_available)
        {
            for (int i = 0; i < jobTypes.Length; i++)
            {
                if (job.Job_Type == jobTypes[i])
                {
                    JobRequestManager.instance.FinishedCheckingJobsAvailable(job);
                    return;
                }
            }
       
        }

        JobRequestManager.instance.FinishedCheckingJobsAvailable();
    }

    public void RemoveJob(Job completedJob)
    {
        jobs_available.Remove(completedJob);
    }
}
