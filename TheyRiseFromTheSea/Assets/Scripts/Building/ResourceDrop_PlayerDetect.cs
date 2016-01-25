using UnityEngine;
using System.Collections;

public class ResourceDrop_PlayerDetect : MonoBehaviour {

	public bool isPlayerDetected { get; protected set; }
    public Vector2 playerPos { get; protected set; }

    // Once the Trigger is set to true, the Resource Drop component will take care of moving it.
    void OnTriggerEnter2D(Collider2D coll)
    {
        if (coll.gameObject.CompareTag("Citizen"))
        {
            isPlayerDetected = true;
            playerPos = coll.transform.position;
        }
    }
}
