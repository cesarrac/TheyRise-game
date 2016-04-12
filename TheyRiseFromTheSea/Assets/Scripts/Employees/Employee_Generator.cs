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

    // Employees that are currently active on a mission.
    public List<Employee> active_employees { get; protected set; }

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

        active_employees = new List<Employee>();

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

            Guid id = Guid.NewGuid();

            if (select == 0)
            {
                emp = new Employee(id.ToString() , EmployeeSpecialty.Operator, basicJumpSuit, 24, 24, 2, employeeSprites[0]);
                emp.SetEmployeeStats(5, 5, 5, 2, 5, 2);
            }
            else if (select == 1)
            {
                emp = new Employee(id.ToString(), EmployeeSpecialty.Medic, basicJumpSuit, 24, 24, 2, employeeSprites[0]);
                emp.SetEmployeeStats(5, 5, 5, 2, 5, 2);
            }
            else
            {
                emp = new Employee(id.ToString(), EmployeeSpecialty.Scientist, basicJumpSuit, 24, 24, 2, employeeSprites[0]);
                emp.SetEmployeeStats(5, 5, 5, 2, 5, 2);
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

    public void SelectAsActive(string name)
    {
        if (employee_roster.ContainsKey(name))
        {
            active_employees.Add(employee_roster[name]);
        }
    }

    public void TestSelectAllAsActive()
    {
        Debug.Log(" Selecting all new employees as active!");
        foreach(Employee e in new_employees.Values)
        {
            active_employees.Add(e);
        }
    }

    // Spawn employees as gameobjects in a level
    public void SpawnActiveEmployees(Vector3 spawnPosition)
    {
        float offset = 0;

        Employee[] actives = active_employees.ToArray();

        Debug.Log("Spawning active employees!");

        for (int i = 0; i < actives.Length; i++)
        {
            Vector3 pos = new Vector3(spawnPosition.x + offset, spawnPosition.y, 0);

            // ALL Employees share the same prefab, the Employee class stats will determine their added components
            GameObject employee = ObjectPool.instance.GetObjectForType("Employee", true, pos);
            
            if (employee != null)
            {
                // TODO: Also set the employees correct sprite here!
                employee.GetComponentInChildren<SpriteRenderer>().sprite = actives[i].MySprite;

                Employee_Handler emp_handler = employee.GetComponent<Employee_Handler>();
                // Set the employee class of the Employee Handler
                if (emp_handler != null)
                {
                    emp_handler.DefineEmployee(actives[i]);

                    // Add components for the 3 stats used to perform actions
                    if (emp_handler.MyEmployee.emp_stats.Extraction > 0)
                    {
                        AddEmployeeComponent<Employee_Extract>(employee);
                    }
                    if (emp_handler.MyEmployee.emp_stats.Mechanics > 0)
                    {
                        AddEmployeeComponent<Employee_Mechanics>(employee);
                    }
                    // TODO: Add a Healing action
                    //if (emp_handler.MyEmployee.emp_stats.Healing > 0)
                    //{
                    //    AddEmployeeComponent<Employee_Extract>(employee);
                    //}
                }
                // Copy the employee's unit stats to the unit stats referenced in its base class
                if (employee.GetComponent<Employee_Attack>() != null)
                {
                    employee.GetComponent<Employee_Attack>().stats = new UnitStats(actives[i].unitStats);
                }

                // Init its move stats
                if (employee.GetComponent<UnitPathHandler>() != null)
                {
                    employee.GetComponent<UnitPathHandler>().mStats.InitStartingMoveStats(2, 2);
                    employee.GetComponent<UnitPathHandler>().mStats.InitMoveStats();
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
        emp.SetEmployeeStats(5, 5, 5, 2, 5, 2);

        Vector3 pos = ResourceGrid.Grid.Hero != null ? ResourceGrid.Grid.Hero.transform.position + Vector3.up : new Vector3(10, 10, 0);

        //Employee_Actions.Instance.DefineActions(emp.Specialty);

        // Grab the correct prefab by passing in this employees specialty (Prefabs are named by specialty so they contain the correct components to work)
        GameObject employee = ObjectPool.instance.GetObjectForType(emp.Specialty.ToString(), true, pos);

        if (employee != null)
        {
            // TODO: Also set the employees correct sprite here!
            employee.GetComponentInChildren<SpriteRenderer>().sprite = emp.MySprite;
            Employee_Handler emp_handler = employee.GetComponent<Employee_Handler>();
            // Set the employee class of the Employee Handler
            if (emp_handler != null)
            {
                emp_handler.DefineEmployee(emp);

                // Add components for the 3 stats used to perform actions
                if (emp_handler.MyEmployee.emp_stats.Extraction > 0)
                {
                    AddEmployeeComponent<Employee_Extract>(employee);
                }
                if (emp_handler.MyEmployee.emp_stats.Mechanics > 0)
                {
                    AddEmployeeComponent<Employee_Mechanics>(employee);
                }
                // TODO: Add a Healing action
                //if (emp_handler.MyEmployee.emp_stats.Healing > 0)
                //{
                //    AddEmployeeComponent<Employee_Extract>(employee);
                //}


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

    void AddEmployeeComponent<T>(GameObject empGObj) where T : Component
    {
        if (empGObj.GetComponent<T>() == null)
        {
            empGObj.AddComponent<T>();
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
