using UnityEngine;
using System.Collections;

public class EnemyAttackHandler_Belcher : Enemy_AttackHandler
{
    int mineAmmo = 1;

    void OnEnable()
    {
        InitPathfindingTargetAction();
        ResetFlagsandTargets();
    }

    void Awake()
    {
        AttackActionCB = Attack;
        AttackRange = 5;

        audio_source = GetComponent<AudioSource>();
        rigid_body = GetComponent<Rigidbody2D>();
    }


    void Attack(Vector3 targetPos)
    {
        if (mineAmmo > 0)
        {
            SpitMine(targetPos);
        }
        else
        {
            StartCoroutine(JumpAttack(targetPos));
        }
    }

    void SpitMine(Vector3 targetPos)
    {
        // Get the Plankton Mine from Pool...
        GameObject mine = ObjectPool.instance.GetObjectForType("Plankton Mine", true, transform.position);

        if (mine)
        {
            // ... and Spit it out, pushing it towards the target's position
            mine.GetComponent<PlanktonMine>().SetTarget(targetPos);

            // ... and subtract one Mine Ammo.
            mineAmmo--;
        }
    }

    IEnumerator JumpAttack(Vector3 targetPosition)
    {

        Vector2 jumpDirection = targetPosition - transform.root.position;
        rigid_body.AddForce(jumpDirection * 1200);
        yield return new WaitForSeconds(0.1f);
        rigid_body.AddForce(-jumpDirection * 1200);
        yield break;

    }


}
