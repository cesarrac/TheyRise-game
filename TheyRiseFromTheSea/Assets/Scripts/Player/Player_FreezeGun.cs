using UnityEngine;
using System.Collections;

public class Player_FreezeGun : Player_GunBaseClass {

	public float frozenTime = 2f; // time in seconds targetHit units stay frozen when hit


	void Awake () 
	{


        sprite_renderer = GetComponent<SpriteRenderer>();

    }

	void Start()
	{
        // Initialize gun stats
        gunStats.Init();

        rigid_body = GetComponentInParent<Rigidbody2D>();

        gameMaster = GameMaster.Instance;

        objPool = ObjectPool.instance;

        // Get sights
        sightStart = GetComponentInParent<Player_HeroAttackHandler>().sightStart;
        sightEnd = GetComponentInParent<Player_HeroAttackHandler>().sightEnd;

        // And Status Indicator to display text over head when reloading
        status_Indicator = GetComponentInParent<Player_HeroAttackHandler>().statusIndicator;
    }

	// Check for a HIT and do EFFECT:

	void Update()
	{
        // Target hit is the gameobject of the enemy that was actually hit (collided) by a projectile
        if (targetHit != null)
        {

            // Tell the enemy unit I'm hitting it
            if (targetHit.GetComponent<Enemy_AttackHandler>().playerUnit == null)
            {

                targetHit.GetComponent<Enemy_AttackHandler>().playerUnit = transform.parent.gameObject;
            }
            // Do Gun Effect
            FreezeEnemy();

        }

        CheckForShoot ();
		FollowMouse ();
	}


	// The GUN EFFECT:

	void FreezeEnemy()
	{
        // Shoot a bubble with a circle collider right on the enemy's location
        GameObject iceBubble = objPool.GetObjectForType("Ice bubble", true, targetHit.transform.position);

        if (iceBubble != null)
        {
            Debug.Log("FREEZE GUN is Freezing an enemy!");
            iceBubble.GetComponent<EasyPool>().timeBeforePool = frozenTime;
            // instantiate a visual FX from the pool
            GameObject fx = objPool.GetObjectForType("Frozen Particles", true, targetHit.transform.position);

            // Freeze the enemy using its path handler
            if (targetHit.GetComponent<Enemy_PathHandler>() != null)
            {
                // Change the enemy's speed, it will automatically reset itself once the frozen time is done
                targetHit.GetComponent<Enemy_PathHandler>().ChangeSpeed(0, frozenTime);
            }

            // After freezing this enemy make targetHit null so we stop calling this method
            targetHit = null;
        }
        else
        {
            // If it couldn't find the Enemy Move Handler component it's probably because the unit is already dead
            targetHit = null;
        }

	}



}
