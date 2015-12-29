using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Enemy_AttackHandler : Unit_Base {

    Enemy_PathHandler pathHandler;


    //	public int targetTilePosX, targetTilePosY;

    //	public GameObject playerUnit;

    ////	public bool canAttack;

    //	public bool startCounter = true;

    //	// KAMIKAZE:
    //	public bool isKamikaze;
    //	private float countDownToPool = 1f;

    [Header ("If ON, this unit will always attack buildings nearby")]
    public bool aggroOnBuildings;

    public enum State { MOVING, ATTACK_TILE, ATTACK_UNIT, POOLING_TARGET };

    private State _state = State.MOVING;

    //	[HideInInspector]
    //	public State state { get { return _state; } set { _state = value; } }

    //	public State debugState;


    private float attackCountDown;

    [Header("Camera Shake Ammount:")]
    public float canShakeAmmount;
    public CameraShake _camShake;

    //	private bool isPlayerAttacker;

    public GameObject attacker; // < ----- public to set from other scripts
    GameObject unitAttackingMe; // < ----- private that will be set to attacker 
   

    void Awake()
    {
        pathHandler = GetComponent<Enemy_PathHandler>();
        audio_source = GetComponent<AudioSource>();
    }

    void Start()
    {

        //		// Get the value of isKamikaze set by the public bool in Move Handler
        //		isKamikaze = moveHandler.isKamikaze;

        //// Get isPlayerAttacker from move Handler as well
        //isPlayerAttacker = moveHandler.isPlayerAttacker;

        // Initialize Unit stats
        stats.Init();
        Debug.Log("ENEMY stats Initialized!");
        Debug.Log("attack: " + stats.curAttack + " rate: " + stats.curRateOfAttk);

        // Get the Grid from the Move Handler
        if (resourceGrid == null)
            resourceGrid = ResourceGrid.Grid;

        // This receives the Object Pool from the Wave Spawner, but just in case...
        if (objPool == null)
            objPool = ObjectPool.instance;

        // Set attack Countdown to this Unit's starting attack rate
        attackCountDown = stats.startAttack;

        // Should receive Camera Shake script from the Wave Spawner, but just in case:
        if (!_camShake)
            _camShake = CameraShake.Instance;

    }

    // Update is called once per frame
    void Update()
    {

        // If I don't have any HP left, Pool myself
        if (stats.curHP <= 0)
            Suicide();

        MyStateMachine(_state);

        //debugState = state;

        if (attacker != null && unitAttackingMe == null)
        {
            Debug.Log("ENEMY: Unit is attacking me!!");
            unitAttackingMe = attacker;
            attacker = null;
            pathHandler.SwitchPathTarget(unitAttackingMe.transform.parent);
            if (_state != State.ATTACK_TILE)
            {
                _state = State.ATTACK_TILE;
            }

        }
           
    }

    void MyStateMachine(State _curState)
    {
        switch (_curState)
        {
            case State.MOVING:
                // Not attacking
                //if (unitAttackingMe != null || attacker != null)
                //{
                //    unitAttackingMe = null;
                //    attacker = null;
                //}
               
                break;
            case State.ATTACK_TILE:
                CountDownToAttack(false);
                break;
            case State.ATTACK_UNIT:
               // CountDownToAttack(true);
                break;
            //		case State.POOLING_TARGET:
            //			if (unitToPool != null){
            //				PoolTarget(unitToPool);
            //			}else{
            //				_state = State.MOVING;
            //			}
            //			break;
            default:
                // Unit is not attacking
                break;
        }
    }


    void CountDownToAttack(bool trueIfUnit)
    {
        if (attackCountDown <= 0)
        {
            // Attack
            if (trueIfUnit)
            {

                // attack unit
                //HandleDamageToUnit();

            }
            else
            {

                // attack tile
                HandleDamageToTile();
            }
            // reset countdown
            attackCountDown = stats.curRateOfAttk;

        }
        else
        {

            attackCountDown -= Time.deltaTime;
        }
    }

    void HandleDamageToTile()
    {
        if (pathHandler != null)
        {
            // Check if tile can still take damage, if so Unit_Base damages it
            if (AttackTile(resourceGrid.TileFromWorldPoint(unitAttackingMe.transform.position)))
            {

                // Change Move Handler state to stop movement
                //				moveHandler.state = Enemy_MoveHandler.State.ATTACKING;

            }
            else
            {

                // Tile has been destroyed, start Moving again
                pathHandler.SwitchToMoving();

                // Set state back to moving to stop attacking
                _state = State.MOVING;

                if (unitAttackingMe != null || attacker != null)
                {
                    unitAttackingMe = null;
                    attacker = null;
                }

            }

        }
    }

    //	/// <summary>
    //	/// Handles the damage to unit by using
    //	/// method from Unit_Base class.
    //	/// </summary>
    //	void HandleDamageToUnit()
    //	{

    //		// Verify that Player Unit is not null
    //		if (playerUnit != null) {

    //			// Store the unit's Unit_Base to access its stats
    //			Unit_Base unitToHit = playerUnit.GetComponent<Unit_Base> ();

    //			// Call the attack
    //			AttackOtherUnit (unitToHit);

    //		} else {

    //			// Player unit is dead, we can STOP attack
    //			_state = State.MOVING;
    //		}

    //	}

    void Suicide()
    {
        // get a Dead sprite to mark my death spot
        GameObject deadE = objPool.GetObjectForType("Blood FX particles", true, transform.position); // Get the dead unit object

        //if (deadE != null)
        //{
        //    deadE.GetComponent<EasyPool>().objPool = objPool;
        //}

        // Calculate the z rotation needed for the blood particle effects to shoot at the angle the shot came from
        var dir = attacker.transform.position - transform.position;
        float bloodAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        deadE.transform.eulerAngles = new Vector3(0, 0, bloodAngle);

        // make sure we Pool any Damage Text that might be on this gameObject
        if (GetComponentInChildren<Text>() != null)
        {
            Text dmgTxt = GetComponentInChildren<Text>();
            objPool.PoolObject(dmgTxt.gameObject);
        }

        Destroy(this.gameObject);
        // and Pool myself
        //objPool.PoolObject(gameObject);
    }

    ////	void PoolTarget(GameObject target)
    ////	{
    ////		unitToPool = null;
    ////
    ////		Destroy (target.GetComponent<Player_AttackHandler>().unitParent);
    ////
    ////		GameObject deadE = objPool.GetObjectForType("dead", false); // Get the dead unit object
    ////		if (deadE != null) {
    ////			deadE.GetComponent<EasyPool> ().objPool = objPool;
    ////			deadE.transform.position = target.transform.position;
    ////		}
    ////
    ////		if (!isKamikaze) {
    ////			// if we are pooling it means its dead so we should check for target again
    ////			playerUnit = null;
    ////
    ////			// Tell move handler we are no longer attacking
    ////			moveHandler.state = Enemy_MoveHandler.State.MOVING;
    ////
    ////			// Set state back to moving (if there's another target to attack the Unit or Tile will cause state to change to attacking)
    ////			_state = State.MOVING;
    ////
    ////		} else {
    ////			// kamikaze units just pool themselves when they hit
    ////			objPool.PoolObject(this.gameObject);
    ////		}
    ////	
    ////	}

    //	/// <summary>
    //	/// Special Attack for when Enemy reaches the Capital,
    //	/// suicide bombs the building doing max special damage.
    //	/// </summary>
    //	/// <param name="x">The x coordinate.</param>
    //	/// <param name="y">The y coordinate.</param>
    //	public void SpecialAttack(int x, int y)
    //	{
    //		// Hit the tile with special damage
    //		resourceGrid.DamageTile (x, y, stats.curSPdamage);

    //		// Spawn an explosion at my position
    //		GameObject explosion = objPool.GetObjectForType ("Explosion Particles", true, transform.position);

    //		if (explosion != null) {
    //			// Explosion must match my layer
    //			string targetLayer = GetComponent<SpriteRenderer>().sortingLayerName;

    //			// assign it to Particle Renderer
    //			explosion.GetComponent<ParticleSystemRenderer>().sortingLayerName = targetLayer;


    //			// SHAKE THE CAMERA!!
    //			if (_camShake){
    //				_camShake.Shake(canShakeAmmount, 0.2f);
    //			}
    //		}


    //		// then pool myself
    //		objPool.PoolObject (this.gameObject);
    //	}


    //	// KAMIKAZE ONLY:
    //	/// <summary>
    //	/// When hit with Player Unit or Building is detected,
    //	/// this unit does full Special Damage. If it was a building/tile it
    //	/// does damage through the Grid. If it was a Unit then it attacks through 
    //	/// Unit Base stats, spawns a dead sprite and pools itself
    //	/// </summary>
    //	/// <param name="x">The x coordinate.</param>
    //	/// <param name="y">The y coordinate.</param>
    //	public void KamikazeAttack(int x, int y, Unit_Base unit = null)
    //	{

    //		if (unit == null) {
    //			// Hit the tile with special damage
    //			resourceGrid.DamageTile (x, y, stats.curSPdamage);
    //			// then pool myself
    //			objPool.PoolObject (this.gameObject);
    //		} else {
    //			// Hit the Player unit with special damage
    //			SpecialAttackOtherUnit(unit);
    //		}

    //		// Spawn an explosion at my position
    //		GameObject explosion = objPool.GetObjectForType ("Explosion Particles", true, transform.position);

    //		if (explosion != null) {
    //			// Explosion must match my layer
    //			string targetLayer = GetComponent<SpriteRenderer>().sortingLayerName;

    //			// assign it to Particle Renderer
    //			explosion.GetComponent<ParticleSystemRenderer>().sortingLayerName = targetLayer;

    //		}
    //	}

    void OnTriggerEnter2D(Collider2D coll)
    {
        
        //if (coll.gameObject.CompareTag("Building"))
        //{
        //    if (aggroOnBuildings)
        //    {
        //        pathHandler.SwitchToAttacking();
        //        Debug.Log("Unit has changed to Attacking!!");
        //    }
        //}
        //if (isKamikaze)
        //{
        //    if (coll.gameObject.tag == "Building")
        //    {
        //        // if unit hits a building, we blow up
        //        KamikazeAttack((int)coll.gameObject.transform.position.x, (int)coll.gameObject.transform.position.y);
        //    }
        //    if (coll.gameObject.tag == "Citizen")
        //    {
        //        // if unit hits a Player Unit, 
        //        // we get the Unit base and blow up
        //        if (coll.gameObject.GetComponentInChildren<Unit_Base>() != null)
        //        {
        //            KamikazeAttack(0, 0, coll.gameObject.GetComponentInChildren<Unit_Base>());
        //        }
        //        else
        //        {
        //            Debug.Log("ENEMY ATTACK: Could not find Player unit's attack handler!");
        //        }
        //    }
        //}
        //else if (isPlayerAttacker)
        //{
        //    if (coll.gameObject.tag == "Citizen")
        //    {
        //        moveHandler.targetPlayer = coll.gameObject;
        //    }
        //}
    }

    //	void OnCollisionStay2D(Collision2D coll)
    //	{
    //		if (!isKamikaze) {
    //			if (coll.gameObject.tag == "Citizen"){
    //				Debug.Log ("ENEMY ATTACK: HIT the Hero!! EFF YEA!");
    ////				AttackOtherUnit(coll.gameObject.GetComponent<Player_HeroAttackHandler>());
    //				if (playerUnit == null){
    //					playerUnit = coll.gameObject;
    //					moveHandler.targetPlayer = playerUnit;
    //					_state = State.ATTACK_UNIT;
    //				}
    //			}
    //		}
    //	}

    //	void OnCollisionExit2D(Collision2D coll)
    //	{
    //		if (!isKamikaze) {
    //			if (coll.gameObject.tag == "Citizen"){
    //				Debug.Log ("ENEMY ATTACK: HIT the Hero!! EFF YEA!");
    //				//				AttackOtherUnit(coll.gameObject.GetComponent<Player_HeroAttackHandler>());
    //				if (playerUnit != null){
    //					playerUnit = null;
    //					// move handler doesn't make this null so it can continue to follow player

    //					_state = State.MOVING;
    //				}
    //			}
    //		}
    //	}

}
