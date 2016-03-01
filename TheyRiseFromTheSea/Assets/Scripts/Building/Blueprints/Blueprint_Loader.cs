using UnityEngine;
using System.Collections;

public class Blueprint_Loader : MonoBehaviour {

    void Start()
    {
        BlueprintDatabase.Instance.ReloadPreviousLoaded();
    }


    public void SelectBlueprint(string bpType)
    {
        BlueprintDatabase.Instance.SelectBlueprint(bpType);

    }

    public void LoadBlueprint()
    {
        BlueprintDatabase.Instance.LoadBlueprint();
    }

    public void UnLoadBlueprint()
    {
        BlueprintDatabase.Instance.UnLoadBlueprint();
    }
}
