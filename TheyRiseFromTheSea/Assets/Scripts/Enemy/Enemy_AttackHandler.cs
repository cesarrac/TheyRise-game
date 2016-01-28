using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Enemy_AttackHandler : Unit_Base {

    Enemy_PathHandler pathHandler;


    //	public int targetTilePosX, targetTilePosY;

    public GameObject playerUnit;

    //	// KAMIKAZE:
    //	public bool isKamikaze;


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


    float attackRange = 5.0f; // < ------- threshold target's can't pass without being attacked by this unit
    public float AttackRange { get { return attackRange; } set { attackRange = Mathf.Clamp(value, 2.0f, 8.0f); } }

    bool movingToAttack;

    public bool isAttacking { get; protected set; }

    bool mainTargetFound;

    bool mainTargetIsTerraformer = false, currTargetIsTile = false;

    Rigidbody2D rigid_body;

    TileData targetAsTile;


    void OnEnable()
    {
        ResetFlagsandTargets();
    }

    void ResetFlagsandTargets()
    {
        mainTargetFound = false;
        movingToAttack = false;
        isAttacking = false;
        mainTargetIsTerraformer = false;

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
            mainTargetIsTerraformer = false;

            // If this unit is NO-Aggro to buildings we can go ahead and set playerUnit here so it attacks the player as soon as it is in range
            if (!isAggroToBuildings)
            {
                playerUnit = mainTarget.gameObject;
                currTargetIsTile = false;
            }
                
        }
        else
        {
           // towerAttackingMe = target.gameObject;

            mainTargetIsTerraformer = true;

            // Set the Target tile to the terraformer
            targetAsTile = ResourceGrid.Grid.terraformerTile;
        }
    }

    void Start()
    {
        // Initialize Unit stats
        //stats.Init();

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

        //StartCoroutine("DebugMyStatus");

    }

    void Update()
    {
        debugState = _state;

        if (stats.curHP > 0)
        {

            // Only listen for attackingTower once our path handler has set our main target and unit is NOT attacking
            if (mainTarget != null && !isAttacking)
                ListenForAttacker();

            // Makes sure that if the tower target was POOLED that the target is nulled
            if (towerAttackingMe != null)
            {
                if (!towerAttackingMe.activeSelf)
                {
                    towerAttackingMe = null;
                    // And just in case this happened in the middle of an attack, Stop Attack routines
                    StopAttackCoRoutines();
                }

            }

            // If at any time the path handler is in range of the target and isAttacking is false, call attack!
            if (pathHandler.InRange && !isAttacking)
            {
                if (currTargetIsTile || mainTargetIsTerraformer)
                {
                    if (towerAttackingMe != null)
                    {
                        Debug.Log("ENEMY: Starting tower attack...");
                        StartCoroutine("TowerAggroAttack");
                        isAttacking = true;
                    }
                    else
                    {
                        StopAttackCoRoutines();
                        StartCoroutine("TowerisMainTargetAttack");
                        isAttacking = true;
                    }

                }
                else
                {
                    if (playerUnit != null)
                    {
                        Debug.Log("ENEMY: Starting player attack...");
                        StartCoroutine("PlayerAttack");
                        isAttacking = true;
                    }

                }
            }
        }



    }

    void LateUpdate()
    {

        // If I don't have any HP left, Pool myself and stop doing everything else
        if (stats.curHP <= 0)
        {
            Suicide();
        }
    }


    IEnumerator DebugMyStatus()
    {
        while (true)
        {
            yield return new WaitForSeconds(5f);
            string status = "isAttacking = " + isAttacking + " towerAttackingMe = " + towerAttackingMe + " attackingTower = " + attackingTower + 
                " playerUnit = " + playerUnit +" In Range = " + pathHandler.InRange + " currTargetIsTile = " + currTargetIsTile;
            Debug.Log("ENEMY ATTACK STATUS: " + status);
        }
    }


    //    //  GENERAL ATTACK BEHAVIOR:
    //    /// Units always start with an assigned MAIN target, they get it from their path handler's target transform. This is their main objective.
    //    /// If they stop to attack something else on the way and survive they will always continue towards that MAIN TARGET.
    //    /// When they are IN RANGE of that MAIN TARGET they stop moving along their path and start attacking UNTIL:
    //    ///    - their target is dead.
    //    ///    - they are dead.
    //    ///    - or their target is no longer in range. 
    //    ///      IN this case they would need to continue moving 
    //    ///      towards their target to get in range, starting the process again.
    //    /// Once a unit gets in range of their main target ONCE, they will ignore all other targets.
    void ListenForAttacker()
    {
        if (!isAttacking)
        {
            if (isAggroToBuildings)
            {
                //                 AGGRESSIVE TO BUILDING ATTACK BEHAVIOR:
                //  Rule: If any tower attacks this unit or is in range and the unit currently does NOT have an attack target besides the player, they ALWAYS will go and attack the tower. 
                // If they DONT have a tower attacking them and the player attacks them, they will go and attack the player until a tower attacks or their main target is in range.

                if (attackingTower != null && towerAttackingMe == null)
                {

                    Debug.Log("A tower is attacking me!!");

                    StopAttackCoRoutines();

                    towerAttackingMe = attackingTower;
                    attackingTower = null;

                    currTargetIsTile = true;

                    // Switch the path handlers target to this tower IF this tower is NOT the main target
                    if (towerAttackingMe.transform != mainTarget || towerAttackingMe.transform.parent != mainTarget)
                        pathHandler.SwitchPathTarget(towerAttackingMe.transform.parent);


                }
                // If BOTH are null, THEN I can get a player as a target
                else if (attackingTower == null && towerAttackingMe == null)
                {
                   if (playerUnit != null)
                    {
                        currTargetIsTile = false;

                        StopAttackCoRoutines();

                        // Switch the path handlers target to this Player IF this Player is NOT the main target
                        if (playerUnit.transform != mainTarget)
                            pathHandler.SwitchPathTarget(playerUnit.transform);
                    }
                }

            }
            else
            {
                // NON-AGGRESSIVE TO BUILDING ATTACK BEHAVIOR:
                // Rule: If Player is attacking this unit, they will go attack the player. BUT if a tower attacks this unit, they ignore it and keep moving towards their main target.
                // These units never attack towers UNLESS a tower is their Main Target.

                if (playerUnit != null)
                {
                    StopAttackCoRoutines();

                    // Switch the path handlers target to this Player IF this Player is NOT the main target
                    if (playerUnit.transform != mainTarget)
                        pathHandler.SwitchPathTarget(playerUnit.transform);
                }

            }
        }

    }


    IEnumerator TowerisMainTargetAttack()
    {
        while (true)
        {
           
            if (mainTarget != null && pathHandler.InRange) 
            {
               // Debug.Log("ENEMY: Attacking the tower!");

                // Play attack sound
                Sound_Manager.Instance.PlaySound("Slimer Attack");

                StartCoroutine(JumpAttack(mainTarget.position));

                HandleDamage_ToMainTargetTile();
            }
            else
            {
               
                isAttacking = false;
                yield break;
            }

            yield return new WaitForSeconds(stats.curRateOfAttk);
        }
    }

    IEnumerator TowerAggroAttack()
    {
        while (true)
        {

            if (towerAttackingMe != null && pathHandler.InRange)
            {
                // Debug.Log("ENEMY: Attacking the tower!");

                // Play attack sound
                Sound_Manager.Instance.PlaySound("Slimer Attack");

                StartCoroutine(JumpAttack(towerAttackingMe.transform.parent.position));

                HandleDamageToBattleTower();
            }
            else
            {

                isAttacking = false;
                yield break;
            }

            yield return new WaitForSeconds(stats.curRateOfAttk);
        }
    }

    IEnumerator PlayerAttack()
    {
        while (true)
        {

            if (playerUnit != null && pathHandler.InRange)
            {
                //Debug.Log("ENEMY: Attacking the player!");

                // Play attack sound
                Sound_Manager.Instance.PlaySound("Slimer Attack");

                StartCoroutine(JumpAttack(playerUnit.transform.position));
                HandleDamageToUnit();
            }
            else
            {
                if (!pathHandler.InRange)
                {
                    Debug.Log("ENEMY: Missed! Player DODGED!!");
                }
                isAttacking = false;
                yield break;
            }
            yield return new WaitForSeconds(stats.curRateOfAttk);
        }
    }

    void StopAttackCoRoutines()
    {
        isAttacking = false;
        StopCoroutine("PlayerAttack");
        StopCoroutine("TowerAttack");
    }

    IEnumerator JumpAttack(Vector3 targetPosition)
    {

        Vector2 jumpDirection = targetPosition - transform.root.position;
        rigid_body.AddForce(jumpDirection * 1200);
        yield return new WaitForSeconds(0.1f);
        rigid_body.AddForce(-jumpDirection * 1200);
        yield break;

    }

    void HandleDamage_ToMainTargetTile()
    {
        if (pathHandler != null)
        {
            if (mainTargetIsTerraformer)
            {
                if (!AttackTile(targetAsTile))
                {
                    // Set state back to moving
                    _state = State.MOVING;

                    // Set attacking flag to false to stop attacking
                    isAttacking = false;

                    Debug.Log("ENEMY: Stopped damaging tower!");

                    // Stop Tower Attack coroutine
                    StopCoroutine("TowerAttack");
                }
            }

        }
    }


    void HandleDamageToBattleTower()
    {
        if (pathHandler != null)
        {
            // Check if tile can still take damage, if so Unit_Base damages it
            if (!AttackTile(resourceGrid.TileFromWorldPoint(towerAttackingMe.transform.parent.position)))
            {
                // Set state back to moving
                _state = State.MOVING;

                // Set attacking flag to false to stop attacking
                isAttacking = false;

                if (towerAttackingMe != null)
                {
                    towerAttackingMe = null;
                    attackingTower = null;
                }

                Debug.Log("ENEMY: Stopped damaging tower!");

                // Stop Tower Attack coroutine
                StopCoroutine("TowerAttack");
            }
        }
    }


    void HandleDamageToUnit()
    {

        if (!AttackUnit(playerUnit.GetComponent<Unit_Base>()))
        {

            // Set state back to moving
            _state = State.MOVING;

            // Set attacking flag to false to stop attacking
            isAttacking = false;

            playerUnit = null;

            StopCoroutine("PlayerAttack");

        }

    }

    void Suicide()
    {
        // Get the Blood splat!
        GameObject deadE = objPool.GetObjectForType("Blood FX particles 2", true, transform.position);

        // Calculate the z rotation needed for the blood particle effects to shoot at the angle the shot came from
        if (towerAttackingMe != null)
        {
            var dir = towerAttackingMe.transform.position - transform.position;
            float bloodAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            deadE.transform.eulerAngles = new Vector3(0, 0, bloodAngle);

            // Register my death with Enemy master and tell them a Tower killed me
            Enemy_Master.instance.RegisterDeath(towerAttackingMe.transform);
        }
        else if (playerUnit != null)
        {
            var dir = playerUnit.transform.position - transform.position;
            float bloodAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            deadE.transform.eulerAngles = new Vector3(0, 0, bloodAngle);

            // Register my death with Enemy master and tell them a Player killed me
            Enemy_Master.instance.RegisterDeath(playerUnit.transform);
        }

        // make sure we Pool any Damage Text that might be on this gameObject
        if (GetComponentInChildren<Text>() != null)
        {
            Text dmgTxt = GetComponentInChildren<Text>();
            objPool.PoolObject(dmgTxt.gameObject);
        }

        // Stop Attack and Path coroutines!
        isAttacking = false;
        playerUnit = null;
        towerAttackingMe = null;
        StopAttackCoRoutines();
        pathHandler.FullStop();

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
