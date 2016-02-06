using UnityEngine;
using System.Collections;

public class Player_AssaultRifle : Player_GunBaseClass {

	float explosiveDamage = 2f; 



	void Awake () 
	{
		
        //sprite_renderer = GetComponent<SpriteRenderer>();

    }

    void Start()
	{

        Debug.Log("Gun Stats starting stats: rate " + gunStats.startingFireRate + " ammo " + gunStats.startingChamberAmmo + " r spd " + gunStats.startingReloadSpeed);
    
        // Initialize gun stats
        gunStats.Init();

        Debug.Log("Gun Stats: rate " + gunStats.curFireRate + " ammo " + gunStats.curChamberAmmo + " r spd " + gunStats.curReloadSpeed);

        objPool = ObjectPool.instance;

        rigid_body = GetComponentInParent<Rigidbody2D>();

        // Get sights
        sightStart = GetComponentInParent<Player_HeroAttackHandler>().sightStart;
        sightEnd = GetComponentInParent<Player_HeroAttackHandler>().sightEnd;

        // And Status Indicator to display text over head when reloading
        status_Indicator = GetComponentInParent<Player_HeroAttackHandler>().statusIndicator;

        gameMaster = GameMaster.Instance;

        explosiveDamage = gunStats.damage;
	}
	
	// Check for a HIT:
	
	void Update()
	{
        // Target hit is the gameobject of the enemy that was actually hit (collided) by a projectile
		if (targetHit != null) {

            // Tell the enemy unit I'm hitting it
            if (targetHit.GetComponent<Enemy_AttackHandler>().playerUnit == null)
            {

                targetHit.GetComponent<Enemy_AttackHandler>().playerUnit = transform.parent.gameObject;
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
					//Debug.Log ("DAMAGING ENEMY!");
				}


				targetHit = null;
			}else{
				//Debug.Log ("ENEMY SHOULD BE DEAD!");
				targetHit = null;
			}

		} else {
			// If it couldn't find the Enemy Move Handler component it's probably because the unit is already dead
			targetHit = null;

		}
	}
}
