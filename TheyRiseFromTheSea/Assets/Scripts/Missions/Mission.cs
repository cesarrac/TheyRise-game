using UnityEngine;
using System.Collections;
using System;


public enum MissionType
{
    SCIENCE, SURVIVAL, ENCOUNTER
}

[Serializable]
public class Mission{

    string missionName;
    public string MissionName    { get { return missionName; } }

    MissionType missionType;
    public MissionType MissionType {  get { return missionType; } }

    string description;
    public string Description { get { return description; } }

    bool isCompleted;
    public bool IsCompleted { get { return isCompleted; } }

    // Each mission requires a specific Blueprint for the Player's Nanobuilder. (This will work like the Terraformer does right now)
    // That blueprint has to be loaded automatically unto the player's nanobuilder, regardless of the memory left (only if the player hasn't already done so!)
    Blueprint requiredBlueprint;
    public Blueprint RequiredBlueprint { get { return requiredBlueprint; } }

    // Each mission has an objective:
    // Survival Objectives are gather a resource
    // Science Objectives is complete an X amount of cycles of a specific machine & sometimes as a secondary obj gather a resource
    // Encounter Objectives are all about one thing... kill a specific target
    TileData.Types objectiveResource;
    public TileData.Types ObjectiveResource { get { return objectiveResource; } }

    // The # of waves you have to survive during this mission (unless its an ENCOUNTER)
    int objectiveAmnt;
    public int ObjectiveAmnt { get { return objectiveAmnt; } }

    // NOTE: Am I going to leave this stages option here? This allows us to create missions where you have to defend
    // a machine (i.e. the Terraformer) while it completes stages of production. -- ????
    int objectiveStages;
    public int ObjectiveStages { get { return objectiveStages; } }

    // This is to know what unit(s) to spawn for the encounter
    string encounterID;
    public string EncounterID { get { return encounterID; } }

    // Callback to register a completed mission
    private Action<Mission> completeMissionCB;


    public Mission() { }

    // Copy Constructor
    public Mission (Mission otherMission)
    {
        missionName = otherMission.missionName;
        missionType = otherMission.missionType;
        description = otherMission.description;
        objectiveAmnt = otherMission.objectiveAmnt;
        isCompleted = otherMission.isCompleted;

    }

    // MAIN MISSION CONSTRUCTOR (total num of waves determines Completion)
    public Mission(string name, MissionType mType, int numOfWaves, string desc = "Survive!")
    {
        missionName = name;
        missionType = mType;

        description = desc;

        objectiveAmnt = numOfWaves;
    }

    public Mission(string name, MissionType mType, Blueprint requiredBP, string desc = "Unknown Signal")
    {
        missionName = name;
        missionType = mType;
        requiredBlueprint = requiredBP;
        description = desc;
    }


    // Survival Mission:
    public Mission(string name, MissionType mType, Blueprint requiredBP, TileData.Types reqResource, int reqAmnt, string desc = "Unknown Signal")
    {
        missionName = name;
        missionType = mType;
        requiredBlueprint = requiredBP;
        description = desc;
        objectiveResource = reqResource;
        objectiveAmnt = reqAmnt;
    }

    // Science Mission:
    public Mission(string name, MissionType mType, Blueprint requiredBP, int stages, string desc = "Unknown Signal")
    {
        missionName = name;
        missionType = mType;
        requiredBlueprint = requiredBP;
        description = desc;

        objectiveStages = stages;
    }

    // This Function will flag this mission as completed, allowing the Player to go back to the ship and cash it in
    public void FlagAsCompleted()
    {
        isCompleted = true;
    }

    // When this Function is called the mission will be cleared from available missions and retired to completed missions
    public void CompleteMission()
    {
        completeMissionCB(this);
    }

    public void RegisterMissionCompleteCallback(Action<Mission> cb)
    {
        completeMissionCB = cb;
    }

    public void UnRegisterMissionCompleteCallback()
    {
        completeMissionCB = null;
    }

    public void ChangeName(string newName)
    {
        missionName = newName;
    }
}








//public class SignalEvent
//{
//    // A signal event can be...
//    // A SCIENCE mission to gather data from the planet's surface.
//    // A SURVIVAL mission to gather needed resources for the ship
//    // An ENCOUNTER mission, something is projecting an unknown signal from the planet... Boss fight!
//    public enum SignalEventType { SCIENCE, SURVIVAL, ENCOUNTER }

//    public SignalEventType signalType { get; protected set; }
//    public bool isCompleted { get; protected set; }
//    public bool isActive { get; protected set; }

//    public SignalEvent(SignalEventType type)
//    {
//        signalType = type;
//        isCompleted = false;
//    }

//    public void MakeActiveEvent()
//    {
//        isActive = true;
//    }

//    public void CompleteSignalObjective()
//    {
//        isActive = false;
//        isCompleted = true;
//    }
//}
