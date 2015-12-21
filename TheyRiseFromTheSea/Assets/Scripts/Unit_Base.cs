using UnityEngine;
using System.Collections;

public class Unit_Base : MonoBehaviour {

	[System.Serializable]
	public class Stats{
		public float maxHP, startDefence, startAttack, startShield, startRate, startDamage, startSpecialDmg;
		public int creditReward;
		private float _hitPoints, _defence, _attack, _shield, _damage, _specialDamage, _rateOfAttack;
		private int _creditValue;
	
		public float curHP { get { return _hitPoints; } set { _hitPoints = Mathf.Clamp (value, 0f, maxHP); } }
		public float curDefence {get {return _defence;} set { _defence = Mathf.Clamp (value, 0f, 100f); }}
		public float curAttack {get {return _attack;} set { _attack = Mathf.Clamp (value, 0f, 100f); }}
		public float curShield {get {return _shield;} set { _shield = Mathf.Clamp (value, 0f, 100f); }}
		public float curRateOfAttk {get {return _rateOfAttack;} set { _rateOfAttack = Mathf.Clamp (value, 0f, 5f); }}
		public float curDamage {get {return _damage;} set { _damage = Mathf.Clamp (value, 0f, 100f); }}
		public float curSPdamage { get {return _specialDamage;} set {_specialDamage = Mathf.Clamp (value, 0f, 100f);}}

		public int curCreditValue { get{return _creditValue;} set {_creditValue = Mathf.Clamp(value, 0, 500);}}



		public void Init(){
			curHP = maxHP;
			curDefence = startDefence;
			curAttack = startAttack;
			curShield = startShield;
			curRateOfAttk = startRate;
			curDamage = startDamage;
			curSPdamage = startSpecialDmg;
			curCreditValue = creditReward;
		}
	}

	public Stats stats = new Stats ();

	public ResourceGrid resourceGrid;

	public ObjectPool objPool;

	public GameObject unitToPool;

	// ENEMY UNITS ONLY:
	// recording the tile they are attacking so we don't have to check its stats with each individual unit
	TileData tileUnderAttack = null;
	float tileDefence, tileShield, tileHP;
	public bool canAttackTile;

//	public Damage_PopUp dmgPopUp; 
	[Header("Optional (For Units): ")]
	[SerializeField]
	private Unit_StatusIndicator _statusIndi;
	public Unit_StatusIndicator statusIndicator { get {return _statusIndi;} set{_statusIndi = value;}}

	[Header ("Optional (For Buildings): ")]
	[SerializeField]
	private Building_StatusIndicator _buildingIndi;
	public Building_StatusIndicator buildingStatusIndicator { get {return _buildingIndi;} set {_buildingIndi = value;}}

    public AudioClip takeDamageSound, doDamageSound;
    public AudioSource audio_source;

    

	void Start(){

		if (_statusIndi != null) {
			_statusIndi.SetHealth(stats.curHP, stats.maxHP);
		}
	}

	public void InitTileStats(int x, int y){
//		Debug.Log ("BASE: Tile stats initialized!");
		resourceGrid.tiles [x, y].hp = stats.curHP;
		resourceGrid.tiles [x, y].def = stats.curDefence;
		resourceGrid.tiles [x, y].attk = stats.curAttack;
		resourceGrid.tiles [x, y].shield = stats.curShield;
	}

	

    public bool AttackTile(TileData tile)
    {
        if (tile.hp > 0)
        {
            float calc = (tile.def + tile.shield ) - stats.curAttack;
            //			
            if (calc <= 0)
            {
                // Apply full damage
                resourceGrid.DamageTile(tile, stats.curDamage);
            }
            else
            {
                // Apply just 1 damage
                resourceGrid.DamageTile(tile, 1f);
            }
            return true;

        }
        else
        {
            tileUnderAttack = null;
            return false;
        }
      
    }

    public void AttackOtherUnit(Unit_Base unit){
		Debug.Log ("UNIT: target unit hp at " + unit.stats.curHP);
		Debug.Log ("UNIT: My damage is " + stats.curDamage + " and my HP is " + stats.curHP);

		if (unit.stats.curHP >= 1f) {

			float def = (unit.stats.curDefence + unit.stats.curShield);

			if (stats.curAttack > def){
				Debug.Log("UNIT: Attacking " + unit.name + " DEF: " + def + " ATTK: " + stats.curAttack);

				// Apply full damage
				TakeDamage(unit, stats.curDamage);

			}else{
				// hit for difference between def and attack
				float calc = def - stats.curAttack;
				float damageCalc = stats.curDamage - calc;

				// always do MINIMUM 1 pt of damage
				float clampedDamage = Mathf.Clamp(damageCalc, 1f, stats.curDamage);

				Debug.Log("UNIT: Can't beat " + unit.name + "'s Defence, so I hit for " + clampedDamage);

				TakeDamage (unit, clampedDamage);

			}
		} else {
			// target is dead by now
			Debug.Log("UNIT: Target Dead!");
			Die (unit.gameObject);
		
		}
	
	}

	// ONLY USED BY KAMIKAZE UNITS ATTACKING PLAYER UNITS
	public void SpecialAttackOtherUnit(Unit_Base unit){
		if (unit.stats.curHP >= 1f) {
			TakeDamage (unit, stats.curSPdamage);
            // Play do damage sound
            if (audio_source)
                audio_source.PlayOneShot(doDamageSound, 1);
		} else {
			Die (unit.gameObject);
		}
	}

	void TakeDamage(Unit_Base unit, float damage){
		unit.stats.curHP -= damage;

		// Indicate damage using Unit / Building's canvas
		if (unit.gameObject.activeSelf) {

			if (unit.gameObject.tag == "Building") {
			
				// indicate damage on building
				if (unit.buildingStatusIndicator != null)
					unit.buildingStatusIndicator.SetHealth (unit.stats.curHP, unit.stats.maxHP, damage);
			
			} else {
				// indicate damage on unit
				if (unit.statusIndicator != null)
					unit.statusIndicator.SetHealth (unit.stats.curHP, unit.stats.maxHP, damage);
			}

		}

	}

	public void TakeDamage(float damage)
	{
		stats.curHP -= damage;
        Debug.Log (gameObject.name + " has " + stats.curHP + "HP left");

        // Play take damage sound
        if (audio_source)
            audio_source.PlayOneShot(takeDamageSound, 0.5f);

		// Indicate damage using Unit / Building's canvas
		if (gameObject.activeSelf) {
			
			if (gameObject.tag == "Building") {
				
				// indicate damage on building
				if (buildingStatusIndicator != null)
					buildingStatusIndicator.SetHealth (stats.curHP, stats.maxHP, damage);
				
			} else {
				// indicate damage on unit
				if (statusIndicator != null)
					statusIndicator.SetHealth (stats.curHP, stats.maxHP, damage);
			}
			
		}
	}

	public void TakeDebuff(float debuffAmmnt, string statID){
		statusIndicator.CreateDamageText (debuffAmmnt, statID);
	}

	void Die(GameObject target){
		this.unitToPool = target; // Pool the unit for later use (the attacker handles the pooling)

		if (target.tag == "Enemy") {
			resourceGrid.AddCreditsForKill(target.GetComponent<Enemy_AttackHandler>().stats.curCreditValue);
		}
	}




}
