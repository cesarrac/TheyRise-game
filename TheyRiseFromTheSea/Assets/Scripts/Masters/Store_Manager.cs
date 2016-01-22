using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Store_Manager : MonoBehaviour {
	public Button oreButton;
	public Button foodButton;
	public Text totalOre;
	public Text totalFood;

	public Text currCredits;

	public GameMaster gm;

	int credits;
	int oreBought;
	int foodBought;

	void Start()
	{
		if (gm == null) {
			gm = GameObject.FindGameObjectWithTag("GM").GetComponent<GameMaster>();
		}

		InitCredits ();


		
	}

	void InitCredits()
	{
		if (gm) {
			credits = gm.curCredits;
			currCredits.text = credits.ToString();
		}
	}

	public void BuyOre()
	{
		oreBought += 10;
		totalOre.text = oreBought.ToString ();
		credits -= 100;
		currCredits.text = credits.ToString();
	}

	public void BuyFood()
	{
		foodBought += 10;
		totalFood.text = foodBought.ToString ();
		credits -= 50;
		currCredits.text = credits.ToString();
	}

	public void ReadyToGo()
	{
		// tell the GM what the new expedition inventory will be
		gm.NewExpeditionInventory (oreBought, foodBought, "Test Weapon", "Test Suit", "Test Tool");
		gm.inventory.ore = oreBought;
		// To get the new ammount of credits just subtract the difference and pass it in as an argument
		gm.AddOrSubtractCredits (-(gm.curCredits - credits));
		// tell GM to load level
		//gm.LoadLevel ();

	}



}
