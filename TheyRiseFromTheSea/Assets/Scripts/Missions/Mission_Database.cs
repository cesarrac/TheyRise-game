using UnityEngine;
using System.Collections.Generic;

public class Mission_Database  {

    public List<Mission> survivalMissions = new List<Mission>();
    public List<Mission> scienceMissions = new List<Mission>();
    public List<Mission> encounterMissions = new List<Mission>();

    public Mission_Database() { }

    public void InitMissions()
    {
        survivalMissions = new List<Mission>()
        {
            new Mission("Food Run", MissionType.SURVIVAL, new Blueprint("Seaweed Farm", TileData.Types.farm_s, BuildingType.UTILITY)),
            new Mission("Water Run", MissionType.SURVIVAL, new Blueprint("Desalination Pump", TileData.Types.desalt_s, BuildingType.UTILITY), TileData.Types.water, 200),
            new Mission("Collect Rock Samples", MissionType.SURVIVAL, new Blueprint("Extractor", TileData.Types.extractor, BuildingType.UTILITY), TileData.Types.rock, 500)
        };

        scienceMissions = new List<Mission>()
        {
            new Mission("Scan Bio Samples", MissionType.SCIENCE, new Blueprint("Sea-Witch Crag", TileData.Types.desalt_s, BuildingType.UTILITY)),
            new Mission("Reshape the Environment", MissionType.SCIENCE, new Blueprint("Terraformer", TileData.Types.terraformer, BuildingType.UTILITY), 3)
        };

        encounterMissions = new List<Mission>()
        {
            new Mission("Boss Fight!", MissionType.ENCOUNTER, new Blueprint("Machine Gun", TileData.Types.machine_gun, BuildingType.BATTLE))
        };

    }

    public Mission GetMission(MissionType mType)
    {
        if (mType == MissionType.SCIENCE)
        {
           return scienceMissions[Random.Range(0, scienceMissions.Count)];
        }
        else if (mType == MissionType.SURVIVAL)
        {
            return survivalMissions[Random.Range(0, survivalMissions.Count)];
        }
        else
        {
            return encounterMissions[Random.Range(0, encounterMissions.Count)];
        }
    }
}
