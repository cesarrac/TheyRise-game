using UnityEngine;
using System.Collections;

public class Player_AssaultRifle : Player_GunBaseClass {

	public float explosiveDamage = 2f; 



	void Awake () 
	{
		// Initialize gun stats
		gunStats.Init ();
		
		rigid_body = GetComponentInParent<Rigidbody2D> ();

        source = GetComponentInParent<AudioSource>();

        gameMaster = GetComponentInParent<Player_HeroAttackHandler>().gameMaster;

        sprite_renderer = GetComponent<SpriteRenderer>();
       

    }

    void Start()
	{
        objPool = ObjectPool.instance;

		if (!gameMaster)
			gameMaster = GameObject.FindGameObjectWithTag ("GM").GetComponent<GameMaster> ();
	}
	
	// Check for a HIT:
	
	void Update()
	{
		if (targetHit != null) {

            // Tell the enemy unit I'm hitting it
            if (targetHit.GetComponent<Enemy_AttackHandler>().attacker == null)
            {

                targetHit.GetComponent<Enemy_AttackHandler>().attacker = gameObject;
            }
			// Do Gun Effect
			DamageEnemy();

		}
//		
//
		CheckForShoot ();
	

		FollowMouse ();
	}

	

	
	// The GUN EFFECT:
	void DamageEnemy()
	{
		Debug.Log ("GOT AN ENEMY TO DAMAGE");
		if (targetHit.GetComponent<Unit_Base> ()) {
			Debug.Log("ENEMY UNIT BASE SCRIPT FOUND!");
			Unit_Base enemy = targetHit.GetComponent<Unit_Base> ();

			if (enemy.stats.curHP > 0){
	
				// instantiate a visual FX from the pool
				GameObject fx = objPool.GetObjectForType("MachineGun_ShootFX", true, enemy.transform.position);


				/* This guns explodes a chunk or part off of the enemy unit IF the unit only has a 4th of their HP left */

				// Get the ammount that is a quarter of this enemy's HP
				float quarterHP = enemy.stats.maxHP * 0.25f;

				if (enemy.stats.curHP <= quarterHP){
					//TODO: Blow up a chunk instead of just normal damage
					enemy.TakeDamage(explosiveDamage);

				}else{
					// just do damage
					enemy.TakeDamage(explosiveDamage);
					Debug.Log ("DAMAGING ENEMY!");
				}


				targetHit = null;
			}else{
				Debug.Log ("ENEMY SHOULD BE DEAD!");
				targetHit = null;
			}

		} else {
			// If it couldn't find the Enemy Move Handler component it's probably because the unit is already dead
			targetHit = null;

		}
	}
}
