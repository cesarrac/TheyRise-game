using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(Enemy_SquadSpawner))]
public class SquadSpawner_ButtonInEditor : Editor {

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (GUILayout.Button("SPAWN"))
        {
            ((Enemy_SquadSpawner)target).Spawn();
        }

        if (GUILayout.Button("Reset"))
        {
            ((Enemy_SquadSpawner)target).Reset();
        }

        if (GUILayout.Button("Kill"))
        {
            ((Enemy_SquadSpawner)target).KillAll();
        }
    }
}
