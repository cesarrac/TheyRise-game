using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class UI_Manager : MonoBehaviour {

    public static UI_Manager Instance { get; protected set; }

    public GameObject blueprint_infoPanel, builder_panel;
    public Text bp_name;
    Dictionary<string, GameObject> loadedBlueprints = new Dictionary<string, GameObject>();

    public GameObject victoryPanel;

    void Awake()
    {
        Instance = this;
    }

    public void DisplayBPInfo(string bpName)
    {
        bp_name.text = bpName;
    }

    public void AddBlueprintToBuilder(string bpName)
    {
        // Get the Button prefab from the Pool ...
        GameObject bp = ObjectPool.instance.GetObjectForType("Loaded Blueprint", true, Vector3.zero);

        //Parent it to the Builder's transform ...
        bp.transform.SetParent(builder_panel.transform);

        // Add an onClick listener to this button, to allow it to callback Select Blueprint with its corresponding BP name...
        bp.GetComponent<Button>().onClick.AddListener(() => { BlueprintDatabase.Instance.SelectBlueprint(bpName); });

        //... and fill its text field with the BP name.
        bp.GetComponentInChildren<Text>().text = bpName;

        // To easily find which is which for removal or later loading them again, add each Button to a Dictionary<string, GameObject>
        if (!loadedBlueprints.ContainsKey(bpName))
        {
            loadedBlueprints.Add(bpName, bp);
        }
    }

    public void RemoveBlueprintTextFromBuilder(string bpName)
    {
        if (loadedBlueprints.ContainsKey(bpName))
        {
            // Pool the text gameobject...
            ObjectPool.instance.PoolObject(loadedBlueprints[bpName]);

            // ... and remove it from Dictionary
            loadedBlueprints.Remove(bpName);
        }
    }

    public void InventoryReady()
    {
        // Load the Ship level
        Application.LoadLevel(0);
    }

    public void DisplayVictoryPanel()
    {
        if (!victoryPanel.activeSelf)
        {
            victoryPanel.SetActive(true);
        }
     
    }
}
