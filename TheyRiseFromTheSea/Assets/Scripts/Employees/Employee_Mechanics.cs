using UnityEngine;
using System.Collections;

public class Employee_Mechanics : MonoBehaviour {

    Transform mainTarget;
    TileData targetAsTile;

    Employee_Handler emp_handler;

    void Awake()
    {
        emp_handler = GetComponent<Employee_Handler>();
    }

    public void SetAssembleTarget(Transform target)
    {
        mainTarget = target;

        if (Employee_Actions.Instance.RangeCheck(mainTarget.position, transform.position))
        {
            AssembleTarget();
        }
        else
        {
            Employee_Actions.Instance.MoveToTarget(gameObject, AssembleTarget, GetTarget);
        }
    }

    void AssembleTarget()
    {
        if (mainTarget != null && mainTarget.GetComponent<Building_Handler>() != null)
        {
            if (Employee_Actions.Instance.RangeCheck(mainTarget.position, transform.position))
            {
                emp_handler.SetIsWorking(true);
                mainTarget.GetComponent<Building_Handler>().StartAssembling();
            }
            else
            {
                Employee_Actions.Instance.MoveToTarget(gameObject, AssembleTarget, GetTarget);
            }
       
        }
    }

    public void SetRepairTarget(Transform target)
    {
        mainTarget = target;

        // Get the target as Tile data
        targetAsTile = ResourceGrid.Grid.TileFromWorldPoint(target.position);

        if (Employee_Actions.Instance.RangeCheck(mainTarget.position, transform.position))
        {

            emp_handler.SetIsWorking(true);

            StopCoroutine("Repair");
            StartCoroutine("Repair");
        }
        else
        {
            StopCoroutine("Repair");
            Employee_Actions.Instance.MoveToTarget(gameObject, RepairTile, GetTarget);
        }
    }

    void RepairTile()
    {
        if (mainTarget != null && Employee_Actions.Instance.RangeCheck(mainTarget.position, transform.position))
        {
            // Get the target as Tile data
            targetAsTile = ResourceGrid.Grid.TileFromWorldPoint(mainTarget.position);

            emp_handler.SetIsWorking(true);

            StopCoroutine("Repair");
            StartCoroutine("Repair");
        }
        else
        {
            StopCoroutine("Repair");
            Employee_Actions.Instance.MoveToTarget(gameObject, RepairTile, GetTarget);
        }
    }

    IEnumerator Repair()
    {
        while (true)
        {
            if (emp_handler.isWorking == false)
                yield break;

            if (targetAsTile == null || !Employee_Actions.Instance.RangeCheck(mainTarget.position, transform.position))
            {
                Employee_Actions.Instance.MoveToTarget(gameObject, RepairTile, GetTarget);
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

}
