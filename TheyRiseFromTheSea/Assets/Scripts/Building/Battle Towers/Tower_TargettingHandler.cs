using UnityEngine;
using System.Collections;
using System;


public class Tower_TargettingHandler : Unit_Base
{
    /// <summary>
    /// Tower Targetting. 
    /// Seeks Enemy units using a Linecast
    /// </summary>

    public TowerGunStats gunStats { get; protected set; }

    public Transform sightStart, sightEnd;

    public LayerMask mask;

    bool enemyInRange = false;

    public GameObject targetUnit;

    LineRenderer lineR;

    SpriteRenderer sr;

    public enum State { ASSEMBLING, SEEKING, ACQUIRE, SHOOTING, RELOADING, STARVED, MANUAL_CONTROL, MANUAL_SHOOTING }

    private State _state = State.SEEKING;

    [HideInInspector]
    public State state { get { return _state; } set { _state = value; } }

    public State debugState;

    private float shootCountDown, reloadCountDown;

    private int ammoCount;

    //	[SerializeField]
    //	private Building_StatusIndicator bStatusIndicator;

    private bool statusIndicated = false;

    Building_Handler build_click_handler;

    Action<Unit_Base> CB_DoDamage;

    public string towerName;

    void OnEnable()
    {
        targetUnit = null;
    }

    void Awake()
    {
        CB_DoDamage = HandleDamageToUnit;

        // Get Stats from Database
        BlueprintDatabase.Instance.GetBattleStats(towerName, InitGunStats, InitTowerStats);
    }

    void InitGunStats(TowerGunStats towerStats)
    {
        gunStats = new TowerGunStats(towerStats.startingAmmo, towerStats.startingReloadTime);

    }

    void InitTowerStats(UnitStats unitStats)
    {
        stats = new UnitStats();
        stats.InitStartingStats(unitStats.maxHP, unitStats.startDefence, unitStats.startAttack, unitStats.startShield, unitStats.startRate, unitStats.startDamage, 0);
    }

    void Start()
    {

        build_click_handler = GetComponentInParent<Building_Handler>();

        stats.Init();

        // Initialize Gun stats, starting ammo
        //gunStats.Init();

        // Set initial reload Count Down to starting reload time
        reloadCountDown = gunStats.currReloadTime;

        // Also set the initial ammo
        ammoCount = gunStats.currAmmo;

        // Set the count down to Shoot to this Tower's fire rate
        shootCountDown = stats.curRateOfAttk;

        resourceGrid = ResourceGrid.Grid;

        objPool = ObjectPool.instance;

        // Get our Sprite Renderer from Parent ( this is how the prefab is set-up )
        if (GetComponentInParent<SpriteRenderer>() != null)
        {

            sr = GetComponentInParent<SpriteRenderer>();

            // Get the Line Renderer Component from Child object sightStart
            lineR = sightStart.GetComponent<LineRenderer>();
            lineR.sortingLayerName = sr.sortingLayerName;
            lineR.sortingOrder = sr.sortingOrder;

        }



    }

    // NOTE: Using Fixed Update becuase SeekEnemies() is controlling a Physics 2D Linecast
    void FixedUpdate()
    {
        // NOTE: Can't Shoot if I'm in a Starved or Reloading!

        if (enemyInRange && _state != State.STARVED && _state != State.MANUAL_CONTROL && targetUnit == null)
        {

            SeekEnemies();

        }

    }

    void SeekEnemies()
    {
        RaycastHit2D hit = Physics2D.Linecast(sightStart.position, sightEnd.position, mask.value);
        if (hit.collider != null)
        {
            if (hit.collider.CompareTag("Enemy"))
            {


                if (targetUnit == null)
                {// DONT GET TARGET if you already have one!

                    targetUnit = hit.collider.gameObject;

                    // Mark the target with particles
                    MarkTarget(targetUnit.transform.position);

                    if (_state != State.STARVED && _state != State.RELOADING && _state != State.MANUAL_CONTROL)
                    {

                        // IF gun has enough ammo SHOOT once
                        if (ammoCount > 0)
                        {
                            //HandleDamageToUnit();

                            ShootProjectile();

                            VisualShooting();
                            // Then start counting down to next shot
                            _state = State.SHOOTING;

                            // Indicate
                            statusIndicated = false;
                        }
                        else
                        {
                            // There's NO AMMO left, start Reloading count down
                            _state = State.RELOADING;
                            // Reloading state will set it back to shooting once we have bullets

                            // Indicate
                            statusIndicated = false;
                        }

                    }

                }
            }
        }
    }

    void Update()
    {

        // Check that my line renderer's sorting layer is the same as mine
        if (lineR.sortingLayerName != sr.sortingLayerName)
        {
            lineR.sortingLayerName = sr.sortingLayerName;
            lineR.sortingOrder = sr.sortingOrder + 1;
        }

        // Wait for the Building to assemble, before allowing it to do anything
        if (build_click_handler.state == Building_Handler.State.READY)
        {
            MyStateManager(_state);
        }
        else
        {
            MyStateManager(State.ASSEMBLING);
        }

        // Shooting under Manual Control
        if (_state == State.MANUAL_CONTROL && _state != State.STARVED)
        {
            if (Input.GetMouseButtonDown(0))
            {
                SeekEnemies();
                _state = State.MANUAL_SHOOTING;
            }
        }

        debugState = state;
    }

    void MyStateManager(State curState)
    {
        switch (curState)
        {
            case State.SEEKING:

                if (targetUnit == null || !targetUnit.activeSelf)
                {
                    targetUnit = null;
                    sightStart.Rotate(Vector3.forward * 90 * Time.deltaTime);
                }
                else
                {
                    _state = State.SHOOTING;
                }

                // indicate
                if (!statusIndicated)
                    IndicateStatus("Seeking");

                break;
            case State.ACQUIRE:
                // this will move the beam to the target
                // indicate
                if (!statusIndicated)
                    IndicateStatus("Acquiring target!");

                break;
            case State.SHOOTING:

                CountDownToShoot();

                break;
            case State.RELOADING:

                CountDownToReload();

                if (!statusIndicated)
                    IndicateStatus("Reloading...");

                break;
            case State.MANUAL_CONTROL:
                //Use the mouse to rotate sight and Left Click to shoot
                Vector3 m = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                float z = Mathf.Atan2((m.y - sightStart.position.y), (m.x - sightStart.position.x)) * Mathf.Rad2Deg - 90;
                sightStart.rotation = Quaternion.AngleAxis(z, Vector3.forward);
                break;
            case State.MANUAL_SHOOTING:
                // Run Shoot countdown and go back to manual control after shooting
                CountDownToShoot();
                break;
            default:
                // Building is Starved
                break;
        }

    }


    void IndicateStatus(string status)
    {
        if (buildingStatusIndicator != null)
        {
            buildingStatusIndicator.CreateStatusMessage(status);

            statusIndicated = true;
        }
    }

    void MarkTarget(Vector3 target)
    {

        GameObject marker = objPool.GetObjectForType("Target Particles", true, target);


    }


    void CountDownToShoot()
    {

        if (shootCountDown <= 0)
        {



            shootCountDown = stats.curRateOfAttk;

            // SHOOT
            if (targetUnit != null && targetUnit.activeSelf)
            {

                // Check gun has bullets
                if (ammoCount >= 1)
                {

                    // Spawn burst particles on my target to show my shot
                    VisualShooting();

                    // Damage Target unit
                    //HandleDamageToUnit();

                    ShootProjectile();


                    //						Debug.Log("MACHINE GUN: Shot fired! Ammo at: " + ammoCount);

                    // If we are in Manual Shoot, set state back to Manual Control after taking shot
                    if (_state == State.MANUAL_SHOOTING)
                    {
                        _state = State.MANUAL_CONTROL;
                    }


                }
                else
                {
                    // Need to reload. Set the reload count down to current reload time
                    reloadCountDown = gunStats.currReloadTime;

                    // If we are NOT Manual Shooting or Starved, change state to reload
                    if (_state != State.MANUAL_SHOOTING && _state != State.STARVED)
                    {

                        _state = State.RELOADING;

                        // indicate status reloading
                        statusIndicated = false;

                    }
                    else if (_state == State.MANUAL_SHOOTING)
                    {
                        // If we are manual shooting & not starved, reload manually
                        CountDownToReload();

                        // indicate status reloading
                        statusIndicated = false;
                        IndicateStatus("Reloading...");
                    }


                }

            }
            else
            {

                //Debug.Log("MACHINE GUN: Target is NULL!");
                // Target is null so we can go back to seeking if NOT starved
                if (_state != State.MANUAL_SHOOTING && _state != State.STARVED)
                {

                    // if we have Enemies in range we need to acquire target (just stops rotation so Trigger can rotate)

                    _state = State.SEEKING;

                    // indicate status seeking
                    statusIndicated = false;



                }
                else if (_state == State.MANUAL_SHOOTING)
                {

                    // Show shot ricochet off the floor
                    VisualShooting();

                    //And send back to Manual Control
                    _state = State.MANUAL_CONTROL;
                }
            }


        }
        else
        {
            shootCountDown -= Time.deltaTime;
        }

    }

    void CountDownToReload()
    {
        if (reloadCountDown <= 0)
        {

            // Reload
            ammoCount = gunStats.currAmmo;

            reloadCountDown = gunStats.currReloadTime;

            //			Debug.Log("MACHINE GUN: Gun Reloaded!");

            // Go back to Shooting if not in Manual or Starved
            if (_state != State.MANUAL_SHOOTING && _state != State.MANUAL_CONTROL && _state != State.STARVED)
            {

                if (enemyInRange)
                {
                    _state = State.SHOOTING;
                }
                else
                {
                    _state = State.SEEKING;
                }


            }
            else if (_state == State.MANUAL_SHOOTING)
            {

                // We ARE manually shooting and gun is reloaded, send back to Manual control
                _state = State.MANUAL_CONTROL;
            }

        }
        else
        {
            reloadCountDown -= Time.deltaTime;
        }


    }


    /// <summary>
    /// Gets a bullet from the pool of bullets and shoots it.
    /// The bullet itself is just for visual reference and will just Pool itself when it hits.
    /// </summary>
    void VisualShooting()
    {
        // Get the muzzle flash for this gun's laser
        GameObject muzzle = objPool.GetObjectForType("MachineGun_ShootFX", true, sightStart.position);
    }

    void ShootProjectile()
    {
        // Get the bullet from object pool using the name of the ammo type
        GameObject projectile = objPool.GetObjectForType("Tower Bullet", true, sightStart.position);
        if (projectile)
        {
           // projectile.GetComponent<SpriteRenderer>().sortingOrder = sprite_renderer.sortingOrder - 10;

            // and its direction
            Vector3 dir = targetUnit.transform.position - sightStart.position;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90;
            projectile.transform.eulerAngles = new Vector3(0, 0, angle);

            // Now fire the bullet trail right behind it
            GameObject bulletTrail = objPool.GetObjectForType("BulletTrail", true, sightStart.position);
            if (bulletTrail)
            {
                bulletTrail.transform.SetParent(projectile.transform);

                //// Place it in the correct local position
                //bulletTrail.transform.localPosition = new Vector3(-2f, 0, 0);

                // and its direction
                float trailAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                bulletTrail.transform.eulerAngles = new Vector3(0, 0, trailAngle);

            }

            // Initialize the Tower Bullet component with the DoDamage Callback and Trail gameObject
            projectile.GetComponent<Bullet_Tower>().InitBullet(CB_DoDamage, bulletTrail);

            // Play shoot sound
            // FIX THIS! THIS WILL BE ONE SOUND FITS ALL!
            Sound_Manager.Instance.PlaySound("Kinetic Rifle");

        }
        else
        {
            Debug.Log("MACHINE GUN: cant find Tower Bullet in Pool!");
        }
    }

    /// <summary>
    /// Handles the damage to unit by using
    /// method from Unit_Base class.
    /// </summary>
    void HandleDamageToUnit(Unit_Base target)
    {
        if (AttackUnit(target))
        {

            // Tell the enemy this Tower is attacking it if it doesn't already have an attacker
            if (target.gameObject.GetComponent<Enemy_AttackHandler>().attackingTower == null)
                target.gameObject.GetComponent<Enemy_AttackHandler>().attackingTower = gameObject;

            // Lose a bullet
            ammoCount--;

      
        }
        else
        {
            targetUnit = null;

            // Target is null, go back to Seeking IF NOT in Manual Control or Starved
            if (_state != State.MANUAL_SHOOTING && _state != State.STARVED)
            {

                // if we have Enemies in range we need to acquire target (just stops rotation so Trigger can rotate)
                _state = State.SEEKING;

            }
            else if (_state == State.MANUAL_SHOOTING)
            {
                // We are Manually Shooting, so send back to Manual Control
                _state = State.MANUAL_CONTROL;
            }
        }

    }

    void OnTriggerStay2D(Collider2D coll)
    {

        // NOT in Manual Control:
        if (coll.gameObject.CompareTag("Enemy") && targetUnit == null && _state != State.STARVED && _state != State.MANUAL_CONTROL)
        {



            // Rotate to the new target
            float z = Mathf.Atan2((coll.transform.position.y - sightStart.position.y), (coll.transform.position.x - sightStart.position.x)) * Mathf.Rad2Deg - 90;
            sightStart.rotation = Quaternion.AngleAxis(z, Vector3.forward);
            enemyInRange = true;

            // IN Manual Control; 
        }
        else if (coll.gameObject.CompareTag("Enemy") && targetUnit == null && _state == State.MANUAL_CONTROL)
        {

            enemyInRange = true;

            // Rotate towards Enemy whenever target IS in range AND targetUnit is not Null & state is not Starved
        }
        else if (coll.gameObject.CompareTag("Enemy") && targetUnit != null && _state != State.STARVED)
        {
            // Already have a target, keep rotating with it
            float z = Mathf.Atan2((targetUnit.transform.position.y - sightStart.position.y), (targetUnit.transform.position.x - sightStart.position.x)) * Mathf.Rad2Deg - 90;
            sightStart.rotation = Quaternion.AngleAxis(z, Vector3.forward);
        }
    }

    void OnTriggerExit2D(Collider2D coll)
    {
        if (coll.gameObject.CompareTag("Enemy") && targetUnit != null)
        {

            targetUnit = null;
            enemyInRange = false;

            // Change state back to seeking IF NOT in Manual Control or Manual Shooting, Starved, Reload or Shooting
            if (_state != State.MANUAL_CONTROL && _state != State.MANUAL_SHOOTING && _state != State.STARVED && _state != State.RELOADING && _state != State.SHOOTING)
                _state = State.SEEKING;
        }
    }


}
