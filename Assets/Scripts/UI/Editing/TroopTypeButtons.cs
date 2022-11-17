using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TroopTypeButtons : MonoBehaviour
{
    private PurchaseTroops purchaseTroops;

    private void Awake()
    {
        purchaseTroops = PurchaseTroops.Instance;
    }

    public void Troop1()
    {
        purchaseTroops.SpawnTroop1();
    }
    public void Troop2()
    {
        purchaseTroops.SpawnTroop2();
    }
}
