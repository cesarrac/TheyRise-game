using UnityEngine;
using System.Collections;

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

    public Job (TileData.Types tileType, Transform target, float hardness = 0.5f)
    {
        job_tileType = tileType;
        job_target = target;
        job_hardness = hardness;
    }

    // Copy constructor
    public Job (Job jobB)
    {
        job_tileType = jobB.job_tileType;
        job_target = jobB.job_target;
        job_hardness = jobB.job_hardness;
    }
}
