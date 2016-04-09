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

    Transform mainTarget;

    public Employee_Attack employee_stats { get; protected set; }

    public bool isWorking { get; protected set; }

    public enum Work_State { Idling, RequestingJob, Mining, OnMachine, Assembling };
    public Work_State workState { get; protected set; }

    bool hasJob = false;

    EmployeeAssignment assignment = EmployeeAssignment.Builder;

    TileData.Types[] tilesIWorkOn;

    void OnEnable()
    {
        employee_stats = GetComponent<Employee_Attack>();
    
        workState = Work_State.Idling;
    }

    public void DefineEmployee(Employee emp)
    {
        myEmployee = emp;

        assignment = EmployeeAssignment.Builder;

        SetTasksBasedOnAssignment();
    }

    void SetTasksBasedOnAssignment()
    {
        tilesIWorkOn = new TileData.Types[3];
        if (assignment == EmployeeAssignment.Builder)
        {
            tilesIWorkOn[0] = TileData.Types.rock;
            tilesIWorkOn[1] = TileData.Types.machine_gun;
            tilesIWorkOn[2] = TileData.Types.extractor;
        }
    }

    public void SelectEmployee()
    {
        Debug.Log("Employee selected.");
    }

    public void SetIsWorking(bool i)
    {
        isWorking = i;
    }

    void Start()
    {
        // TESTING THE JOB QUEUE SYSTEM:

        if (workState == Work_State.Idling)
        {
            StartCoroutine("RequestAJob");
        }
    }

    IEnumerator RequestAJob()
    {
        while (true)
        {
            if (workState == Work_State.Idling && hasJob == false)
            {
                FindJob();
            }

            yield return new WaitForSeconds(2f);
        }
    }

    void FindJob()
    {   
        workState = Work_State.RequestingJob;
        Debug.Log("Employee requesting job!");
        JobRequestManager.RequestJob(tilesIWorkOn, DoAction);
    }

    public void DoAction(Job job, bool success)
    {
       if (success && job != null)
       {
            Debug.Log("Employee going to work on " + job.Job_TileType.ToString());

            hasJob = true;

            mainTarget = job.Job_Target;

            Action<GameObject, Transform> act = Employee_Actions.Instance.GetAction(job.Job_TileType);
            if (act != null)
            {
                isWorking = false;

                if (job.Job_TileType == TileData.Types.rock)
                {
                    workState = Work_State.Mining;
                }
                else if (job.Job_TileType != TileData.Types.empty)
                {
                    workState = Work_State.OnMachine;
                }
                else
                {
                    workState = Work_State.Assembling;
                }

                act(gameObject, job.Job_Target);
            }
            else
            {
                workState = Work_State.Idling;
                Debug.Log("Could not find " + myEmployee.Specialty.ToString() + " Action for " + job.Job_TileType.ToString());
            }

        }
        else
        {
            Debug.Log("Could not find job!");
            workState = Work_State.Idling;
            hasJob = false;
        }
        
        //mainTarget = target;

        // Check if this employee has an action for this type of tile
       // Action<GameObject, Transform> act = Employee_Actions.Instance.GetAction(tile);
        // If it does, call it and pass it the main target
        //if ( act != null)
        //{
        //    isWorking = false;

        //    if (tile == TileData.Types.rock)
        //    {
        //        workState = Work_State.Mining;
        //    }
        //    else if (tile != TileData.Types.empty)
        //    {
        //        workState = Work_State.OnMachine;
        //    }

        //    act(gameObject, target);


        //}
        //else
        //{
        //    workState = Work_State.Idling;
        //    Debug.Log("Could not find " + myEmployee.Specialty.ToString() +  " Action for " + tile.ToString());
        //}
    }


    public void FinishedJob()
    {
        Debug.Log("Employee finished action");
        workState = Work_State.Idling;
        hasJob = false;
    }
}

public enum EmployeeAssignment { Builder, Medic, Guard }
