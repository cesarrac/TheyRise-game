using UnityEngine;
using System.Collections;

public class Employee_Mechanics : MonoBehaviour {

    Transform mainTarget;
    TileData targetAsTile;

    Employee_Handler emp_handler;

    NanoBuilding_Handler nanoBuild_handler;

    void Awake()
    {
        emp_handler = GetComponent<Employee_Handler>();
    }

    void Start()
    {

        nanoBuild_handler = ResourceGrid.Grid.Hero.GetComponent<NanoBuilding_Handler>();
    }

    public void SetAssembleTarget(Transform target)
    {
        mainTarget = target;

        // Main Target is the machine that needs to be assembled.
        // First the Employee needs to get the construction materials from the transporter
        if (Employee_Actions.Instance.RangeCheck(emp_handler.GetTransporterTransform(transform.position).position, transform.position))
        {
            GetSuppliesFromTransporter();
        }
        else
        {
            Employee_Actions.Instance.MoveToTarget(gameObject, GetSuppliesFromTransporter, emp_handler.GetTransporterTransform);
        }
  

        //if (Employee_Actions.Instance.RangeCheck(mainTarget.position, transform.position))
        //{
        //    AssembleTarget();
        //}
        //else
        //{
        //    Employee_Actions.Instance.MoveToTarget(gameObject, AssembleTarget, GetTarget);
        //}
    }

    void GetSuppliesFromTransporter()
    {
        // Wait to get the supplies needed...
        // Then call AssembleTarget so it will move to the assembly taret when it detects it is out of range
        Debug.Log("Getting supplies from Transporter!");
        Blueprint curBP = nanoBuild_handler.GetAvailableBlueprint(emp_handler.curJob.Job_TileType);

        if (emp_handler.curJob.HasBeenStarted == false)
        {
            if (nanoBuild_handler.CheckBuildCost(curBP))
            {
                // NOTE: Right now grabbing the supplies from transporter just means the 
                // Employee waits 3 seconds and charges the Inventory for the resources.
                // What I'd like is:
                // 1: Ask the machine's inventory if it is missing any of its Blueprints required materials
                // 2: If NO, proceed to Assemble... If YES move to transport
                // 3: Ask the machine for the resource and ammnt it's missing, take from Ship Inventory and fill up character inventory with each one
                // 4: IF character inventory is FULL and not all materials have been gathered, go to target machine to dump materials
                // 5: Deposit materials to machine inventory, IF the machine inventory contains all required resources, Assemble, 
                //    IF NOT go to transport and do step 3

                /*  *****    EXAMPLE of method for finding what Resource is required by the machine we are trying to assemble: ****

                Inventory machine = new Inventory(); <----- this we would get from the job target's gObj by getting its Building_Handler comp and finding its inventory
                                                        ---- OR, the TileData itself could hold an Inventory, when it gets swapped it would reset
                Inventory machine = job.tile.tile_inventory;

                foreach(Resource_Required resource in curBP.buildReq.buildRequirements)
                {
                    if (machine.CheckForResource(resource.resource, resource.ammnt) == false)
                    {
                        Resource_Required resRequiredForThisMachine = resource;
                    }

                }

                */

                // Charge the building cost to the ship's inventory
                nanoBuild_handler.ChargeBuildResources(curBP);
                Invoke("AssembleTarget", 3f);
            }
            else
            {
                // If we don't have enough resources to build this...
                // Add the job again to the last position of the Job Queue and go get a new job!
                emp_handler.CancelJob(false);
            }
        }
        else
        {
            // This assembly job has already been started, so resources have already been charged.
            // Employee can go to Assemble immediately.
            AssembleTarget();
        }
   
    }

    void AssembleTarget()
    {
        Debug.Log("Assembling!");
        if (mainTarget != null && mainTarget.GetComponent<Building_Handler>() != null)
        {
            if (Employee_Actions.Instance.RangeCheck(mainTarget.position, transform.position))
            {
                emp_handler.SetIsWorking(true);
                mainTarget.GetComponent<Building_Handler>().StartAssembling();
                StartCoroutine("WaitToFinishAssembling");
            }
            else
            {
                Employee_Actions.Instance.MoveToTarget(gameObject, AssembleTarget, GetTarget);
            }
       
        }
    }

    IEnumerator WaitToFinishAssembling()
    {
        while (true)
        {
            if (emp_handler.workState != Employee_Handler.Work_State.Assembling)
            {
                mainTarget.GetComponent<Building_Handler>().StopAssembling();
                Debug.Log(" Assembly stopped! work state is " + emp_handler.workState.ToString());
                yield break;
            }
               

            if (mainTarget.GetComponent<Building_Handler>() != null)
            {
                if (mainTarget.GetComponent<Building_Handler>().state == Building_Handler.State.READY)
                {
                    emp_handler.FinishedJob();
                    yield break;
                }
            }
            else
                yield break;


            yield return null;
        }
    }

    public void SetRepairTarget(Transform target)
    {
        mainTarget = target;

        // Get the target as Tile data
        targetAsTile = ResourceGrid.Grid.GetTileFromWorldPos(target.position);

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
            targetAsTile = ResourceGrid.Grid.GetTileFromWorldPos(mainTarget.position);

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
            if (emp_handler.isWorking == false || emp_handler.workState != Employee_Handler.Work_State.Repairing)
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
                    emp_handler.FinishedJob();

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
