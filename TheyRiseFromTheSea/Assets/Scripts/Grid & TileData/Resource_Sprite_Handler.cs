using UnityEngine;
using System.Collections;

public class Resource_Sprite_Handler : MonoBehaviour {

	public Sprite[] rockSprites;
	public Sprite[] mineralSprites;

    // Chunk sprites
    public Sprite[] rockChunkSprites;
    public Sprite[] mineralChunkSprites;


    public Sprite GetRockSprite(Rock.RockType rockType, Rock.RockSize rockSize)
    {
        Sprite rockSprite = new Sprite();

        if (rockType == Rock.RockType.rock)
        {
            switch (rockSize)
            {
                case Rock.RockSize.single:
                    rockSprite = rockSprites[0];
                    break;
                case Rock.RockSize.tiny:
                    rockSprite = rockSprites[1];
                    break;
                case Rock.RockSize.small:
                    rockSprite = rockSprites[2];
                    break;
                case Rock.RockSize.medium:
                    rockSprite = rockSprites[3];
                    break;
                case Rock.RockSize.large:
                    rockSprite = rockSprites[4];
                    break;
                case Rock.RockSize.larger:
                    rockSprite = rockSprites[5];
                    break;
                default:
                    rockSprite = rockSprites[5];
                    break;
            }
        }
        else if (rockType == Rock.RockType.mineral)
        {
            switch (rockSize)
            {
                case Rock.RockSize.single:
                    rockSprite = mineralSprites[0];
                    break;
                case Rock.RockSize.tiny:
                    rockSprite = mineralSprites[1];
                    break;
                case Rock.RockSize.small:
                    rockSprite = mineralSprites[2];
                    break;
                case Rock.RockSize.medium:
                    rockSprite = mineralSprites[3];
                    break;
                case Rock.RockSize.large:
                    rockSprite = mineralSprites[4];
                    break;
                case Rock.RockSize.larger:
                    rockSprite = mineralSprites[5];
                    break;
                default:
                    rockSprite = mineralSprites[5];
                    break;
            }
        }

        return rockSprite;
    }


    public Sprite GetChunkSprite(Rock.RockType rockType)
    {
        Sprite chunkSprite = new Sprite();
        int randomChunkSelection = Random.Range(0, rockChunkSprites.Length - 1);
        int randomMineralSelection = Random.Range(0, mineralChunkSprites.Length - 1);

        switch (rockType)
        {
            case Rock.RockType.rock:            
                chunkSprite = rockChunkSprites[randomChunkSelection];
                break;
            case Rock.RockType.mineral:
                 chunkSprite = rockChunkSprites[randomMineralSelection];
                break;
            default:
                chunkSprite = rockChunkSprites[randomChunkSelection];
                break;
        }

        return chunkSprite;
    }
}
