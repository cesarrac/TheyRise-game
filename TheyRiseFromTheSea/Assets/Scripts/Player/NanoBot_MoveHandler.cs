using UnityEngine;
using System.Collections;

public class NanoBot_MoveHandler : MonoBehaviour {

	public Transform player;
	float speed = 10;

	public ObjectPool objPool;
	// Use this for initialization
	void Start () {
//		speed = Random.Range (3, 14);
	}
	
	// Update is called once per frame
	void Update () {
		// turn towards the player
		if (player) 
			TurnToPlayer ();

	}

	void TurnToPlayer()
	{
		float z = Mathf.Atan2 ((player.position.y - transform.position.y), (player.position.x - transform.position.x)) * Mathf.Rad2Deg - 90;		
		transform.rotation = Quaternion.AngleAxis (z, Vector3.forward);

		// then move
		MoveToPlayer ();
	}

	void MoveToPlayer()
	{
		transform.position = Vector2.MoveTowards(transform.position, player.position, speed * Time.deltaTime);
	}

	void OnTriggerEnter2D(Collider2D coll){
		if (coll.gameObject.CompareTag ("Citizen")) {
			objPool.PoolObject(this.gameObject);
		}
	}
}
