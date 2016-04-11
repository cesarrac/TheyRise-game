using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ToolTip_Manager : MonoBehaviour {

    public static ToolTip_Manager Instance;

    public Text xCoord, yCoord;
    public Text tileTypeDescription;

    void Awake()
    {
        Instance = this;
    }

    public void ViewTile(int x, int y, string tileType)
    {
        xCoord.text = x.ToString();
        yCoord.text = y.ToString();
        tileTypeDescription.text = tileType;
    }
}
