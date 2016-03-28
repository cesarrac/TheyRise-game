using UnityEngine;
using System.Collections;

public class MouseTest : MonoBehaviour {

    void OnMouseDown()
    {
        Debug.Log("Left mouse clicked once!");
    }

    void OnMouseDrag()
    {
        Debug.Log("Mouse is being held down!");
    }
}
