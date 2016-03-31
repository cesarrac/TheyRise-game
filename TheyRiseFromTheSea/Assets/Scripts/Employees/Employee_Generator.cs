﻿using UnityEngine;
using System.Collections.Generic;
using System;

public class Employee_Generator : MonoBehaviour {

    public static Employee_Generator Instance { get; protected set; }

    // A map of newly generated employees
    Dictionary<string, Employee> new_employees = new Dictionary<string, Employee>();

    // A list of employees that have been hired
    Dictionary<string, Employee> employee_roster = new Dictionary<string, Employee>();

    // The maximum number of employees that can be taken on a mission (default value of 2)
    int maxEmployeesOnMission = 2; 
    public int MaxEmployeesOnMission { get { return maxEmployeesOnMission; } }

    // Array of Employees that are currently active on a mission.
    Employee[] active_employees;

    // Array of string keys for any employee marked to die -- always limited to the max Employees on mission
    List<string> markedAsDead;

    public Sprite[] employeeSprites;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;

            DontDestroyOnLoad(gameObject);
        }
        else
        {
            DestroyImmediate(gameObject);
        }

        active_employees = new Employee[maxEmployeesOnMission];

        if (new_employees.Count == 0)
        {
            GenerateNewEmployees(3);
        }
    }

    // NOTE: This currently does not allow employees of the same name, 
    // that means that sometimes you'll get less availabel employees than the maximum allowed.

    // Method that generates new Employees randomly
    void GenerateNewEmployees(int total = 1)
    {
        for (int i = 1; i <= total; i++)
        {
            int select = UnityEngine.Random.Range(0, 3);

            Employee emp = new Employee();

            Armor basicJumpSuit = new Armor("Basic Jumpsuit", 2, 0);

            if (select == 0)
            {
                emp = new Employee("Operator", EmployeeSpecialty.Operator, basicJumpSuit, 24, 24, 2, employeeSprites[0]);
            }
            else if (select == 1)
            {
                emp = new Employee("Medic", EmployeeSpecialty.Medic, basicJumpSuit, 24, 24, 2, employeeSprites[1]);
            }
            else
            {
                emp = new Employee("Scientist", EmployeeSpecialty.Scientist, basicJumpSuit, 24, 24, 2, employeeSprites[2]);
            }

            if (!new_employees.ContainsKey(emp.Name) && emp != null)
            {
                new_employees.Add(emp.Name, emp);
                Debug.Log("Generated Employee!  Name: " + emp.Name + "     Specialty: " + emp.Specialty);
            }
            else
            {
                Debug.Log("Already have a " + emp.Name + " in my list!");
            }
        }
    }


    // Method to Add / Hire an employee from the list of new Employees to the Employee Roster
    void HireEmployee(string name)
    {
        if (!new_employees.ContainsKey(name))
            return;

        if (!employee_roster.ContainsKey(name))
        {
            employee_roster.Add(name, new_employees[name]);

            RemoveFromNewEmployees(name);
        }
    }

    // Remove from New Employees list
    void RemoveFromNewEmployees(string name)
    {
        if (new_employees.ContainsKey(name))
        {
            new_employees.Remove(name);
        }
    }

    // Fire an Employee
    void FireEmployee(string name)
    {
        if (employee_roster.ContainsKey(name))
        {
            employee_roster.Remove(name);
        }
    }

    // Spawn employees as gameobjects in a level
    public void SpawnActiveEmployees(Vector3 spawnPosition)
    {
        float offset = 0;
        foreach(Employee emp in active_employees)
        {
            Vector3 pos = new Vector3(spawnPosition.x + offset, spawnPosition.y, 0);

            // Grab the correct prefab by passing in this employees specialty (Prefabs are named by specialty so they contain the correct components to work)
            GameObject employee = ObjectPool.instance.GetObjectForType(emp.Specialty.ToString(), true, pos);
            
            if (employee != null)
            {
                // TODO: Also set the employees correct sprite here!
                employee.GetComponentInChildren<SpriteRenderer>().sprite = emp.MySprite;

                // Set the employee class of the Employee Handler
                if (employee.GetComponent<Employee_Handler>() != null)
                {
                    employee.GetComponent<Employee_Handler>().DefineEmployee(emp);
                }
                // Copy the employee's unit stats to the unit stats referenced in its base class
                if (employee.GetComponent<Employee_Attack>() != null)
                {
                    employee.GetComponent<Employee_Attack>().stats = new UnitStats(emp.unitStats);
                }

                offset++;
            }

        }

        // Initialize employeess marked for death array using the length active employees
        markedAsDead = new List<string>();
    }

    public void TestSpawnEmployee()
    {
        Employee emp = new Employee("Operator", EmployeeSpecialty.Operator, new Armor("Basic Jumpsuit", 2, 0), 24, 24, 2, employeeSprites[0]);

        Vector3 pos = ResourceGrid.Grid.Hero != null ? ResourceGrid.Grid.Hero.transform.position + Vector3.up : new Vector3(10, 10, 0);

        Employee_Actions.Instance.DefineActions(emp.Specialty);

        // Grab the correct prefab by passing in this employees specialty (Prefabs are named by specialty so they contain the correct components to work)
        GameObject employee = ObjectPool.instance.GetObjectForType(emp.Specialty.ToString(), true, pos);

        if (employee != null)
        {
            // TODO: Also set the employees correct sprite here!
            employee.GetComponentInChildren<SpriteRenderer>().sprite = emp.MySprite;

            // Set the employee class of the Employee Handler
            if (employee.GetComponent<Employee_Handler>() != null)
            {
                employee.GetComponent<Employee_Handler>().DefineEmployee(emp);
            }
            // Copy the employee's unit stats to the unit stats referenced in its base class
            if (employee.GetComponent<Employee_Attack>() != null)
            {
                employee.GetComponent<Employee_Attack>().stats = new UnitStats(emp.unitStats);
            }

            // Init its move stats
            if (employee.GetComponent<UnitPathHandler>() != null)
            {
                employee.GetComponent<UnitPathHandler>().mStats.InitStartingMoveStats(2, 2);
                employee.GetComponent<UnitPathHandler>().mStats.InitMoveStats();
            }
        }
    }

    // Mark an Employee for death so they get removed when loading back to the ship
    public void MarkAsDead(string name)
    {
        if (!markedAsDead.Contains(name))
            markedAsDead.Add(name);
    }

    // Remove an Employee that has died during a level, this is called when loading Ship after a planet level
    public void RemoveDeadEmployees()
    {
        if (markedAsDead != null && markedAsDead.Count > 0)
        {
            foreach (string name in markedAsDead)
            {
                if (employee_roster.ContainsKey(name))
                {
                    employee_roster.Remove(name);

                    // TODO: Remove it visually from the UI graphics representing the roster
                }
            }
        }
    }



}