using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditButtons : MonoBehaviour
{
    public PopupHandler popupHandler;

    public void ToggleTerrainEditing()
    {
        if (TerrainEditor.Instance.editing) //if it is already editing turn it off
            TerrainEditor.Instance.DisableTerrainEditing();
        else
        {
            //disable other editors
            TowerEditor.Instance.DisableTowerEditing();
            PurchaseTroops.Instance.MenuDisabled();
            popupHandler.DisableControls();

            //enable
            TerrainEditor.Instance.EnableTerrainEditing();
        }
    }
    public void ToggleTowerEditing()
    {
        if (TowerEditor.Instance.editing) //if it is already editing turn it off
            TowerEditor.Instance.DisableTowerEditing();
        else
        {
            //disable other editors
            TerrainEditor.Instance.DisableTerrainEditing();
            PurchaseTroops.Instance.MenuDisabled();
            popupHandler.DisableControls();

            //enable
            TowerEditor.Instance.EnableTowerEditing();
        }
    }
    public void ToggleDefault()
    {
        //disable other editors
        TerrainEditor.Instance.DisableTerrainEditing();
        TowerEditor.Instance.DisableTowerEditing();

        //enable
        popupHandler.LoadSavedControls();
    }
}
