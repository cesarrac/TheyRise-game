using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Character_Creator : MonoBehaviour {

    string pName = "Hiro";

    public InputField nameInput;

    public void SetName()
    {
        pName = nameInput.text;
    }

    public void CreateCharacter()
    {
        GameMaster.Instance.CreateDefaultHero(pName);
    }
}
