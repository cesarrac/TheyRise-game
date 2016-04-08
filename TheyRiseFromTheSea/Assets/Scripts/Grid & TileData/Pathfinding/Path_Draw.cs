using UnityEngine;
using System.Collections.Generic;

public class Path_Draw : MonoBehaviour {

    LineRenderer lr;

    void Awake()
    {
        lr = GetComponent<LineRenderer>();
        lr.enabled = false;
        gameObject.layer = 11;
    }

    public void DrawPath(Vector3[] path)
    {
        if (lr.enabled == false)
            lr.enabled = true;
        lr.SetVertexCount(path.Length);
        lr.SetPositions(path);

    }

    public void UpdatePath(Vector3[] path, int x)
    {
        if (path.Length <= 1)
            return;

        List<Vector3> drawPath = new List<Vector3>();
        for (int i = 0; i < path.Length; i++)
        {
            if (i == x)
            {
                continue;
            }
            else
                drawPath.Add(path[i]);
        }

        if (lr.enabled == false)
            lr.enabled = true;

        lr.SetVertexCount(drawPath.Count);
        lr.SetPositions(drawPath.ToArray());

    }

}
