using UnityEngine;
using System.Collections;

public class Detect_Visible_Objects : MonoBehaviour {

    public int enemiesLayerID, resourcesLayerID;

    public int notVisibleLayerID = 15;

    void OnTriggerEnter2D(Collider2D coll)
    {
        if (coll.gameObject.CompareTag("Enemy") && coll.gameObject.layer == notVisibleLayerID) {

            // Change layer to Enemies layer
            coll.gameObject.layer = enemiesLayerID;
        }

        if (coll.gameObject.CompareTag("Rock") && coll.gameObject.layer == notVisibleLayerID)
        {

            // Change layer to Resources layer
            coll.gameObject.layer = resourcesLayerID;
        }
    }


    void OnTriggerExit2D(Collider2D coll)
    {
        if (coll.gameObject.CompareTag("Enemy"))
        {

            // Change layer to Not Visible layer
            coll.gameObject.layer = notVisibleLayerID;
        }

        /*
            Once you discover a rock, it stays rendered
        if (coll.gameObject.CompareTag("Rock"))
        {

           
            coll.gameObject.layer = notVisibleLayerID;
        }
        */
    }

}
