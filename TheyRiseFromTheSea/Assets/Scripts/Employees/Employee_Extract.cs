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

    // Set by mouse click
    public void SetExtractionTarget(Transform target)
    {
        mainTarget = target;
        if (Employee_Actions.Instance.RangeCheck(target.position, transform.position))
        {
            posX = Mathf.RoundToInt(target.position.x);
            posY = Mathf.RoundToInt(target.position.y);

            StartCoroutine("Extraction");
        }
        else
        {
            Employee_Actions.Instance.MoveToTarget(gameObject, Extract, GetTarget);
        }
    }

    void Extract()
    {
        if (mainTarget != null && Employee_Actions.Instance.RangeCheck(mainTarget.position, transform.position))
        {
            posX = Mathf.RoundToInt(mainTarget.position.x);
            posY = Mathf.RoundToInt(mainTarget.position.y);

            StartCoroutine("Extraction");
        }
        else
        {
            Employee_Actions.Instance.MoveToTarget(gameObject, Extract, GetTarget);
        }
    }

    IEnumerator Extraction()
    {
        while (true)
        {
            if (mainTarget == null || !Employee_Actions.Instance.RangeCheck(mainTarget.position, transform.position))
            {
                Employee_Actions.Instance.MoveToTarget(gameObject, Extract, GetTarget);
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


    Transform GetTarget(Vector3 pos)
    {
        // Find the nearest rock and return its transform
        return mainTarget;
    }

}
