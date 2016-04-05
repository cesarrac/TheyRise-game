using UnityEngine;
using System.Collections;


public enum EmployeeSpecialty
{
    Operator,
    Medic,
    Scientist
}

public class EmployeeStats
{
    float workRate;
    public float WorkRate { get { return workRate; } }
    float extraction;
    public float Extraction { get { return extraction; } }
    float mechanics; // Could work as a Construction & Repair ability?
    public float Mechanics { get { return mechanics; } }
    float healing;
    public float Healing { get { return healing; } }
    float moveSpeed;
    public float MoveSpeed { get { return moveSpeed; } }

    float stressResistance; // This is the value all resistance stress checks are made against
    public float StressResistance { get { return stressResistance; } }

    float stress { get { return stress; } set { Mathf.Clamp(value, 0, 10); } }
    public float Stress { get { return stress; } }


    public EmployeeStats (float rate, float _extraction, float _mechanics, float _healing, float _speed, float _resitance, float _stress = 0)
    {
        workRate = rate;
        extraction = _extraction;
        mechanics = _mechanics;
        healing = _healing;
        moveSpeed = _speed;
        stressResistance = _resitance;
        stress = _stress;
    }
}
public class Employee {


    string name;
    public string Name { get { return name; } }

    public UnitStats unitStats { get; protected set; }

    EmployeeSpecialty specialty;
    public EmployeeSpecialty Specialty { get { return specialty; } }

    Sprite mySprite;
    public Sprite MySprite { get { return mySprite; } }

    public EmployeeStats emp_stats { get; protected set; }

    public Employee() { }

    public Employee (string _name, EmployeeSpecialty spec, Armor _armor, float maxHP, float curHP, float attk, Sprite _sprite)
    {
        name = _name;

        // all we would use from the Armor would be its defense and shield stats but it 
        // might be useful to store it as an Armor item in order to use the same logic
        // This could also lead to armor/item upgrades for the employees as well.

        unitStats = new UnitStats(maxHP, attk, _armor.armorStats.startingDefense, _armor.armorStats.startingShield);

        // Using only this enum variable I can from another component 
        // derive what Actions this unit takes since each Specialy dictates
        // a specific list of actions.
        specialty = spec;

        mySprite = _sprite;

        //emp_stats = new EmployeeStats(rate);

        // TODO: Employee Level? 
        // I can also pair that action logic with some sort of level that the 
        // Player upgrades. Then when re-loading an employee it would get the actions/abilities
        // of their specialty, upgraded with bonuses according to their level.


    }

    public void SetEmployeeStats(float extract, float mech, float heal, float speed, float stressResist, float rate = 2f, float stress = 0)
    {
        emp_stats = new EmployeeStats(rate, extract, mech, heal, speed, stressResist, stress);
    }


}
