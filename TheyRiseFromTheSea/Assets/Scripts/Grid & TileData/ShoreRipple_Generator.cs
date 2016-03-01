using UnityEngine;
using System.Collections;

public class ShoreRipple_Generator : MonoBehaviour {

    GameObject ripple;

    Vector3 spawnPosition;

    System.Random pseudoRandom;


    void Start()
    {
        pseudoRandom = TileTexture_3.instance.pseudoRandom;
        StartCoroutine("WaitToRipple");
    }

    IEnumerator WaitToRipple()
    {
        while (true)
        {
            yield return new WaitForSeconds(2f);

            if (ResourceGrid.Grid != null)
            {
                if (ResourceGrid.Grid.worldGridInitialized)
                {
                    SpawnRipple();
                    SpawnRipple();
                }
            }
        }
    }

    void SpawnRipple()
    {
        // Get a position from empty tile positions on grid
        Vector2 posV2 = ResourceGrid.Grid.waterTilePositions[pseudoRandom.Next(0, ResourceGrid.Grid.waterTilePositions.Count)];

        if (posV2.x != spawnPosition.x && posV2.y != spawnPosition.y)
        {
            spawnPosition = new Vector3(posV2.x, posV2.y, 0);
        }

        // Spawn from pool
        ripple = ObjectPool.instance.GetObjectForType("Shore Ripples 1", true, spawnPosition);

        // Rotate it according to its surrounding tiles
        if (ripple != null)
        {
            float zRot = CheckPositions(spawnPosition);
            ripple.transform.rotation = Quaternion.Euler(0, 0, zRot);
        }
      
    }

    float CheckPositions(Vector3 position)
    {
        bool left = false, right = false, bottom = false, top = false;
        bool topLeft = false, topRight = false, botLeft = false, botRight = false;

        if (ResourceGrid.Grid.emptyTilePositions.Contains(position + Vector3.left))
        {
            left = true;
        }
        if (ResourceGrid.Grid.emptyTilePositions.Contains(position + Vector3.up))
        {
            top = true;
        }
        if (ResourceGrid.Grid.emptyTilePositions.Contains(position + Vector3.right))
        {
            right = true;
        }
        if (ResourceGrid.Grid.emptyTilePositions.Contains(position + Vector3.down))
        {
            bottom = true;
        }
        if (ResourceGrid.Grid.emptyTilePositions.Contains((position + Vector3.down) + Vector3.left))// bottom left
        {
            botLeft = true;
        }
        if (ResourceGrid.Grid.emptyTilePositions.Contains((position + Vector3.up) + Vector3.left)) // top left
        {
            topLeft = true;
        }
        if (ResourceGrid.Grid.emptyTilePositions.Contains((position + Vector3.up) + Vector3.right)) // top right
        {
            topRight = true;
        }
        if (ResourceGrid.Grid.emptyTilePositions.Contains((position + Vector3.down) + Vector3.right)) // botttom right
        {
            botRight = true;
        }

        return GetZRotation(top, right, bottom, left, topLeft, topRight, botRight, botLeft);
    }

    float GetZRotation (bool top, bool right, bool bottom, bool left, bool topLeft, bool topRight, bool bottRight, bool bottLeft)
    {
        float zRotation = 90;

        if (topLeft && !top && !topRight && !right && !bottRight && !bottom && bottLeft && left)
        {
            // Right (straight)
            return zRotation;
        }
        else if (topLeft && top && topRight && !right && !bottRight && !bottom && !bottLeft && left)
        {
            // Bottom Right diagonal
            return zRotation = -45f;
        }
        else if (topLeft && top && topRight && !right && !bottRight && !bottom && !bottLeft && !left)
        {
            // Bottom Right diagonal with no left
            return zRotation = -45f;
        }
        else if (!topLeft && top && topRight && right && !bottRight && !bottom && !bottLeft && !left)
        {
            // Bottom Left diagonal
            return zRotation = -135f;
        }
        else if (!topLeft && top && topRight && !right && !bottRight && !bottom && !bottLeft && !left)
        {
            // Bottom Left diagonal no right
            return zRotation = -135f;
        }
        else if (!topLeft && !top && !topRight && !right && bottRight && bottom && bottLeft && !left)
        {
            // Top
            return zRotation = 90f;
        }
        else if (topLeft && top && topRight && !right && !bottRight && !bottom && !bottLeft && !left)
        {
            // Bottom
            return zRotation = -90f;
        }
        else if (!topLeft && !top && !topRight && right && bottRight && bottom && !bottLeft && !left)
        {
            // Top left diagonal
            return zRotation = 135f;
        }
        else if (!topLeft && !top && !topRight && !right && bottRight && bottom && !bottLeft && !left)
        {
            // Top left diagonal no Right
            return zRotation = 135f;
        }
        else if (!topLeft && !top && topRight && right && bottRight && !bottom && !bottLeft && !left)
        {
            // Left
            return zRotation = -180f;
        }
        else if (!topLeft && !top && !topRight && !right && !bottRight && bottom && bottLeft && left)
        {
            // Top left diagonal
            return zRotation = 45f;
        }
        else if (!topLeft && !top && !topRight && !right && !bottRight && bottom && bottLeft && !left)
        {
            // Top left diagonal no left
            return zRotation = 45f;
        }
        else
        {
            zRotation = 90;
        }

        return zRotation;
    }
}
