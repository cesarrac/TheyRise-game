using UnityEngine;
using System.Collections;

public class Employee_Repair : MonoBehaviour {

    int posX, posY;
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


    public void RepairTile(Transform target)
    {
        mainTarget = target;

        targetAsTile = ResourceGrid.Grid.TileFromWorldPoint(target.position);


        StartCoroutine("Repair");
    }

    IEnumerator Repair()
    {
        while (true)
        {
            if (targetAsTile == null || !RangeCheck(mainTarget.position))
            {
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
}
