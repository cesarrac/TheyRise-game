using UnityEngine;
using System.Collections;

public class NanoBuilder  {

    public int processingPower { get; protected set; }
    
    public int nanoBots { get; protected set; }

    int _wpnSlots, _toolSlots;
    public int weaponSlots { get { return _wpnSlots; } set { _wpnSlots = Mathf.Clamp(value, 1, 5); } }
    public int toolSlots { get { return _toolSlots; } set { _toolSlots = Mathf.Clamp(value, 1, 3); } }

    // Default or Starting Nanobuilder
    public NanoBuilder()
    {
        processingPower = 50;
        nanoBots = 50;
        weaponSlots = 1;
        toolSlots = 1;
    }
}
