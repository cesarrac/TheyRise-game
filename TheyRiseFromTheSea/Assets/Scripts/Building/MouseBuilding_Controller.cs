using UnityEngine;
using System.Collections;

public class MouseBuilding_Controller:MonoBehaviour
{
    public static MouseBuilding_Controller MouseController { get; protected set; }
    public static TileData.Types TileTypeUnderMouse { get; protected set; }

    Vector3 currMouseP;
    public GameObject tileUnderMouseAsGameObj { get; protected set; }
   

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
    }

    void ZoomWithMouseWheel()
    {
        Camera.main.orthographicSize -= Camera.main.orthographicSize * Input.GetAxis("Mouse ScrollWheel");

        Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize, 6f, 12.5f);
    }

    public void GetTileUnderMouse()
    {
        // Offset is only set to more than 0 in the case that we need to check tiles adjacent to mouse position
        //int x = (Mathf.FloorToInt(currMouseP.x)) + xOffset;
        //int y = (Mathf.FloorToInt(currMouseP.y)) + yOffset;
        //if (ResourceGrid.Grid.CheckIsInMapBounds(x, y))
        //{
        //    // If player right clicks we find out what tile type the mouse is over
        //    //TileTypeUnderMouse = GetTileAtMouse(x, y);
        //    //Debug.Log("Tile under mouse is: " + TileTypeUnderMouse);
        //    TileTypeUnderMouse = ResourceGrid.Grid.TileFromWorldPoint(currMouseP).tileType;
        //}
        TileTypeUnderMouse = ResourceGrid.Grid.TileFromWorldPoint(currMouseP).tileType;
    }

    //TileData.Types GetTileAtMouse(int x, int y)
    //{
    //    return ResourceGrid.Grid.GetTileType(x, y);
    //}

    public void GetTileGObj()
    {
        //int x = (Mathf.FloorToInt(currMouseP.x)) + xOffset;
        //int y = (Mathf.FloorToInt(currMouseP.y)) + yOffset;
        //if (ResourceGrid.Grid.CheckIsInMapBounds(x, y))
        //{
        //    tileUnderMouseAsGameObj = GetTileObjectAtMouse(x, y);
        //}
        tileUnderMouseAsGameObj = ResourceGrid.Grid.GetTileGameObjFromWorldPos(currMouseP);

    }
   
    public GameObject GetTileGameObj()
    {
        if (ResourceGrid.Grid.GetTileGameObjFromWorldPos(currMouseP) != null)
        {
            return ResourceGrid.Grid.GetTileGameObjFromWorldPos(currMouseP);
        }
        else
        {
            return null;
        }
        
    }




}
