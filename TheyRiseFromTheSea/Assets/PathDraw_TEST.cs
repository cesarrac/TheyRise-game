using UnityEngine;
using System.Collections;

public class PathDraw_TEST : MonoBehaviour {

	
    void Start()
    {
        Vector3[] path = new Vector3[10];
        for (int i = 0; i < path.Length; i++)
        {
            path[i] = new Vector3(i, i, 0);
        }

        GetComponent<Path_Draw>().DrawPath(path);
    }
}
