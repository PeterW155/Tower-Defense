using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarketBuilding : MonoBehaviour
{
    public int buildingLevel;

    private void Start()
    {
        buildingLevel = 1;
    }

    public void PayPlayer(int level)
    {
        if (level == 1)
        {
            PlayerStats.Instance.money += 150;
        }
        else if (level == 2)
        {
            PlayerStats.Instance.money += 300;
        }
        else
        {
            PlayerStats.Instance.money += 750;
        }
    }
}
