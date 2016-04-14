using UnityEngine;
using System.Collections;

public enum JobType
{
    Assemble,
    Mine,
    Pump_Water,
    Heal,
    Repair,
    Operate, 
    Cancel
}

public class Job {

    // An Employee requires a Tile Type and the Transform of the game Object located at 
    // the tile where the job is.
    // A Job requires a Tile Type and a Transform target

    TileData.Types job_tileType;
    public TileData.Types Job_TileType { get { return job_tileType; } }

    Transform job_target;
    public Transform Job_Target { get { return job_target; } }

    float job_hardness; // Determines how much energy an employee loses every time they perform an action to do this job
    public float Job_Hardness { get { return job_hardness; } }

    bool isCompleted = false;
    public bool IsCompleted { get { return isCompleted; } }

    bool hasBeenStarted = false;
    public bool HasBeenStarted { get { return hasBeenStarted; } }

    JobType job_type;
    public JobType Job_Type { get { return job_type; } }

    public Job() { }

    public Job (JobType _jobType, TileData.Types tileType, Transform target, bool hasStarted = false, float hardness = 0.5f)
    {
        job_type = _jobType;
        job_tileType = tileType;
        job_target = target;
        job_hardness = hardness;
        hasBeenStarted = hasStarted;
    }

    // Copy constructor
    public Job (Job jobB)
    {
        job_type = jobB.job_type;
        job_tileType = jobB.job_tileType;
        job_target = jobB.job_target;
        job_hardness = jobB.job_hardness;
        hasBeenStarted = jobB.hasBeenStarted;
    }

    //public void StartJob()
    //{
    //    hasBeenStarted = true;
    //}

    public void CompleteJob()
    {
        isCompleted = true;
    }
}
