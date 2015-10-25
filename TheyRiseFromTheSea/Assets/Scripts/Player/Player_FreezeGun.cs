using UnityEngine;
using System.Collections;

public class Player_FreezeGun : Player_GunBaseClass {

	public float frozenTime = 2f; // time in seconds targetHit units stay frozen when hit

	// To find weapon when selecting available weapons
	public int wpnIndex = 1;

	public string ammoType = "ice bullet";

	void Awake () 
	{
		// Initialize gun stats
		gunStats.Init ();

		rigid_body = GetComponentInParent<Rigidbody2D> ();

	}

	void Start()
	{
		objPool = GetComponentInParent<Player_HeroAttackHandler> ().objPool;
	}

	// Check for a HIT and do EFFECT:

	void Update()
	{
		if (targetHit != null) {
			// If gun linecast has hit a targetHit, then freeze the enemy here
			FreezeEnemy();
		}

		CheckForShoot ();
		FollowMouse ();
	}


	// The GUN EFFECT:

	void FreezeEnemy()
	{
		if (targetHit.GetComponent<Enemy_MoveHandler> ()) {
			Enemy_MoveHandler enemy = targetHit.GetComponent<Enemy_MoveHandler> ();

			// freeze the enemy by changing its Move Handler's state to FROZEN
			enemy.state = Enemy_MoveHandler.State.FROZEN;

			// tell the enemy for how long it will be frozen
			enemy.frozenTime = frozenTime;

			// instantiate a visual FX from the pool
			GameObject fx = objPool.GetObjectForType("Frozen Particles", true, enemy.transform.position);


			// After freezing this enemy make targetHit null so we stop calling this method
			targetHit = null;

		} else {
			// If it couldn't find the Enemy Move Handler component it's probably because the unit is already dead
			targetHit = null;
		}
	}



}
