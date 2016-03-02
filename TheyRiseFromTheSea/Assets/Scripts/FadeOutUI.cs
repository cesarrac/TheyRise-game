using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class FadeOutUI : MonoBehaviour {

    Color A = Color.white;
    Color B = Color.clear;
    public float colorChangeDuration = 20;
    private float colorTime;

    Image img;


    void OnEnable()
    {
        img = GetComponent<Image>();
        img.color = A;
    }

    void LateUpdate()
    {
        FadeOut();
    }

    //void FadeIn()
    //{
    //    img.color = Color.Lerp(B, A, colorTime);

    //    if (colorTime < 1)
    //    {
    //        // increment colorTime it at the desired rate every update:
    //        colorTime += Time.deltaTime / colorChangeDuration;
    //    }

    //    if (img.color == A)
    //    {
    //        isFadedOut = false;
    //        colorTime = 0;
    //    }
    //}

    void FadeOut()
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
            gameObject.SetActive(false);
        }
    }
}
