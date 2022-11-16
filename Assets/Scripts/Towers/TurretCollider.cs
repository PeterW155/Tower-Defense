using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretCollider : MonoBehaviour
{
    public GameObject turret;

    private void OnMouseDown()
    {
        turret.GetComponent<TowerData>().BeginUpgrade();
        Debug.Log("---------------");
    }

}
