using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System;

public class Pathfinding : MonoBehaviour
{

    ResourceGrid grid;

    PathRequestManager requestManager;

    void Awake()
    {
        grid = ResourceGrid.Grid;
        requestManager = GetComponent<PathRequestManager>();
    }



    public void StartFindPath(Vector3 startPos, Vector3 targetPos)
    {
        StartCoroutine(FindPath(startPos, targetPos));
    }

    int GetMovementPenalty(Node currNode, Node nextNode)
    {
        if (currNode.gridX != nextNode.gridX && currNode.gridY != nextNode.gridY)
            return (nextNode.moveCost + 1);
        else
            return nextNode.moveCost;
            
    }

    IEnumerator FindPath(Vector3 startPos, Vector3 targetPos)
    {
        if (grid == null)
            grid = ResourceGrid.Grid;

        //Stopwatch sw = new Stopwatch();
        //sw.Start();

        Vector3[] waypoints = new Vector3[0];
        bool pathSuccess = false;

        Node startNode = grid.NodeFromWorldPoint(startPos);
        Node targetNode = grid.NodeFromWorldPoint(targetPos);

        if (startNode.isWalkable && targetNode.isWalkable)
        {
            Heap<Node> openSet = new Heap<Node>(grid.MaxSize);
            HashSet<Node> closedSet = new HashSet<Node>();

            openSet.Add(startNode);

            while (openSet.Count > 0)
            {
                // Find the Node in the open set with the lowest F cost and remove it
                // using Heap...

                Node currentNode = openSet.RemoveFirstItem();


                // Add to the Closed set
                closedSet.Add(currentNode);

                if (currentNode == targetNode)
                {
                    //sw.Stop();
                    //print("Path found: " + sw.ElapsedMilliseconds + " ms");
                    pathSuccess = true;
                    // Found the end node

                    break;
                }

                foreach (Node neighbor in grid.GetNeighbors(currentNode))
                {
                    // If this neighbor Node is NOT walkable or it is in the CLOSED SET, skip to next neighbor
                    if (!neighbor.isWalkable || closedSet.Contains(neighbor))
                    {
                        continue;
                    }

                    // If this new path is SHORTER than last path OR neighbor is NOT in OPEN SET
                    int newMovementCostToNeighbor = currentNode.gCost + GetDistance(currentNode, neighbor) + (GetMovementPenalty(currentNode, neighbor));

                    if (newMovementCostToNeighbor < neighbor.gCost || !openSet.Contains(neighbor))
                    {
                        neighbor.gCost = newMovementCostToNeighbor;
                        // HCost = distance from this node to the target node
                        neighbor.hCost = GetDistance(neighbor, targetNode);
                        // Set this neighbor's parent so it knows what Node it just chose to path from
                        neighbor.nodeParent = currentNode;

                        if (!openSet.Contains(neighbor))
                        {
                            openSet.Add(neighbor);
                        }
                        else
                        {
                            // if it IS in the Open Set then its value has changed
                            openSet.UpdateItem(neighbor);
                        }

                    }

                }
            }
        }


        yield return null;
        if (pathSuccess)
        {
            waypoints = RetracePath(startNode, targetNode);
        }

        requestManager.FinishedProcessingPath(waypoints, pathSuccess);
    }

    Vector3[] RetracePath(Node startNode, Node endNode)
    {
        List<Node> path = new List<Node>();
        Node currNode = endNode;

        while (currNode != startNode)
        {
            path.Add(currNode);
            currNode = currNode.nodeParent;
        }

        Vector3[] waypoints = SimplifyPath(path);

        // get the path the right way around
        Array.Reverse(waypoints);

        return waypoints;

    }

    Vector3[] SimplifyPath(List<Node> path)
    {
        List<Vector3> waypoints = new List<Vector3>();
        Vector2 directionOld = Vector2.zero;

        for (int i = 1; i < path.Count; i++)
        {
            Vector2 directionNew = new Vector2(path[i - 1].gridX - path[i].gridX, path[i - 1].gridY - path[i].gridY);
            if (directionNew != directionOld)
            {
                waypoints.Add(path[i].worldPosition);

            }
            directionOld = directionNew;
        }

        return waypoints.ToArray();
    }


    int GetDistance(Node nodeA, Node nodeB)
    {
        int dstX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
        int dstY = Mathf.Abs(nodeA.gridY - nodeB.gridY);

        if (dstX > dstY)
            return 14 * dstY + 10 * (dstX - dstY);
        return 14 * dstX + 10 * (dstY - dstX);
    }
}
