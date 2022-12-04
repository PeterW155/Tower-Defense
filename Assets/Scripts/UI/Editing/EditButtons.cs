using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditButtons : MonoBehaviour
{
    public void ToggleTerrainEditing()
    {
        //disable other editors
        TowerEditor.Instance.DisableTowerEditing();
        PurchaseTroops.Instance.MenuDisabled();


        if (TerrainEditor.Instance.editing) //if it is already editing turn it off
            TerrainEditor.Instance.DisableTerrainEditing();
        else //enable
            TerrainEditor.Instance.EnableTerrainEditing();
    }
    public void ToggleTowerEditing()
    {
        //disable other editors
        TerrainEditor.Instance.DisableTerrainEditing();
        PurchaseTroops.Instance.MenuDisabled();


        if (TowerEditor.Instance.editing) //if it is already editing turn it off
            TowerEditor.Instance.DisableTowerEditing();
        else //enable
            TowerEditor.Instance.EnableTowerEditing();
    }
    public void ToggleTroopSpawning()
    {
        //disable other editors
        TowerEditor.Instance.DisableTowerEditing();
        TerrainEditor.Instance.DisableTerrainEditing();


        if (PurchaseTroops.Instance.menuActive) //if it is already active turn it off
            PurchaseTroops.Instance.MenuDisabled();
        else //enable
            PurchaseTroops.Instance.MenuEnabled();
    }
}
