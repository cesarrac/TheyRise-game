using UnityEngine;
using System.Collections;

public class PlanktonMine : MonoBehaviour {

    Vector2 targetPosition;
    Unit_Base target;

    float poisonDamage = 5f;
    float damageTime = 5f;
    bool isDamaging;

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

            StartCoroutine("Damage");

        }
    }

    IEnumerator Damage()
    {
        isDamaging = true;
        float timeElapsed = 0;

        while (true)
        {

            DoDamage();

            yield return new WaitForSeconds(1f);

            timeElapsed += 1;

            if (timeElapsed >= damageTime || target != null)
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

        Debug.Log("PLANKTON MINE: Damaging Hero for " + poisonDamage);
    }
}
