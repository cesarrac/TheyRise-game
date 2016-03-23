using UnityEngine;
using System.Collections;

public class PlanktonMine : MonoBehaviour {

    Vector2 targetPosition;
    Unit_Base target;

    float poisonDamage = 5f;
    float damageTime = 5f;
    bool isDamaging;
    float timeElapsed;

    public void SetTarget(Vector3 pos)
    {
        targetPosition = pos;

        PushToTarget();
    }


    void PushToTarget()
    {
        var heading = targetPosition - (Vector2)transform.position;

        GetComponent<Rigidbody2D>().AddForce(heading * 30, ForceMode2D.Impulse);
    }

    void OnTriggerEnter2D(Collider2D coll)
    {
        if (coll.gameObject.CompareTag("Citizen"))
        {
            target = coll.gameObject.GetComponent<Unit_Base>();

            if (!isDamaging)
            {
                isDamaging = true;
                timeElapsed = 0;
            }
        }
    }

    IEnumerator Damage()
    {
        while (true)
        {

            DoDamage();

            yield return new WaitForSeconds(1f);

            timeElapsed += 1;

            if (timeElapsed >= damageTime || target == null || !target.gameObject.activeSelf)
            {
                isDamaging = false;
                target = null;
                yield break;
            }

        }
    }

    void DoDamage()
    {
        if (target != null)
            target.TakeDamage(poisonDamage);
    }
}
