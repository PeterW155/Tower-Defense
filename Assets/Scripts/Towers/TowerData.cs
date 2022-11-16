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
    public Vector3Int size;
    public GameObject main;
    public GameObject proxy;
    public GameObject lvl2Main;
    public GameObject lvl2Proxy;
    [Space]
    public bool showGizmo;


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

    private void OnMouseDown()
    {
        main.SetActive(false);
        lvl2Main.SetActive(true);
        main = lvl2Main;
        proxy = lvl2Proxy;
    }
}
