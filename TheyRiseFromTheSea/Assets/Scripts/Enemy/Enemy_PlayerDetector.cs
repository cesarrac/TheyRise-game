using UnityEngine;
using System.Collections;

public class Enemy_PlayerDetector : MonoBehaviour {

	Enemy_MoveHandler move_handler;

	void Awake()
	{
		move_handler = GetComponentInParent<Enemy_MoveHandler> ();
	}

	void OnTriggerEnter2D(Collider2D coll)
	{
		if (coll.gameObject.CompareTag ("Citizen")) {
			//move_handler.targetPlayer = coll.gameObject;
		}
	}
}
