using UnityEngine;
using System.Collections;

public class TradeOrderGenerator : MonoBehaviour {

	public TradeOrder GenerateCorporateOrderForRaw(int difficulty, TileData.Types resourceType)
    {
        return new TradeOrder(TradeClient.Corporate, GetCompensationFromDifficulty(difficulty), resourceType, GetQuotaTotalFromDifficulty(difficulty));
    }

    public TradeOrder GenerateCorpOrderForGoods(int difficulty, Item good)
    {
        return new TradeOrder(TradeClient.Corporate, GetCompensationFromDifficulty(difficulty), good, GetQuotaTotalFromDifficulty(difficulty));
    }

    public TradeOrder GenerateCorpProBonoOrder(int difficulty, TileData.Types resourceType)
    {
        return new TradeOrder(TradeClient.Corporate, 0, resourceType, GetQuotaTotalFromDifficulty(difficulty));
    }



    public TradeOrder GenerateIndieGoodsOrder(int difficulty, Item good)
    {
        return new TradeOrder(TradeClient.Independent, GetCompensationFromDifficulty(difficulty), good, GetQuotaTotalFromDifficulty(difficulty));
    }
    public TradeOrder GenerateIndieResourceOrder(int difficulty, TileData.Types resource)
    {
        return new TradeOrder(TradeClient.Independent, GetCompensationFromDifficulty(difficulty), resource, GetQuotaTotalFromDifficulty(difficulty));
    }
    public TradeOrder GenerateIndieProBonoOrder(int difficulty, TileData.Types resourceType)
    {
        return new TradeOrder(TradeClient.Independent, 0, resourceType, GetQuotaTotalFromDifficulty(difficulty));
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

    int GetCompensationFromDifficulty(int difficulty)
    {
        if (difficulty <= 1)
        {
            // Easy
            return Random.Range(1000, 3000);
        }
        else if (difficulty == 2)
        {
            // Average
            return Random.Range(3000, 5000);
        }
        else if (difficulty == 3 || difficulty == 4)
        {
            // Medium
            return Random.Range(5000, 10000);
        }
        else
        {
            // Hard
            return Random.Range(20000, 100000);
        }
    }
}
