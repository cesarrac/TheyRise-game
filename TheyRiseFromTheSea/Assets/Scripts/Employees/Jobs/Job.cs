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

    public Job (TileData.Types tileType, Transform target)
    {
        job_tileType = tileType;
        job_target = target;
    }
}
