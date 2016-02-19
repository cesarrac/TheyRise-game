using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class TradeOrder_Manager : MonoBehaviour {

    public static TradeOrder_Manager Instance { get; protected set; }
    TradeOrderGenerator tradeGenerator;

    int maxAvailable = 10;
    int maxActive = 4;

    Dictionary<int, TradeOrder> availableOrders = new Dictionary<int, TradeOrder>();
    Dictionary<int, TradeOrder> activeOrders;
    Dictionary<int, TradeOrder> completedOrders;

    TradeOrder selectedTradeOrder;

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

        tradeGenerator = new TradeOrderGenerator();

        // InitFirstOrders();
    }



    public void InitFirstOrders()
    {


        activeOrders = new Dictionary<int, TradeOrder>();
        completedOrders = new Dictionary<int, TradeOrder>();

        // By Default generate One Corporate order and the rest indies

        availableOrders.Add(0, tradeGenerator.GenerateCorporateOrderForRaw(1, TileData.Types.rock));

        for (int i = 1; i < maxAvailable; i++)
        {
            if (Random.Range(0, 2) == 0)
            {
                // Order for Rock
                availableOrders.Add(i, tradeGenerator.GenerateIndieResourceOrder(1, TileData.Types.rock));
            }
            else
            {
                // Order for Water
                availableOrders.Add(i, tradeGenerator.GenerateIndieResourceOrder(1, TileData.Types.water));
            }
          
        }

        Debug.Log("TRADE MANAGER: First orders initialized!");
    }

    public void LoadOrders(Dictionary<int, TradeOrder> available, Dictionary<int, TradeOrder> active, Dictionary<int, TradeOrder> completed)
    {
        availableOrders = new Dictionary<int, TradeOrder>();
        availableOrders = available;
        activeOrders = new Dictionary<int, TradeOrder>();
        completedOrders = new Dictionary<int, TradeOrder>();
        activeOrders = active;
        completedOrders = completed;
        
    }

    public Dictionary<int, TradeOrder> GetAvailable()
    {
        return availableOrders;
    }
    public Dictionary<int, TradeOrder> GetActive()
    {
        return activeOrders;
    }
    public Dictionary<int, TradeOrder> GetCompleted()
    {
        return completedOrders;
    }

    public void CheckForNewTradeOrders()
    {
        if (availableOrders.Count < maxAvailable)
        {
            // Add new Trade orders
        }
    }


    public void DisplayOrders()
    {
        foreach(int id in availableOrders.Keys)
        {
            UI_Manager.Instance.AddAvailableOrder(id, availableOrders[id].orderName, availableOrders[id].timeLimit);
        }

        foreach (int id in activeOrders.Keys)
        {
            UI_Manager.Instance.AddAvailableOrder(id, activeOrders[id].orderName, activeOrders[id].timeLimit);
            UI_Manager.Instance.AddToActiveOrders(id);
        }
    }

    public void SelectTradeOrderFromUI(int id)
    {
        if (availableOrders.ContainsKey(id))
        {
            if (selectedTradeOrder == availableOrders[id])
            {
                // This means we clicked on it a second time which means we should add it to active
                AcceptTradeOrder(id);
            }
            else
            {
                selectedTradeOrder = availableOrders[id];

                // First time player clicks on Order, display its info through UI
                DisplayTradeOrderInfo();
            }
  
        }
        else if (activeOrders.ContainsKey(id))
        {
            selectedTradeOrder = activeOrders[id];
            DisplayTradeOrderInfo();
        }
    }

    void DisplayTradeOrderInfo()
    {
        if (selectedTradeOrder != null)
        {
            UI_Manager.Instance.DisplayOrderInfo(selectedTradeOrder.orderName, selectedTradeOrder.tradeClient.ToString(),
                                                   selectedTradeOrder.timeLimit.ToString(),
                                                   selectedTradeOrder.tradeResource.ToString(),
                                                   selectedTradeOrder.tradeQuota.ToString());
        }
    }

    void AcceptTradeOrder(int id)
    {
        if (!activeOrders.ContainsKey(id))
        {
            UI_Manager.Instance.AddToActiveOrders(id);

            // Add it to active orders...
            activeOrders.Add(id, availableOrders[id]);

            // ... and remove it from available orders.
            availableOrders.Remove(id);

            // Stop displaying its info in the UI
            UI_Manager.Instance.ClearOrderInfo();
        }

    }
    
}
