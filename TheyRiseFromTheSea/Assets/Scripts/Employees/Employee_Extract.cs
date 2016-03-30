using UnityEngine;
using System.Collections;

public class Employee_Extract : MonoBehaviour {

    int posX, posY;
    Transform mainTarget;

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

    // Set by mouse click
    public void SetExtractionTarget(Transform target)
    {
        mainTarget = target;
        if (RangeCheck(target.position))
        {
            posX = Mathf.RoundToInt(target.position.x);
            posY = Mathf.RoundToInt(target.position.y);

            StartCoroutine("Extraction");
        }
        else
        {
            MoveToTarget();
        }
    }


    Transform GetTarget(Vector3 pos)
    {
        // Find the nearest rock and return its transform
        return mainTarget;
    }

    void Extract()
    {
        if (mainTarget != null && RangeCheck(mainTarget.position))
        {
            posX = Mathf.RoundToInt(mainTarget.position.x);
            posY = Mathf.RoundToInt(mainTarget.position.y);

            StartCoroutine("Extraction");
        }
        else
        {
            MoveToTarget();
        }
    }

    void MoveToTarget()
    {
        UnitPathHandler path_Handler = GetComponent<UnitPathHandler>();
        path_Handler.RegisterDestinationReachedCB(Extract);
        path_Handler.RegisterGetTargetFunc(GetTarget);
        path_Handler.AssignTarget();
    }

    IEnumerator Extraction()
    {
        while (true)
        {
            if (mainTarget == null || !RangeCheck(mainTarget.position))
            {
                MoveToTarget();
                yield break;
            }
                

            if (ResourceGrid.Grid.ExtractFromTile(posX, posY, 10, true) > 0)
            {
                Debug.Log("Extracting " + ResourceGrid.Grid.ExtractFromTile(posX, posY, 10) + " out of " + ResourceGrid.Grid.tiles[posX, posY].maxResourceQuantity);
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
