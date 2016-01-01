using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Enemy_AttackHandler : Unit_Base {

    Enemy_PathHandler pathHandler;


    //	public int targetTilePosX, targetTilePosY;

    public GameObject playerUnit;

    //	// KAMIKAZE:
    //	public bool isKamikaze;


    [Header ("If ON, this unit will always attack buildings nearby")]
    public bool aggroOnBuildings;

    public enum State { MOVING, ATTACK_TILE, ATTACK_UNIT, ATTACKING, POOLING_TARGET };

    private State _state = State.MOVING;

    //	[HideInInspector]
    //	public State state { get { return _state; } set { _state = value; } }

    public State debugState;

    [Header("Camera Shake Ammount (Kamikaze Attacks):")]
    public float canShakeAmmount;
    public CameraShake _camShake;


    public GameObject attackingTower; // < ----- public to set from other scripts
    GameObject towerAttackingMe; // < ----- private that will be set to attackingTower 

    public Transform mainTarget { get; protected set; } // < ---- always the same as my path's original target.


    float attackRange = 5f; // < ------- threshold target's can't pass without being attacked by this unit

    bool movingToAttack;

    public bool isAttacking { get; protected set; }

    bool mainTargetFound;

    bool mainTargetIsTile;

    Rigidbody2D rigid_body;


    void OnEnable()
    {
        // Initialize Unit stats
        stats.Init();

        ResetFlagsandTargets();
    }

    void ResetFlagsandTargets()
    {
        mainTargetFound = false;
        movingToAttack = false;
        isAttacking = false;
        mainTargetIsTile = false;

        attackingTower = null;
        towerAttackingMe = null;
        playerUnit = null;
    }

    void Awake()
    {
        pathHandler = GetComponent<Enemy_PathHandler>();
        audio_source = GetComponent<AudioSource>();
        rigid_body = GetComponent<Rigidbody2D>();
    }

    public void SetMainTarget(Transform target)
    {
        mainTarget = target;

        if (mainTarget.gameObject.tag == "Citizen")
        {
            mainTargetIsTile = false;
            playerUnit = mainTarget.gameObject;
        }
        else
        {
            mainTargetIsTile = true;
        }
    }

    void Start()
    {

        //		// Get the value of isKamikaze set by the public bool in Move Handler
        //		isKamikaze = moveHandler.isKamikaze;

        //// Get isPlayerattackingTower from move Handler as well
        //isPlayerattackingTower = moveHandler.isPlayerattackingTower;

        if (resourceGrid == null)
            resourceGrid = ResourceGrid.Grid;

        if (objPool == null)
            objPool = ObjectPool.instance;

        if (!_camShake)
            _camShake = CameraShake.Instance;

    }

    void Update()
    {
        debugState = _state;

        // If I don't have any HP left, Pool myself
        if (stats.curHP <= 0)
            Suicide();

        // Only listen for attackingTower once our path handler has set our main target and unit is NOT attacking
        if (mainTarget != null)
            ListenForAttacker();

        // Once an attackingTower has been assigned we need to move to the attackingTower if they are NOT in range.
        if (towerAttackingMe != null && !isAttacking)
            SeekTarget(towerAttackingMe, true);


    }

    void ListenForAttacker()
    {
        //  GENERAL ATTACK BEHAVIOR:
        /// Units always start with an assigned MAIN target, they get it from their path handler's target transform. This is their main objective.
        /// If they stop to attack something else on the way and survive they will always continue towards that MAIN TARGET.
        /// When they are IN RANGE of that MAIN TARGET they stop moving along their path and start attacking UNTIL:
        ///    - their target is dead.
        ///    - they are dead.
        ///    - or their target is no longer in range. 
        ///      IN this case they would need to continue moving 
        ///      towards their target to get in range, starting the process again.
        /// Once a unit gets in range of their main target ONCE, they will ignore all other targets.

        // No matter WHAT, if the main target is in range, stop everything and go and attack it.
        if (CheckRange(mainTarget.position, attackRange) && mainTarget.gameObject != null)
        {
            // Main target is in range!
            mainTargetFound = true;

            // Stop all movement from the path handler.
           // pathHandler.FullStop();

            if (!isAttacking)
            {                                                           
                // Check if the main target is Player
                if (!mainTargetIsTile)
                {
                    if (playerUnit == null)
                    {
                        // Main target is Player
                        playerUnit = mainTarget.gameObject;
                    }

                    // Do the attack HERE.
                    StopCoroutine("PlayerAttack");
                    StartCoroutine("PlayerAttack");    
                }
                else
                {
                    // Main target is NOT player, so it MUST be an attack tower or a building. It's the same attack method regardless of it being a battle or utility building.
                    towerAttackingMe = mainTarget.gameObject;

                    // Do the attack HERE.
                    StopCoroutine("TowerAttack");
                    StartCoroutine("TowerAttack");
                }

                if (_state != State.ATTACKING)
                {
                    _state = State.ATTACKING;
                }

                isAttacking = true;
                movingToAttack = false;
            }
           


        }
        else
        {
            // AS OF NOW! Once the main target has been spotted, this unit will ignore other targets and go for main target, always chasing after it if it moved
            if (mainTargetFound && mainTarget.gameObject != null)
            {
                
                SeekTarget(mainTarget.gameObject, mainTargetIsTile);
                if (_state != State.MOVING)
                {
                    pathHandler.SwitchToMoving();
                    _state = State.MOVING;
                }

            }
            else
            {
                if (aggroOnBuildings)
                {
                    ///                 AGGRESSIVE TO BUILDING ATTACK BEHAVIOR:
                    ///  Rule: If any tower attacks this unit or is in range and the unit currently does NOT have an attack target besides the player, they ALWAYS will go and attack the tower. 
                    /// If they DONT have a tower attacking them and the player attacks them, they will go and attack the player until a tower attacks or their main target is in range.

                    if (attackingTower != null && towerAttackingMe == null)
                    {
                        // The tile unit that is attacking me stores their gameobject in my attackingTower variable. 
                        // Then I take that variable and store it as towerAttackingMe

                        Debug.Log("A tower is attacking me!!");

                        // Since there's a chance that a coroutine might be running when a tower attacks and overrides this unit's target, we should stop all attack routines here before starting another one.
                        StopAttackCoRoutines();

                        towerAttackingMe = attackingTower;
                        attackingTower = null;

                        movingToAttack = false;

                    }
                    else if (playerUnit != null && towerAttackingMe == null)
                    {
                        SeekTarget(playerUnit, false);
                    }

                }
                else
                {
                    /// NON-AGGRESSIVE TO BUILDING ATTACK BEHAVIOR:
                    /// Rule: If Player is attacking this unit, they will go attack the player. BUT if a tower attacks this unit, they ignore it and keep moving towards their main target.
                    /// These units never attack towers UNLESS a tower is their Main Target.

                    if (playerUnit != null && !isAttacking)
                    {
                        SeekTarget(playerUnit, false);
                    }

                }
            }
    
        }

     
    }


    bool CheckRange(Vector3 target, float threshold)
    {
        if ((target - transform.position).sqrMagnitude <= threshold)
        {
            return true;
        }
        else
            return false;
    }

    void SeekTarget(GameObject target, bool isTile)
    {
        // Check if this new target is in range
        if (CheckRange(target.transform.position, attackRange))
        {
            // attackingTower is in range! 
            movingToAttack = false;

            if (_state != State.ATTACKING)
            {
                _state = State.ATTACKING;
            }

            // Do the attack HERE.
            if (isTile)
            {
                if (!isAttacking)
                {
                    // Tell path handler to stop moving first!
                    pathHandler.FullStop();

                    StopCoroutine("TowerAttack");
                    StartCoroutine("TowerAttack");
                    isAttacking = true;
                }
            }
            else
            {
                // Tell path handler to stop moving first!
                pathHandler.FullStop();

                StopCoroutine("PlayerAttack");
                StartCoroutine("PlayerAttack");
                isAttacking = true;
            }
        


        }
        else if (!movingToAttack)
        {
            //attackingTower in NOT in range so I should create a path to them
            // But dont bother if the target is equal to the main target because we already have a path to them
            if (target.transform.position != mainTarget.transform.position)
            {
                if (mainTargetIsTile)
                    pathHandler.SwitchPathTarget(target.transform.parent);
                else
                    pathHandler.SwitchPathTarget(target.transform);
            }
        

            movingToAttack = true;

        }
    }




  
    IEnumerator TowerAttack()
    {
        while (true)
        {
            yield return new WaitForSeconds(stats.curRateOfAttk);

            if (towerAttackingMe != null && CheckRange(towerAttackingMe.transform.position, attackRange)) 
            {
                StartCoroutine(JumpAttack(towerAttackingMe.transform.parent.position));
                HandleDamageToTile();
            }
            else
            {
                // Just in case this unit didnt destroy the tower or it somehow became null for some other reason make sure to keep moving
                if (towerAttackingMe == null)
                {
                    pathHandler.SwitchToMoving();
                    _state = State.MOVING;
                }

                isAttacking = false;
                yield break;
            }
        }
    }

    IEnumerator PlayerAttack()
    {
        while (true)
        {
            yield return new WaitForSeconds(stats.curRateOfAttk);

            if (playerUnit != null && CheckRange(playerUnit.transform.position, attackRange))
            {
                StartCoroutine(JumpAttack(playerUnit.transform.position));
                HandleDamageToUnit();
            }
            else
            {
                isAttacking = false;
                yield break;
            }
        }
    }

    void StopAttackCoRoutines()
    {
        StopCoroutine("PlayerAttack");
        StopCoroutine("TowerAttack");
    }

    IEnumerator JumpAttack(Vector3 targetPosition)
    {

        Vector2 jumpDirection = targetPosition - transform.root.position;
        rigid_body.AddForce(jumpDirection * 1800f);
        yield return new WaitForSeconds(0.1f);
        rigid_body.AddForce(-jumpDirection * 1800f);
        yield break;

    }


    void HandleDamageToTile()
    {
        if (pathHandler != null)
        {
            // Check if tile can still take damage, if so Unit_Base damages it
            if (!AttackTile(resourceGrid.TileFromWorldPoint(towerAttackingMe.transform.position)))
            {
                // Tile has been destroyed, start Moving again towards my main target
                pathHandler.SwitchToMoving();

                // Set state back to moving to stop attacking
                _state = State.MOVING;

                // Set attacking flag to false
                isAttacking = false;

                if (towerAttackingMe != null)
                {
                    towerAttackingMe = null;
                    //attackingTower = null;
                }

                // Stop Tower Attack coroutine
                StopCoroutine("TowerAttack");
            }

        }
    }


    void HandleDamageToUnit()
    {

        if (!AttackUnit(playerUnit.GetComponent<Unit_Base>()))
        {
            // Set state back to moving to stop attacking
            _state = State.MOVING;

            // Player is dead, so this is probably unnecessary!
            pathHandler.SwitchToMoving();

            // Set attacking flag to false
            isAttacking = false;

            playerUnit = null;

            StopCoroutine("PlayerAttack");

        }

    }

    void Suicide()
    {
        // get a Dead sprite to mark my death spot
        GameObject deadE = objPool.GetObjectForType("Blood FX particles 2", true, transform.position); // Get the dead unit object

        //if (deadE != null)
        //{
        //    deadE.GetComponent<EasyPool>().objPool = objPool;
        //}

        // Calculate the z rotation needed for the blood particle effects to shoot at the angle the shot came from
        if (towerAttackingMe != null)
        {
            var dir = towerAttackingMe.transform.position - transform.position;
            float bloodAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            deadE.transform.eulerAngles = new Vector3(0, 0, bloodAngle);
        }
        else if (playerUnit != null)
        {
            var dir = playerUnit.transform.position - transform.position;
            float bloodAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            deadE.transform.eulerAngles = new Vector3(0, 0, bloodAngle);
        }

        // make sure we Pool any Damage Text that might be on this gameObject
        if (GetComponentInChildren<Text>() != null)
        {
            Text dmgTxt = GetComponentInChildren<Text>();
            objPool.PoolObject(dmgTxt.gameObject);
        }

        
        // and Pool myself
        objPool.PoolObject(gameObject);
    }



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

  

}
