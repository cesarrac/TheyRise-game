using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;


public class GameTracker : MonoBehaviour {

    public static GameTracker Instance { get; protected set; }

    int days = 1;
    public int Days { get { return days; } set { days = Mathf.Clamp(value, 1, 10000); } }

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
    }

    public Scene GetScene()
    {
        return SceneManager.GetActiveScene();
    }

    public void AddDay(int multiple = 0)
    {
        if (multiple > 0)
        {
            Days = multiple;
        }
        else
            Days += 1;

        // Check if new missions are available
        if (TradeOrder_Manager.Instance != null)
        {
            TradeOrder_Manager.Instance.CheckDay(Days);
        }
    }

    public void Save()
    {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(Application.persistentDataPath + "playerInfo.dat");


        GameData gameData = new GameData();

        gameData.SaveResources(Ship_Inventory.Instance.rawResourcesMap);
        gameData.SaveDays(Days);
        gameData.SaveHero(GameMaster.Instance.theHero.weapons[0].itemName,
                          GameMaster.Instance.theHero.weapons[1].itemName,
                          GameMaster.Instance.theHero.armor.itemName,
                          GameMaster.Instance.theHero.tools[0].itemName,
                          new HeroData(GameMaster.Instance.theHero.heroStats.maxHP,
                                        GameMaster.Instance.theHero.heroStats.curHP,
                                        GameMaster.Instance.theHero.heroStats.startAttack));

        gameData.SaveOrders(TradeOrder_Manager.Instance.GetAvailable(), TradeOrder_Manager.Instance.GetActive(), TradeOrder_Manager.Instance.GetCompleted());

        gameData.SaveNanoBuilder(GameMaster.Instance.theHero.nanoBuilder);

        bf.Serialize(file, gameData);

        file.Close();

        Debug.Log("TRACKER: Data Saved Succesfully!");
    }

    public void Load()
    {
        if (File.Exists(Application.persistentDataPath + "playerInfo.dat"))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath + "playerInfo.dat", FileMode.Open);


            GameData gameData = (GameData)bf.Deserialize(file);

            file.Close();

            // Load the Resources map
            foreach (TileData.Types resource in gameData.savedResourceMap.Keys)
            {
                Ship_Inventory.Instance.StoreItems(resource, gameData.savedResourceMap[resource]);
            }

            // Load the Days
            AddDay(gameData.days);

            // Load the Hero
            GameMaster.Instance.CreateHero(gameData.weaponOne_ID, gameData.weaponTwo_ID, gameData.tool_ID, gameData.armor_ID, 
                                            gameData.savedHeroData.maxHP, gameData.savedHeroData.curHP, gameData.savedHeroData.Attack, gameData.savedNanoBuilder);

            // Load the Trade Orders
            TradeOrder_Manager.Instance.LoadOrders(gameData.availableOrders, gameData.activeOrders, gameData.completedOrders);

            // If all was succesful we should load the Ship scene
            GameMaster.Instance.RestartToShip();

            Debug.Log("TRACKER: Data Loaded Succesfully!");
        }
    }
}

[Serializable]
public class GameData
{
    public Dictionary<TileData.Types, int> savedResourceMap { get; protected set; }
    public int days { get; protected set; }

    // Hero:
        // stats:
    public HeroData savedHeroData { get; protected set; }
        // equipment:
    public string weaponOne_ID { get; protected set; }
    public string weaponTwo_ID { get; protected set; }
    public string armor_ID { get; protected set; }
    public string tool_ID { get; protected set; }

    // Trade Orders:
    public Dictionary<int, TradeOrder> availableOrders { get; protected set; }
    public Dictionary<int, TradeOrder> activeOrders { get; protected set; }
    public Dictionary<int, TradeOrder> completedOrders { get; protected set; }

    // Nanobuilder:
    public NanoBuilder savedNanoBuilder { get; protected set; }

    public GameData()
    {
        savedResourceMap = new Dictionary<TileData.Types, int>();
    }

    public void SaveResources(Dictionary<TileData.Types, int> storedResources)
    {
        foreach(TileData.Types resource in storedResources.Keys)
        {
            savedResourceMap.Add(resource, storedResources[resource]);
        }
    }

    public void SaveDays(int d)
    {
        days = d;
    }

    public void SaveHero(string wpn1, string wpn2, string armor, string tool, HeroData heroData)
    {
        weaponOne_ID = wpn1;
        weaponTwo_ID = wpn2;
        armor_ID = armor;
        tool_ID = tool;

        savedHeroData = heroData;
    }

    public void SaveOrders(Dictionary<int, TradeOrder> available, Dictionary<int, TradeOrder> active, Dictionary<int, TradeOrder> completed)
    {
        availableOrders = new Dictionary<int, TradeOrder>();
        activeOrders = new Dictionary<int, TradeOrder>();
        completedOrders = new Dictionary<int, TradeOrder>();

        availableOrders = available;
        activeOrders = active;
        completedOrders = completed;
    }

    public void SaveNanoBuilder(NanoBuilder builder)
    {
        savedNanoBuilder = builder;
    }
}
