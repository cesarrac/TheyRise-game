using UnityEngine;
using System.Collections;

public class Equip_Item : MonoBehaviour {

    public Transform Weapon_Down, Weapon_Right, Weapon_Left, Weapon_Up;
    public SpriteRenderer wpn_render_Down, wpn_render_Left, wpn_render_Right, wpn_render_Up;

    Animator anim;

    Player_HeroAttackHandler attk_handler;

    public GameObject equipment_Holder;

    public enum EquipState { UP, DOWN, RIGHT, LEFT }

    public EquipState equipState { get; protected set; }

    public Transform sightStart;

    void Awake()
    {
        anim = GetComponentInChildren<Animator>();
        attk_handler = GetComponent<Player_HeroAttackHandler>();
    }

    public void CheckCurrentRigTransform(float x, float y)
    {
        if (x == 0 && y == 1)
        {
            // up
            if (equipState != EquipState.UP)
            {
                SwitchRig(EquipState.UP);
            }
        }
        else if (x == 0 && y == -1)
        {
            // down
            if (equipState != EquipState.DOWN)
            {
                SwitchRig(EquipState.DOWN);
            }
        }
        else if (x == 1 && y == 0 || x == 1 && y == -1 || x == 1 && y == 1)
        {
            // right / right down / right up
            if (equipState != EquipState.RIGHT)
            {
                SwitchRig(EquipState.RIGHT);
            }
        }
        else if (x == -1 && y == 0 || x == -1 && y == -1 || x == -1 && y == 1)
        {
            // left / left down / left up
            if (equipState != EquipState.LEFT)
            {
                SwitchRig(EquipState.LEFT);
            }
        }
    }

    public void SwitchRig(EquipState _curState)
    {
        // Reset the wpn's Sprite Renderer x and y flip values
        equipment_Holder.GetComponent<SpriteRenderer>().flipX = false;
        equipment_Holder.GetComponent<SpriteRenderer>().flipY = false;

        switch (_curState)
        {
            case EquipState.DOWN:
                // Switch to down rig

                equipState = EquipState.DOWN;

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

                // ... tell the Attack Handler what the current active player arms are (for rotation)
                attk_handler.SetCurrentArms(Weapon_Down.transform.parent.parent);

                // .. Parent the sightStart to the correct holder
                sightStart.SetParent(Weapon_Down, false);

                // ... and reset its Transform
                sightStart.transform.localPosition = new Vector3(0, 0.12f, 0);
                sightStart.transform.localRotation = Quaternion.Euler(0, 0, 0);
                sightStart.transform.localScale = new Vector3(1, 1, 1);

                break;
            case EquipState.LEFT:
                // Switch to LEFT rig

                equipState = EquipState.LEFT;

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

                // ... tell the Attack Handler what the current active player arms are (for rotation)
                attk_handler.SetCurrentArms(Weapon_Left.transform.parent.parent);


                // .. Parent the sightStart to the correct holder
                sightStart.SetParent(Weapon_Left, false);

                // ... and reset its Transform
                sightStart.transform.localPosition = new Vector3(0, 0.18f, 0);
                sightStart.transform.localRotation = Quaternion.Euler(0, 0, 0);
                sightStart.transform.localScale = new Vector3(1, 1, 1);

                break;
            case EquipState.UP:
                // Switch to up rig

                equipState = EquipState.UP;

                // First Set Parent...
                equipment_Holder.transform.SetParent(Weapon_Up);

                // ... and Reset their transform
                equipment_Holder.transform.localPosition = Vector3.zero;
                equipment_Holder.transform.localRotation = Quaternion.Euler(0, 0, 0);
                equipment_Holder.transform.localScale = new Vector3(1, 1, 1);

                // ... then "sort" its sorting layer
                equipment_Holder.GetComponent<Weapon_SortingLayer>().CheckSortingLayer(wpn_render_Up);

                // ... tell the Attack Handler what the current active player arms are (for rotation)
                attk_handler.SetCurrentArms(Weapon_Up.transform.parent.parent);

                // .. Parent the sightStart to the correct holder
                sightStart.SetParent(Weapon_Up, false);

                // ... and reset its Transform
                sightStart.transform.localPosition = new Vector3(0, 0.12f, 0);
                sightStart.transform.localRotation = Quaternion.Euler(0, 0, 0);
                sightStart.transform.localScale = new Vector3(1, 1, 1);

                break;
            case EquipState.RIGHT:
                // Switch to right rig

                equipState = EquipState.RIGHT;

                // Set Parent...
                equipment_Holder.transform.SetParent(Weapon_Right);

                // ... and Reset their transform
                equipment_Holder.transform.localPosition = Vector3.zero;
                equipment_Holder.transform.localRotation = Quaternion.Euler(0, 0, 0);
                equipment_Holder.transform.localScale = new Vector3(1, 1, 1);

                // ... then "sort" its sorting layer
                equipment_Holder.GetComponent<Weapon_SortingLayer>().CheckSortingLayer(wpn_render_Right);

                // ... tell the Attack Handler what the current active player arms are (for rotation)
                attk_handler.SetCurrentArms(Weapon_Right.transform.parent.parent);

                // .. Parent the sightStart to the correct holder
                sightStart.SetParent(Weapon_Right, false);

                // ... and reset its Transform
                sightStart.transform.localPosition = new Vector3(0, 0.18f, 0);
                sightStart.transform.localRotation = Quaternion.Euler(0, 0, 0);
                sightStart.transform.localScale = new Vector3(1, 1, 1);

                break;
            default:
                // Switch to Down rig

                equipState = EquipState.DOWN;

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

                // ... tell the Attack Handler what the current active player arms are (for rotation)
                attk_handler.SetCurrentArms(Weapon_Down.transform.parent.parent);


                // .. Parent the sightStart to the correct holder
                sightStart.SetParent(Weapon_Down, false);

                // ... and reset its Transform
                sightStart.transform.localPosition = new Vector3(0, 0.12f, 0);
                sightStart.transform.localRotation = Quaternion.Euler(0, 0, 0);
                sightStart.transform.localScale = new Vector3(1, 1, 1);

                break;

        }
    }



    public void SwitchSprite(string name)
    {
        // This would switch the wpn Holder's sprite to the correct wpn sprite.
        equipment_Holder.GetComponent<SpriteRenderer>().sprite = Equipment_SpriteDatabase.Instance.GetSprite(name);

    }
}
