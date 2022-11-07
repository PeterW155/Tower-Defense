using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditButtons : MonoBehaviour
{
    public TerrainEditor terrainEditor;
    public TowerEditor towerEditor;
    public PurchaseTroops purchaseTroops;
    public void ToggleTerrainEditing()
    {
        //disable other editors
        if (towerEditor.editing)
            towerEditor.DisableTowerEditing();
        if (purchaseTroops.menuActive)
            purchaseTroops.MenuDisabled();


        if (terrainEditor.editing) //if it is already editing turn it off
            terrainEditor.DisableTerrainEditing();
        else //enable
            terrainEditor.EnableTerrainEditing();
    }
    public void ToggleTowerEditing()
    {
        //disable other editors
        if (terrainEditor.editing)
            terrainEditor.DisableTerrainEditing();
        if (purchaseTroops.menuActive)
            purchaseTroops.MenuDisabled();


        if (towerEditor.editing) //if it is already editing turn it off
            towerEditor.DisableTowerEditing();
        else //enable
            towerEditor.EnableTowerEditing();
    }
    public void ToggleTroopSpawning()
    {
        //disable other editors
        if (towerEditor.editing)
            towerEditor.DisableTowerEditing();
        if (terrainEditor.editing)
            terrainEditor.DisableTerrainEditing();


        if (purchaseTroops.menuActive) //if it is already active turn it off
            purchaseTroops.MenuDisabled();
        else //enable
            purchaseTroops.MenuEnabled();
    }
}
