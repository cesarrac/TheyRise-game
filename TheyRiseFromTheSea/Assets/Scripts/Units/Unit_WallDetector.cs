using UnityEngine;
using System.Collections;

public class Unit_WallDetector: MonoBehaviour {

    // Detects walls in a path by checking the next waypoint in the path everytime it changes
    // during the path handler's Follow Path coRoutine

    public bool attacksWalls = false;

    UnitPathHandler pathHandler;

    Vector3 path_waypoint;

    void Awake()
    {
        if (GetComponent<UnitPathHandler>() != null)
            pathHandler = GetComponent<UnitPathHandler>();
    }

    void Update()
    {
        if (pathHandler != null)
        {
            if (pathHandler.currWayPoint != null)
            {
                if (path_waypoint != pathHandler.currWayPoint)
                {
                    path_waypoint = pathHandler.currWayPoint;
                    CheckWaypointForWall();
                }
            }
        }
    }

    void CheckWaypointForWall()
    {
        if (ResourceGrid.Grid.GetTileFromWorldPos(path_waypoint).tileType == TileData.Types.wall)
        {
            Debug.Log("UNIT is at a wall! GameObj's name is " + ResourceGrid.Grid.GetTileGameObjFromWorldPos(path_waypoint).name);

             // Stop following path and Attack
            pathHandler.StopFollowingPathAndAttack((ResourceGrid.Grid.GetTileGameObjFromWorldPos(path_waypoint).transform));
    
        }
    }

}
