using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditButtons : MonoBehaviour
{
    public TerrainEditor terrainEditor;
    public TowerEditor towerEditor;
    public void ToggleTerrainEditing()
    {
        if (terrainEditor.editing) //if it is already editing turn it off
            terrainEditor.DisableTerrainEditing();
        else
        {
            //disable other editors
            towerEditor.DisableTowerEditing();
            //enable
            terrainEditor.EnableTerrainEditing();
        }
    }
    public void ToggleTowerEditing()
    {
        if (towerEditor.editing) //if it is already editing turn it off
            towerEditor.DisableTowerEditing();
        else
        {
            //disable other editors
            terrainEditor.DisableTerrainEditing();
            //enable
            towerEditor.EnableTowerEditing();
        }
    }
}
