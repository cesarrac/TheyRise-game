using UnityEngine;
using System.Collections;

public class ClimateMap
{
    public ClimateType climateType { get; protected set; }
    public bool hasTopLayer { get; protected set; }

    public GraphicTile.TileLandTypes baseLandType { get; protected set; }
    public GraphicTile.TileLandTypes secondLandType { get; protected set; }

    public ClimateMap (ClimateType cl_type, GraphicTile.TileLandTypes baseLand, bool hasTop = false, GraphicTile.TileLandTypes secondLand = GraphicTile.TileLandTypes.ASH)
    {
        climateType = cl_type;
        hasTopLayer = hasTop;
        baseLandType = baseLand;
        if (hasTopLayer)
        {
            secondLandType = secondLand;
        }
    }

}

public enum ClimateType
{
    ARTIC,
    COLD,
    TEMPERATE,
    TROPIC,
    DESERT
}


public class Climate_Manager : MonoBehaviour {

    public static Climate_Manager Instance { get; protected set; }

    ClimateMap desertMap = new ClimateMap(ClimateType.DESERT, GraphicTile.TileLandTypes.SAND);
    ClimateMap tropicMap = new ClimateMap(ClimateType.TROPIC, GraphicTile.TileLandTypes.SAND, true, GraphicTile.TileLandTypes.MUD);
    ClimateMap temperateMap = new ClimateMap(ClimateType.TEMPERATE, GraphicTile.TileLandTypes.MUD, true, GraphicTile.TileLandTypes.SAND);
    ClimateMap coldMap = new ClimateMap(ClimateType.COLD, GraphicTile.TileLandTypes.ASH, true, GraphicTile.TileLandTypes.MUD);
    ClimateMap articMap = new ClimateMap(ClimateType.ARTIC, GraphicTile.TileLandTypes.ASH);

    public ClimateMap curClimateMap { get; protected set; }

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

    // This can be accessed by a UI button
    public void SelectClimate(string climateType)
    {
        switch (climateType)
        {
            case "Artic":
                curClimateMap = articMap;
                break;
            case "Cold":
                curClimateMap = coldMap;
                break;
            case "Temperate":
                curClimateMap = temperateMap;
                break;
            case "Tropic":
                curClimateMap = tropicMap;
                break;
            case "Desert":
                curClimateMap = desertMap;
                break;
            default:
                curClimateMap = temperateMap;
                break;
        }
    }
}
