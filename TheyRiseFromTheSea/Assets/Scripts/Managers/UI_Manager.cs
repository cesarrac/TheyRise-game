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

    public GameObject victoryPanel, abilityButton;

    public Text ore_total, water_total, food_total, organics_total;
    public Text ore_new, water_new, food_new, organics_new;

    public Text days;

    // Orders:
    Dictionary<int, GameObject> loadedOrdersMap = new Dictionary<int, GameObject>();
    public GameObject availableOrders_panel, activeOrders_panel, completeOrders_panel, orderInfo;
    public Text orderName, orderNameDefault, orderClient, orderTime, orderResource, orderAmount;

    // MAIN MENU PANELS
    public GameObject mainMenuPanel, charCreationPanel;

    // (CENTRAL LEVEL) MAIN UI PANELS
    public GameObject characterPanel, ordersPanel, resoucesPanel, bpPanel, missionsPanel;
    GameObject currActivePanel;

    // Hero Character Info
    public Text heroName;
    bool nameIsDisplayed = false;

    // MISSIONS:
    public Text m1Name, m1Desc, m1Cost;
    public Text m2Name, m2Desc, m2Cost;
    public Text m3Name, m3Desc, m3Cost;
    public Text m4Name, m4Desc, m4Cost;
    public Text m5Name, m5Desc, m5Cost;

    public GameObject buildWarningGObj;
    public Text buildWarning;

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

    public void EndDay()
    {
        GameMaster.Instance.EndDay();
    }

    // *****************************************                             RESOURCES:

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

    void DisplayHeroName()
    {
        // FIX THIS! We need to replace "Prospector" with the Hero's current rank
        if (!nameIsDisplayed)
        {
            nameIsDisplayed = true;
            heroName.text = "Prospector " + GameMaster.Instance.theHero.heroName;
        }
    }

    public void DisplayBuildWarning(TileData.Types resource)
    {
        if (buildWarningGObj.activeSelf == false)
        {
            buildWarningGObj.gameObject.SetActive(true);
        }

        buildWarning.text = "Need more " + resource.ToString() + "!";
    }

    // *****************************************                             BLUEPRINTS:

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

    public void RemoveAllBlueprintsText()
    {
        foreach(string key in loadedBlueprints.Keys)
        {
            ObjectPool.instance.PoolObject(loadedBlueprints[key]);
        }

        // after pooling all text objects, clear the Dictionary
        loadedBlueprints.Clear();
    }

    public void InventoryReady()
    {
        // Load the Ship level
        GameMaster.Instance.NewGameLoadShip();
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



    // *****************************************                            TRADE ORDERS:

    public void AddAvailableOrder(int id, string name, int timeLeft)
    {
 
        if (!loadedOrdersMap.ContainsKey(id))
        {
            // Get the Button prefab from the Pool ...
            GameObject order = ObjectPool.instance.GetObjectForType("Trade Order", true, Vector3.zero);

        //Parent it to the Builder's transform ...
        order.transform.SetParent(availableOrders_panel.transform);

        // Add an onClick listener to this button, to allow it to callback Select Blueprint with its corresponding BP name...
        order.GetComponent<Button>().onClick.AddListener(() => { TradeOrder_Manager.Instance.SelectTradeOrderFromUI(id); });

        // ... and fill both its Text children...

        // ... first the Name
        order.GetComponentsInChildren<Text>()[0].text = timeLeft.ToString();

        // ... then the Time Left.
        order.GetComponentsInChildren<Text>()[1].text =  name;


            // To easily find which is which for removal or later loading them again, add each Button to a Dictionary<string, GameObject>
            loadedOrdersMap.Add(id, order);
        }
    }

    public void AddToActiveOrders(int id)
    {
        // All we have to do here is find the mission from available Orders and set its parent to the available Orders panel
        if (loadedOrdersMap.ContainsKey(id))
        {
            loadedOrdersMap[id].transform.SetParent(activeOrders_panel.transform);
        }
    }

    public void DisplayOrders()
    {
        TradeOrder_Manager.Instance.DisplayOrders();
    }

    public void DisplayOrderInfo(string mName, string mClient, string mTimeLimit, string reqResource, string reqAmmnt)
    {
        if (orderNameDefault.gameObject.activeSelf)
        {
            orderNameDefault.gameObject.SetActive(false);
            orderInfo.gameObject.SetActive(true);
        }
        orderName.text = mName;
        orderClient.text = mClient;
        orderTime.text = mTimeLimit;
        orderResource.text = reqResource;
        orderAmount.text = reqAmmnt;
    }

    public void ClearOrderInfo()
    {
        if (!orderNameDefault.gameObject.activeSelf)
        {
            orderNameDefault.gameObject.SetActive(true);
            orderInfo.SetActive(false);

        }
    }

    // *****************************************                            MISSIONS:
    public void SelectMission(int missionIndex)
    {
        Mission_Manager.Instance.SelectMission(missionIndex);
    }
    
    public void AddMission(int num, string name, string desc, int cost, bool isCompleted = false)
    {
        string completed = "(Completed)";
        if (num == 0)
        {
            if (isCompleted)
            {
                m1Name.text = name + " " + completed;
            }
            else
            {
                m1Name.text = name;
            }

            m1Desc.text = desc;
            m1Cost.text = cost.ToString();
        }
        else if (num == 1)
        {
            if (isCompleted)
            {
                m2Name.text = name + " " + completed;
            }
            else
            {
                m2Name.text = name;
            }

            m2Desc.text = desc;
            m2Cost.text = cost.ToString();
        }
        else if (num == 2)
        {
            if (isCompleted)
            {
                m3Name.text = name + " " + completed;
            }
            else
            {
                m3Name.text = name;
            }

            m3Desc.text = desc;
            m3Cost.text = cost.ToString();
        }
        else if (num == 3)
        {
            if (isCompleted)
            {
                m4Name.text = name + " " + completed;
            }
            else
            {
                m4Name.text = name;
            }

            m4Desc.text = desc;
            m4Cost.text = cost.ToString();
        }
        else
        {
            if (isCompleted)
            {
                m5Name.text = name + " " + completed;
            }
            else
            {
                m5Name.text = name;
            }

            m5Desc.text = desc;
            m5Cost.text = cost.ToString();
        }
    }

    public void DisplayMissions()
    {
        Mission_Manager.Instance.DisplayAvailableMissions();
    }



    // *****************************************                            LOAD SCENES:
    public void ReturnToShip()
    {
        GameMaster.Instance.NewGameLoadShip();
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


    // *****************************************                            SAVE & LOAD GAME:

    public void Save()
    {
        GameTracker.Instance.Save();
    }

    public void Load()
    {
        GameTracker.Instance.Load();
    }



    // *****************************************                             PANEL CONTROLS:

    public void DisplayCharacterPanel()
    {
        if (characterPanel.activeSelf == false)
        {
            // Deactivate curr active panel...
            DeactivateCurrActivePanel();

            // ... activate Character Panel ...
            characterPanel.SetActive(true);

            // ... display the Hero's name ...
            DisplayHeroName();

            // ... and set it as current active
            currActivePanel = characterPanel;
        }
    }

    public void DisplayOrdersPanel()
    {
        if (ordersPanel.activeSelf == false)
        {
            // Deactivate curr active panel...
            DeactivateCurrActivePanel();

            // ... activate Orders Panel ...
            ordersPanel.SetActive(true);

            // ... show available Orders,
            DisplayOrders();

            // and set it as current active.
            currActivePanel = ordersPanel;
        }
    }

    public void DisplayResourcesPanel()
    {
        if (resoucesPanel.activeSelf == false)
        {
            // Deactivate curr active panel...
            DeactivateCurrActivePanel();

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
            DeactivateCurrActivePanel();

            // ... activate Resources Panel ...
            bpPanel.SetActive(true);

            // ... Reload any previous blueprints ...
            BlueprintDatabase.Instance.ReloadPreviousLoaded();

            // and set it as current active.
            currActivePanel = bpPanel;
        }
    }

    public void DisplayCharacterCreationPanel()
    {
        if (charCreationPanel.activeSelf == false)
        {
            // Deactivate curr active panel...
            DeactivateCurrActivePanel();

            // ... make sure Main Menu panel is still not on...
            if (mainMenuPanel.activeSelf)
            {
                mainMenuPanel.SetActive(false);
            }

            // ... activate CharacterCreation Panel ...
            charCreationPanel.SetActive(true);

            // and set it as current active.
            currActivePanel = charCreationPanel;
        }
    }

    public void DisplayMainMenuPanel()
    {
        if (mainMenuPanel.activeSelf == false)
        {
            // Deactivate curr active panel...
            DeactivateCurrActivePanel();

            // ... activate CharacterCreation Panel ...
            mainMenuPanel.SetActive(true);

            // and set it as current active.
            currActivePanel = mainMenuPanel;
        }
    }


    public void DisplayMissionsPanel()
    {
        if (missionsPanel.activeSelf == false)
        {
            // Deactivate curr active panel...
            DeactivateCurrActivePanel();

            // ... activate Missions Panel ...
            missionsPanel.SetActive(true);

            // ... Display Missions...
            DisplayMissions();

            // and set it as current active.
            currActivePanel = missionsPanel;
        }
    }

    void DeactivateCurrActivePanel()
    {
        // Deactivate curr active panel...
        if (currActivePanel != null)
        {
            // If the current panel was the Orders panel...
            if (currActivePanel == ordersPanel)
            {
                // Clear its info before deactivating.
                ClearOrderInfo();
                
            }

            currActivePanel.SetActive(false);
        }
            
    }

    //public void DisplayEnemyIndicator(Vector3 enemyPos)
    //{
    //    // TODO: later on we can add space to show total enemies and the threat level!
    //    if (!incomingIndicator.activeSelf)
    //    {
    //        incomingIndicator.SetActive(true);

    //    }


    //    incomingIndicator.GetComponentInChildren<EnemyIncoming_Indicator>().InitSpawnPos(enemyPos);
    //}

    //public void StopDisplayingEnemyIndicator()
    //{
    //    if (incomingIndicator.activeSelf)
    //    {
    //        incomingIndicator.SetActive(false);
    //    }
    //}
}
