﻿using UnityEngine;
using System.Collections;
using System;

public class Employee_Handler : MonoBehaviour {

    /// <summary>
    /// This component serves as the Controller for all interactions with the Employee. This can communicate with all the components the 
    /// employee needs to define, set, and perform their actions (Selection, Pathing, Doing job)
    /// </summary>

    Employee myEmployee;
    public Employee MyEmployee { get { return myEmployee; } }
    float curToolPower;

    public Job curJob { get; protected set; }

    Transform mainTarget;

    public Employee_Attack employee_stats { get; protected set; }

    public bool isWorking { get; protected set; }

    public enum Work_State { Idling, Recharging, RequestingJob, Mining, OnMachine, Assembling, Repairing };
    public Work_State workState { get; protected set; }

    bool hasJob = false;

    EmployeeAssignment assignment = EmployeeAssignment.Builder;

    JobType[] jobsIWork;

    void OnEnable()
    {
        employee_stats = GetComponent<Employee_Attack>();
    
        workState = Work_State.Idling;
    }

    public void DefineEmployee(Employee emp)
    {
        myEmployee = emp;

        curToolPower = myEmployee.emp_stats.ToolPower;

        assignment = EmployeeAssignment.Builder;

        SetTasksBasedOnAssignment();
    }

    void SetTasksBasedOnAssignment()
    {
        jobsIWork = new JobType[3];
        if (assignment == EmployeeAssignment.Builder)
        {
            jobsIWork[0] = JobType.Assemble;
            jobsIWork[1] = JobType.Mine;
            jobsIWork[2] = JobType.Repair;
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
        JobRequestManager.RequestJob(jobsIWork, DoAction);
    }

    public void DoAction(Job job, bool success)
    {
       if (success && job != null)
       {
            Debug.Log("Employee took a job of type: " + job.Job_Type);
            Debug.Log("Going to work on " + job.Job_TileType.ToString());

            hasJob = true;
            curJob = new Job(job);

            mainTarget = job.Job_Target;

            Action<GameObject, Transform> act = Employee_Actions.Instance.GetAction(job.Job_Type);
            if (act != null)
            {
                isWorking = false; // the action will set this to true once it begins actually working

                if (job.Job_Type == JobType.Mine)
                {
                    workState = Work_State.Mining;
                }
                else if (job.Job_Type == JobType.Assemble)
                {
                    workState = Work_State.Assembling;
                }
                else if (job.Job_Type == JobType.Repair)
                {
                    workState = Work_State.Repairing;
                }

                act(gameObject, job.Job_Target);

                // using Tool Energy begins as soon as the job action is called
                StartCoroutine("UsingToolPower");

            }
            else
            {
                Debug.Log("Could not find " + myEmployee.Specialty.ToString() + " Action for " + job.Job_TileType.ToString());
                if (curToolPower < myEmployee.emp_stats.ToolPower)
                {
                    OutOfPower();
                }
                else
                {
                    workState = Work_State.Idling;
                    hasJob = false;
                    curJob = null;
                }
            }

        }
        else
        {
           // Debug.Log("Could not find job!");
            if (curToolPower < myEmployee.emp_stats.ToolPower)
            {
                OutOfPower();
            }
            else
            {
                workState = Work_State.Idling;
                hasJob = false;
                curJob = null;
            }

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

    IEnumerator UsingToolPower()
    {
        while (true)
        {
            if (!hasJob || workState == Work_State.Idling)
                yield break;

            if (IsToolPowered() == false)
            {
                OutOfPower();
                yield break;
            }

            yield return new WaitForSeconds(myEmployee.emp_stats.WorkRate);
        }
    }

    bool IsToolPowered()
    {
        if (curToolPower <= 0)
            return false;
        else
        {
            if (curJob != null)
            {
                curToolPower -= curJob.Job_Hardness;
            }

            return true;
        }
    }

    void OutOfPower()
    {
       // Debug.Log("Employee tool out of power!");
        // Could not complete job because the employee ran out of power.
        // The Employee needs to go and recharge their tool...
        // and the same job needs to be added again because the request manager already got rid of it.
        if (curToolPower < myEmployee.emp_stats.ToolPower)
        {
           // Add the old job back on the job queue
            if (curJob != null)
            {
                if (curJob.IsCompleted == false)
                {
                    AddOldJobToQueue();
                }
            }

            // Go to recharge tool before finding another job
            workState = Work_State.Recharging;
            Employee_Actions.Instance.MoveToTarget(gameObject, RechargeToolPower, GetTransporterTransform);
        }
        else
        {
            workState = Work_State.Idling;
        }
    }

    void AddOldJobToQueue()
    {
        // Add job back into the list, telling the Job Manager that this job was already started by this employee or someone else
        Job_Manager.Instance.AddJob(curJob.Job_Type, curJob.Job_TileType, curJob.Job_Target, hasStarted: true);

        curJob = null;
    }

    public void CancelJob()
    {
        AddOldJobToQueue();
        if (curToolPower < myEmployee.emp_stats.ToolPower)
        {
            OutOfPower();
        }
        else
        {
            workState = Work_State.Idling;
            hasJob = false;
            curJob = null;
        }
    }

    public void FinishedJob()
    {
        Debug.Log("Employee finished action");
        curJob.CompleteJob();

        workState = Work_State.Idling;
        hasJob = false;
    }

    public Transform GetTransporterTransform(Vector3 unitPosition)
    {
        return ResourceGrid.Grid.transporterGObj.transform;
    }

    void RechargeToolPower()
    {
        StartCoroutine("Recharge");

    }

    IEnumerator Recharge()
    {
        while (true)
        {
            if (curToolPower >= myEmployee.emp_stats.ToolPower)
            {
                // Now ready to get a new job
                hasJob = false;
                workState = Work_State.Idling;
                yield break;
            }
            else
                curToolPower += 1;

            yield return new WaitForSeconds(1);
        }
    }

}

public enum EmployeeAssignment { Builder, Medic, Guard }
