using UnityEngine;
using System.Collections.Generic;

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

    public ClimateMap()
    {

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

    List<ClimateMap> climateMaps = new List<ClimateMap>();

    public ClimateMap curClimateMap { get; protected set; }

    public string climateMapID { get; protected set; }

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

        InitClimates();

       

    }


    void InitClimates()
    {
        climateMaps = new List<ClimateMap>()
        { // 0 = Desert, 1 = Tropic, 2 = Temperate, 3 = Cold, 4 = Artic
            new ClimateMap(ClimateType.DESERT, GraphicTile.TileLandTypes.SAND),
            new ClimateMap(ClimateType.TROPIC, GraphicTile.TileLandTypes.SAND, true, GraphicTile.TileLandTypes.MUD),
            new ClimateMap(ClimateType.TEMPERATE, GraphicTile.TileLandTypes.MUD, true, GraphicTile.TileLandTypes.SAND),
            new ClimateMap(ClimateType.COLD, GraphicTile.TileLandTypes.ASH, true, GraphicTile.TileLandTypes.MUD),
            new ClimateMap(ClimateType.ARTIC, GraphicTile.TileLandTypes.ASH),
        };

        // Default:
        curClimateMap = climateMaps[2];

        Debug.Log("CLIMATE: Climate maps initialized!");
    }

    // This can be accessed by a UI button
    public void SelectClimate(string climateType)
    {
        if (climateMaps == null)
        {
            InitClimates();
        }

        switch (climateType)
        {
            case "Artic":
                curClimateMap = climateMaps[4];
                break;
            case "Cold":
                curClimateMap = climateMaps[3];
                break;
            case "Temperate":
                curClimateMap = climateMaps[2];
                break;
            case "Tropic":
                curClimateMap = climateMaps[1];
                break;
            case "Desert":
                curClimateMap = climateMaps[0];
                break;
            case "Unknown":
                curClimateMap = SelectRandomClimate();
                break;
            default:
                curClimateMap = climateMaps[2];
                break;
        }

        climateMapID = climateType;
    }

    ClimateMap SelectRandomClimate()
    {
        return climateMaps[Random.Range(0, climateMaps.Count)];
    }
}
