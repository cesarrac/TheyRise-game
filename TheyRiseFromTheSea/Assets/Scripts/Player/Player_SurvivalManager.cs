using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Player_SurvivalManager : MonoBehaviour {

	// SURVIVAL STATS ARE: Health (from attack script), Hunger, Oxygen, and Stamina

	// Health regenerates when Hunger is at 80% or higher, it sets and gets curHP from attack stats
	// Hunger goes down by x every y seconds. It only goes up by visiting a farm or nutrient generator
	// Oxygen is same as Hunger. It can only be replenished at the Capital.
	// Stamina goes down every time you dash by x and every time you attack by 1/2 of x. It regenerates when NOT attacking or dashing

	[System.Serializable]
	public class SurvivalStats
	{
		public float maxHunger, maxOxygen;
		private float _health, _hunger, _oxygen, _stamina;
	
		public float curHunger {get {return _hunger;} set {_hunger = Mathf.Clamp(value, 0f, maxHunger);}}
		public float curOxygen {get{return _oxygen;}set{_oxygen = Mathf.Clamp(value, 0, 100f);}}
		public float curStamina {get {return _stamina;} set {_stamina = Mathf.Clamp(value, 0, maxOxygen);}}

		public void Init()
		{
			curHunger = 100f;
			curOxygen = 100f;
			curStamina = 100f;
		}
	}

	public SurvivalStats survStats = new SurvivalStats();
	
	public Player_HeroAttackHandler playerAttackHandler;

	private float _oxyCountDown = 2f, _hungerCountDown = 2f;

	public Player_ResourceManager playerResourceMan;

	[SerializeField]
	private RectTransform hungerBarRect, oxyBarRect;

	[SerializeField]
	private Text healthText;

	// Ammount lost every cycle 
	float hungerAmmt = 1f, oxyAmmt = 0.3f;



	void Start()
	{
		// Make sure this script has access to Player Attack Handler
		if (!playerAttackHandler) {
			playerAttackHandler = GetComponent<Player_HeroAttackHandler> ();
		} 

		// Initialize the survival stats
		survStats.Init();
	
	}
	
	void Update () 
	{
		// Make sure Player Attack handler is NOT null before calling all other methods
		if (playerAttackHandler) {
		

			// Call for decrease Oxygen & Hunger
			HungerMeterDecrease();
			OxygenDeplete();
		}

		// PROTOTYPE FEEDING METHOD: // TODO: This will be replaced with specific code for grabbing food from cafeterias, farms, and such
		if (Input.GetKeyDown (KeyCode.F)) {

			if (playerResourceMan.food > 0){
				survStats.curHunger += 5f;
				playerResourceMan.ChangeResource("Food", 5);
				IncreaseBar();
			}
			
		}
		if (Input.GetKeyDown (KeyCode.O)) {
			survStats.curOxygen +=10f;
			IncreaseBar();
		}
	}

	void HungerMeterDecrease()
	{
		// If hunger is more than 0 decrease the meter (the LOWER the meter the HUNGRIER the Player is)
		if (survStats.curHunger > 0) {

			// countdown to decrease hunger every 2 seconds
			if (_hungerCountDown <= 0){

				survStats.curHunger -= hungerAmmt;

				// Calculate how much Hunger is left and store it in _value
				float _value = survStats.curHunger / survStats.maxHunger;
				// Decrease the hunger bar
				hungerBarRect.localScale = new Vector3(_value, hungerBarRect.localScale.y, hungerBarRect.localScale.z);

				_hungerCountDown = 2f;

			}else{
				_hungerCountDown -= Time.deltaTime;
			}
		}else {
			// if player has NO FOOD left start taking away HP every 2 seconds
			if (_hungerCountDown <= 0) {
				
				//take it down from player attack handler, this script will update the survival health every time they don't match
				playerAttackHandler.stats.curHP --;
				
				_hungerCountDown = 2f;
				
			} else {
				_hungerCountDown -= Time.deltaTime;
			}
		}
	}

	void OxygenDeplete()
	{
		// If oxygen is more than 0 decrease the meter
		if (survStats.curOxygen > 0) {
			
			// countdown to decrease oxygen every 2 seconds
			if (_oxyCountDown <= 0) {
				
				survStats.curOxygen -= oxyAmmt;

				// Calculate how much Oxygen is left and store it in _value
				float _value = survStats.curOxygen / survStats.maxOxygen;
				// Decrease the oxygen bar
				oxyBarRect.localScale = new Vector3(_value, oxyBarRect.localScale.y, oxyBarRect.localScale.z);

				//Reset
				_oxyCountDown = 2f;
				
			} else {
				_oxyCountDown -= Time.deltaTime;
			}
		} else {
			// if player has NO oxygen left start taking away HP every 2 seconds
			if (_oxyCountDown <= 0) {
				
				//take it down from player attack handler, this script will update the survival health every time they don't match
				playerAttackHandler.stats.curHP --;
				
				_oxyCountDown = 2f;
				
			} else {
				_oxyCountDown -= Time.deltaTime;
			}
		}
	}

	void IncreaseBar()
	{
		// HUNGER:
		// Calculate how much Hunger is left and store it in _value
		float _hValue = survStats.curHunger / survStats.maxHunger;
		// Decrease the hunger bar
		hungerBarRect.localScale = new Vector3(_hValue, hungerBarRect.localScale.y, hungerBarRect.localScale.z);
		// OXYGEN:
		float _oValue = survStats.curOxygen / survStats.maxOxygen;
		oxyBarRect.localScale = new Vector3(_oValue, oxyBarRect.localScale.y, oxyBarRect.localScale.z);
	}
}
