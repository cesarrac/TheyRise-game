using UnityEngine;
using System.Collections;

public class Employee_Extract : MonoBehaviour {

    int posX, posY;
    Transform mainTarget;

    Employee_Handler emp_handler;

    int extractAmmount = 1;

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

            emp_handler.SetIsWorking(true);

            StopCoroutine("Extraction");
            StartCoroutine("Extraction");
        }
        else
        {
            StopCoroutine("Extraction");
            Employee_Actions.Instance.MoveToTarget(gameObject, Extract, GetTarget);
        }
    }

    void Extract()
    {
        // This is called when the Path Handler reaches its destination

        if (mainTarget != null && Employee_Actions.Instance.RangeCheck(mainTarget.position, transform.position))
        {
            posX = Mathf.RoundToInt(mainTarget.position.x);
            posY = Mathf.RoundToInt(mainTarget.position.y);

            emp_handler.SetIsWorking(true);

            StopCoroutine("Extraction");
            StartCoroutine("Extraction");
        }
        else
        {
            StopCoroutine("Extraction");
            Employee_Actions.Instance.MoveToTarget(gameObject, Extract, GetTarget);
        }
    }

    IEnumerator Extraction()
    {
        while (true)
        {
            if (emp_handler.isWorking == false)
                yield break;

            if (mainTarget == null || !Employee_Actions.Instance.RangeCheck(mainTarget.position, transform.position))
            {
                Employee_Actions.Instance.MoveToTarget(gameObject, Extract, GetTarget);
                yield break;
            }
                

            if (ResourceGrid.Grid.ExtractFromTile(posX, posY, extractAmmount, true) > 0)
            {
                Debug.Log("Extracting " + ResourceGrid.Grid.ExtractFromTile(posX, posY, 10) + " out of " + ResourceGrid.Grid.tiles[posX, posY].maxResourceQuantity);
            }
            else
            {
                if (emp_handler != null)
                    emp_handler.FinishedAction();

                yield break;
            }

            //(hardness / power) + currExtractRate
            float hardness = ResourceGrid.Grid.tiles[posX, posY].hardness;
            float power = emp_handler.MyEmployee.emp_stats.Extraction;
            float rate = emp_handler.MyEmployee.emp_stats.WorkRate;
            float currExtractionRate = (hardness / power) + rate;
            Debug.Log("Current Rate of Extraction = " + currExtractionRate);

            yield return new WaitForSeconds(currExtractionRate);
        }
    }


    Transform GetTarget(Vector3 pos)
    {
        if (mainTarget == null || mainTarget.gameObject.activeSelf == false)
        {
            // Find the nearest rock and return its transform
            Vector2 nearestRock = Rock_Generator.Instance.FindNearestRockFromPosition(transform.position);
            mainTarget = ResourceGrid.Grid.GetTileGameObjFromIntCoords((int)nearestRock.x, (int)nearestRock.y).transform;
        }
        
        // If not it will just return the target set by the player manually
        return mainTarget;
    }


    // ****************************************************************
    // REPLACE PLAYER TARGET WITH NEAREST ROCK: (Feels very annoying!!!)
    // ****************************************************************
    // Check if the target that was set is in fact the nearest possible rock to mine
    /*
    float distToTarget = ((Vector2)target.position - (Vector2)transform.position).magnitude;
    Vector2 nearestRock = Rock_Generator.Instance.FindNearestRockFromPosition(transform.position);
    float distToNearestRock = (nearestRock - (Vector2)transform.position).magnitude;

        if (distToNearestRock<distToTarget)
        {
            mainTarget = ResourceGrid.Grid.GetTileGameObjFromIntCoords((int)nearestRock.x, (int)nearestRock.y) != null ?
                ResourceGrid.Grid.GetTileGameObjFromIntCoords((int)nearestRock.x, (int)nearestRock.y).transform :
                target;
        }
        else
        {
            mainTarget = target;
        }
     */

}
