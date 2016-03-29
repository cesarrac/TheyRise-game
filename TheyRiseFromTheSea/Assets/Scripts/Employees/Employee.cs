using UnityEngine;
using System.Collections;


public enum EmployeeSpecialty
{
    OPERATOR,
    MEDIC,
    SCIENTIST
}


public class Employee {


    string name;
    public string Name { get { return name; } }

    public UnitStats unitStats { get; protected set; }

    EmployeeSpecialty specialty;
    public EmployeeSpecialty Specialty { get { return specialty; } }

    // TODO: Figure out if I want to include their specialty actions here as part of the constructor.
    // This constructor is probably going to be used when first generating the employee and
    // when loading from a save file.

    public Employee (string _name, EmployeeSpecialty spec, Armor _armor, float maxHP, float curHP, float attk)
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

        // TODO: Employee Level? 
        // I can also pair that action logic with some sort of level that the 
        // Player upgrades. Then when re-loading an employee it would get the actions/abilities
        // of their specialty, upgraded with bonuses according to their level.
    }


}
