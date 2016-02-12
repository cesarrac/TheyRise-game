using UnityEngine;
using System.Collections;

public class Player_MoveHandler : MonoBehaviour {

	[System.Serializable]
	public class MStats 
	{
		public float startSpeed = 2f;
		public float startDSpeed = 4f;
		private float _speed, _dashSpeed;
		public float speed { get{return _speed;} set{_speed = Mathf.Clamp(value, 1, 5);}}
		public float dashSpeed { get{return _dashSpeed;} set{_dashSpeed = Mathf.Clamp(value, 1, 20);}}


		public void Init()
		{
			speed = startSpeed;
			dashSpeed = startDSpeed;
		}
	}

	public MStats mStats = new MStats();

	public ResourceGrid resourceGrid;

	int mapX, mapY;

	public enum State { IDLING, ATTACKING, MOVING, DASHING };

	private State _state = State.IDLING;

	public State debugState;

	Animator anim;

	Vector2 move_vector;

	Rigidbody2D rBody;

	// DOUBLE-TAP DASH
	private float tapSpeed = 0.5f; // seconds
	private float lastTapTime = 0;
	private float timeDashing = 0.25f; // seconds that Hero can move using dash speed
	float dashCountdown;

	public Transform myWeapon;

    public bool isOnShip { get; protected set; }

    Vector3 currPos = new Vector3();
    Vector2 dashTarget = new Vector2();

    bool isDashing;

    Equip_Item equip_wpn;

    float dashDistance = 5f;

    void Awake()
	{
		anim = GetComponentInChildren<Animator> ();

		rBody = GetComponent<Rigidbody2D> ();

		mStats.Init ();

        if (Application.loadedLevel == 0)
        {
            isOnShip = true;
        }
        else
        {
            isOnShip = false;
        }


        if (!resourceGrid && !isOnShip)
            resourceGrid = ResourceGrid.Grid;

        equip_wpn = GetComponent<Equip_Item>();
	}



	void Start () {

		lastTapTime = 0;

        //if (!isOnShip)
        //{
        //    mapX = resourceGrid.mapSizeX;
        //    mapY = resourceGrid.mapSizeY;
        //}
	
	}
    

	void Update ()
    {
		debugState = _state;

		MyStateMachine (_state);
		// Store the axis Inputs in a Vector 2
		move_vector = new Vector2 (Input.GetAxisRaw ("Horizontal"), Input.GetAxisRaw ("Vertical"));
        
        if(_state == State.IDLING)
        {
            ListenForMouseDash();
        }


	}


	void MyStateMachine (State _curState){
		switch (_curState) {
		case State.ATTACKING:
			// attack
			break;
		case State.IDLING:
//			if (Input.GetKeyDown (KeyCode.W) || Input.GetKeyDown (KeyCode.A) || Input.GetKeyDown (KeyCode.S) || Input.GetKeyDown (KeyCode.D))
			// If Player tries to move
			    if (move_vector != Vector2.zero)
                {
                    //				if ((Time.time - lastTapTime) < tapSpeed && !isOnShip) {
                    //					// dashing
                    //					_state = State.DASHING;
                    ////					Move (mStats.dashSpeed);

                    //				}else{
                    //					// just moving
                    //					_state = State.MOVING;
                    ////					Move (mStats.speed);
                    //				}
                    //				lastTapTime = Time.time;

                    _state = State.MOVING;
                }

			break;
		case State.MOVING:
			// Move
			Move (mStats.speed);
			break;

		case State.DASHING:
                // Dash
                //if (dashCountdown <=0){
                //	dashCountdown = timeDashing;
                //	_state = State.MOVING;
                //}else{
                //	dashCountdown -= Time.deltaTime;
                //	Dash (mStats.dashSpeed);
                //}

                Dash(mStats.dashSpeed);


			break;

		default:
			// Starved!?
			break;
		}
	}

	void Move(float _curSpeed)
	{
        /* This is the logic for moving using the map coordinates to check if next tile is walkable:

                float _inputX = Input.GetAxis ("Horizontal");
                float _inputY = Input.GetAxis ("Vertical");
                float inputX = Mathf.Clamp (_inputX,(float) -mapX, (float) mapX);
                float inputY = Mathf.Clamp (_inputY,(float) -mapY, (float) mapY);

                Vector3 move = new Vector3 (inputX, inputY, 0.0f);

                Vector3 newMovePos = new Vector3 (transform.position.x + inputX, transform.position.y + inputY, 0.0f);

        //		if (CheckWalkabale (newMovePos)) {
        //
        //			transform.position += move * speed * Time.deltaTime;
        //
        //		}
                transform.position += move * speed * Time.deltaTime;

        */
        // Equip Weapon will need to know our direction
        //  equip_wpn.TransformSwitch(move_vector.x, move_vector.y);

        equip_wpn.CheckCurrentRigTransform(move_vector.x, move_vector.y);

		// Moving using Rigidbody 2D
		rBody.MovePosition(rBody.position + move_vector * _curSpeed * Time.deltaTime);


		// Control animation & return to Idling
		if (move_vector != Vector2.zero) {
			anim.SetBool ("isWalking", true);
			anim.SetFloat("input_x", move_vector.x);
			anim.SetFloat("input_y", move_vector.y);
		} else {
			anim.SetBool("isWalking", false);
			_state = State.IDLING;
		}

	}

	void Dash(float _curSpeed)
	{
		//rBody.MovePosition(rBody.position + move_vector * _curSpeed * Time.deltaTime);

        transform.position = Vector2.MoveTowards(transform.position, dashTarget, mStats.dashSpeed * Time.deltaTime);


        //var distance = (currPos - transform.position).sqrMagnitude;

        //if (distance >= dashDistance)
        //{
        //    _state = State.IDLING;
        //}

        if (dashCountdown <= 0)
        {
            _state = State.IDLING;
        }
        else
        {
            dashCountdown -= Time.deltaTime;
        }

    }


    void ListenForMouseDash()
    {
        if (Mouse_Controller.MouseController.isRightClickingForDash)
        {
            currPos = transform.position;

            dashTarget = Mouse_Controller.MouseController.currMouseP;

            dashCountdown = timeDashing;

            _state = State.DASHING;
           // StartCoroutine("Dashing");
        }
    }

    //IEnumerator Dashing()
    //{
    //    while (true)
    //    {
    //        transform.position = Vector2.MoveTowards(transform.position, dashTarget , mStats.dashSpeed * Time.deltaTime);
        

    //        var distance = (currPos - transform.position).sqrMagnitude;

    //        if (distance >= MouseBuilding_Controller.MouseController.dashDistance)
    //        {
    //            yield break;
    //        }

    //        yield return null;
    //    }
    //}

    //bool CheckWalkabale(Vector3 pos){
    //	int posX = Mathf.RoundToInt (pos.x);
    //	int posY = Mathf.RoundToInt (pos.y);

    //	if (resourceGrid.UnitCanEnterTile (posX, posY)) {
    //		return true;
    //	} else {
    //		return false;
    //	}

    //}

    //void MouseWeaponAim(Transform weapon)
    //{
    //	var mousePos = Camera.main.ScreenToWorldPoint (Input.mousePosition);
    //	Quaternion rot = Quaternion.LookRotation (weapon.position - mousePos, Vector3.forward);
    //	weapon.rotation = rot;
    //	Vector3 facingRot = new Vector3 (0, 0, weapon.eulerAngles.z);
    //	weapon.eulerAngles = facingRot;

    //}
}
