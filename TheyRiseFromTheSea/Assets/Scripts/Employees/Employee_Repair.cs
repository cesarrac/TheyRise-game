using UnityEngine;
using System.Collections;

public class Employee_Repair : MonoBehaviour {

    Transform mainTarget;
    TileData targetAsTile;

    Employee_Handler emp_handler;

    void Awake()
    {
        emp_handler = GetComponent<Employee_Handler>();
    }

    bool RangeCheck(Vector3 targetPos)
    {
        var heading = targetPos - transform.position;

        if (heading.sqrMagnitude <= 2 * 2)
        {
            return true;
        }
        else
        {
            return false;
        }
    }


    public void SetRepairTarget(Transform target)
    {
        mainTarget = target;

        // Get the target as Tile data
        targetAsTile = ResourceGrid.Grid.TileFromWorldPoint(target.position);

        if (RangeCheck(target.position))
        {
            StartCoroutine("Repair");
        }
        else
        {
            MoveToTarget();
        }
    }

    void RepairTile()
    {
        if (mainTarget != null && RangeCheck(mainTarget.position))
        {
            // Get the target as Tile data
            targetAsTile = ResourceGrid.Grid.TileFromWorldPoint(mainTarget.position);

            StartCoroutine("Repair");
        }
        else
        {
            MoveToTarget();
        }
    }

    IEnumerator Repair()
    {
        while (true)
        {
            if (targetAsTile == null || !RangeCheck(mainTarget.position))
            {
                MoveToTarget();
                yield break;
            }
           

            if (emp_handler.employee_stats != null && emp_handler.employee_stats.HealTile(targetAsTile, 10))
            {
                Debug.Log("healed tile for 10 hit points");
            }
            else
            {
                if (emp_handler != null)
                    emp_handler.FinishedAction();

                yield break;
            }

            yield return new WaitForSeconds(1);
        }
    }

    Transform GetTarget(Vector3 pos)
    {
        // Find the nearest rock and return its transform
        return mainTarget;
    }


    void MoveToTarget()
    {
        UnitPathHandler path_Handler = GetComponent<UnitPathHandler>();
        path_Handler.RegisterDestinationReachedCB(RepairTile);
        path_Handler.RegisterGetTargetFunc(GetTarget);
        path_Handler.AssignTarget();
    }
}
