using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Player_UIHandler : MonoBehaviour {

    public static Player_UIHandler instance;

	// Resources Panel 
	public Text oreText, foodText, creditsText, waterText, energyText;
    public Text tempOreTxt, tempFoodTxt, tempCreditTxt, tempWaterTxt, tempEnergyTxt;
	int ore, food, credits, totalFood, farmCount, water, extractCount, waterPumpCount, energy;

	// Food Production Panel
	//public Text foodProd_txt, farmCount_txt, foodCost_txt;

	// Ore & Water Production Panel
	//public Text oreProd_txt, extractCount_txt, waterProd_txt, waterCount_txt, waterCost_txt;

	// Storage Info Panel
	//public Text storageName_txt, oreStored_txt, oreCap_txt, waterStored_txt, waterCap_txt;


	public Player_ResourceManager resourceManager;

	// Display the Player's Hit Points in the Player Panel
	public Player_HeroAttackHandler playerhero;
	public Text hpText;
	private float _curHP;

    // Blueprint panel & Nanobot count
    public GameObject bpPanel;
    public Text bluePrintTextBox;
    public Text nanoBotCount;

    void Awake()
    {
        instance = this;
    }

	void Start () 
	{
        // Ask Ship Inventory to init ui
        Ship_Inventory.Instance.InitUI();

		if (resourceManager == null) {
			resourceManager = GetComponent<Player_ResourceManager>();
		}


        // NOTE: Turning off the Panels for now
        //		GetResourcesText ();

        //foodCostText.color = Color.red;

        // Get the player hero component
        /*if (!playerhero) {
			playerhero = GameObject.FindGameObjectWithTag("Player").GetComponent<Player_HeroAttackHandler>();
		}
        */
    }

    public void InitPlanetUIText(int _food, int _water, int _ore)         // FIX THIS: Add the rest of the resources!
    {
        // Right now this will initialize the text for the Transporter storage panel
        tempOreTxt.text = _ore.ToString();
        tempFoodTxt.text = _food.ToString();
        tempWaterTxt.text = _water.ToString();

    }

    public void InitShipUI(int _food, int _water, int _ore)
    {
        oreText.text = _ore.ToString();
        foodText.text = _food.ToString();
        waterText.text = _water.ToString();
    }

    public void DisplayShipInventoryText(TileData.Types statThatChanges, int ammnt)
    {
        // This method will only be called when a stat is changing. This will update the text. 
        switch (statThatChanges)
        {
            case TileData.Types.rock:
                oreText.text = ammnt.ToString();
                break;
            case TileData.Types.food:
                foodText.text = ammnt.ToString();
                break;
            case TileData.Types.water:
                waterText.text = ammnt.ToString();
                break;
            default:
                // do nothing
                break;
        }
    }

    public void DisplayTransporterStorage(TileData.Types statThatChanges, int ammnt)
    {
        // This method will only be called when a stat is changing. This will update the text. 
        switch (statThatChanges)
        {
            case TileData.Types.rock:
                tempOreTxt.text = ammnt.ToString();
                break;
            case TileData.Types.food:
                tempFoodTxt.text = ammnt.ToString();
                break;
            case TileData.Types.water:
                tempWaterTxt.text = ammnt.ToString();
                break;
            default:
                // do nothing
                break;
        }
    }

    public void DisplayBlueprint(string curBPName)
    {
        bluePrintTextBox.text = curBPName;
    }

    public void DisplayNanoBotCount(int newTotal)
    {
        nanoBotCount.text = newTotal.ToString();
    }


    //	void GetResourcesText()
    //	{
    //		if (resourceManager.food < resourceManager.totalFoodCost) {
    //			foodText.color = Color.red;
    //		} else {
    //			foodText.color = Color.green;
    //		}

    //		waterText.color = Color.blue;
    //		energyText.color = Color.cyan;

    //		oreText.text = resourceManager.ore.ToString();
    //		foodText.text = resourceManager.food.ToString ();
    //		creditsText.text = resourceManager.credits.ToString();
    //		waterText.text = resourceManager.water.ToString();
    //		energyText.text = resourceManager.energy.ToString ();

    //		ore = resourceManager.ore;
    //		food=resourceManager.food;
    //		credits =resourceManager.credits;
    //		totalFood = resourceManager.totalFoodCost;
    //		water =resourceManager.water;
    //		energy = resourceManager.energy;

    //		if (resourceManager.totalFoodCost > 0) {
    //			foodCostText.gameObject.SetActive(true);
    //			foodCostText.text = "/ - " + resourceManager.totalFoodCost.ToString ();
    //		} else {
    //			foodCostText.gameObject.SetActive(false);
    //		}

    //		// food production panel
    //		farmCount_txt.text = resourceManager.farmCount.ToString ();
    ////		foodCost_txt.text = resourceManager.totalFoodCost.ToString ();
    //		foodProd_txt.text = resourceManager.foodProducedPerDay.ToString ();
    //		farmCount = resourceManager.farmCount;

    //		// ore and water production panel
    //		extractCount_txt.text = resourceManager.extractorCount.ToString ();
    //		oreProd_txt.text = resourceManager.oreExtractedPerDay.ToString ();

    //		waterCount_txt.text = resourceManager.waterPumpCount.ToString ();
    //		waterProd_txt.text = resourceManager.waterProducedPerDay.ToString ();
    //		waterCost_txt.text = resourceManager.totalWaterCost.ToString ();
    //		extractCount = resourceManager.extractorCount;
    //		waterPumpCount = resourceManager.waterPumpCount;

    //	}


    void Update () {
		//if (resourceManager.ore != ore || resourceManager.food != food || resourceManager.credits != credits 
		//    || resourceManager.totalFoodCost != totalFood || resourceManager.farmCount != farmCount || resourceManager.water != water
		//    || resourceManager.extractorCount != extractCount || resourceManager.waterPumpCount != waterPumpCount ||
		//    resourceManager.energy != energy) {
		//	GetResourcesText();
		//}

		//if (playerhero) {
		//	if (_curHP != playerhero.stats.curHP){
		//		DisplayPlayerHealth();
		//	}
		//}
	}

	//void DisplayPlayerHealth()
	//{
	//	if (hpText) {
	//		hpText.text = "HP: " + playerhero.stats.curHP + "/" + playerhero.stats.maxHP;
	//		_curHP = playerhero.stats.curHP;
	//	}

	//}

	//public void DisplayStorageInfo(Storage storageSelected){
	//	if (storageSelected != null) {
	//		oreStored_txt.text = storageSelected.oreStored.ToString();
	//		oreCap_txt.text = "/ " + storageSelected.oreCapacity.ToString();

	//		waterStored_txt.text = storageSelected.waterStored.ToString();
	//		waterCap_txt.text = "/ " + storageSelected.waterCapacity.ToString();
	//	}
	//}
}
