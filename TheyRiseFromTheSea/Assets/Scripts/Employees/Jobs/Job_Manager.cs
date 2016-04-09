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

    public void AddJob(TileData.Types tileType, Transform target)
    {
        jobs_available.Add(new Job(tileType, target));
        Debug.Log("Job Added for " + tileType.ToString());
    }

    public void FindJobs(TileData.Types[] tileTypes)
    {
        foreach (Job job in jobs_available)
        {
            for (int i = 0; i < tileTypes.Length; i++)
            {
                if (job.Job_TileType == tileTypes[i])
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
