using UnityEngine;
using System.Collections;

public class MouseBuilding_Controller:MonoBehaviour
{
    public static MouseBuilding_Controller MouseController { get; protected set; }
  

    public Vector3 currMouseP { get; protected set; }

    public bool isRightClickingForBuilding { get; protected set; }
    public bool isRightClickingForDash { get; protected set; }

    float distance = 0;
    float maxDashThreshold = 20f;
    public float dashDistance { get; protected set; }

    void OnEnable()
    {
        MouseController = this;
        dashDistance = 0;
    }

    void Update()
    {
        Vector3 mouseP = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseP.z = 0;
        if (currMouseP != mouseP)
        {
            currMouseP = mouseP;
        }

        if (ResourceGrid.Grid.transporter_built)
        {
            ZoomWithMouseWheel();
        }


        // FOR DEBUGGING PURPOSES:
        // Tile Under Mouse Tool: print to console the tile type and position of the tile under mouse
        if (!Build_MainController.Instance.currentlyBuilding)
        {
            DebugTileUnderMouse();
            DebugGraphicTile();
        }


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
                    // While holding down the right mouse button calculate distance between player and mouse within a distance threshold.
                    distance = (ResourceGrid.Grid.Hero.transform.position - currMouseP).sqrMagnitude;


                }
                else if (Input.GetMouseButtonUp(1))
                {
                    // When you let go of the right mouse button you will move to the furthest distance calculated within the threshold.
                    if (distance <= maxDashThreshold)
                    {
                        dashDistance = distance;
                    }
                    else
                    {
                        dashDistance = maxDashThreshold;
                    }

                    // Holding down Right click without holding Left Shift
                    isRightClickingForDash = true;
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
        return ResourceGrid.Grid.TileFromWorldPoint(currMouseP);
    }



    // DEBUGGING TOOLS:

    void DebugTileUnderMouse()
    {
        if (Input.GetMouseButtonDown(1))
        {
            TileData tile = GetTileUnderMouse();
            print("Tile under mouse is of type: " + tile.tileType + " tile Position: " + tile.posX + " " + tile.posY);
            print("ACCORDING TO THE GRID: This tile points to this gameobject: " + ResourceGrid.Grid.spawnedTiles[tile.posX, tile.posY]);
        }
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



  




}
