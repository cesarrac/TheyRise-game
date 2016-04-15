using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class Employee_Actions : MonoBehaviour {

    public static Employee_Actions Instance { get; protected set; }

    Dictionary<JobType, Action<GameObject, Transform>> employee_Actions;

    void Awake()
    {
        Instance = this;
        DefineActions();
    }

    public void DefineActions()
    {
        employee_Actions = new Dictionary<JobType, Action<GameObject, Transform>>();
        employee_Actions.Add(JobType.Mine, ExtractFromTile);
        employee_Actions.Add(JobType.Assemble, AssembleMachine);
        employee_Actions.Add(JobType.Operate, OperateBuilding);
        employee_Actions.Add(JobType.Repair, RepairBuilding);
    }


    public Action<GameObject, Transform> GetAction(JobType jobType)
    {
        if (employee_Actions.ContainsKey(jobType))
            return employee_Actions[jobType];

        return null;
    }

    void ExtractFromTile(GameObject gObj, Transform t)
    {
        if (gObj.GetComponent<Employee_Extract>() != null)
        {
            gObj.GetComponent<Employee_Extract>().SetExtractionTarget(t);
        }
        else
        {
            InvalidAction(t);
        }
    }

    void RepairBuilding(GameObject gObj, Transform t)
    {
        if (gObj.GetComponent<Employee_Mechanics>() != null)
        {
            gObj.GetComponent<Employee_Mechanics>().SetRepairTarget(t);
        }
        else
        {
            InvalidAction(t);
        }
    }

    void AssembleMachine(GameObject gObj, Transform t)
    {
        // The building handler sets its state to READY when it finishes constructing, maybe with buildings
        // that need operators they can build and set their state to WAITING until an 
        // Operator can be there to make it work.
        if (gObj.GetComponent<Employee_Mechanics>() != null)
        {
            gObj.GetComponent<Employee_Mechanics>().SetAssembleTarget(t);
        }
        else
        {
            InvalidAction(t);
        }
    }


    void OperateBuilding (GameObject gObj, Transform t)
    {
        // This would allow the Employee to walk up to a machine and boost its
        // productivity, its fire rate and/or damage (like energize works!)
        if (gObj.GetComponent<Employee_Mechanics>() != null)
        {
            gObj.GetComponent<Employee_Mechanics>().SetOperateTarget(t);
        }
        else
        {
            InvalidAction(t);
        }
    }

    // Action to return when no proper action was found. 
    // When this gets called a message will tell the player that the unit they have selected has no action
    // for the tile they have indicated.
    void InvalidAction(Transform t)
    {
        Debug.LogError("This employee does not contain the component required to work a " + t.gameObject.name);
    }


    //  *****************************************************************************
    //                               SHARED ACTIONS:
    //      Functions required by all components on the Employee's Gobj to perform 
    //                      their specialty's actions / jobs.
    //  *****************************************************************************

    public bool RangeCheck(Vector3 target, Vector3 unit, float range = 2f)
    {
        var heading = target - unit;

        if (heading.sqrMagnitude <= range * range)
        {
            return true;
        }
        else
        {
            return false;
        }
    }



    public void MoveToTarget(GameObject gObj, Action destinationReachedCB, Func<Vector3, Transform> getTargetFunc)
    {
        if (gObj.GetComponent<UnitPathHandler>() != null)
        {
            UnitPathHandler pathHandler = gObj.GetComponent<UnitPathHandler>();
            pathHandler.RegisterDestinationReachedCB(destinationReachedCB);
            pathHandler.RegisterGetTargetFunc(getTargetFunc);
            pathHandler.AssignTarget();
        }
    }
}
