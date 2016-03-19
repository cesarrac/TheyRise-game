using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;

public class Enemy_AttackHandler : Unit_Base {

    public UnitPathHandler pathHandler;

    public GameObject playerUnit;

    public enum State { MOVING, ATTACK_TILE, ATTACK_UNIT, ATTACKING, POOLING_TARGET };

    private State _state = State.MOVING;

    public State debugState;

    //[Header("Camera Shake Ammount (Kamikaze Attacks):")]
    //public float canShakeAmmount;
    //public CameraShake _camShake;

    public Transform mainTarget { get; protected set; } // < ---- always the same as my path's original target.
    TileData targetAsTile;

    float attackRange = 2f; // < ------- threshold target's can't pass without being attacked by this unit
    public float AttackRange { get { return attackRange; } set { attackRange = Mathf.Clamp(value, 2.0f, 8.0f); } }

    public bool isAttacking { get; protected set; }

    bool currTargetIsTile = false;

    public Rigidbody2D rigid_body
    {
        get; protected set;
    }

    public Action<Vector3> AttackActionCB; // This is assigned to the specific attack action of this unit


    public void InitPathfindingTargetAction()
    {
        if (pathHandler == null)
            pathHandler = GetComponent<UnitPathHandler>();

        // Set the path handler's assign target callback
        pathHandler.RegisterAssignTargetToHandlerCB(SetMainTarget);

        // Set the path handler's action to call when destination reached
        pathHandler.RegisterDestinationReachedCB(AttackMainTarget);
    }

    public void ResetFlagsandTargets()
    {
        isAttacking = false;
        currTargetIsTile = true;

        playerUnit = null;
    }

    public void SetMainTarget(Transform target)
    {
        mainTarget = target;

        if (mainTarget.gameObject.tag == "Citizen")
        {

            // If this unit is NO-Aggro to buildings we can go ahead and set playerUnit here so it attacks the player as soon as it is in range
            if (!isAggroToBuildings)
            {
                playerUnit = mainTarget.gameObject;
                currTargetIsTile = false;
            }

        }
        else
        {
            // Get the target as a Tile
            targetAsTile = ResourceGrid.Grid.TileFromWorldPoint(target.position);
            currTargetIsTile = true;
        }
    }

    void Start()
    {

        if (resourceGrid == null)
            resourceGrid = ResourceGrid.Grid;

        if (objPool == null)
            objPool = ObjectPool.instance;

        //if (!_camShake)
        //    _camShake = CameraShake.Instance;

    }

    void Update()
    {

    }

    void LateUpdate()
    {

        // If I don't have any HP left, Pool myself and stop doing everything else
        if (stats.curHP <= 0)
        {
            Suicide();
        }
    }

    void IsMainTargetInRange()
    {
        if (RangeCheck() == true)
        {
            AttackMainTarget();
        }
        else
        {
            // If the main target is not in range, request a new path...
            if (mainTarget != null && mainTarget.gameObject.activeSelf == true)
            {
                Debug.Log("ENEMY: Target NOT in range, requesting a new path to target!");
                pathHandler.GetANewPath();
            }
            else
            {
                //... or a whole new target if it's dead or inactive
                pathHandler.AssignTarget();
            }
        }
    }

    bool RangeCheck()
    {
        var heading = mainTarget.position - transform.position;

        if (heading.sqrMagnitude <= attackRange * attackRange)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    void AttackMainTarget()
    {
        if (currTargetIsTile)
        {
            if (mainTarget != null && mainTarget.gameObject.activeSelf == true)
            {
                StopAttackCoRoutines();
                StartCoroutine("TowerisMainTargetAttack");
                isAttacking = true;
            }
            else
            {
                // Get a new target because the Main Target has been downed
                pathHandler.AssignTarget();
                isAttacking = false;
            }
        }
        else
        {
            if (playerUnit != null)
            {
                //Debug.Log("ENEMY: Starting player attack...");
                StartCoroutine("PlayerAttack");
                isAttacking = true;
            }

        }
    }

    IEnumerator TowerisMainTargetAttack()
    {
        while (true)
        {
           
            if (mainTarget != null && RangeCheck() == true) 
            {
               // Debug.Log("ENEMY: Attacking the tower!");

                // Play attack sound
                Sound_Manager.Instance.PlaySound("Slimer Attack");

                // StartCoroutine(JumpAttack(mainTarget.position));
                AttackActionCB(mainTarget.position);

                HandleDamage_ToMainTargetTile();


            }
            else
            {
               
                isAttacking = false;

                // Call to check range again and set target or new path
                IsMainTargetInRange();

                yield break;
            }

            yield return new WaitForSeconds(stats.curRateOfAttk);
        }
    }

    IEnumerator PlayerAttack()
    {
        while (true)
        {
            if (playerUnit != null && RangeCheck() == true)
            {
                //Debug.Log("ENEMY: Attacking the player!");

                // Play attack sound
                Sound_Manager.Instance.PlaySound("Slimer Attack");

                //StartCoroutine(JumpAttack(playerUnit.transform.position));
                AttackActionCB(playerUnit.transform.position);

                HandleDamageToUnit();
            }
            else
            {
                isAttacking = false;

                // Call to check range again and set target or new path
                IsMainTargetInRange();

                yield break;
            }
            yield return new WaitForSeconds(stats.curRateOfAttk);
        }
    }

    void StopAttackCoRoutines()
    {
        isAttacking = false;
        StopCoroutine("PlayerAttack");
        StopCoroutine("TowerisMainTargetAttack");
    }

    void HandleDamage_ToMainTargetTile()
    {
        if (pathHandler != null)
        {
            if (!AttackTile(targetAsTile))
            {
                // Set state back to moving
                _state = State.MOVING;

                // Set attacking flag to false to stop attacking
                isAttacking = false;

                Debug.Log("ENEMY: Stopped damaging tower!");

                // Stop Tower Attack coroutine
                StopAttackCoRoutines();
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

           // playerUnit = null;

            StopAttackCoRoutines();

        }

    }

    void Suicide()
    {
        // Get the Blood splat!
        GameObject deadE = objPool.GetObjectForType("Blood FX particles 2", true, transform.position);

        //// Calculate the z rotation needed for the blood particle effects to shoot at the angle the shot came from
        //if (towerAttackingMe != null)
        //{
        //    var dir = towerAttackingMe.transform.position - transform.position;
        //    float bloodAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        //    deadE.transform.eulerAngles = new Vector3(0, 0, bloodAngle);

        //    // Register my death with Enemy master and tell them a Tower killed me
        //    Enemy_Master.instance.RegisterDeath(towerAttackingMe.transform);
        //}
        //else if (playerUnit != null)
        //{
        //    var dir = playerUnit.transform.position - transform.position;
        //    float bloodAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        //    deadE.transform.eulerAngles = new Vector3(0, 0, bloodAngle);

        //    // Register my death with Enemy master and tell them a Player killed me
        //    Enemy_Master.instance.RegisterDeath(playerUnit.transform);
        //}

        // make sure we Pool any Damage Text that might be on this gameObject
        if (GetComponentInChildren<Text>() != null)
        {
            Text dmgTxt = GetComponentInChildren<Text>();
            objPool.PoolObject(dmgTxt.gameObject);
        }

        // Stop Attack and Path coroutines!
        isAttacking = false;
        playerUnit = null;

        StopAttackCoRoutines();

        // and Pool myself
        objPool.PoolObject(gameObject);
    }
  

}
