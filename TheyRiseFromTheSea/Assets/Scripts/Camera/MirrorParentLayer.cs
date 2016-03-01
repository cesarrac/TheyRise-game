using UnityEngine;
using System.Collections;

public class MirrorParentLayer : MonoBehaviour {

    int parentLayer;
    int myLayer;

    void OnEnable()
    {
        myLayer = gameObject.layer;

        StartCoroutine("MirrorLayer");
    }

    IEnumerator MirrorLayer()
    {
       

        while (true)
        {
            parentLayer = transform.parent.gameObject.layer;

            if (myLayer != parentLayer)
            {
                myLayer = parentLayer;
                gameObject.layer = myLayer;
            }

            yield return null;
        }
     
    }
}
