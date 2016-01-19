using UnityEngine;
using System.Collections;

public class NanoBuilder  {

    public int processingPower { get; protected set; }
    
    public int nanoBots { get; protected set; }

    public NanoBuilder()
    {
        processingPower = 50;
        nanoBots = 50;
    }
}
