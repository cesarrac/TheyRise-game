using UnityEngine;
using System.Collections;

public class Sword_TargetHandler : MonoBehaviour {

	public Player_HeroAttackHandler playerAttackHandler;

	BoxCollider2D myCollider;

	void Awake(){
		myCollider = GetComponent<BoxCollider2D> ();

		if (playerAttackHandler == null) {
			Debug.Log("SWORD Target Handler: Cannot find Player Attack Handler!!");
			myCollider.enabled = false;
		}
	}

	void OnTriggerEnter2D(Collider2D coll){
		//if (coll.gameObject.tag == "Enemy") {
		//	Debug.Log("Hit!");
		//	playerAttackHandler.AttackOtherUnit(coll.gameObject.GetComponent<Enemy_AttackHandler>());
		//}
	}
}
