using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.Events;


public class UI_Manager : MonoBehaviour
{

    public static UI_Manager Instance { get; protected set; }

    public GameObject blueprint_infoPanel, builder_panel;
    public Text bp_name, bp_desc, cur_nano_memory, total_nano_memory;
    public Image player_healthBar;

    Dictionary<string, GameObject> loadedBlueprints = new Dictionary<string, GameObject>();

    public GameObject victoryPanel;

    public Text ore_total, water_total, food_total, organics_total;
    public Text ore_new, water_new, food_new, organics_new;

    public Text days;

    Dictionary<int, GameObject> loadedMissionsMap = new Dictionary<int, GameObject>();

    public GameObject availableMissions_panel, activeMissions_panel, completeMissions_panel;

    // MAIN UI PANELS
    public GameObject characterPanel, missionsPanel, resoucesPanel, bpPanel;
    GameObject currActivePanel;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (GameTracker.Instance.GetScene().name == "Level_CENTRAL")
        {
            DisplayDaysPassed();

            DisplayCharacterPanel();
        }
    }

    public void DisplayDaysPassed()
    {
        days.text = GameTracker.Instance.Days.ToString();
    }

    public void DisplayTotalResources()
    {
        ore_total.text = Ship_Inventory.Instance.DisplayResourceAmount(TileData.Types.rock).ToString();
        ore_new.text = Ship_Inventory.Instance.tempOre.ToString();

        water_total.text = Ship_Inventory.Instance.DisplayResourceAmount(TileData.Types.water).ToString();
        water_new.text = Ship_Inventory.Instance.tempWater.ToString();

        food_total.text = Ship_Inventory.Instance.DisplayResourceAmount(TileData.Types.food).ToString();
        food_new.text = Ship_Inventory.Instance.tempFood.ToString();

        //organics_total.text = Ship_Inventory.Instance.DisplayResourceAmount(TileData.Types.organic).ToString();
        //organics_new.text = Ship_Inventory.Instance.tempOrganics.ToString();
    }

    // BLUEPRINT PANEL:
    public void DisplayNanoBuilderMemory(int curMemory, int totalMemory)
    {
        cur_nano_memory.text = curMemory.ToString();
        total_nano_memory.text = totalMemory.ToString();
    }

    public void DisplayBPInfo(string bpName, string bpDesc)
    {
        bp_name.text = bpName;
        bp_desc.text = bpDesc;
    }

    public void AddBlueprintToBuilder(string bpName)
    {
        // Only allow Adding to Builder if it hasn't already been added
        if (!loadedBlueprints.ContainsKey(bpName))
        {
            // Get the Button prefab from the Pool ...
            GameObject bp = ObjectPool.instance.GetObjectForType("Loaded Blueprint", true, Vector3.zero);

            //Parent it to the Builder's transform ...
            bp.transform.SetParent(builder_panel.transform);

            // Add an onClick listener to this button, to allow it to callback Select Blueprint with its corresponding BP name...
            bp.GetComponent<Button>().onClick.AddListener(() => { BlueprintDatabase.Instance.SelectBlueprint(bpName); });

            //... and fill its text field with the BP name.
            bp.GetComponentInChildren<Text>().text = bpName;

            loadedBlueprints.Add(bpName, bp);
        }
    }

    public void RemoveBlueprintTextFromBuilder(string bpName)
    {
        if (loadedBlueprints.ContainsKey(bpName))
        {
            // Pool the text gameobject...
            ObjectPool.instance.PoolObject(loadedBlueprints[bpName]);

            // ... and remove it from Dictionary
            loadedBlueprints.Remove(bpName);
        }
    }

    public void InventoryReady()
    {
        // Load the Ship level
        GameMaster.Instance.RestartToShip();
    }

    public void DisplayVictoryPanel()
    {
        if (!victoryPanel.activeSelf)
        {
            victoryPanel.SetActive(true);
        }

    }

    public void CloseVictoryPanel()
    {
        victoryPanel.SetActive(false);
    }

    // Load Scenes:
    public void ReturnToShip()
    {
        GameMaster.Instance.RestartToShip();
    }

    public void GoToBlueprintsScene()
    {
        GameMaster.Instance.LoadBlueprintsScene();
    }

    public void GoToLaunchScene()
    {
        GameMaster.Instance.LoadLauncherScene();
    }

    public void GoToEquipScene()
    {
        GameMaster.Instance.LoadEquipmentScreen();
    }


    // Trade Orders / Missions

    public void AddAvailableMission(int id, string name, int timeLeft)
    {
 
        if (!loadedMissionsMap.ContainsKey(id))
        {
            // Get the Button prefab from the Pool ...
            GameObject mission = ObjectPool.instance.GetObjectForType("Mission", true, Vector3.zero);

        //Parent it to the Builder's transform ...
        mission.transform.SetParent(availableMissions_panel.transform);

        // Add an onClick listener to this button, to allow it to callback Select Blueprint with its corresponding BP name...
        mission.GetComponent<Button>().onClick.AddListener(() => { TradeOrder_Manager.Instance.SelectTradeOrderFromUI(id); });

        // ... and fill both its Text children...

        // ... first the Name
        mission.GetComponentsInChildren<Text>()[0].text = timeLeft.ToString();

        // ... then the Time Left.
        mission.GetComponentsInChildren<Text>()[1].text =  name;


            // To easily find which is which for removal or later loading them again, add each Button to a Dictionary<string, GameObject>
            loadedMissionsMap.Add(id, mission);
        }
    }

    public void AddToActiveMissions(int id)
    {
        // All we have to do here is find the mission from available missions and set its parent to the available missions panel
        if (loadedMissionsMap.ContainsKey(id))
        {
            loadedMissionsMap[id].transform.SetParent(activeMissions_panel.transform);
        }
    }

    public void DisplayMissions()
    {
        TradeOrder_Manager.Instance.DisplayOrders();
    }

    // Save & Load
    public void Save()
    {
        GameTracker.Instance.Save();
    }

    public void Load()
    {
        GameTracker.Instance.Load();
    }

    // PANEL CONTROLS
    public void DisplayCharacterPanel()
    {
        if (characterPanel.activeSelf == false)
        {
            // Deactivate curr active panel...
            if (currActivePanel != null)
                currActivePanel.SetActive(false);

            // ... activate Character Panel ...
            characterPanel.SetActive(true);

            // ... and set it as current active
            currActivePanel = characterPanel;
        }
    }

    public void DisplayMissionsPanel()
    {
        if (missionsPanel.activeSelf == false)
        {
            // Deactivate curr active panel...
            if (currActivePanel != null)
                currActivePanel.SetActive(false);

            // ... activate Missions Panel ...
            missionsPanel.SetActive(true);

            // ... show available missions,
            DisplayMissions();

            // and set it as current active.
            currActivePanel = missionsPanel;
        }
    }

    public void DisplayResourcesPanel()
    {
        if (resoucesPanel.activeSelf == false)
        {
            // Deactivate curr active panel...
            if (currActivePanel != null)
                currActivePanel.SetActive(false);

            // ... activate Resources Panel ...
            resoucesPanel.SetActive(true);

            // ... display Resources
            DisplayTotalResources();

            // and set it as current active.
            currActivePanel = resoucesPanel;
        }
    }

    public void DisplayBPPanel()
    {
        if (bpPanel.activeSelf == false)
        {
            // Deactivate curr active panel...
            if (currActivePanel != null)
                currActivePanel.SetActive(false);

            // ... activate Resources Panel ...
            bpPanel.SetActive(true);

            // ... Reload any previous blueprints ...
            BlueprintDatabase.Instance.ReloadPreviousLoaded();

            // and set it as current active.
            currActivePanel = bpPanel;
        }
    }
}
