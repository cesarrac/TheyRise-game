using UnityEngine;
using System.Collections;

public class Wave_Bobbing : MonoBehaviour {

    public float scale = 1;
    public float heightScale = 1;
    public float widthScale = 1;

    public float z = 0;

    float waveTime = 10f;
    bool isCounting = false;

    public bool bobOnStart = true;
    bool canBob = false;

    public bool useLocalPosition = true;

    void Awake()
    {
        if (bobOnStart)
        {
            canBob = true;
        }
    }

	void Update () {

        if (bobOnStart)
        {
            float y = heightScale * Mathf.PerlinNoise(Time.time + (transform.position.x * scale), Time.time + (transform.position.y * scale));
            float x = widthScale * Mathf.PerlinNoise(Time.time + (transform.position.x * scale), Time.time + (transform.position.y * scale));

            if (useLocalPosition)
            {
                transform.localPosition = new Vector3(x, y, z);

            }
            else
            {
                transform.position = new Vector3(x, y, z);
            }
        }
        else
        {
            if (canBob)
            {
                float y = heightScale * Mathf.PerlinNoise(Time.deltaTime + (transform.position.x * scale), Time.deltaTime + (transform.position.y * scale));
                float x = widthScale * Mathf.PerlinNoise(Time.deltaTime + (transform.position.x * scale), Time.deltaTime + (transform.position.y * scale));

                if (useLocalPosition)
                {
                    transform.localPosition = new Vector3(x, y, z);

                }
                else
                {
                    transform.position = new Vector3(x, y, z);
                }
            }
        }



        
    }

    public void CanBob()
    {
        canBob = true;
    }

    IEnumerator Wave()
    {
        while(true)
        {
            if (waveTime > 0)
            {
                waveTime -= Time.time;
                float y = heightScale * Mathf.PerlinNoise(Time.time + (transform.position.x * scale), Time.time + (transform.position.y * scale));
                float x = widthScale * Mathf.PerlinNoise(Time.time + (transform.position.x * scale), Time.time + (transform.position.y * scale));
                transform.localPosition = new Vector3(x, y,z);

            }
            else
            {
                yield break;
            }

        }
    }

    IEnumerator CountToNextWave()
    {
        while (true)
        {
            yield return new WaitForSeconds(5f);

            waveTime = 10f;

            StartCoroutine("Wave");

            yield break;
        }
    }
}
