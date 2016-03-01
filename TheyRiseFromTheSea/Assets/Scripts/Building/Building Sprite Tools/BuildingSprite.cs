using UnityEngine;

public class BuildingSprite
{
    public string name { get; protected set; }
    public Sprite sprite { get; protected set; }
    public TileData.Types tileType { get; protected set; }

    public BuildingSprite(string _name, Sprite _sprite, TileData.Types _type)
    {
        name = _name;
        sprite = _sprite;
        tileType = _type;
    }
}
