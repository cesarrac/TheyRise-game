using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class FadeToPool : MonoBehaviour {

	SpriteRenderer sr;
    Color A = Color.white;
    Color B = Color.clear;
    public float colorChangeDuration = 2;
    private float colorTime;

    public bool trueIfSprite;
	Image img;

    bool isFading;

    void OnEnable()
    {
        if (trueIfSprite)
        {
            sr = GetComponent<SpriteRenderer>();
            sr.color = A;

            isFading = true;
        }
        else
        {
            img = GetComponent<Image>();
            img.color = A;

            isFading = true;
        }
    }

	void Start ()
    {

		if (sr != null || img != null)
        {
            isFading = true;
		}

	}

    void Update()
    {
        if (isFading && sr != null)
        {
            FadeOut();
        }
        else if (isFading && img != null)
        {
            FadeOutImage();
        }
    }


    void FadeOut()
    {
        if (isFading)
        {
            sr.color = Color.Lerp(A, B, colorTime);

            if (colorTime < 1)
            {
                // increment colorTime it at the desired rate every update:
                colorTime += Time.deltaTime / colorChangeDuration;
            }

            if (sr.color == B)
            {
                colorTime = 0;

                isFading = false;

                Pool();

            }
        }

    }


    void FadeOutImage()
    {
        if (isFading)
        {
            img.color = Color.Lerp(A, B, colorTime);

            if (colorTime < 1)
            {
                // increment colorTime it at the desired rate every update:
                colorTime += Time.deltaTime / colorChangeDuration;
            }

            if (img.color == B)
            {
                colorTime = 0;
                isFading = false;
                Pool();
            }
        }

    }

    void Pool()
    {
        ObjectPool.instance.PoolObject(gameObject);
    }

}
