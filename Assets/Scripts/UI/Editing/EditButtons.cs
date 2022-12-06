using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EditButtons : MonoBehaviour
{
    public PopupHandler popupHandler;
    public int towerEditorPopupIndex;
    public int terrainEditorPopupIndex;
    public List<Button> disabledDuringWave;

    private int reactivateIndex;

    private static EditButtons _instance;
    public static EditButtons Instance { get { return _instance; } }

    private void Awake()
    {
        // If an instance of this already exists and it isn't this one
        if (_instance != null && _instance != this)
        {
            // We destroy this instance
            Destroy(this.gameObject);
        }
        else
        {
            // Make this the instance
            _instance = this;
        }
    }

    public void ToggleTerrainEditing()
    {
        if (TerrainEditor.Instance.editing) //if it is already editing turn it off
        {
            reactivateIndex = -1;
            TerrainEditor.Instance.DisableTerrainEditing();
        }
        else
        {
            //disable other editors
            TowerEditor.Instance.DisableTowerEditing();
            popupHandler.DisableControls();

            //enable
            TerrainEditor.Instance.EnableTerrainEditing();
        }
    }
    public void ToggleTowerEditing()
    {
        if (TowerEditor.Instance.editing) //if it is already editing turn it off
        {
            reactivateIndex = -1;
            TowerEditor.Instance.DisableTowerEditing();
        }
        else
        {
            //disable other editors
            TerrainEditor.Instance.DisableTerrainEditing();
            popupHandler.DisableControls();

            //enable
            TowerEditor.Instance.EnableTowerEditing();
        }
    }
    public void ToggleDefault()
    {
        reactivateIndex = -1;

        //disable other editors
        TerrainEditor.Instance.DisableTerrainEditing();
        TowerEditor.Instance.DisableTowerEditing();

        //enable
        popupHandler.LoadSavedControls();
    }

    public void DisableButtons()
    {
        foreach (Button button in disabledDuringWave)
            button.interactable = false;

        if (popupHandler.currentActive == terrainEditorPopupIndex)
        {
            TerrainEditor.Instance.DisableTerrainEditing();
            reactivateIndex = terrainEditorPopupIndex;
            popupHandler.ActivatePopup(popupHandler.currentActive);
        }
        if (popupHandler.currentActive == towerEditorPopupIndex)
        {
            TowerEditor.Instance.DisableTowerEditing();
            reactivateIndex = towerEditorPopupIndex;
            popupHandler.ActivatePopup(popupHandler.currentActive);
        }
    }

    public void EnableButtons()
    {
        foreach (Button button in disabledDuringWave)
            button.interactable = true;

        if (popupHandler.currentActive == -1) //activate the last used terrain editor if no current popup is active
        {
            popupHandler.ActivatePopup(reactivateIndex);
            if (reactivateIndex == terrainEditorPopupIndex)
                TerrainEditor.Instance.EnableTerrainEditing();
            else if (reactivateIndex == towerEditorPopupIndex)
                TowerEditor.Instance.EnableTowerEditing();
        }
    }
}
