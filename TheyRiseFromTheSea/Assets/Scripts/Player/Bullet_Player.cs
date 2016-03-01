using UnityEngine;
using System.Collections;

public class Bullet_Player : MonoBehaviour {

	/// <summary>
	/// The bullet quickly travels to its target, if it hits it will pool itself. 
	/// If it misses for some reason, it will just pool itself.
	/// </summary>
	public float bulletSpeed;
	Rigidbody2D rb;


	public Player_GunBaseClass myWeapon;

	void Awake ()
    {
		rb = GetComponent<Rigidbody2D> ();
	}

    void Update()
    {
        transform.position += transform.up * bulletSpeed * Time.deltaTime;
    }

	void OnTriggerEnter2D(Collider2D coll)
    {
		if (coll.gameObject.CompareTag("Enemy")) {

			// Give target to weapon so it can apply damage
			myWeapon.targetHit = coll.gameObject;

			ObjectPool.instance.PoolObject(gameObject);


		}
	}
}
