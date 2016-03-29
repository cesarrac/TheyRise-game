using UnityEngine;
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
                emp = new Employee("Operator", EmployeeSpecialty.OPERATOR, basicJumpSuit, 24, 24, 2);
            }
            else if (select == 1)
            {
                emp = new Employee("Medic", EmployeeSpecialty.MEDIC, basicJumpSuit, 24, 24, 2);
            }
            else
            {
                emp = new Employee("Scientist", EmployeeSpecialty.SCIENTIST, basicJumpSuit, 24, 24, 2);
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

    // Method to remove from New Employees list
    void RemoveFromNewEmployees(string name)
    {
        if (new_employees.ContainsKey(name))
        {
            new_employees.Remove(name);
        }
    }

    // Method to Fire an Employee
    void FireEmployee(string name)
    {
        if (employee_roster.ContainsKey(name))
        {
            employee_roster.Remove(name);
        }
    }

}
