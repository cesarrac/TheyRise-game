using UnityEngine;
using System.Collections.Generic;

public class Mission_Database  {

    public List<Mission> survivalMissions = new List<Mission>();
    public List<Mission> scienceMissions = new List<Mission>();
    public List<Mission> encounterMissions = new List<Mission>();

    public List<string> nameStarts;
    public List<string> nameEnds;

    public Mission_Database() { }

    public void InitMissions()
    {
        survivalMissions = new List<Mission>()
        {
            new Mission("I sea", MissionType.SURVIVAL, 4),
            new Mission("Oh el Capitan, mi Capitan", MissionType.SURVIVAL, 6),
            new Mission("Fin-ito", MissionType.SURVIVAL, 6),
            new Mission("Don't feed the fish", MissionType.SURVIVAL, 6),
            new Mission("Brake for algae", MissionType.SURVIVAL, 6),
            new Mission("Reefur Madness", MissionType.SURVIVAL, 6),
            new Mission("Aquatwister", MissionType.SURVIVAL, 6),
            new Mission("That's no shark", MissionType.SURVIVAL, 6),
            new Mission("Oh My, Fishsticks", MissionType.SURVIVAL, 6),
            new Mission("Deeper Horizons", MissionType.SURVIVAL, 6),
            new Mission("Mauve Submarine", MissionType.SURVIVAL, 6),
            new Mission("School of Bubbles", MissionType.SURVIVAL, 6)
        };

        scienceMissions = new List<Mission>()
        {
            new Mission("Scan My Samples", MissionType.SCIENCE, 4),
            new Mission("Regretful Waters", MissionType.SCIENCE, 6),
            new Mission("Calm Waters", MissionType.SCIENCE, 6),
            new Mission("Chase Waterfalls", MissionType.SCIENCE, 6),
            new Mission("Don't forget the tartar", MissionType.SCIENCE, 6),
            new Mission("Squidz 4 Life", MissionType.SCIENCE, 6),
            new Mission("Human sea-sources", MissionType.SCIENCE, 6),
            new Mission("Gone with the Fishes", MissionType.SCIENCE, 6),
            new Mission("Lovely Isn't it?", MissionType.SCIENCE, 6),
            new Mission("Choppy, Choppy", MissionType.SCIENCE, 6),
            new Mission("Row, row, row", MissionType.SCIENCE, 6),
            new Mission("Hold your breath", MissionType.SCIENCE, 6),
            new Mission("Fishing for an insult", MissionType.SCIENCE, 6),
            new Mission("Swimming Downstream", MissionType.SCIENCE, 6)
        };

        encounterMissions = new List<Mission>()
        {
            new Mission("Boss Fight!", MissionType.ENCOUNTER, new Blueprint("Machine Gun", TileData.Types.machine_gun, BuildingType.BATTLE))
        };

        InitNameLists();
    }

    public Mission GetMission(MissionType mType, string lastMissionName = "none")
    {
        // TODO: Check what the Last returned mission was (if any) and try to return a mission that 
        // does NOT have the same name. 
        

        if (mType == MissionType.SCIENCE)
        {
            int select = Random.Range(0, scienceMissions.Count);
            if (lastMissionName != "none")
            {
                if (scienceMissions[select].MissionName == lastMissionName)
                {
                    scienceMissions[select].ChangeName(NameGenerator());
                    return scienceMissions[select];
                }
                else
                    return scienceMissions[select];
            }
            else
            {
                return scienceMissions[select];
            }
    
        }
        else if (mType == MissionType.SURVIVAL)
        {
            int select = Random.Range(0, survivalMissions.Count);
            if (lastMissionName != "none")
            {
                if (survivalMissions[select].MissionName == lastMissionName)
                {
                    survivalMissions[select].ChangeName(NameGenerator());
                    return survivalMissions[select];
                }
                else
                    return survivalMissions[select];
            }
            else
                return survivalMissions[select];
        }
        else
        {
            return encounterMissions[Random.Range(0, encounterMissions.Count)];
        }
    }


    void InitNameLists()
    {
        nameStarts = new List<string>()
        {
            "Zalda",
            "Crystalline",
            "Stey Treewhan",
            "Plifrah Geoffromy",
            "Kin",
            "Ondreon",
            "Blene"
        };

        nameEnds = new List<string>()
        {
            "Nah Uquo",
            "Ime Shent",
            "Torf Eppie",
            "Thest Essexia",
            "Ulo Ell",
            "Ondreon"
        };
    }

    string NameGenerator()
    {
        int selectOne = Random.Range(0, nameStarts.Count);
        int selectTwo = Random.Range(0, nameEnds.Count);

        return nameStarts[selectOne] + " " + nameEnds[selectTwo];
    }
    




}   

