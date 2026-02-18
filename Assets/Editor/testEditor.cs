using UnityEditor;
using UnityEngine;

public class testEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        WormScript script = (WormScript)target;
        if (GUILayout.Button("Reset Velocity"))
        {
            // logique custom
        }
    }
}
