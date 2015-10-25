using UnityEngine;
using System.Collections;

public class Bullet_FastMoveHandler : MonoBehaviour {

	/// <summary>
	/// The bullet quickly travels to its target, if it hits it will pool itself. 
	/// If it misses for some reason, it will just pool itself.
	/// </summary>
	public float bulletSpeed;
	Rigidbody2D rb;
	public ObjectPool objPool;

	public float timeToDie;
	float startTime;

	public Player_GunBaseClass myWeapon;

	void Awake () {
		startTime = Time.time;

		rb = GetComponent<Rigidbody2D> ();

		if (!objPool)
			objPool = GameObject.FindGameObjectWithTag ("Pool").GetComponent<ObjectPool> ();


	}

	void Start(){

	}

	void OnEnable()
	{
		startTime = Time.time;

	}
	void Update(){

		if (Time.time - startTime > timeToDie) {
			objPool.PoolObject (gameObject);
		} else {
			transform.position += transform.up * bulletSpeed * Time.deltaTime;

		}
	}

	void LateUpdate()
	{
		// turn on sprite
//		sprite_renderer.color = Color.white;

	}

	void OnTriggerEnter2D(Collider2D coll){
		if (coll.gameObject.CompareTag("Enemy")) {

			// Give target to weapon so it can apply damage
			myWeapon.targetHit = coll.gameObject;

			objPool.PoolObject(gameObject);


		}
	}
}
