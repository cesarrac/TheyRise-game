using UnityEngine;
using System.Collections;

public class Player_PickUpItems : MonoBehaviour {

    public ObjectPool objPool;

    public Player_ResourceManager resource_manager;

	Rock rock, mineral;

    void Start()
    {
        if (!objPool)
            objPool = ObjectPool.instance;

        if (!resource_manager)
            resource_manager = GameObject.FindGameObjectWithTag("Capital").GetComponent<Player_ResourceManager>();

		rock = new Rock (Rock.RockType.rock, Rock.RockSize.single);
		mineral = new Rock (Rock.RockType.mineral, Rock.RockSize.single);
    }

    // PICK UP CHUNKS OF ROCK!
    void OnCollisionEnter2D(Collision2D coll)
    {
        if (!objPool)
            objPool = ObjectPool.instance;

        if (!resource_manager)
            resource_manager = GameObject.FindGameObjectWithTag("Capital").GetComponent<Player_ResourceManager>();


        if (coll.gameObject.CompareTag("Rock Chunk"))
        {
            if (resource_manager && objPool)
            {
                resource_manager.AddOre(rock, 1);
                objPool.PoolObject(coll.gameObject);
            }
            else
            {
                Debug.Log("PLAYER PICKUP: Resource Manager & Object Pool not set!");
            }
        }

        if (coll.gameObject.CompareTag("Mineral Chunk"))
        {
            if (resource_manager && objPool)
            {
                resource_manager.AddOre(mineral, 1);
                objPool.PoolObject(coll.gameObject);
            }
            else
            {
                Debug.Log("PLAYER PICKUP: Resource Manager & Object Pool not set!");
            }
        }
    }
}
