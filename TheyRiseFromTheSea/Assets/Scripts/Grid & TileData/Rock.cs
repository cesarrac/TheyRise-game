using UnityEngine;
using System.Collections;

public class Rock {

	public enum RockType
    {
        rock,
        mineral
    }

    public enum RockSize
    {
        single,
        tiny,
        small,
        medium,
        large,
        larger

    }

	public enum RockProductionType
	{
		common,
		enriched
	}


    public RockSize _rockSize;
    public RockType _rockType;
	public RockProductionType _rockProductionType;

    public Rock (RockType type, RockSize size)
    {
        _rockType = type;
        _rockSize = size;
		_rockProductionType = GetProductionType(type);
    }

	RockProductionType GetProductionType(RockType type)
	{
		RockProductionType pType = RockProductionType.common;
		switch (type) 
		{
		case RockType.rock:
			pType = RockProductionType.common;
			break;
		case RockType.mineral:
			pType = RockProductionType.enriched;
			break;
		default:
			pType = RockProductionType.common;
			break;
		}
		return pType;
	}
}
