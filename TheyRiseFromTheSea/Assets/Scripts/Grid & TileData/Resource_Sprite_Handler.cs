using UnityEngine;
using System.Collections;
using System.Collections.Generic;


[System.Serializable]
public class RockSprite
{
    public Rock.RockSize rockSize;
    public Rock.RockType rockType;
    public Sprite[] rSprites;

    public Sprite PickASprite()
    {
        return rSprites[ResourceGrid.Grid.pseudoRandom.Next(0, rSprites.Length)];
    }
}

public class Resource_Sprite_Handler : MonoBehaviour {

	//public Sprite[] sharpSprites;
	//public Sprite[] hexSprites;
 //   public Sprite[] tubeSprites;
   

 //   // Chunk sprites
 //   public Sprite[] sharpChunks;
 //   public Sprite[] hexChunks;
 //   public Sprite[] tubeChunks;

    public RockSprite[] rockSprites;

    // Dictionaries of rock sprites, one for each type of rock
    Dictionary<Rock.RockSize, RockSprite> hexSpritesMap = new Dictionary<Rock.RockSize, RockSprite>();
    Dictionary<Rock.RockSize, RockSprite> sharpSpritesMap = new Dictionary<Rock.RockSize, RockSprite>();
    Dictionary<Rock.RockSize, RockSprite> tubeSpritesMap = new Dictionary<Rock.RockSize, RockSprite>();

    void Awake()
    {
        InitRockSpritesMap();
    }

    // Takes the data in the public component on this gameObj and splits it into Sprite dictionaries by the rock's size
    void InitRockSpritesMap()
    {
        foreach (RockSprite rsprite in rockSprites)
        {
            if (rsprite.rockType == Rock.RockType.hex)
            {
                hexSpritesMap.Add(rsprite.rockSize, rsprite);
            }
            else if (rsprite.rockType == Rock.RockType.tube)
            {
                tubeSpritesMap.Add(rsprite.rockSize, rsprite);
            }
            else if (rsprite.rockType == Rock.RockType.sharp)
            {
                sharpSpritesMap.Add(rsprite.rockSize, rsprite);
            }
        }
    }

    public Sprite GetRockSprite(Rock.RockType rockType, Rock.RockSize rockSize)
    {
        Sprite rockSprite = new Sprite();

        if (rockType == Rock.RockType.sharp)
        {
            rockSprite = sharpSpritesMap[rockSize].PickASprite();
            //switch (rockSize)
            //{
            //    case Rock.RockSize.single:
            //        rockSprite = sharpSprites[0];
            //        break;
            //    case Rock.RockSize.tiny:
            //        rockSprite = sharpSprites[1];
            //        break;
            //    case Rock.RockSize.small:
            //        rockSprite = sharpSprites[2];
            //        break;
            //    case Rock.RockSize.medium:
            //        rockSprite = sharpSprites[3];
            //        break;
            //    case Rock.RockSize.large:
            //        rockSprite = sharpSprites[4];
            //        break;
            //    case Rock.RockSize.larger:
            //        rockSprite = sharpSprites[5];
            //        break;
            //    default:
            //        rockSprite = sharpSprites[5];
            //        break;
            //}
        }
        else if (rockType == Rock.RockType.hex)
        {
            rockSprite = hexSpritesMap[rockSize].PickASprite();
            //switch (rockSize)
            //{
            //    case Rock.RockSize.single:
            //        rockSprite = hexSprites[0];
            //        break;
            //    case Rock.RockSize.tiny:
            //        rockSprite = hexSprites[1];
            //        break;
            //    case Rock.RockSize.small:
            //        rockSprite = hexSprites[2];
            //        break;
            //    case Rock.RockSize.medium:
            //        rockSprite = hexSprites[3];
            //        break;
            //    case Rock.RockSize.large:
            //        rockSprite = hexSprites[4];
            //        break;
            //    case Rock.RockSize.larger:
            //        rockSprite = hexSprites[5];
            //        break;
            //    default:
            //        rockSprite = hexSprites[5];
            //        break;
            //}
        }
        else if (rockType == Rock.RockType.tube)
        {
            rockSprite = tubeSpritesMap[rockSize].PickASprite();
            //switch (rockSize)
            //{
            //    case Rock.RockSize.single:
            //        rockSprite = tubeSprites[0];
            //        break;
            //    case Rock.RockSize.tiny:
            //        rockSprite = tubeSprites[1];
            //        break;
            //    case Rock.RockSize.small:
            //        rockSprite = tubeSprites[2];
            //        break;
            //    case Rock.RockSize.medium:
            //        rockSprite = tubeSprites[3];
            //        break;
            //    case Rock.RockSize.large:
            //        rockSprite = tubeSprites[4];
            //        break;
            //    case Rock.RockSize.larger:
            //        rockSprite = tubeSprites[5];
            //        break;
            //    default:
            //        rockSprite = tubeSprites[5];
            //        break;
            //}
        }
       

        return rockSprite;
    }


    //public Sprite GetChunkSprite(Rock.RockType rockType)
    //{
    //    Sprite chunkSprite = new Sprite();
    //    int randomSharpSelection = Random.Range(0, sharpChunks.Length);
    //    int randomHexSelection = Random.Range(0, hexChunks.Length);
    //    int randomTubeSelection = Random.Range(0, tubeChunks.Length);

    //    switch (rockType)
    //    {
    //        case Rock.RockType.sharp:            
    //            chunkSprite = sharpChunks[randomSharpSelection];
    //            break;
    //        case Rock.RockType.hex:
    //            chunkSprite = hexSprites[randomHexSelection];
    //            break;
    //        case Rock.RockType.tube:
    //            chunkSprite = tubeChunks[randomTubeSelection];
    //            break;
    //        default:
    //            chunkSprite = sharpChunks[randomSharpSelection];
    //            break;
    //    }

    //    return chunkSprite;
    //}
}
