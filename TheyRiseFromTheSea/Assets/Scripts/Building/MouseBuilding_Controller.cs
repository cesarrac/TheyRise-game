using UnityEngine;
using System.Collections;

public class MouseBuilding_Controller:MonoBehaviour
{
    public static MouseBuilding_Controller MouseController { get; protected set; }
  

    public Vector3 currMouseP { get; protected set; }

   

    void Awake()
    {
        MouseController = this;
    }

    void Update()
    {
        Vector3 mouseP = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseP.z = 0;
        if (currMouseP != mouseP)
        {
            currMouseP = mouseP;
        }

        ZoomWithMouseWheel();

        // FOR DEBUGGING PURPOSES:
        // Tile Under Mouse Tool: print to console the tile type and position of the tile under mouse
        if (!Build_MainController.Instance.currentlyBuilding)
        {
            DebugTileUnderMouse();
        }

    }

    void DebugTileUnderMouse()
    {
        if (Input.GetMouseButtonDown(1))
        {
            TileData tile = GetTileUnderMouse();
            print("Tile under mouse is of type: " + tile.tileType + " tile Position: " + tile.posX + " " + tile.posY);
            print("ACCORDING TO THE GRID: This tile points to this gameobject: " + ResourceGrid.Grid.spawnedTiles[tile.posX, tile.posY]);
        }
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

  




}
