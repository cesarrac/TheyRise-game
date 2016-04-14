using UnityEngine;
using System.Collections.Generic;

public class Job_Manager : MonoBehaviour {

	public static Job_Manager Instance { get; protected set; }

    List<Job> jobs_available;
    List<Job> jobs_cancelled; // A list that will get filled with all cancelled jobs to check against when adding new jobs.
                              // This will keep Employees from adding jobs that I wanted to cancel whenever they leave a job
                              // they think they are leaving unfinished.
    Dictionary<Vector3, GameObject> taskCircles = new Dictionary<Vector3, GameObject>();

    void Awake()
    {
        Instance = this;

        jobs_available = new List<Job>();
        jobs_cancelled = new List<Job>();

        // Every 60 seconds we check the cancelled jobs list, if it contains jobs clear it
        InvokeRepeating("CheckCancelledJobs", 60, 60);
    }

    void Start()
    {
        InitTaskButtons();
    }

    void InitTaskButtons()
    {
        // TODO: Read this from an array of possible tasks or an external database of tasks.
        UI_Manager.Instance.CreateTaskJobButtons(JobType.Mine);
        UI_Manager.Instance.CreateTaskJobButtons(JobType.Repair);
        UI_Manager.Instance.CreateTaskJobButtons(JobType.Cancel);


    }

    void CheckCancelledJobs()
    {
        // Clear the cancelled jobs list every 60 seconds
        if (jobs_cancelled.Count > 0)
            jobs_cancelled.Clear();
    }

    // These are manually assigned Jobs, called by the Mouse Controller
    public void AddOrCancelTaskJob(TileData tile, JobType jType)
    {
        Transform tileTransform = ResourceGrid.Grid.GetTileGameObjFromIntCoords
                                                 (tile.posX, tile.posY).transform
                                   != null ?
                                   ResourceGrid.Grid.GetTileGameObjFromIntCoords
                                                 (tile.posX, tile.posY).transform
                                   :
                                   null;
        if (tileTransform != null)
        {
            if (jType != JobType.Cancel)
            {
                AddJob(jType, tile.tileType, tileTransform, true);
            }
            else
            {
                CancelJob(jType, tile.tileType, tileTransform);
            }
        }
    }

    public void AddJob(JobType jType, TileData.Types tileType, Transform target, bool isTask = false, bool hasStarted = false)
    {
        if (IsDuplicateJob(jType, target) == true)
            return;

        if (jobs_cancelled.Count > 0)
        {
            if (IsCancelledJob(jType, target) == true)
                return;
        }

        // Make the job's hardness less than the default 0.5 for jobs like construction/assembly
        if (tileType != TileData.Types.rock)
            jobs_available.Add(new Job(jType, tileType, target, hasStarted, 0.1f));
        else
        {
            jobs_available.Add(new Job(jType, tileType, target, hasStarted));
        }

        if (isTask)
        {
            // Spawn a selection circle on the selected tile to indicate the task's / job's target
            GameObject task_circle = ObjectPool.instance.GetObjectForType("Selection Circle", true, target.position);
            taskCircles.Add(target.position, task_circle);
        }

        Debug.Log("Job Added for " + tileType.ToString());
    }

    bool IsDuplicateJob(JobType jType, Transform target)
    {
        // Check that there isn't a job for this target with this same job type already,
        // to avoid duplication of jobs!
        foreach (Job j in jobs_available)
        {
            if (j.Job_Target == target)
            {
                if (j.Job_Type == jType)
                    return true;
            }
        }

        return false;
    }

    bool IsCancelledJob(JobType jType, Transform target)
    {
        foreach (Job j in jobs_cancelled)
        {
            if (j.Job_Target == target)
            {
                if (j.Job_Type == jType)
                    return true;
            }
        }
        return false;
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

    public void CancelJob(JobType jType, TileData.Types tileType, Transform target)
    {
        Debug.Log("Cancelling job!");
        jobs_cancelled.Add(new Job(jType, tileType, target));
        if (taskCircles.ContainsKey(target.position))
        {
            ObjectPool.instance.PoolObject(taskCircles[target.position]);
            taskCircles.Remove(target.position);
        }
    }

}
