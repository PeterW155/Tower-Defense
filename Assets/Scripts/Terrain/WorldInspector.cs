using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(World))]
public class WorldInspector : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        World world = (World)target;

        GUILayout.Space(25);

        if (GUILayout.Button("Generate World"))
        {
            world.GenerateWorld();
        }
    }
}
