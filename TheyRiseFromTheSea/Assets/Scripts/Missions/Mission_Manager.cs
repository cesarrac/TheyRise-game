using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class Mission_Manager : MonoBehaviour {

    // Example. 
    // Generate 5 missions and place them on the map. These 5 missions would be removed from a list of Available Missions so they don't get
    // selected again.
    // Once the last mission on the map is completed...
    // Generate 5 missions again from the available missions list.
    // Every time a mission is generated there's a check to see what day the player is on. This will determine the difficulty. (i.e. Day 2 = Difficulty 2)

    public static Mission_Manager Instance { get; protected set; }

    Dictionary<string, Mission> availableMissions_Map = new Dictionary<string, Mission>();
    List<Mission> availableMissions = new List<Mission>();
    public List<Mission> Available { get { return availableMissions; } }
    List<Mission> completedMissions = new List<Mission>();

    Mission activeMission;
    public Mission ActiveMission { get { return activeMission; } }

    Mission_Database mission_database;

    int missionsCompletedCount = 0;

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

        mission_database = new Mission_Database();

        mission_database.InitMissions();

    }

    public void Init()
    {
        CheckToGenerateNewMissions();
    }
    
    public Dictionary<string, Mission> GetAvailable()
    {
        //availableMissions_Map = new Dictionary<string, Mission>();
        availableMissions_Map.Clear();

        for (int i = 0; i < availableMissions.Count; i++)
        {
            // Unregister the Complete Mission callback so the class can be safely serialized...
            availableMissions[i].UnRegisterMissionCompleteCallback();

            // ... then add to the dictionary for saving.
            availableMissions_Map.Add("Available " + i, availableMissions[i]);
        }

        // Also UnRegister callback on active mission
        activeMission.UnRegisterMissionCompleteCallback();
        availableMissions_Map.Add("Active", activeMission);

        return availableMissions_Map;
    }

    public void LoadAvailableMissions(Mission newMission)
    {
        Mission M = new Mission(newMission);
        M.RegisterMissionCompleteCallback(CompleteMission);
        availableMissions.Add(M);
    }

    public void LoadActiveMission(Mission newMission)
    {
        Mission M = new Mission(newMission);
        M.RegisterMissionCompleteCallback(CompleteMission);
        activeMission = M;
        Debug.Log("MISSION MANAGER: current active mission is " + activeMission.MissionName);
    }

    public void CheckToGenerateNewMissions()
    {
        if (missionsCompletedCount >= 5 || missionsCompletedCount == 0)
        {
            missionsCompletedCount = 0;
            GenerateNewSetOfMissions();
        }
    }

    void GenerateNewSetOfMissions()
    {
        int difficulty = GameTracker.Instance.Days;

        availableMissions.Clear();

        // Here, generate 5 new missions from the Mission Database
        for (int i = 0; i < 5; i++)
        {
            if (i > 0)
                availableMissions.Add(MissionGenerator(availableMissions[i-1]));
            else
                availableMissions.Add(MissionGenerator());
            Debug.Log("Added mission -- " + availableMissions[i].MissionName + " at index " + i);
        }
        // After generating them the set would be sent to UI Manager to display on map

        // FOR TESTING Im hardcoding the active mission
        //activeMission = availableMissions[0];

        //Debug.Log("MISSION MANAGER: current active mission is " + activeMission.MissionName);

    }

    Mission MissionGenerator(Mission lastMission = null)
    {
        Mission newMission = new Mission();

        // 50 / 50 chance of generating a Science or a Survival mission
        int select = Random.Range(0, 4);
        if (select == 0 || select == 2)
        {
            // Grab a Science mission from the database
            if (lastMission != null)
                newMission = new Mission(mission_database.GetMission(MissionType.SCIENCE, lastMission.MissionName));
            else
                newMission = new Mission(mission_database.GetMission(MissionType.SCIENCE));
        }
        else
        {
            // Get a Survival mission from the database
            if (lastMission != null)
                newMission = new Mission(mission_database.GetMission(MissionType.SURVIVAL, lastMission.MissionName));
            else
                newMission = new Mission(mission_database.GetMission(MissionType.SURVIVAL));
        }

        //// *** FOR TESTING I AM FORCING MISSIONS!
        //newMission = mission_database.GetMission(MissionType.SCIENCE);

        // Set its Complete Mission callback...
        newMission.RegisterMissionCompleteCallback(CompleteMission);

        // ... before returning.
        return newMission;

        //ENCOUNTER Type missions should be generated through some other logic that decides when a boss fight should happen
    }

    // This can be called by a UI element that visually represents available missions as Buttons. When pressed it would become the active mission.
    public void SelectMission(int missionIndex)
    {
        if (missionIndex >= 0 && missionIndex < availableMissions.Count)
        {
            // If the mission is NOT a completed mission, select it
            if (!availableMissions[missionIndex].IsCompleted)
            {
                activeMission = availableMissions[missionIndex];

                // Load the required Blueprint for this mission
                //BlueprintDatabase.Instance.InitRequiredBlueprint();

                Debug.Log("ACTIVE MISSION IS: " + activeMission.MissionName);
            }
        }
            
    }

    // **************************************************
    // NOTE:  The code below it for completing missions. The logic is that each mission would have
    //        a different completed condition (i.e. Collect 200 rock). 
    //      I'm going to replace that locic with something simpler, all missions are either ENCOUNTERS, where you have
    //      to kill a specific unit to complete it, and EVERYTHING ELSE, where you have to survive X amount of waves of enemies to complete.
    // **************************************************

    // Checks to verify if Mission has been completed:
    public void CheckSurvivalMissionCompleted()
    {
        // A Survival Mission's objective is to gather x amount of y resource.
        if (Ship_Inventory.Instance.CheckForSpecificResource(activeMission.ObjectiveResource, true) >= activeMission.ObjectiveAmnt)
        {
            activeMission.FlagAsCompleted();

            // Display objective completed message
            UI_Manager.Instance.DisplayVictoryPanel();
        }
    }

    public void CheckScienceMissionCompleted(int currStageCount)
    {
        if (currStageCount >= activeMission.ObjectiveStages)
        {
            activeMission.FlagAsCompleted();

            // Display objective completed message
            UI_Manager.Instance.DisplayVictoryPanel();
        }
    }

    public void SetMissionStages(StagedProgress_Handler stage_handler)
    {
        stage_handler.MaxStages = activeMission.ObjectiveStages;
    }

    // **************************************************

    // ********
    //      This code below is the NEW version of Checking for mission Completed.
    //      It will be called by the Enemy Master after a wave has been killed. 
    //      This simply checks if the number of waves survived => the objective
    //      ammount of this active mission.
    // ********
    public bool AllWavesAreCompleted (int wavesSurvived)
    {
        if (wavesSurvived >= activeMission.ObjectiveAmnt)
        {
            return true;
        }
        else
            return false;

    }
    public void CompleteMissionEnemyWaves (int wavesSurvived)
    {
        if (AllWavesAreCompleted(wavesSurvived) == false)
            return;

        activeMission.FlagAsCompleted();

        // Display objective completed message
        UI_Manager.Instance.DisplayVictoryPanel();
    }
    // ********

    // Function called by GM when Launching back to ship from the Planet to complete the mission
    public void CompleteActiveMission()
    {
        activeMission.CompleteMission();
    }

    // Function called as callback by the mission once it has been verified as completed
    void CompleteMission(Mission completed)
    {
        if (availableMissions.Contains(completed))
        {
            //   availableMissions.Remove(completed);
            missionsCompletedCount++;
        }

        //completedMissions.Add(completed);

       // CheckToGenerateNewMissions();
    }

   // UI
   public void DisplayAvailableMissions()
    {
        for (int i = 0; i < availableMissions.Count; i++)
        {

            // FIX ME: Right now the Mission class does not have an Energy Cost variable, I am hardcoding it here!
            UI_Manager.Instance.AddMission(i, availableMissions[i].MissionName,
                                              availableMissions[i].MissionName,
                                              20,
                                              availableMissions[i].IsCompleted);
        }
 
    }
    
}
