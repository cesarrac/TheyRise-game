using UnityEngine;
using System.Collections;

public class Blueprint_Extraction : Blueprint {

    public ExtractorStats extractorStats { get; protected set; }

    public Blueprint_Extraction(float rate, int ammount, float power, int personalStorageCap, int secondStorageCap = 0, int materialConsumed = 0)
    {
        extractorStats = new ExtractorStats(rate, ammount, power, personalStorageCap, secondStorageCap, materialConsumed);
    }
}
