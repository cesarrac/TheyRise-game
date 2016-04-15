using UnityEngine;
using System.Collections;

[System.Serializable]
public enum ResourceType
{
    Steel,
    Wood,
    Water,
    Food,
    Vit,
    Energy,
    Empty
}

/// <summary>
///                         Raw Resource:       Represents the Resources gathered from
///                                             Extracting from a tile.
/// </summary>
//public class RawResource  {

//    ResourceType rType;
//    public ResourceType ResourceType { get { return rType;  } }
//    public RawResource(ResourceType _rType)
//    {
//        rType = _rType;
//    }
//}
