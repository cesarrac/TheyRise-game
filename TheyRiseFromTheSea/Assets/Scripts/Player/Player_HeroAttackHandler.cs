using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class Player_HeroAttackHandler : Unit_Base {
	Animator anim;

	[SerializeField]
	private SpriteRenderer weaponSprite;

	public GameMaster gameMaster;

    public List<GameObject> equipped_items = new List<GameObject>();
    int curItemIndex = 0;

    public Transform sightStart, sightEnd;

    Transform currArms;

    public float wpnRotationAngle { get; protected set; }

	void Awake()
    {

		anim = GetComponent<Animator> ();

        if (!gameMaster)
            gameMaster = GameMaster.Instance;

	}

    public void AddEquippedItems(GameObject item)
    {
        equipped_items.Add(item);
    }
	

	void Update ()
    {
		
//		if (Input.GetMouseButton (0)) {
//			anim.SetTrigger ("attack");
//			if (anim.GetFloat("input_y") > 0){
//				weaponSprite.sortingLayerName = "Units";
//				weaponSprite.sortingOrder = -10;
//			}else{
//				weaponSprite.sortingLayerName = "Units Above";
//				weaponSprite.sortingOrder = 10;
//			}
//		} else {
//			anim.ResetTrigger("attack");
//		}

		//if (scrollWheel != 0)
        if (Input.GetButtonDown("Next Tool"))
        {
            // Swap what item they are using
            SwapItems();
        }

        if (currArms != null)
        {
            ArmsFollowMouse();
        }

        //if (stats.curHP <= 0)
        //    Suicide();
    }

    public void SetCurrentArms(Transform arms)
    {
        if (arms != currArms)
        {
            currArms = arms;
        }
    }

    void ArmsFollowMouse()
    {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        //float mouseDirection = Mathf.Atan2((mousePosition.y - sightStart.position.y), (mousePosition.x - sightStart.position.x)) * Mathf.Rad2Deg - 90;
        //if (mousePosition != transform.root.position)
        //{
        //    sightStart.rotation = Quaternion.AngleAxis(mouseDirection, Vector3.forward);

        //    mousePosition.z = 0;

        //}

        // Check what Direction State our Equip Item is in to know what type of rotation we need

        Equip_Item equip = GetComponent<Equip_Item>();
        if (equip != null)
        {
            if (equip.equipState == Equip_Item.EquipState.DOWN || equip.equipState == Equip_Item.EquipState.RIGHT)
            {
                wpnRotationAngle = Mathf.Atan2((mousePosition.y - currArms.position.y), (mousePosition.x - currArms.position.x)) * Mathf.Rad2Deg + 90;

                if (mousePosition != transform.root.position)
                {

                    currArms.rotation = Quaternion.AngleAxis(wpnRotationAngle, Vector3.forward);

                    mousePosition.z = 0;

                }
            }
            else
            {
                wpnRotationAngle = Mathf.Atan2((mousePosition.y - currArms.position.y), (mousePosition.x - currArms.position.x)) * Mathf.Rad2Deg - 90;

                if (mousePosition != transform.root.position)
                {

                    currArms.rotation = Quaternion.AngleAxis(wpnRotationAngle, Vector3.forward);

                    mousePosition.z = 0;

                }
            }
        }

        Debug.DrawLine(sightStart.position, mousePosition, Color.cyan);

     
    }


    void SwapItems()
	{
        if (equipped_items.Count > 0)
        {
            // Check if by adding + 1 to current Index, if we are still within the Equipped Items array bounds
            if (curItemIndex + 1 < equipped_items.Count)
            {
                // If we are, deactivate the current item using the index...
                equipped_items[curItemIndex].SetActive(false);

                // ... add + 1 to index...
                curItemIndex += 1;

                // ... then activate the next item using the new index.
                equipped_items[curItemIndex].SetActive(true);
            }
            else
            {
                // Adding + 1 is out of array's range. So first deactivate the current item...
                equipped_items[curItemIndex].SetActive(false);

                // ... go back to the start of the Array...
                curItemIndex = 0;

                // ... and activate that first gameobject.
                equipped_items[curItemIndex].SetActive(true);
            }

            // tell the equipped weapon script to swap the Sprite to the correct activated Wpn/Tool
            GetComponent<Equip_Item>().SwitchSprite(equipped_items[curItemIndex].name);
        }

    }




	void Suicide()
	{
		// get a Dead sprite to mark my death spot
		GameObject deadE = objPool.GetObjectForType("dead", false, transform.position); // Get the dead unit object
		
		// make sure we Pool any Damage Text that might be on this gameObject
		if (GetComponentInChildren<Text>() != null){
			Text dmgTxt = GetComponentInChildren<Text>();
			objPool.PoolObject(dmgTxt.gameObject);
		}
		
		// and Pool myself
		objPool.PoolObject (this.gameObject);


        MasterState_Manager.Instance.mState = MasterState_Manager.MasterState.PLAYER_DEAD;
    }


 

}
