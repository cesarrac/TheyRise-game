using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class Employee_Actions : MonoBehaviour {

    public static Employee_Actions Instance { get; protected set; }

    Dictionary<TileData.Types, Action<GameObject, Transform>> employeeActions;

    void Awake()
    {
        Instance = this;
        DefineActions();
    }

    public void DefineActions()
    {
        employeeActions = new Dictionary<TileData.Types, Action<GameObject, Transform>>();
        employeeActions.Add(TileData.Types.rock, ExtractFromTile);
        employeeActions.Add(TileData.Types.machine_gun, AssembleMachine);
        employeeActions.Add(TileData.Types.extractor, OperateBuilding);
    }


    public Action<GameObject, Transform> GetAction(TileData.Types tile)
    {
        if (tile == TileData.Types.empty)
            return null;

        //if (spec == EmployeeSpecialty.Operator)
        //{
        //    if (operatorActions.ContainsKey(tile))
        //    {
        //        return operatorActions[tile];
        //    }
        //}

        // NOTE: Instead of finding an action based on the Employee's specialty, ALL employess will be capable of finding an action
        // but the will fail to accomplish the task (or probably not be able to do it at all!) if they have less than 1 point in the relevant Employee Stat

        if (employeeActions.ContainsKey(tile))
            return employeeActions[tile];

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
        // This would cause the employee to walk up to the machine and operate it
        // so we'll need to hook this up to a component on the machine that tells it
        // it can start operating 
        // (probably the Building Handler, set its state to READY ... how would that work?)
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
