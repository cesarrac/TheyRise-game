using UnityEngine;
using System.Collections;


public enum TradeClient
{
    Corporate,
    Independent
}

public class TradeResource
{
    public enum TradeResourceType
    {
        Raw,
        Fabricated
    }
    public TradeResourceType tradeResourceType { get; protected set; }
    public TileData.Types rawResource { get; protected set; }
    public Item fabricatedGood { get; protected set; }

    public TradeResource (TileData.Types raw)
    {
        rawResource = raw;

        tradeResourceType = TradeResourceType.Raw;

    }

    public TradeResource (Item goods)
    {
        fabricatedGood = goods;

        tradeResourceType = TradeResourceType.Fabricated;
    }
}


public class TradeOrder {

    public TradeResource tradeResource { get; protected set; }

    public int tradeQuota { get; protected set; }

    public enum TradeOrderStatus
    {
        Pending,
        Current,
        Rejected,
        Completed
    }

    public TradeOrderStatus tradeOrderStatus { get; protected set; }

    public int compensation { get; protected set; }

    public TradeClient tradeClient { get; protected set; }

    // Constructor for Raw Resources Trade Order:
    public TradeOrder(TradeClient client, int comp, TileData.Types rawResource, int ammnt)
    {
        tradeClient = client;
        compensation = comp;

        tradeResource = new TradeResource(rawResource);
        tradeQuota = ammnt;

        tradeOrderStatus = TradeOrderStatus.Pending;
    }

    // Constructor for Fabricated Goods Trade Order:
    public TradeOrder(TradeClient client, int comp, Item fabGoods, int ammnt)
    {
        tradeClient = client;
        compensation = comp;

        tradeResource = new TradeResource(fabGoods);
        tradeQuota = ammnt;

        tradeOrderStatus = TradeOrderStatus.Pending;
    }

    public void AcceptOrder()
    {
        if (tradeOrderStatus == TradeOrderStatus.Pending)
        {
            tradeOrderStatus = TradeOrderStatus.Current;
        }
    }

    public void RejectOrder()
    {
        if (tradeOrderStatus == TradeOrderStatus.Pending || tradeOrderStatus == TradeOrderStatus.Current)
        {
            tradeOrderStatus = TradeOrderStatus.Rejected;
        }
    }

    public void CompleteOrder()
    {
        if (tradeOrderStatus == TradeOrderStatus.Current)
        {
            tradeOrderStatus = TradeOrderStatus.Completed;
        }
    }
}
