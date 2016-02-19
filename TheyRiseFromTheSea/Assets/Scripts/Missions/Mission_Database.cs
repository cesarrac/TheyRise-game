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
            //new Mission("Food Run", MissionType.SURVIVAL, new Blueprint("Seaweed Farm", TileData.Types.farm_s)),
            new Mission("Water Run", MissionType.SURVIVAL, new Blueprint("Desalination Pump", TileData.Types.desalt_s), TileData.Types.water, 200)
        };

        scienceMissions = new List<Mission>()
        {
            //new Mission("Collect Rock Samples", MissionType.SCIENCE, new Blueprint("Extractor", TileData.Types.extractor)),
            //new Mission("Scan Bio Samples", MissionType.SCIENCE, new Blueprint("Sea-Witch Crag", TileData.Types.desalt_s)),
            new Mission("Reshape the Environment", MissionType.SCIENCE, new Blueprint("Terraformer", TileData.Types.terraformer), 3)
        };

        encounterMissions = new List<Mission>()
        {
            new Mission("Boss Fight!", MissionType.ENCOUNTER, new Blueprint("Machine Gun", TileData.Types.machine_gun))
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
