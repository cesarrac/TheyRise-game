using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Mission_Manager : MonoBehaviour {
    /* The GM calls on the Mission Manager every time the Ship level is loaded to check if the player currently has a company task. If they don't it will assing a new one. 
    In between levels, during BALANCE SHEET state, the GM will check if the current task's quota/objective has been met, if it has it will call another method here
    that removes the current task and adds it to a completed tasks list. That way when the ship loads the Player will get a new task automatically. */

    List<Mission> CurrentMission = new List<Mission>();
    public List<Mission> CompletedMissions = new List<Mission>();

    public void CheckForCorpMission()
    {
        // If CurrentMission count is 0, player has NO current mission
        if (CurrentMission.Count == 0)
        {
            // Create a new corporate mission (player always has to have a corporate mission!)
            Mission newMission = CreateCorporateMission();
            // Make it the current mission
            CurrentMission.Add(newMission);

        }
        else
        {
            // Player currently has missions, let's make sure that one of them is a corporate mission!
            bool noCorpMissions = true;
            foreach(Mission mission in CurrentMission)
            {
                if (mission.missionFaction == Mission.Faction.CORPORATE)
                {
                    noCorpMissions = false;
                    return;
                }
            }

            if (noCorpMissions)
            {
                // Add a mission since we were not able to find a corporate mission
                // Create a new corporate mission (player always has to have a corporate mission!)
                Mission newMission = CreateCorporateMission();
                // Make it the current mission
                CurrentMission.Add(newMission);
            }
        }
    }


    Mission CreateCorporateMission()
    {
        Mission newMission = new Mission();

        int difficulty = Random.Range(0, 6);  //TODO: Add a way for the GM to balance the difficulty instead of picking a random one
        int total = GetQuotaTotalFromDifficulty(difficulty);

        // First select what type of resource will be required
        int resourcePick = Random.Range(0, 6);
        if (resourcePick <= 2)
        {
            // Common Ore         
            newMission = new Mission(Mission.Faction.CORPORATE, Mission.Quota.RequiredResource.COMMON_ORE, total);
        }
        else if (resourcePick == 3)
        {
            // Water
            newMission = new Mission(Mission.Faction.CORPORATE, Mission.Quota.RequiredResource.WATER, total);
        }
        else if (resourcePick == 4)
        {
            // Food
            newMission = new Mission(Mission.Faction.CORPORATE, Mission.Quota.RequiredResource.FOOD, total);
        }
        else
        {
            // Enriched Ore
            newMission = new Mission(Mission.Faction.CORPORATE, Mission.Quota.RequiredResource.ENRICHED_ORE, total);
        }
        return newMission;
    }

    int GetQuotaTotalFromDifficulty(int difficulty)
    {
        int quota = 0;
        if (difficulty <= 1)
        {
            // Easy
            quota = Random.Range(100, 500);
        }
        else if (difficulty == 2)
        {
            // Average
            quota = Random.Range(501, 999);
        }
        else if (difficulty == 3 || difficulty == 4)
        {
            // Medium
            quota = Random.Range(1000, 3000);
        }
        else
        {
            // Hard
            quota = Random.Range(3000, 7000);
        }
        return quota;
    }
}
