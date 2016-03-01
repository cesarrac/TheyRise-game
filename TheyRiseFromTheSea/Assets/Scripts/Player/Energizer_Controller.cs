using UnityEngine;
using System.Collections;
using System;

public class Energizer_Controller : MonoBehaviour {

    float range = 10f;
    public float Range { get { return range; } set { range = Mathf.Clamp(value, 5, 50); } }

    float boost = 2f;
    public float Boost { get { return boost; } set { boost = Mathf.Clamp(value, 1, 10); } }

    float timeToBoost = 1f;
    public float TimeToBoost { get { return timeToBoost; } set { timeToBoost = Mathf.Clamp(value, 1, 10); } }

    float boostDuration = 1f;
    public float BoostDuration { get { return boostDuration; } set { boostDuration = Mathf.Clamp(value, 1, 10); } }

    TileData tile;

    bool countingDown;

    public void StartListen()
    {
        StartCoroutine("Listen");
    }

    IEnumerator Listen()
    {
        while (true)
        {
            ListenToEnergize();

            yield return null;
        }
    }

    void ListenToEnergize()
    {
        if (Input.GetMouseButtonDown(0) && !countingDown)
        {

            // From the moment the Player hits the Left Mouse while the Energizer is on...
            // ... we check if the tile under mouse is not empty...

            tile = Mouse_Controller.MouseController.GetTileUnderMouse();
            if (tile != null)
            {
                if (tile.tileType != TileData.Types.empty)
                {
                    Debug.Log("ENERGIZER: The Tile is NOT null and NOT Empty!");
                    // If it's not, the COUNTDOWN begins.
                    if (!countingDown)
                    {
                        countingDown = true;

                        // Stop Listening
                        StopCoroutine("Listen");

                        // Start Countdown
                        StartCoroutine("Countdown");
                    }
                }
            }
        }
    }

    IEnumerator Countdown()
    {
        while (true)
        {
            yield return new WaitForSeconds(TimeToBoost);

            // When the count reaches 0 ...
            Debug.Log("ENERGIZER: Countdown is done! Checking tile gameobject...");
            GameObject tileGObj = ResourceGrid.Grid.GetTileGameObjFromIntCoords(tile.posX, tile.posY);
            if (tileGObj != null)
            {
                // ... Check if it's a Tower ...
                if (tileGObj.GetComponentInChildren<Unit_Base>() != null)
                {
                    // ... and ENERGIZE!
                    tileGObj.GetComponentInChildren<Unit_Base>().EnergizeCallback(EnergizeTowerUnit);
                }
                // or an Extraction Building.
                else if (tileGObj.GetComponent<ExtractionBuilding>() != null)
                {
                    // ... and ENERGIZE!

                }
            }
            else
            {
                Debug.Log("ENERGIZER: The Tile GameObject is null! Are the Tile coords wrong?!");
            }

            countingDown = false;
            yield break;
        }
    }

    void EnergizeTowerUnit(Unit_Base tower)
    {
        Debug.Log("TOWER is ENERGIZING!");
        // Attack boost
        tower.stats.curAttack = boost * tower.stats.curAttack;

        // Rate boost
        tower.stats.curRateOfAttk = boost * tower.stats.curRateOfAttk;

        // Damage boost 
        tower.stats.curDamage = boost * tower.stats.curDamage;

        // Reload Speed boost (subtracts because Reload speed is how long it takes to reload in seconds)
        tower.stats.curReloadSpeed = tower.stats.curReloadSpeed - boost;

        Debug.Log("Attack " + tower.stats.curAttack + " Damage " + tower.stats.curDamage);

        Debug.Log("Past Attack " + tower.stats.startAttack + " Past Damage " + tower.stats.startDamage);

        tower.CountDownToDeEnergize(DeEnergizeTowerUnit, BoostDuration);

    }

    public void DeEnergizeTowerUnit(UnitStats tower)
    {
        Debug.Log("DeEnergizing!!");
        Debug.Log("BEFORE: Attack " + tower.curAttack + " Damage " + tower.curDamage);
        // Set stats back to starting values
        tower.curAttack = tower.startAttack;
        tower.curRateOfAttk = tower.startRate;
        tower.curDamage = tower.startDamage;
        tower.curReloadSpeed = tower.startReloadSpd;

        Debug.Log("AFTER: Attack " + tower.curAttack + " Damage " + tower.curDamage);
    }

    //var distance = (mouseV2 - sightV2).sqrMagnitude;

    void EnergizeExtractionBuilding(ExtractionBuilding extractor)
    {
        // Calculate boosted Rate and Power...
        var power = boost * extractor.extractorStats.extractPower;
        var rate = boost * extractor.extractorStats.extractRate;

        // ... and Energize the Extractor's stats.
        extractor.extractorStats.Energize(rate, power);

        // Now start counting down to De Energize
        extractor.CountDownToDeEnergize(DeEnergizeExtractor, BoostDuration);
    }

    void DeEnergizeExtractor(ExtractionBuilding extractor)
    {
        extractor.extractorStats.DeEnergize();
    }
}
