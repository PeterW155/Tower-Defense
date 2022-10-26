using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shop : MonoBehaviour
{
    public TurretBlueprint standardTurret;
    public TurretBlueprint anotherTurret;
    public TurretBlueprint fireTurret;


    BuildManager buildManager;

    void Start ()
    {
        buildManager = BuildManager.instance;
    }

    public void SelectStandardTurret()
    {
        Debug.Log("Standard Turret Selected");
        buildManager.SelectTurretToBuild(standardTurret);
         
    }

    public void SelectAnotherTurret()
    {
        Debug.Log("Cannon Turret Selected");
        buildManager.SelectTurretToBuild(anotherTurret);

    }

    public void SelectFireTurret()
    {
        Debug.Log("Fire Turret Selected");
        buildManager.SelectTurretToBuild(fireTurret);
    }
}
