using UnityEngine;

class BuildingSprite_Manager : MonoBehaviour
{
    // All this does is hold the sprites for ONLY the blueprints the player has access to when Loading a new Level

    // RUN THIS SCRIPT WHEN LOADING A NEW MAP LEVEL!

    public BuildingSprite[] buildingSprites;

    public static BuildingSprite_Manager Instance { get; protected set; }

    void OnEnable()
    {
        Instance = this;
    }

    public Sprite GetSprite(string name)
    {
        for (int i = 0; i < buildingSprites.Length; i++)
        {
            if (buildingSprites[i].name == name)
            {
                return buildingSprites[i].sprite;
            }

        }
        return new Sprite();
    }


}
