using UnityEngine;
using System.Collections;

public class Player_GunBaseClass : MonoBehaviour {

    public GunStats gunStats;


	public Transform sightStart, sightEnd; // where the gun's range starts and ends
	
	public LayerMask mask;

	public GameObject targetHit, targetInSight;

	float countDownToFire = 0; // counts up in seconds until it reaches fire rate

	public bool canFire = false;

	public ObjectPool objPool;

	public Vector3 mousePosition;

//	public bool fireButtonPressed;

	public Rigidbody2D rigid_body;

	// Handle the weapon sorting layer from here
	public SpriteRenderer sprite_renderer;

	public Transform bulletTrailFab;

	public GameMaster gameMaster;

    public bool isReloading { get; protected set; }

    public Unit_StatusIndicator status_Indicator { get; protected set; }

	void Awake()
	{
		//sprite_renderer = GetComponent<SpriteRenderer> ();
		//parent_srenderer = GetComponentInParent<SpriteRenderer> ();

        objPool = ObjectPool.instance;

	}

	void Update()
	{
		// keep sorting order always + 1 players sorting order
//		sprite_renderer.sortingOrder = parent_srenderer.sortingOrder + 1;

//		if (gameMaster) {
//			if (gameMaster._canFireWeapon){
//				canFire = true;
//			}else{
//				canFire = false;
//			}
//		}

	}
	public void FollowMouse()
	{
		mousePosition = Camera.main.ScreenToWorldPoint (Input.mousePosition);
       
		float mouseDirection = Mathf.Atan2 ((mousePosition.y - sightStart.position.y), (mousePosition.x - sightStart.position.x)) * Mathf.Rad2Deg - 90;		
		if (mousePosition != transform.root.position) {
			sightStart.rotation = Quaternion.AngleAxis (mouseDirection, Vector3.forward);
			transform.rotation = Quaternion.AngleAxis (mouseDirection, Vector3.forward);
            mousePosition.z = 0;
            
        }

	}

    //public void CanFire()
    //{

    //    if (countDownToFire >= gunStats.startingFireRate)
    //    {
    //        canFire = true;

    //    }
    //    else
    //    {
    //        countDownToFire += Time.deltaTime;
    //    }
    //}

    public void CheckForShoot()
	{

		if (gunStats.curChamberAmmo > 0)
        {
            if (gunStats.curFireRate == 0)
            {
                if (Input.GetButtonDown("Fire1"))
                {
                    //if (gameMaster){
                    //	if (gameMaster._canFireWeapon) 
                    //		FireWeapon ();
                    //}else{
                    //	Debug.Log("GUN: GM is null!!!");
                    //}
                    FireWeapon();
                }

            }
            else
            {
                if (Input.GetButton("Fire1") && Time.time > countDownToFire)
                {
                    //if (gameMaster){
                    //	if (gameMaster._canFireWeapon) {
                    //		countDownToFire = Time.time + 1 / gunStats.curFireRate;
                    //		FireWeapon ();
                    //	}
                    //}
                    countDownToFire = Time.time + 1 / gunStats.curFireRate;
                    FireWeapon();
                }
            }
        }
        else
        {
            // Chamber out of Bullets, must Reload
            if (!isReloading)
            {
                isReloading = true;

                status_Indicator.CreateStatusMessage("Reloading!");

                StartCoroutine("Reload");
            }
        }
	

	}

    void FireWeapon()
    {
        if (!Build_MainController.Instance.currentlyBuilding)
        {
            // Fire an Raycast towards the mouse to check for a hit
            targetInSight = RaycastToGetTarget();

            // Actually shoot the bullet
            VisualProjectileShoot();

            // Play gun shot sound
            Sound_Manager.Instance.PlaySound(gameObject.name);

            if (targetInSight != null)
            {
                
                // Apply gun kick to Player
                StartCoroutine(GunKick());
                // Take a bullet from the gun's chamber ammo
                gunStats.curChamberAmmo--;
            }

       
        }
	}


	
	GameObject RaycastToGetTarget()
	{
		Debug.DrawLine (transform.position, sightEnd.position, Color.cyan);

		RaycastHit2D hit = Physics2D.Linecast (sightStart.position, sightEnd.position, mask.value);
		if (hit.collider != null) {

			if (hit.collider.CompareTag ("Enemy")) {
               // Debug.Log("GUN DETECTED ENEMY!");
				// Linecast HIT an enemy, so store the enemy unit as the target
				return hit.collider.gameObject;

			} else {
				return null;
			}
		} else {
			return null;
		}
		
	}

	void VisualProjectileShoot()
	{
		string _projectileType = gunStats.projectileType;

		// Get the bullet from object pool using the name of the ammo type
		GameObject projectile = objPool.GetObjectForType (_projectileType, true, sightStart.position);
		if (projectile) {
			//Debug.Log("Firing " + _projectileType + " !");
            //			// give it the position of the player
            //			projectile.transform.position = sightStart.position;
            projectile.GetComponent<SpriteRenderer>().sortingOrder = sprite_renderer.sortingOrder - 10;

			// and access to this gun to do damage
			projectile.GetComponent<Bullet_Player> ().myWeapon = this;

			// and its direction
			Vector3 dir = mousePosition - sightStart.position;
			float angle = Mathf.Atan2 (dir.y, dir.x) * Mathf.Rad2Deg - 90;
			projectile.transform.eulerAngles = new Vector3 (0, 0, angle);

			// Now fire the bullet trail right behind it
			GameObject bulletTrail = objPool.GetObjectForType("BulletTrail", true, sightStart.position);
			if (bulletTrail){
                bulletTrail.GetComponent<LineRenderer>().sortingOrder = sprite_renderer.sortingOrder - 10;
                // and its direction
                float trailAngle = Mathf.Atan2 (dir.y, dir.x) * Mathf.Rad2Deg;
				bulletTrail.transform.eulerAngles = new Vector3 (0, 0, trailAngle);
				bulletTrail.transform.SetParent(projectile.transform);
			}

		} else {
			Debug.Log("cant find " + _projectileType + " in Pool!");
		}
	}

//	void GunKick ()
//	{
//		Vector2 kickDirection = mousePosition - transform.root.position;
//		rigid_body.AddForce (-kickDirection * gunStats.kickAmmt);
//		rigid_body.AddForce (kickDirection * gunStats.kickAmmt);
//
//
//	}

	IEnumerator GunKick()
	{
		Vector2 kickDirection = mousePosition - transform.root.position;
		rigid_body.AddForce (-kickDirection * gunStats.kickAmmt);
		yield return new WaitForSeconds (0.1f);
		rigid_body.AddForce (kickDirection * gunStats.kickAmmt);
		yield break;
	}


    IEnumerator Reload()
    {
        while (true)
        {
            // Wait for time in seconds according to this gun's Reload Speed stat
            yield return new WaitForSeconds(gunStats.curReloadSpeed);
            // Add bullets
            gunStats.curChamberAmmo = gunStats.startingChamberAmmo;
            isReloading = false;
            yield break;
        }
    }

}
