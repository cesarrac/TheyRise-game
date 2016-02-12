using UnityEngine;
using System.Collections;

public class EnemyAttackHandler_Grunt : Enemy_AttackHandler {

    void OnEnable()
    {
        ResetFlagsandTargets();
    }

    void Awake()
    {
        AttackActionCB = Attack;
        AttackRange = 5;
        pathHandler = GetComponent<Enemy_PathHandler>();
        audio_source = GetComponent<AudioSource>();
        rigid_body = GetComponent<Rigidbody2D>();
    }

    void Attack(Vector3 targetPos)
    {
        StartCoroutine(JumpAttack(targetPos));
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
