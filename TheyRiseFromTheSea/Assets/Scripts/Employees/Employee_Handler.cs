using UnityEngine;
using System.Collections;
using System;

public class Employee_Handler : MonoBehaviour {

    /// <summary>
    /// This component serves as the Controller for all interactions with the Employee. This can communicate with all the components the 
    /// employee needs to define, set, and perform their actions (Selection, Pathing, Doing job)
    /// </summary>

    Employee myEmployee;
    public Employee MyEmployee { get { return myEmployee; } }

    UnitPathHandler path_handler;

    Transform mainTarget;

    public Employee_Attack employee_stats { get; protected set; }

    void Awake()
    {
        employee_stats = GetComponent<Employee_Attack>();
    }

    public void DefineEmployee(Employee emp)
    {
        myEmployee = emp;
    }

    public void SelectEmployee()
    {
        Debug.Log("Employee selected.");
    }


    public void DoAction(TileData.Types tile, Transform target)
    {

        Debug.Log("Employee going to work on " + tile.ToString());
        mainTarget = target;

        // Check if this employee has an action for this type of tile
        Action<GameObject, Transform> act = Employee_Actions.Instance.GetAction(tile, myEmployee.Specialty);
        // If it does, call it and pass it the main target
        if ( act != null)
        {
            act(gameObject, target);
        }
        else
        {
            Debug.Log("Could not find " + myEmployee.Specialty.ToString() +  " Action for " + tile.ToString());
        }
    }


    public void FinishedAction()
    {
        Debug.Log("Employee finished action");
    }
}
