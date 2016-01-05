using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Building_StatusIndicator : MonoBehaviour {

	/// <summary>
	/// Indicate through this Building's UI canvas changes made to the building:
	/// 
	/// 	- Damage taken	(text)
	/// 	- Food Condition (text)
	/// 	- HP (text)
	/// 	- Current Task / Task Completed Notification
	/// 
	/// </summary>

	[SerializeField]
	private RectTransform healthBarRect;
	
	[SerializeField]
	private Canvas canvas;
	
	private GameObject _damageText, _statusMsgText;
	
	[HideInInspector]
	public ObjectPool objPool;


	void Start () {

        objPool = ObjectPool.instance;

        if (healthBarRect == null) {
			Debug.Log("BUILDING INDICATOR: No health bar referenced!!");
		}
	}
	
	public void SetHealth(float _cur, float _max, float _damage = 0)
	{
		float _value = _cur / _max;
		
		if (_value < 0.4f) {
			healthBarRect.gameObject.GetComponent<Image> ().color = Color.red;
			
		} else if (_value < 0.6f) {
			healthBarRect.gameObject.GetComponent<Image> ().color = Color.yellow;
		} else {
			healthBarRect.gameObject.GetComponent<Image> ().color = Color.green;
		}
		
		healthBarRect.localScale = new Vector3 (_value, healthBarRect.localScale.y, healthBarRect.localScale.z);
		
		if (_damage > 0)
			CreateDamageText (_damage);
	}

	void CreateDamageText(float _damage)
	{
		//		Debug.Log ("Creating Damage text for " + _damage + " " + damageTypeID);
		Vector2 min = new Vector2 (0.5f, 0.5f);
		Vector2 max = new Vector2 (0.5f, 0.5f);
		Vector2 size = new Vector2 (113, 137);
		Vector3 scale = new Vector3(1, 1, 1);
		Vector3 _scaleCalc = canvas.transform.localScale - scale;
		
		_damageText = objPool.GetObjectForType ("Building Damage Text", true, Vector3.zero);
		
		if (_damageText != null) {

			RectTransform rectTransform = _damageText.GetComponent<RectTransform> ();
			rectTransform.SetParent (canvas.transform, true);
			rectTransform.anchorMax = min;
			rectTransform.anchorMin = max;
			rectTransform.offsetMax = Vector2.zero;
			rectTransform.offsetMin = Vector2.zero;
			rectTransform.sizeDelta = size;
			rectTransform.localScale = -_scaleCalc;
			
			_damageText.GetComponent<Text>().color = Color.red;
			_damageText.GetComponent<Text>().text = _damage.ToString ();

		} else {
			Debug.Log ("STATUS INDICATOR: Could NOT find Damage Text in Pool!");
		}
	}

	public void CreateStatusMessage(string _message, Color color = default(Color))
	{
		Vector2 min = new Vector2 (0.5f, 0.5f);
		Vector2 max = new Vector2 (0.5f, 0.5f);
		Vector2 size = new Vector2 (306, 64);
		Vector3 scale = new Vector3(1, 1, 1);
		Vector3 _scaleCalc = canvas.transform.localScale - scale;
		
		_statusMsgText = objPool.GetObjectForType ("Status Text", true, Vector3.zero);
		
		if (_statusMsgText != null) {
			
			RectTransform rectTransform = _statusMsgText.GetComponent<RectTransform> ();
			rectTransform.SetParent (canvas.transform, true);
			rectTransform.anchorMax = min;
			rectTransform.anchorMin = max;
			rectTransform.offsetMax = Vector2.zero;
			rectTransform.offsetMin = Vector2.zero;
			rectTransform.sizeDelta = size;
			rectTransform.localScale = -_scaleCalc;
			

			_statusMsgText.GetComponent<Text>().text = _message;
			if (color != Color.clear)
				_statusMsgText.GetComponent<Text>().color = color;

		} else {
			Debug.Log ("STATUS INDICATOR: Could NOT find Status Text in Pool!");
		}
	}
}
