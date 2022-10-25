using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TerrainEditor))]
public class TerrainEditingInspector : Editor
{
    private static GUIStyle ToggleButtonStyleNormal = null;
    private static GUIStyle ToggleButtonStyleToggled = null;

    private int index = -1;

    private bool editing = false;

    public override void OnInspectorGUI()
    {
        if (ToggleButtonStyleNormal == null)
        {
            ToggleButtonStyleNormal = "Button";
            ToggleButtonStyleToggled = new GUIStyle(ToggleButtonStyleNormal);
            ToggleButtonStyleToggled.normal.background = ToggleButtonStyleToggled.active.background;
        }

        DrawDefaultInspector();

        TerrainEditor terrainEditor = (TerrainEditor)target;

        GUILayout.Space(25);

        GUIStyle style = new GUIStyle();
        style.wordWrap = true;
        style.normal.textColor = Color.cyan;
        GUILayout.Label("\"left-click\" to place block \"shift-left-click\" to remove block. Gizmos must be turned on to work", style, GUILayout.ExpandWidth(true));

        using (new EditorGUILayout.HorizontalScope())
        {
            // Draw Label
            EditorGUILayout.PrefixLabel("Edit");

            // Start Change check to see if we clicked on an already selected button
            EditorGUI.BeginChangeCheck();
            int prevIndex = index;
            // Draw "Toolbar" (a group of selectable buttons). "AppCommand" is the style used for the Tools toolbar
            index = GUILayout.Toolbar(index, new[] { EditorGUIUtility.IconContent("d_editicon.sml") }, "AppCommand");
            // Enter the condition if a button is pressed (even an already selected one)
            if (EditorGUI.EndChangeCheck())
            {
                // if we clicked on the same index, deselect it
                if (index == prevIndex)
                {
                    index = -1;
                    editing = false;
                }
                else
                {
                    editing = true;
                }
            }
        }

    }
    private void OnSceneGUI()
    {
        if (editing)
        {
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));

            TerrainEditor terrainEditor = (TerrainEditor)target;

            if (terrainEditor.world == null)
                terrainEditor.world = terrainEditor.GetComponent<World>();

            if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && !Event.current.alt)
            {
                Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition); ;
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, Mathf.Infinity))
                {
                    if (Event.current.shift)
                        terrainEditor.ModifyTerrainEditor(hit);
                    else
                        terrainEditor.ModifyTerrainEditor(hit, terrainEditor.blockType, true);
                }
            }
        }
    }
}
