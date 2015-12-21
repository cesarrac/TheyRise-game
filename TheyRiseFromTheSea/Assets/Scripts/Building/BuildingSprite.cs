using UnityEngine;

public class BuildingSprite
{
    public string name { get; protected set; }
    public Sprite sprite { get; protected set; }

    public BuildingSprite(string _name, Sprite _sprite)
    {
        name = _name;
        sprite = _sprite;
    }
}
