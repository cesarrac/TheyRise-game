using UnityEngine;
using System.Collections;

[System.Serializable]
public class UnitStats
{
    public float maxHP, startDefence, startAttack, startShield, startRate, startDamage, startSpecialDmg, startReloadSpd;
    public int creditReward;
    private float _hitPoints, _defence, _attack, _shield, _damage, _specialDamage, _rateOfAttack, _reloadSpd;
    private int _creditValue;

    public float curHP { get { return _hitPoints; } set { _hitPoints = Mathf.Clamp(value, 0f, maxHP); } }
    public float curDefence { get { return _defence; } set { _defence = Mathf.Clamp(value, 0f, 100f); } }
    public float curAttack { get { return _attack; } set { _attack = Mathf.Clamp(value, 0f, 100f); } }
    public float curShield { get { return _shield; } set { _shield = Mathf.Clamp(value, 0f, 100f); } }
    public float curRateOfAttk { get { return _rateOfAttack; } set { _rateOfAttack = Mathf.Clamp(value, 0f, 5f); } }
    public float curDamage { get { return _damage; } set { _damage = Mathf.Clamp(value, 0f, 100f); } }
    public float curReloadSpeed { get { return _reloadSpd; } set { _reloadSpd = Mathf.Clamp(value, 0.1f, 10f); } }


    public float curSPdamage { get { return _specialDamage; } set { _specialDamage = Mathf.Clamp(value, 0f, 100f); } }

    public int curCreditValue { get { return _creditValue; } set { _creditValue = Mathf.Clamp(value, 0, 500); } }


    // Use this to initialize current stats from a Unit's gameobject (Attack Handler)
    public void Init()
    {
        curHP = maxHP;
        curDefence = startDefence;
        curAttack = startAttack;
        curShield = startShield;
        curRateOfAttk = startRate;
        curDamage = startDamage;
        curSPdamage = startSpecialDmg;
        curCreditValue = creditReward;
        curReloadSpeed = startReloadSpd;
    }



    // Use this to initialize Stats from the Spawner
    public void InitStartingStats(float hp, float def, float attk, float shi, float rate, float dmg, float sp_dmg)
    {
        maxHP = hp;
        startDefence = def;
        startAttack = attk;
        startShield = _shield;
        startRate = rate;
        startDamage = dmg;
        startSpecialDmg = sp_dmg;
    }

    // Default empty constructor used by Enemy Units
    public UnitStats() { }

    // Constructor used by hero
    public UnitStats(float hp, float attk, float def, float shi)
    {
        maxHP = hp;
        startDefence = def;
        startAttack = attk;
        startShield = _shield;
        startRate = 0;
        startDamage = 0;
        startSpecialDmg = 0;
       // startReloadSpd = 0;

        Init();
    }

    // Constructor used by battle towers
    public UnitStats(float hp, float attk, float def, float shi, float rate)
    {
        maxHP = hp;
        startDefence = def;
        startAttack = attk;
        startShield = _shield;
        startRate = rate;
        startDamage = 0;
        startSpecialDmg = 0;
       // startReloadSpd = rel_spd;

        Init();
    }
}

public class Unit_Base : MonoBehaviour {



    public UnitStats stats;

	public ResourceGrid resourceGrid;

	public ObjectPool objPool;

	public GameObject unitToPool;

	// ENEMY UNITS ONLY:
	// recording the tile they are attacking so we don't have to check its stats with each individual unit
	//TileData tileUnderAttack = null;
	//float tileDefence, tileShield, tileHP;
	//public bool canAttackTile;

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

    [Header("If ON, this unit will always attack buildings nearby")]
    public bool isAggroToBuildings = false;
    

	void Start(){

		if (_statusIndi != null) {
			_statusIndi.SetHealth(stats.curHP, stats.maxHP);
		}
	}

//	public void InitTileStats(int x, int y){
////		Debug.Log ("BASE: Tile stats initialized!");
//		resourceGrid.tiles [x, y].hp = stats.curHP;
//		resourceGrid.tiles [x, y].def = stats.curDefence;
//		resourceGrid.tiles [x, y].attk = stats.curAttack;
//		resourceGrid.tiles [x, y].shield = stats.curShield;
//	}

	

    public bool AttackTile(TileData tile)
    {
        if (tile.tileStats.HP > 0)
        {
            float calc = (tile.tileStats.Defense + tile.tileStats.Shield ) - stats.curAttack;
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
            //Debug.Log("TILE: tile takes damage " + stats.curDamage);

            return true;

        }
        else
        {
            // Check if the Tile attacked was the Terraformer... if it was it's GAME OVER!
            if (tile.tileType == TileData.Types.terraformer)
            {
                MasterState_Manager.Instance.mState = MasterState_Manager.MasterState.MISSION_FAILED;
            }
           // tileUnderAttack = null;
            return false;
        }
      
    }

    public bool AttackTileUnit(Unit_Base target, TileData tile)
    {
        if (target.stats.curHP > 0)
        {

            float def = (target.stats.curDefence + target.stats.curShield);

            if (stats.curAttack > def)
            {
                //Debug.Log("UNIT: Attacking " + target.name + " DEF: " + def + " ATTK: " + stats.curAttack);

                // Apply full damage
                TakeDamage(target, stats.curDamage);


            }
            else
            {
                // hit for difference between def and attack
                float calc = stats.curAttack - def;
                float damageCalc = stats.curDamage - calc;

                // always do MINIMUM 1 pt of damage
                float clampedDamage = Mathf.Clamp(damageCalc, 1f, stats.curDamage);

                // Debug.Log("UNIT: Can't beat " + target.name + "'s Defence, so I hit for " + clampedDamage);

                TakeDamage(target, clampedDamage);

            }

            return true;
        }
        else
        {
            // target is dead by now
            resourceGrid.DamageTile(tile, tile.tileStats.HP);

            Debug.Log("UNIT: Target Dead!");
            //Die(unit.gameObject);

            return false;

        }
    }

    // ********************** WARNING! This is the old Attack Unit method that really does not work as well!! ******************************
 //   public void AttackOtherUnit(Unit_Base unit){
	//	Debug.Log ("UNIT: target unit hp at " + unit.stats.curHP);
	//	Debug.Log ("UNIT: My damage is " + stats.curDamage + " and my HP is " + stats.curHP);

	//	if (unit.stats.curHP > 0) {

	//		float def = (unit.stats.curDefence + unit.stats.curShield);

	//		if (stats.curAttack > def){
	//			Debug.Log("UNIT: Attacking " + unit.name + " DEF: " + def + " ATTK: " + stats.curAttack);

	//			// Apply full damage
	//			TakeDamage(unit, stats.curDamage);

	//		}else{
	//			// hit for difference between def and attack
	//			float calc = stats.curAttack - def;
	//			float damageCalc = stats.curDamage - calc;

	//			// always do MINIMUM 1 pt of damage
	//			float clampedDamage = Mathf.Clamp(damageCalc, 1f, stats.curDamage);

	//			Debug.Log("UNIT: Can't beat " + unit.name + "'s Defence, so I hit for " + clampedDamage);

	//			TakeDamage (unit, clampedDamage);

	//		}
	//	} else {
	//		// target is dead by now
	//		Debug.Log("UNIT: Target Dead!");
	//		Die (unit.gameObject);
		
	//	}
	
	//}
    // *************************************************************************


    // *********** THIS IS THE NEW ONE! ***************************
    public bool AttackUnit(Unit_Base target)
    {
        if (target.stats.curHP > 0)
        {

            float def = (target.stats.curDefence + target.stats.curShield);

            if (stats.curAttack > def)
            {
                //Debug.Log("UNIT: Attacking " + target.name + " DEF: " + def + " ATTK: " + stats.curAttack);

                // Apply full damage
                TakeDamage(target, stats.curDamage);

             
            }
            else
            {
                // hit for difference between def and attack
                float calc = stats.curAttack - def;
                float damageCalc = stats.curDamage - calc;

                // always do MINIMUM 1 pt of damage
                float clampedDamage = Mathf.Clamp(damageCalc, 1f, stats.curDamage);

               // Debug.Log("UNIT: Can't beat " + target.name + "'s Defence, so I hit for " + clampedDamage);

                TakeDamage(target, clampedDamage);

            }

            return true;
        }
        else
        {
            // target is dead by now
            Debug.Log("UNIT: Target Dead!");
            //Die(unit.gameObject);

            return false;

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
       // Debug.Log (gameObject.name + " has " + stats.curHP + "HP left");

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
