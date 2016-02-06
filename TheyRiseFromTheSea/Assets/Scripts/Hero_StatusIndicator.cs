using UnityEngine;
using System.Collections;

public class Hero_StatusIndicator : Unit_StatusIndicator {

    void Awake()
    {
        if (healthBarRect != null)
        healthBarRect = UI_Manager.Instance.player_healthBar.rectTransform;
    }

    public override void SetHealth(float _cur, float _max, float _damage = 0)
    {
        base.SetHealth(_cur, _max, _damage);
    }
}
