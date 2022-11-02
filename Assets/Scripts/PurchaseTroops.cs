using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PurchaseTroops : MonoBehaviour
{
    public Transform troop1Prefab;
    public Transform troop2Prefab;

    public Transform spawnPoint;


    public void SpawnTroop1()
    {
        //Temp cost of 100

        if(PlayerStats.Instance.money >= 100)
        {
            Instantiate(troop1Prefab, spawnPoint);
            PlayerStats.Instance.money -= 100;
        }
    }

    public void SpawnTroop2()
    {
        //Temp cost of 100

        if (PlayerStats.Instance.money >= 100)
        {
            Instantiate(troop2Prefab, spawnPoint);
            PlayerStats.Instance.money -= 100;
        }
    }
}
