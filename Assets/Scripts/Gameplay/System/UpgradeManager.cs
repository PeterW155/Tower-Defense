using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeManager : MonoBehaviour
{
    public GameObject infoText;
    private int cost;
    private int lvl;
    private string turretType;
    private GameObject target;
    public GameManager gm;

    public void getInfo(int cost, GameObject target, int currentLevel, string turretType)
    {
        this.cost = cost;
        this.lvl = currentLevel;
        this.turretType = turretType;
        this.target = target;
        updateInfo();
    }

    private void updateInfo()
    {
        infoText.GetComponent<Text>().text = "Turret Type: " + turretType + "\nCurrent Level: " + lvl + "\n<b>Cost To Level-Up: $" + cost + " </b>";
    }

    public void upgradeTarget()
    {
        if (gm.GetComponent<PlayerStats>().money >= cost)
        {
            target.GetComponent<TowerData>().Upgrade();
            gm.GetComponent<PlayerStats>().money -= cost;
            gameObject.SetActive(false);
        }
    }

    public void Cancel()
    {
        gameObject.SetActive(false);
    }
}
