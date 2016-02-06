using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Equipment_SpriteDatabase : MonoBehaviour {

    public static Equipment_SpriteDatabase Instance { get; protected set; }

    public Dictionary<string, Sprite> equipmentMap = new Dictionary<string, Sprite>();
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
        equipmentMap.Add("Assault", allSprites[0]);
        equipmentMap.Add("Shotgun", allSprites[1]);
        equipmentMap.Add("Thunder", allSprites[2]);
    }

    public Sprite GetSprite(string name)
    {
        if (equipmentMap.ContainsKey(name))
        {
            return equipmentMap[name];
        }

        Debug.Log("Equipment DB: Could not find a sprite for: " + name);

        return new Sprite();
    }

}

