using UnityEngine;
using System.Collections;

public class Player_HandDrill : MonoBehaviour {

    /* As long as the Player is holding down the fire button, a beam will shoot from the gun and "mine" the target, IF the mouse if over a tile that is of
    tile type rock or mineral. */


    // How long does it take to mine?
    public float miningTime;
    private float miningCountDown;

    // How much does it mine?
    public int mineAmmnt;

    // Access the Resource Grid to know what type of rock player clicks on
    ResourceGrid resourceGrid;
    ObjectPool objPool;

    enum State {MINING, EXTRACTING};
    State _state = State.MINING;

    // Mouse position as int
    int mX, mY;

    // Where the beam shoots from
    public Transform sightStart;
    // Range is a set float, when trying to mine calculate distance between mouse and sighStart, if its <= to range THEN player can mine
    public float range = 5;
    Vector2 sightV2, mouseV2;
    Vector3 m;

    // Using a Line Renderer on the Sight Start object to shoot the beam
    LineRenderer lineR;

    SpriteRenderer parentRenderer;
    int mySortingOrder;

    public LayerMask mask;
	
    void OnEnable()
    {
        miningCountDown = miningTime;
    }

    void Start()
    {
        

        resourceGrid = GetComponentInParent<Player_MoveHandler>().resourceGrid;
        objPool = GetComponentInParent<Player_HeroAttackHandler>().objPool;

        lineR = sightStart.GetComponent<LineRenderer>();
        // Set line renderer sorting layer
        lineR.sortingLayerName = GetComponentInParent<SpriteRenderer>().sortingLayerName;
       
        if (!resourceGrid) Debug.Log("WTF?! No grid attached!");
    }

    void Update () {
        
        MyStateMachine(_state);
        FollowMouse();
	}

    public void FollowMouse()
    {
        m = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        float mouseDirection = Mathf.Atan2((m.y - sightStart.position.y), (m.x - sightStart.position.x)) * Mathf.Rad2Deg - 90;
        if (m != transform.root.position)
        {
            sightStart.rotation = Quaternion.AngleAxis(mouseDirection, Vector3.forward);
            transform.rotation = Quaternion.AngleAxis(mouseDirection, Vector3.forward);
        }

    }

    void MyStateMachine(State _curState)
    {
        switch (_curState)
        {
         
            case State.MINING:
                // If Player continues to hold down button, show Beam and start timer
                if (Input.GetMouseButtonDown(0))
                {
                    sightV2 = new Vector2(sightStart.position.x, sightStart.position.y);
                    mouseV2 = new Vector2(m.x, m.y);

                    var mouseX = m.x;

                    // Keep making sure the mouse stays in range
                    var distance = (mouseV2 - sightV2).sqrMagnitude;

                    if (distance <= range)
                    {
                        mX = Mathf.RoundToInt(m.x);
                        mY = Mathf.RoundToInt(m.y);

                        if (resourceGrid.CheckIsInMapBounds(mX, mY))
                        {
                            if (resourceGrid.GetTileType(mX, mY) == TileData.Types.rock || resourceGrid.GetTileType(mX, mY) == TileData.Types.mineral)
                            {
                                StopCoroutine(Mining(mouseX));
                                StartCoroutine(Mining(mouseX));
                            }
                        }
                    }
                    else
                    {
                        // The moment it breaks range stop mining and go to idle
                        // _state = State.IDLE;
                        Debug.Log("Drill NOT in range!");
                    }
                    
                }

                

                break;

            case State.EXTRACTING:
                // Give the player resources the ammount extracted
                Extract(mX, mY);
                // if player is still pressing button, go to mining again
                _state = State.MINING;
                break;

            default:
                _state = State.MINING;
                break;
        }

    }

    void Mine()
    {
        if (miningCountDown <= 0)
        {
            miningCountDown = miningTime;
            _state = State.EXTRACTING;
        }
        else
        {
            //Debug.Log("Counting down to mine");

            miningCountDown -= Time.deltaTime;
        }
    }

    /*
    void ShootMiningBeam(Vector3 mousePosition)
    {
        // Get it from pool first
        GameObject beam = objPool.GetObjectForType("Mining Beam", true, sightStart.position);
        if (beam)
        {
            // Give it a direction
            Vector3 dir = mousePosition - sightStart.position;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90;
            beam.transform.eulerAngles = new Vector3(0, 0, angle);
        }
    }
    */

    IEnumerator Mining(float mX)
    {
        lineR.enabled = true;

        var offset = 1f;

        while (Input.GetMouseButton(0) && mX >= (m.x - offset) && mX <= (m.x + offset))
        {

        
           RaycastHit2D ray = Physics2D.Raycast(sightStart.position, sightStart.up, range, mask);
           if (ray.collider != null)
           {
                if (ray.collider.gameObject.CompareTag("Rock"))
                {
                    lineR.SetPosition(0, sightStart.position);
                    lineR.SetPosition(1, ray.point);
                    // Mine
                    Mine();
                }
        
           }

                    
             


            // mX = Mathf.RoundToInt(m.x);
            // mY = Mathf.RoundToInt(m.y);

            /*
            if (resourceGrid.CheckIsInMapBounds(mX, mY))
            {
                if (resourceGrid.GetTileType(mX, mY) == TileData.Types.rock || resourceGrid.GetTileType(mX, mY) == TileData.Types.mineral)
                {
                    RaycastHit2D ray = Physics2D.Raycast(sightStart.position, sightStart.up, range, mask);
                    if (ray.collider != null)
                    {
                        lineR.SetPosition(0, sightStart.position);
                        lineR.SetPosition(1, ray.point);
                    }
                   
                    // Mine
                    Mine();
                }
            }
          */

            yield return null;
        }

        lineR.enabled = false;
    }

    void Extract(int x, int y)
    {
        if (resourceGrid.MineARock(x, y, mineAmmnt, true) > 0)
        {
            Debug.Log("Extracting " + resourceGrid.MineARock(x, y, mineAmmnt) + " out of " + resourceGrid.tiles[x,y].maxResourceQuantity);
        }
       
       

       
    }

    
}
