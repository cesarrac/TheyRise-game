using UnityEngine;
using System.Collections;
using System;

public class Bullet_Tower : MonoBehaviour {

    // When this Bullet impacts an enemy, it calls a Callback action on the Tower that shot it...
    // ... and does Damage!

    public float bulletSpeed;
    Rigidbody2D rb;

    Action<Unit_Base> DoDamage;

    GameObject bulletTrail;

    public void InitBullet(Action<Unit_Base> damageCallback, GameObject trail)
    {
        DoDamage = damageCallback;

        bulletTrail = trail;
    }

    void Update()
    {
        transform.position += transform.up * bulletSpeed * Time.deltaTime;
    }

    void OnTriggerEnter2D(Collider2D coll)
    {
        if (coll.gameObject.CompareTag("Enemy"))
        {

            // Apply Damage
            DoDamage(coll.gameObject.GetComponent<Unit_Base>());

            // Get an explosion for the VFX of laser hitting enemy (Burst particle)...
            GameObject explosion = ObjectPool.instance.GetObjectForType("Burst Particles", true, coll.transform.position);

            if (explosion != null)
            {
                // ... get the target's Sprite Renderer's layer
                string targetLayer = coll.gameObject.GetComponent<SpriteRenderer>().sortingLayerName;

                // ... assign layer to Particle Renderer
                explosion.GetComponent<ParticleSystemRenderer>().sortingLayerName = targetLayer;
            }


            // Pool the Bullet Trail that is a child of this gameObject (They need to be pooled separately)
            if (bulletTrail != null)
                ObjectPool.instance.PoolObject(bulletTrail);

            // ... and finally Pool this bullet gameObject.
            ObjectPool.instance.PoolObject(gameObject);
        }
    }
}
