using UnityEngine;
using System.Collections;

public class Rock_Handler : MonoBehaviour {

    public Rock myRock;

    SpriteRenderer sprite_renderer;

    public Resource_Sprite_Handler res_sprite_handler;

    public Rock.RockType myRockType;


    void Awake()
    {
        sprite_renderer = GetComponent<SpriteRenderer>();
    }
	
    public void InitRock(Rock.RockType _type, Rock.RockSize _size)
    {
        myRock = new Rock(_type, _size);
        myRockType = myRock._rockType;
    } 

    public void ShrinkDownSize()
    {
        switch (myRock._rockSize)
        {
            case Rock.RockSize.single:
                // Cant shrink a single rock!
                break;
            case Rock.RockSize.tiny:
                sprite_renderer.sprite = res_sprite_handler.GetRockSprite(myRock._rockType, Rock.RockSize.single);
                myRock._rockSize = Rock.RockSize.single;
                break;
            case Rock.RockSize.small:
                sprite_renderer.sprite = res_sprite_handler.GetRockSprite(myRock._rockType, Rock.RockSize.tiny);
                myRock._rockSize = Rock.RockSize.tiny;
                break;
            case Rock.RockSize.medium:
                sprite_renderer.sprite = res_sprite_handler.GetRockSprite(myRock._rockType, Rock.RockSize.small);
                myRock._rockSize = Rock.RockSize.small;
                break;
            case Rock.RockSize.large:
                sprite_renderer.sprite = res_sprite_handler.GetRockSprite(myRock._rockType, Rock.RockSize.medium);
                myRock._rockSize = Rock.RockSize.medium;
                break;
            case Rock.RockSize.larger:
                sprite_renderer.sprite = res_sprite_handler.GetRockSprite(myRock._rockType, Rock.RockSize.large);
                myRock._rockSize = Rock.RockSize.large;
                break;
            default:
                sprite_renderer.sprite = res_sprite_handler.GetRockSprite(myRock._rockType, Rock.RockSize.single);
                myRock._rockSize = Rock.RockSize.single;
                break;
        }

        myRockType = myRock._rockType;
    }
}
