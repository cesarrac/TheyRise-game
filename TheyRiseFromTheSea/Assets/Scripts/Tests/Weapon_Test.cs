using UnityEngine;
using System.Collections;

public class Weapon_Test : MonoBehaviour {

    public GameObject testWeapon;

    Player_MoveHandler player_move;
    public GameObject player;
    Equip_Weapon equip_wpn;

    void Start()
    {
        player_move = player.GetComponent<Player_MoveHandler>();
        equip_wpn = player.GetComponent<Equip_Weapon>();

        SpawnWeapon();
    }

    void SpawnWeapon()
    {
        //Once the weapon gets instantiated, parent it to the Front Rig's weapon Holder (this transform can be stored as a var on an EquipWeapon script on the Hero parent)
        ////        since this is the default direction.
        //GameObject wpn = Instantiate(testWeapon, player.transform.position, Quaternion.identity) as GameObject;

        //// Tell the Player Move Handler that this is the test weapon
        //player_move.test_wpn = wpn;

        // Use the method in Equip Weapon to set the default transform to down
       // equip_wpn.TestSwitch(0, -1f);

        // We would know the name of the first equipped Weapon or Tool from the GM, but here we are hardcoding it for test
        equip_wpn.SwitchSprite("Thunder");
    }
}
