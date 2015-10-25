using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Player_HeroAttackHandler : Unit_Base {
	Animator anim;

	[SerializeField]
	private SpriteRenderer weaponSprite;


	//Weapons Hero is carrying (Selected in Expedition Supplies / Store screen)
	public GameObject _curTool, _curWeapon1, _curWeapon2;
	public static float scrollWheel { get { return Input.mouseScrollDelta.y / 10; } }

	public GameMaster gameMaster;

	void Awake(){

		anim = GetComponent<Animator> ();

		if (!gameMaster)
			gameMaster = GameObject.FindGameObjectWithTag ("GM").GetComponent<GameMaster> ();
	}

	void Start () {
		// Initialize Unit stats
		stats.Init ();
	}
	

	void Update () {
		
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

		if (scrollWheel != 0)
			// Swap what item they are using
			SwapItems(scrollWheel);


		if (stats.curHP <= 0)
			Suicide ();
	}

	void SwapItems(float scrollDelta)
	{
		if (scrollDelta > 0) {
			if (_curWeapon1.activeSelf) {

				_curWeapon1.SetActive (false);
				_curWeapon2.SetActive (true);
			}else if (_curWeapon2.activeSelf){
				_curWeapon1.SetActive (true);
				_curWeapon2.SetActive (false);
			}
		} else {
			if (_curWeapon2.activeSelf) {
				
				_curWeapon2.SetActive (false);
				_curWeapon1.SetActive (true);
			}else if (_curWeapon1.activeSelf){
				_curWeapon2.SetActive (true);
				_curWeapon1.SetActive (false);
			}
		}
		
	}




	void Suicide()
	{
		// get a Dead sprite to mark my death spot
		GameObject deadE = objPool.GetObjectForType("dead", false, transform.position); // Get the dead unit object
		if (deadE != null) {
			deadE.GetComponent<EasyPool> ().objPool = objPool;

		}
		
		// make sure we Pool any Damage Text that might be on this gameObject
		if (GetComponentInChildren<Text>() != null){
			Text dmgTxt = GetComponentInChildren<Text>();
			objPool.PoolObject(dmgTxt.gameObject);
		}
		
		// and Pool myself
		objPool.PoolObject (this.gameObject);
	}

}
