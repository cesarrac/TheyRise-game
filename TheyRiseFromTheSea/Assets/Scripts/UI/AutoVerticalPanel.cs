using UnityEngine;
using System.Collections;

public class AutoVerticalPanel : MonoBehaviour {

    public float childHeight = 35f;

	public void AdjustPanelSize()
    {
        Vector2 size = this.GetComponent<RectTransform>().sizeDelta;
        size.y = transform.childCount * childHeight;

        this.GetComponent<RectTransform>().sizeDelta = size;
    }
}
