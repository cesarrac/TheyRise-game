using UnityEngine;


public class Blueprint
{
    // Type of Building: Battle or Utility
    public enum BuildingType
    {
        BATTLE,
        UTILITY
    }

    public TileData.Types tileType { get; protected set; }

    // Blueprint PU Cost that can be updated
    public int processingCost { get; protected set; }

    // Nanobot cost of this building
    public int nanoBotCost { get; protected set; }

    // Name of the Building
    public string buildingName { get; protected set; }

    public Blueprint() { }

    public Blueprint (string Name, int PUCost, int NanoBotCost, TileData.Types _Ttype)
    {
        buildingName = Name;
        processingCost = PUCost;
        nanoBotCost = NanoBotCost;
        tileType = _Ttype;
    }

    public void ChangePUCost(int change)
    {
        processingCost += change;
    }

    public void ChangeNanoCost(int change)
    {
        nanoBotCost += change;
    }

    public void ChangeName(string newName)
    {
        buildingName = newName;
    }

    
}
