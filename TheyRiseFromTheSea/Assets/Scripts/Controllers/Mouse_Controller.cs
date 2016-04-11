﻿using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class Mouse_Controller:MonoBehaviour
{
    public static Mouse_Controller Instance { get; protected set; }
  

    public Vector3 currMouseP { get; protected set; }

    public bool isRightClickingForBuilding { get; protected set; }
    public bool isRightClickingForDash { get; protected set; }

    float distance = 0;
    float maxDashThreshold = 20f;
    public float dashDistance { get; protected set; }

    public Employee_Handler selected_Employee { get; protected set; }

    public LayerMask unitSelectionMask, tileSelectionMask;

    public GameObject unitSelectionBox;
    GameObject spawned_selectBox;

    void OnEnable()
    {
        Instance = this;
        dashDistance = 0;
    }

    void Update()
    {
        UpdateMousePosition();

        if (ResourceGrid.Grid.transporter_built)
        {
            ZoomWithMouseWheel();
        }

        MouseClickInteraction();

        // FOR DEBUGGING PURPOSES:
        DebugTileUnderMouse();
    }

    void UpdateMousePosition()
    {
        Vector3 mouseP = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseP.z = 0;
        if (currMouseP != mouseP)
        {
            currMouseP = mouseP;
        }
    }

    void MouseClickInteraction()
    {
        // If the mouse is over any UI element it will stop listening for clicks
        if (EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        if (!Build_MainController.Instance.currentlyBuilding)
        {
            
            //DebugGraphicTile();

            Select();
           // DeSelectUnit();
        }

        // Build - Right Click!
        if (selected_Employee == null)
            ListenForRightClick();
    }


    void ListenForRightClick()
    {
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetMouseButtonDown(1))
        {
           // BUILDING CONTROL:

                // Holding Down Left Shift and press Right click
                isRightClickingForBuilding = true;
      
        }
        else
        {
            isRightClickingForBuilding = false;

            // DASH CONTROL:

            if (!Input.GetKey(KeyCode.LeftShift))
            {
                if (Input.GetMouseButton(1))
                {
                    // DASHING:

                    // While holding down the right mouse button calculate distance between player and mouse...

                    // ... record the distance.
                    distance = (ResourceGrid.Grid.Hero.transform.position - currMouseP).sqrMagnitude;

                }

                // When you let go of the right mouse button...
                else if (Input.GetMouseButtonUp(1))
                {
                    if (GetTileUnderMouse() == null)
                        return;

                    // .. IF the mouse is NOT over a Water tile...
                    if (GetTileUnderMouse().tileType != TileData.Types.water)
                    {
                        // ... you will move to the furthest distance calculated within the threshold.

                        // If distance is within the threshold...
                        if (distance <= maxDashThreshold)
                        {
                            // ... return distance.
                            dashDistance = distance;
                        }
                        else
                        {
                            // Else, return the maximum allowed distance.
                            dashDistance = maxDashThreshold;
                        }

                        // Set flag to allow Player to dash.
                        isRightClickingForDash = true;
                    }
              
                }
                else
                {
                    isRightClickingForDash = false;
                }
            }

        }

        // Not holding down Left Shift & not Right Clicking
    }


    void ZoomWithMouseWheel()
    {
        Camera.main.orthographicSize -= Camera.main.orthographicSize * Input.GetAxis("Mouse ScrollWheel");

        Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize, 6f, 12.5f);
    }


    public TileData GetTileUnderMouse()
    {
        //return ResourceGrid.Grid.GetTileFromWorldPos(currMouseP);
        //Vector3 trueTilePos = ResourceGrid.Grid.WorldPosToTilePos(currMouseP);
        return ResourceGrid.Grid.GetTileFromWorldPos(currMouseP);
    }



    // DEBUGGING TOOLS:
    // Tile Under Mouse Tool: print to console the tile type and position of the tile under mouse
    void DebugTileUnderMouse()
    {
        TileData tile = GetTileUnderMouse();

        if (tile == null)
            return;

        ToolTip_Manager.Instance.ViewTile(tile.posX, tile.posY, tile.tileType.ToString(), tile.isWalkable);
    }

    void DebugGraphicTile()
    {
        if (Input.GetMouseButtonDown(1))
        {
            GraphicTile gTile = TileTexture_3.instance.GetGraphicTileFromPos(currMouseP);
            if (gTile != null)
            {
                Debug.Log("GRAPHIC TILE: " + gTile.MyTileLandType + " " + gTile.MyTilePosType);
            }
            else
                Debug.Log("GRAPHIC TILE is NULL at " + (Mathf.Round(currMouseP.x)) + " " + (Mathf.Round(currMouseP.y)));
          
        }
      
    }


    void Select()
    {
        // Select Unit
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit2D hit = Physics2D.Raycast(currMouseP, Vector2.zero, 100f, unitSelectionMask.value);

            if (hit.collider != null)
            {
                GameObject unit = hit.collider.transform.root.gameObject;

                if (unit.GetComponent<Employee_Handler>() != null)
                {
                    if (selected_Employee != null)
                    {
                        DeSelectUnit();
                    }

                    selected_Employee = unit.GetComponent<Employee_Handler>();

                    // Place a selection box to mark the unit
                    spawned_selectBox = Instantiate(unitSelectionBox, hit.transform.position, Quaternion.identity) as GameObject;

                    selected_Employee.SelectEmployee();

                    Debug.Log("Employee selected. Their current work state is: " + selected_Employee.workState);

                    Sound_Manager.Instance.PlaySound("Select");
                }

            }
        }

        // Select Tile for Unit to interact with
        if (Input.GetMouseButtonDown(1))
        {
            if (selected_Employee != null)
                DeSelectUnit();

            TileData tile = GetTileUnderMouse();
            if (tile != null)
            {
                if (tile.tileType != TileData.Types.rock)
                    return;
                
                // Add a mining job
                Job_Manager.Instance.AddJob(JobType.Mine,tile.tileType, 
                    ResourceGrid.Grid.GetTileGameObjFromIntCoords(tile.posX, tile.posY).transform);

            }

   
        }

    }

    void DeSelectUnit()
    {
        selected_Employee = null;
        if (spawned_selectBox != null)
            Destroy(spawned_selectBox);
    }
  




}
