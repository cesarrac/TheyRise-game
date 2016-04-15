using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Equipment_SpriteDatabase : MonoBehaviour {

    public static Equipment_SpriteDatabase Instance { get; protected set; }

    public Dictionary<string, Sprite> itemSpritesMap = new Dictionary<string, Sprite>();
    Sprite[] allSprites;

    void OnEnable()
    {
        Instance = this;
    }

    void Awake()
    {
        InitSpritesMap();
    }

    void InitSpritesMap()
    {

        allSprites = Resources.LoadAll<Sprite>("Sprites/PlayerWeapons");


        // Here we would have to manually name and map each sprite to the equipment map
        itemSpritesMap.Add("Kinetic Rifle", allSprites[0]);
        itemSpritesMap.Add("Mining Drill", allSprites[1]);
        itemSpritesMap.Add("Freeze Gun", allSprites[2]);
    }

    public Sprite GetSprite(string name)
    {
        if (itemSpritesMap.ContainsKey(name))
        {
            return itemSpritesMap[name];
        }

        Debug.Log("Equipment DB: Could not find a sprite for: " + name);

        return new Sprite();
    }

}

