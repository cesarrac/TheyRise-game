using UnityEngine;
using System.Collections;

public class Player_PickUpItems : MonoBehaviour {

    public ObjectPool objPool;

 //   public Player_ResourceManager resource_manager;

	//Rock rock, mineral;

 //   bool isOnShip = false;

    void Start()
    {
        //if (GetComponent<Player_MoveHandler>().isOnShip)
        //{
        //    isOnShip = true;
        //}
        //else
        //{
        //    isOnShip = false;
        //}


        //if (!isOnShip)
        //{
        //    if (!objPool)
        //        objPool = ObjectPool.instance;

        //    if (!resource_manager)
        //        resource_manager = GameObject.FindGameObjectWithTag("Capital").GetComponent<Player_ResourceManager>();

        //    rock = new Rock(Rock.RockType.sharp, Rock.RockSize.single);
        //    mineral = new Rock(Rock.RockType.hex, Rock.RockSize.single);
        //}
 
    }

    // PICK UP CHUNKS OF ROCK!
    void OnCollisionEnter2D(Collision2D coll)
    {
        if (coll.gameObject.CompareTag("Resource Drop"))
        {
            if (coll.gameObject.GetComponent<ResourceDrop>() != null)
            {
                coll.gameObject.GetComponent<ResourceDrop>().PickUp();
            }
        }
       
        //if (coll.gameObject.CompareTag("Rock Chunk"))
        //{
        //    if (resource_manager && objPool)
        //    {
        //        resource_manager.AddOre(rock, 1);
        //        objPool.PoolObject(coll.gameObject);
        //    }
        //    else
        //    {
        //        Debug.Log("PLAYER PICKUP: Resource Manager & Object Pool not set!");
        //    }
        //}

        //if (coll.gameObject.CompareTag("Mineral Chunk"))
        //{
        //    if (resource_manager && objPool)
        //    {
        //        resource_manager.AddOre(mineral, 1);
        //        objPool.PoolObject(coll.gameObject);
        //    }
        //    else
        //    {
        //        Debug.Log("PLAYER PICKUP: Resource Manager & Object Pool not set!");
        //    }
        //}
    }
}
