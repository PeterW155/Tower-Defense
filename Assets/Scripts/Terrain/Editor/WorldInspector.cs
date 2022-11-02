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

        GUILayout.Space(25);

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Save World"))
        {
            world.SaveWorld();
        }
        if (GUILayout.Button("Save As World"))
        {
            world.SaveWorld(true);
        }
        GUILayout.EndHorizontal();
        if (GUILayout.Button("Load World"))
        {
            world.LoadWorld(true);
        }
    }
}
