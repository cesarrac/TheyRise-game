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
            if (emp_handler.isWorking == false || emp_handler.workState != Employee_Handler.Work_State.OnMachine)
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

    public void SetOperateTarget(Transform target)
    {
        mainTarget = target;

        if (Employee_Actions.Instance.RangeCheck(mainTarget.position, transform.position))
        {
            if (mainTarget.GetComponentInChildren<Unit_Base>() != null)
            {
                emp_handler.SetIsWorking(true);
                Operate(mainTarget.GetComponentInChildren<Unit_Base>(), emp_handler.MyEmployee.emp_stats.Mechanics);
            }
            else if (mainTarget.GetComponent<ExtractionBuilding>() != null)
            {
                emp_handler.SetIsWorking(true);
                Operate(mainTarget.GetComponent<ExtractionBuilding>(), emp_handler.MyEmployee.emp_stats.Mechanics);
            }
            else
                Debug.Log("Employee could not find building to operate!");
        }
        else
        {
            Employee_Actions.Instance.MoveToTarget(gameObject, OperateTarget, GetTarget);
        }
    }


    void OperateTarget()
    {
        if (mainTarget != null && Employee_Actions.Instance.RangeCheck(mainTarget.position, transform.position))
        {
            if (mainTarget.GetComponentInChildren<Unit_Base>() != null)
            {
                emp_handler.SetIsWorking(true);
                Operate(mainTarget.GetComponentInChildren<Unit_Base>(), emp_handler.MyEmployee.emp_stats.Mechanics);
            }
            else if (mainTarget.GetComponent<ExtractionBuilding>() != null)
            {
                emp_handler.SetIsWorking(true);
                Operate(mainTarget.GetComponent<ExtractionBuilding>(), emp_handler.MyEmployee.emp_stats.Mechanics);
            }
            else
                Debug.Log("Employee could not find building to operate!");
        }
        else
        {
            Employee_Actions.Instance.MoveToTarget(gameObject, OperateTarget, GetTarget);
        }
    }

    void Operate(Unit_Base tower, float boost)
    {
        Debug.Log("TOWER is ENERGIZING!");
        // Attack boost
        tower.stats.curAttack = boost * tower.stats.curAttack;

        // Rate boost
        tower.stats.curRateOfAttk = boost * tower.stats.curRateOfAttk;

        // Damage boost 
        tower.stats.curDamage = boost * tower.stats.curDamage;

        // Reload Speed boost (subtracts because Reload speed is how long it takes to reload in seconds)
        tower.stats.curReloadSpeed = tower.stats.curReloadSpeed - boost;

        //Debug.Log("Attack " + tower.stats.curAttack + " Damage " + tower.stats.curDamage);

        //Debug.Log("Past Attack " + tower.stats.startAttack + " Past Damage " + tower.stats.startDamage);

        // Wait to Stop Operating on building when this unit gets a new order/action
        StartCoroutine(WaitToStopOperating(tower.gameObject));

    }
    void Operate(ExtractionBuilding extractor, float boost)
    {
        Debug.Log("TOWER is ENERGIZING!");
        // Calculate boosted Rate and Power...
        var power = boost + extractor.extractorStats.extractPower;
        //var rate = boost * extractor.extractorStats.extractRate;

        // ... and Energize the Extractor's stats.
        extractor.extractorStats.Energize(power);

        // Wait to Stop Operating on building when this unit gets a new order/action
        StartCoroutine(WaitToStopOperating(extractor.gameObject));

    }

    IEnumerator WaitToStopOperating(GameObject gObj)
    {
        while (true)
        {
            if (emp_handler.isWorking == false || emp_handler.workState != Employee_Handler.Work_State.OnMachine)
            {
                Debug.Log("DE-ENERGIZING TOWER!");
                // De-energize and break
                if (gObj.GetComponentInChildren<Unit_Base>() != null)
                {
                    gObj.GetComponentInChildren<Unit_Base>().DeEnergize();
                }
                else if ( gObj.GetComponent<ExtractionBuilding>() != null)
                {
                    gObj.GetComponent<ExtractionBuilding>().extractorStats.DeEnergize();
                }

                yield break;
            }

            yield return null;
        }
    }

    Transform GetTarget(Vector3 pos)
    {
        // Find the nearest rock and return its transform
        return mainTarget;
    }

}
