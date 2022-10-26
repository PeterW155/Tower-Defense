using System.Collections.Generic;
using UnityEngine;

public class UnitSelections : MonoBehaviour
{
    public List<GameObject> unitList = new List<GameObject>();
    public List<GameObject> unitsSelected = new List<GameObject>();

    private static UnitSelections _instance;
    public static UnitSelections Instance { get { return _instance; } }

    void Awake()
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

    public void ClickSelect(GameObject unitToAdd)
    {
        DeselectAll();
        unitsSelected.Add(unitToAdd);
        unitToAdd.transform.GetChild(0).gameObject.SetActive(true);
        if (LayerMask.LayerToName(unitToAdd.gameObject.layer) == "Player")
        {
            unitToAdd.GetComponent<MovePlayer>().enabled = true;
        }
    }

    public void ShiftClickSelect(GameObject unitToAdd)
    {
        if (!unitsSelected.Contains(unitToAdd))
        {
            unitsSelected.Add(unitToAdd);
            unitToAdd.transform.GetChild(0).gameObject.SetActive(true);
            if (LayerMask.LayerToName(unitToAdd.gameObject.layer) == "Player")
            {
                unitToAdd.GetComponent<MovePlayer>().enabled = true;
            }
        }
        else
        {
            if (LayerMask.LayerToName(unitToAdd.gameObject.layer) == "Player")
            {
                unitToAdd.GetComponent<MovePlayer>().enabled = false;
            }
            unitToAdd.transform.GetChild(0).gameObject.SetActive(false);
            unitsSelected.Remove(unitToAdd);
        }
    }

    public void DragSelect(GameObject unitToAdd)
    {
        if (!unitsSelected.Contains(unitToAdd))
        {
            unitsSelected.Add(unitToAdd);
            unitToAdd.transform.GetChild(0).gameObject.SetActive(true);
            if (LayerMask.LayerToName(unitToAdd.gameObject.layer) == "Player")
            {
                unitToAdd.GetComponent<MovePlayer>().enabled = true;
            }
        }
    }

    public void DeselectAll()
    {
        foreach (var unit in unitsSelected)
        {
            if (LayerMask.LayerToName(unit.gameObject.layer) == "Player")
            {
                unit.GetComponent<MovePlayer>().enabled = false;
            }           
            unit.transform.GetChild(0).gameObject.SetActive(false);
        }
        
        unitsSelected.Clear();
    }

    public void Deselect(GameObject unitToDeselect)
    {
        
    }
}
