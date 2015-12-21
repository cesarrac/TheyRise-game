using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class Fauna_MoveHandler : MonoBehaviour {

    //public ResourceGrid resourceGrid;

    //public int posX;
    //public int posY;

    //// This stores this unit's path
    //public List<Node> currentPath = null;

    //public float moveSpeed = 1f;

    //void Awake()
    //{
    //    // Store initial position for Grid as an int
    //   // posX = (int)transform.position.x;
    //    //posY = (int)transform.position.y;
    //}


    //void Update()
    //{
    //    if (currentPath != null)
    //    {
    //        ActualMove();
    //        DrawLine();
    //    }
    //}

    //void DrawLine()
    //{
    //    // Debug Draw line for visual reference
    //    if (currentPath != null)
    //    {
    //        int currNode = 0;
    //        while (currNode < currentPath.Count - 1)
    //        {
    //            Vector3 start = resourceGrid.TileCoordToWorldCoord(currentPath[currNode].x, currentPath[currNode].y);
    //            Vector3 end = resourceGrid.TileCoordToWorldCoord(currentPath[currNode + 1].x, currentPath[currNode + 1].y);
    //            ;
    //            Debug.DrawLine(start, end, Color.blue);
    //            currNode++;
    //        }
    //    }
    //}


    //// Physically moves the unit through world space
    //void ActualMove()
    //{
    //    // Movement:

    //    // Have we moved close enough to the target tile that we can move to next tile in current path?
    //    if (Vector2.Distance(transform.position, resourceGrid.TileCoordToWorldCoord(posX, posY)) < (0.1f))
    //    {
    //        MoveToNextTile();
    //    }
    //    transform.position = Vector2.MoveTowards(transform.position,
    //                                             resourceGrid.TileCoordToWorldCoord(posX, posY),
    //                                             moveSpeed * Time.deltaTime);
    //    /*
    //    // ANIMATION CONTROLS:
    //    if (posX > transform.position.x)
    //    {
    //        anim.SetTrigger("movingRight");
    //        anim.ResetTrigger("movingLeft");
    //    }
    //    else if (posX < transform.position.x)
    //    {
    //        anim.SetTrigger("movingLeft");
    //        anim.ResetTrigger("movingRight");
    //    }
    //    */

    //}

    //// Move through Path:
    //void MoveToNextTile()
    //{
    //    if (currentPath == null)
    //    {
    //        return;
    //    }

    //    // Remove the old first node from the path
    //    if (currentPath[0] != null)
    //     currentPath.RemoveAt(0);

    //    if (currentPath.Count > 1)
    //    {
           
    //        // Check if the next tile is a UNWAKABLE tile OR if it is clear path
    //        if (resourceGrid.UnitCanEnterTile(currentPath[1].x, currentPath[1].y) == false)
    //        {
    //            currentPath = null;
    //        }
    //        else
    //        {
    //            if (currentPath.Count > 1)
    //            {
    //                // Move to the next Node position in path
    //                posX = currentPath[1].x;
    //                posY = currentPath[1].y;
    //            }
    //            else if (currentPath.Count == 1)
    //            {
    //                currentPath = null;
    //            }

    //        }
    //    }
    //    else if (currentPath.Count == 1)
    //    {
    //        // Move to the next Node position in path
    //        posX = currentPath[0].x;
    //        posY = currentPath[0].y;
    //        currentPath = null;
    //    }

        
    //}

}
