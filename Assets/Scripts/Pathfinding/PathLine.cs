using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PathLine : MonoBehaviour
{   
    //List<
    private void Awake()
    {
        StartCoroutine(GetAllUnitsWithSameTag());
    }

    private IEnumerator GetAllUnitsWithSameTag()
    {
        for (; ;)
        {
            Debug.Log("Running PathLine.cs");
            ShowOnlyOneEnemyPathLine();
            
            yield return new WaitForSeconds(0.1f);
        }
    }

    private void ShowOnlyOneEnemyPathLine()
    {
        var enemyUnits = GameObject.FindGameObjectsWithTag("Enemy");
        NavMeshAgent previousAgent = null;
        //int count = 0;
        foreach (GameObject enemyUnit in enemyUnits)
        {   
            NavMeshAgent agent = enemyUnit.GetComponent<NavMeshAgent>();
            if (previousAgent == null && agent.hasPath)
            {
                enemyUnit.GetComponent<LineRenderer>().enabled = true;
            }
            else if (previousAgent != null && agent.destination == previousAgent.destination)
            {          
                enemyUnit.GetComponent<LineRenderer>().enabled = false;
            }
            else if (previousAgent != null && !DeterminePathSimilarityByCorner(agent, previousAgent))
            {
                enemyUnit.GetComponent<LineRenderer>().enabled = true;
            }

            // TODO: Group all units going to same destination as a list and compare their path similarities
            // ideally units going to one destination should only display one path line. Only displays different
            // paths if they have different destinations or have very different paths.

            /*else if (previousAgent != null && determinePathSimilarityByCorner(agent, previousAgent))
            {
                /if (count >= enemyUnits.Length / 2)
                {
                    Debug.Log("Enemy with same path draw");
                    //DrawPath(enemyUnit, agent, agent.GetComponent<LineRenderer>());
                    enemyUnit.GetComponent<LineRenderer>().enabled = true;
                    count = 0;
                }
                else
                {
                    Debug.Log("Enemy with same path not draw");
                    enemyUnit.GetComponent<LineRenderer>().enabled = false;
                    count++;
                }
            }*/
            previousAgent = agent;
        }
    }

    private void GetAllPlayerWithTag()
    {
        
    }

    private bool DeterminePathSimilarityByCorner(NavMeshAgent agent1, NavMeshAgent agent2)
    {
        Vector3[] agent1Corners = agent1.path.corners;
        Vector3[] agent2Corners = agent2.path.corners;

        if (agent1Corners.Length == agent2Corners.Length)
        {
            for (int i = 0; i < agent1Corners.Length; i++)
            {
                float cornerXDifference = Mathf.Abs(agent1Corners[i].x - agent2Corners[i].x);
                float cornerZDifference = Mathf.Abs(agent1Corners[i].z - agent2Corners[i].z);

                if (cornerXDifference >= 25.0f && cornerZDifference >= 25.0f)
                {
                    return false;
                }
            }
            return true;
        }
        return false;
    }

    /*private void DrawPath(GameObject unit, NavMeshAgent agent, LineRenderer myLineRenderer)
    {
        myLineRenderer.positionCount = agent.path.corners.Length;
        myLineRenderer.SetPosition(0, unit.transform.position);

        if (agent.path.corners.Length < 2)
        {
            return;
        }

        for (int i = 1; i < agent.path.corners.Length; i++)
        {
            Vector3 pointPosition = new Vector3(agent.path.corners[i].x, agent.path.corners[i].y, agent.path.corners[i].z);
            myLineRenderer.SetPosition(i, pointPosition);
        }
    }*/
}
