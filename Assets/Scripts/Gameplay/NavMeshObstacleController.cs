using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NavMeshObstacleController : MonoBehaviour
{
    private void Awake()
    {
        StartCoroutine(GetAllUnitsWithSameTag());
    }

    private IEnumerator GetAllUnitsWithSameTag()
    {
        for (; ;)
        {
            EnableNavMeshObstacleIfTowerIsPlaced();
            
            yield return new WaitForSeconds(0.1f);
        }
    }

    private void EnableNavMeshObstacleIfTowerIsPlaced()
    {
        var towers = GameObject.FindGameObjectsWithTag("Tower");
        foreach (GameObject tower in towers)
        {
            if (!tower.transform.Find("Proxy").gameObject.activeSelf)
            {
                tower.GetComponent<NavMeshObstacle>().enabled = true;
            }
            else
            {
                tower.GetComponent<NavMeshObstacle>().enabled = false;
            }
        }
    }
}
