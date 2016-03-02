using UnityEngine;
using System.Collections;

public class Resource_Sprite_Handler : MonoBehaviour {

	public Sprite[] sharpSprites;
	public Sprite[] hexSprites;
    public Sprite[] tubeSprites;
   

    // Chunk sprites
    public Sprite[] sharpChunks;
    public Sprite[] hexChunks;
    public Sprite[] tubeChunks;


    public Sprite GetRockSprite(Rock.RockType rockType, Rock.RockSize rockSize)
    {
        Sprite rockSprite = new Sprite();

        if (rockType == Rock.RockType.sharp)
        {
            switch (rockSize)
            {
                case Rock.RockSize.single:
                    rockSprite = sharpSprites[0];
                    break;
                case Rock.RockSize.tiny:
                    rockSprite = sharpSprites[1];
                    break;
                case Rock.RockSize.small:
                    rockSprite = sharpSprites[2];
                    break;
                case Rock.RockSize.medium:
                    rockSprite = sharpSprites[3];
                    break;
                case Rock.RockSize.large:
                    rockSprite = sharpSprites[4];
                    break;
                case Rock.RockSize.larger:
                    rockSprite = sharpSprites[5];
                    break;
                default:
                    rockSprite = sharpSprites[5];
                    break;
            }
        }
        else if (rockType == Rock.RockType.hex)
        {
            switch (rockSize)
            {
                case Rock.RockSize.single:
                    rockSprite = hexSprites[0];
                    break;
                case Rock.RockSize.tiny:
                    rockSprite = hexSprites[1];
                    break;
                case Rock.RockSize.small:
                    rockSprite = hexSprites[2];
                    break;
                case Rock.RockSize.medium:
                    rockSprite = hexSprites[3];
                    break;
                case Rock.RockSize.large:
                    rockSprite = hexSprites[4];
                    break;
                case Rock.RockSize.larger:
                    rockSprite = hexSprites[5];
                    break;
                default:
                    rockSprite = hexSprites[5];
                    break;
            }
        }
        else if (rockType == Rock.RockType.tube)
        {
            switch (rockSize)
            {
                case Rock.RockSize.single:
                    rockSprite = tubeSprites[0];
                    break;
                case Rock.RockSize.tiny:
                    rockSprite = tubeSprites[1];
                    break;
                case Rock.RockSize.small:
                    rockSprite = tubeSprites[2];
                    break;
                case Rock.RockSize.medium:
                    rockSprite = tubeSprites[3];
                    break;
                case Rock.RockSize.large:
                    rockSprite = tubeSprites[4];
                    break;
                case Rock.RockSize.larger:
                    rockSprite = tubeSprites[5];
                    break;
                default:
                    rockSprite = tubeSprites[5];
                    break;
            }
        }
       

        return rockSprite;
    }


    public Sprite GetChunkSprite(Rock.RockType rockType)
    {
        Sprite chunkSprite = new Sprite();
        int randomSharpSelection = Random.Range(0, sharpChunks.Length);
        int randomHexSelection = Random.Range(0, hexChunks.Length);
        int randomTubeSelection = Random.Range(0, tubeChunks.Length);

        switch (rockType)
        {
            case Rock.RockType.sharp:            
                chunkSprite = sharpChunks[randomSharpSelection];
                break;
            case Rock.RockType.hex:
                chunkSprite = hexSprites[randomHexSelection];
                break;
            case Rock.RockType.tube:
                chunkSprite = tubeChunks[randomTubeSelection];
                break;
            default:
                chunkSprite = sharpChunks[randomSharpSelection];
                break;
        }

        return chunkSprite;
    }
}
