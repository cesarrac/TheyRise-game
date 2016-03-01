using UnityEngine;
using System.Collections;

public class EasyPool : MonoBehaviour {

	public float timeBeforePool;

	private float poolCountdown;

	private bool haveStartedToCount;

	enum State { INIT, COUNTING, POOLED };

	private State state = State.POOLED;

    void OnEnable()
    {
        state = State.COUNTING;
        poolCountdown = timeBeforePool;
    }
	void Update () {

		if (state == State.POOLED)
			poolCountdown = timeBeforePool;


		if (poolCountdown <= 0 && state == State.COUNTING) {

			// Pool object
			ObjectPool.instance.PoolObject (this.gameObject);
			state = State.POOLED;


		} else {
            poolCountdown -= Time.deltaTime;
			
		}
		
	}
}
