using UnityEngine;
using System.Collections;

class AnimatedTileTexture : MonoBehaviour
{
    public int columns = 2;
    public int rows = 2;
    public float framesPerSecond = 10f;

    //the current frame to display
    private int index = 0;

    public int tileSize;

    bool reverseIndex;

    void Start()
    {
        StartCoroutine(updateTiling());

        ////set the tile size of the texture (in UV units), based on the rows and columns
        ////Vector2 size = new Vector2(tileSize, tileSize);
        //Vector2 size = new Vector2(tileSize / columns, tileSize / rows);
        ////Vector2 size = new Vector2(rows, columns);

        //GetComponent<Renderer>().sharedMaterial.SetTextureScale("_MainTex", size);
    }

    private IEnumerator updateTiling()
    {
        while (true)
        {
            //move to the next index
           // index++;
            //if (index >= rows * columns)
              //  index = 0;

            if (!reverseIndex)
            {
                index++;
            }
            if (index >= rows * columns || reverseIndex)
            {
                reverseIndex = true;
                index--;
                if (index <= 0)
                {
                    reverseIndex = false;
                }
            }
            

            //split into x and y indexes
            Vector2 offset = new Vector2((float)index / columns - (index / columns), //x index
                              (index / columns) / (float)rows);          //y index
          

            GetComponent<Renderer>().sharedMaterial.SetTextureOffset("_MainTex", offset);

            yield return new WaitForSeconds(1f / framesPerSecond);
        }

    }
}