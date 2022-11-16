using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Unity.AI.Navigation;

public class TowerData : MonoBehaviour
{
    public int cost;
    public int lvl;
    [Space]
    private int costToLvl = 350;
    public string type = "Archer";
    [Space]
    public Vector3Int size;
    public GameObject main;
    public GameObject proxy;
    [Space]
    public GameObject lvl2Main;
    public GameObject lvl2Proxy;
    [Space]
    public GameObject lvl3Main;
    public GameObject lvl3Proxy;

    [Space]
    public bool showGizmo;
    public GameObject upgradeUI;



#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (showGizmo)
        {
            if (Selection.activeObject == gameObject)
                Gizmos.color = new Color(0, 1, 0, 0.4f);
            else
                Gizmos.color = new Color(1, 0, 1, 0.4f);

            Gizmos.DrawCube(transform.position + new Vector3(0, size.y / 2f, 0), size);
        }
    }
#endif

    public void BeginUpgrade()
    {
        if (lvl < 3)
        {
            upgradeUI.SetActive(true);
            upgradeUI.GetComponent<UpgradeManager>().getInfo(costToLvl, gameObject, lvl, type);
        }
    }

    void Awake()
    {
        upgradeUI = GameObject.Find("UpgradeUI");
        if(upgradeUI != null)
        {
            Debug.Log("NOT NULL");
        }
        else
        {
            Debug.Log("NULL");
        }
    }


    public void Upgrade()
    {
        if (lvl == 1)
        {
            main.SetActive(false);
            lvl2Main.SetActive(true);
            main = lvl2Main;
            proxy = lvl2Proxy;
            lvl++;
            costToLvl *= 2;
        }else if(lvl == 2)
        {
            main.SetActive(false);
            lvl3Main.SetActive(true);
            main = lvl3Main;
            proxy = lvl3Proxy;
            lvl++;
        }
    }


}
