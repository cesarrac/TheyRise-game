using UnityEngine;
using System.Collections;

public class Equip_Weapon : MonoBehaviour {

    public Transform Weapon_Down, Weapon_Right, Weapon_Left, Weapon_Up;
    public SpriteRenderer wpn_render_Down, wpn_render_Left, wpn_render_Right, wpn_render_Up;

    Animator anim;

    Player_HeroAttackHandler attk_handler;

    public GameObject equipment_Holder;

    void Awake()
    {
        anim = GetComponentInChildren<Animator>();
        attk_handler = GetComponent<Player_HeroAttackHandler>();
    }


    public void SwitchTransform(float x, float y)
    {
        // Reset the wpn's Sprite Renderer x and y flip values
        equipment_Holder.GetComponent<SpriteRenderer>().flipX = false;
        equipment_Holder.GetComponent<SpriteRenderer>().flipY = false;

        if (x == 0 && y == 1)
        {
            // up

            // First Set Parent...
            equipment_Holder.transform.SetParent(Weapon_Up);

            // ... and Reset their transform
            equipment_Holder.transform.localPosition = Vector3.zero;
            equipment_Holder.transform.localRotation = Quaternion.Euler(0, 0, 0);
            equipment_Holder.transform.localScale = new Vector3(1, 1, 1);

            // ... then "sort" its sorting layer
            equipment_Holder.GetComponent<Weapon_SortingLayer>().CheckSortingLayer(wpn_render_Up);
        }
        else if (x == 0 && y == -1)
        {
            // down 

            // Set Parent...
            equipment_Holder.transform.SetParent(Weapon_Down, false);
            
            // ... and Reset their transform
            equipment_Holder.transform.localPosition = Vector3.zero;
            equipment_Holder.transform.localRotation = Quaternion.Euler(0, 0, 0);
            equipment_Holder.transform.localScale = new Vector3(1, 1, 1);


            // ... flip X is true for this case, so get the Sprite Renderer
            equipment_Holder.GetComponent<SpriteRenderer>().flipX = true;

            // ... then "sort" its sorting layer
            equipment_Holder.GetComponent<Weapon_SortingLayer>().CheckSortingLayer(wpn_render_Down);
        }
        else if (x == 1 && y == 0 || x == 1 && y == -1 || x == 1 && y == 1)
        {
            // right / right down / right up

            // Set Parent...
            equipment_Holder.transform.SetParent(Weapon_Right);

            // ... and Reset their transform
            equipment_Holder.transform.localPosition = Vector3.zero;
            equipment_Holder.transform.localRotation = Quaternion.Euler(0, 0, 0);
            equipment_Holder.transform.localScale = new Vector3(1, 1, 1);

            // ... then "sort" its sorting layer
            equipment_Holder.GetComponent<Weapon_SortingLayer>().CheckSortingLayer(wpn_render_Right);
        }
        else if (x == -1 && y == 0 || x == -1 && y == -1 || x == -1 && y == 1)
        {
            // left / left down / left up

            // Set Parent...
            equipment_Holder.transform.SetParent(Weapon_Left);

            // ... and Reset their transform
            equipment_Holder.transform.localPosition = Vector3.zero;
            equipment_Holder.transform.localRotation = Quaternion.Euler(0, 0, 0);
            equipment_Holder.transform.localScale = new Vector3(1, 1, 1);

            // ... flip X is true for this case, so get the Sprite Renderer
            equipment_Holder.GetComponent<SpriteRenderer>().flipX = true;

            // ... then "sort" its sorting layer
            equipment_Holder.GetComponent<Weapon_SortingLayer>().CheckSortingLayer(wpn_render_Left);
        }
    }

    //public void TransformSwitch(float x, float y)
    //{
    //    foreach(GameObject wpn in attk_handler.equipped_items)
    //    {

    //        if (x == 0 && y == 1)
    //        {
    //            // up
    //            wpn.transform.SetParent(Weapon_Up);
    //        }
    //        else if (x == 0 && y == -1)
    //        {
    //            // down 
    //            wpn.transform.SetParent(Weapon_Up);
    //        }
    //        else if (x == 1 && y == 0 || x == 1 && y == -1 || x == 1 && y == 1)
    //        {
    //            // right / right down / right up
    //            wpn.transform.SetParent(Weapon_Right);
    //        }
    //        else if (x == -1 && y == 0 || x == -1 && y == -1 || x == -1 && y == 1)
    //        {
    //            // left / left down / left up
    //            wpn.transform.SetParent(Weapon_Left);
    //        }

    //    }
    //}

    public void SwitchSprite(string name)
    {
        // This would switch the wpn Holder's sprite to the correct wpn sprite.
        equipment_Holder.GetComponent<SpriteRenderer>().sprite = Equipment_SpriteDatabase.Instance.GetSprite(name);

    }
}
