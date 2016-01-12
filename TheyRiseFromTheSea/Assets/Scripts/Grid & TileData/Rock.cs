using UnityEngine;
using System.Collections;

public class Rock {

	public enum RockType
    { 
        hex,
        tube,
        sharp
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

	RockProductionType GetProductionType(RockType _type)
	{
        if (_type == RockType.sharp)
        {
            return RockProductionType.common;
        }
        else
        {
            return RockProductionType.enriched;
        }
	}
}
