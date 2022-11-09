using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyTargetPathChecker : MonoBehaviour
{
    private LineRenderer myLineRenderer;
    private NavMeshAgent agent;
    public GameObject enemySpawnPoint;
    public GameObject enemyTarget;
    private static bool hasAtLeastOnePath = true;
    private NavMeshPath pathToDraw;
    private static EnemyTargetPathChecker _instance;
    public static EnemyTargetPathChecker Instance { get { return _instance; } }
    private void Awake()
    {
        // If an instance of this already exists and it isn't this one
        if (_instance != null && _instance != this)
        {
            // We destroy this instance
            Destroy(this.gameObject);
        }
        else
        {
            // Make this the instance
            _instance = this;
        }

        myLineRenderer = enemyTarget.GetComponent<LineRenderer>();
        pathToDraw = new NavMeshPath();
        agent = enemyTarget.GetComponent<NavMeshAgent>();

        myLineRenderer.startWidth = 0.15f;
        myLineRenderer.endWidth = 0.15f;
        myLineRenderer.positionCount = 0;
        
        DrawPath();
    }

    public bool CheckPathFromTargetToEnemy()
    {
        //var enemyUnits = GameObject.FindGameObjectsWithTag("Enemy");
        NavMeshPath path = new NavMeshPath();
        bool validPath = NavMesh.CalculatePath(enemyTarget.transform.position, enemySpawnPoint.transform.position, NavMesh.AllAreas, path);
        
        //Debug.Log("Calculate Path: " + NavMesh.CalculatePath(enemyTarget.transform.position, enemySpawnPoint.transform.position, NavMesh.AllAreas, path));
        if (validPath && path.status == NavMeshPathStatus.PathComplete)
        {
            hasAtLeastOnePath = true;
            Debug.Log("Is there a path? " + hasAtLeastOnePath);
        }
        else
        {
            hasAtLeastOnePath = false;
            Debug.Log("Is there a path? " + hasAtLeastOnePath);
        }
        
        /*foreach (GameObject enemyUnit in enemyUnits)
        {
            if (NavMesh.CalculatePath(enemyTarget.transform.position, enemyUnit.transform.position, NavMesh.AllAreas, new NavMeshPath()))
            {
                hasAtLeastOnePath = true;
                break;
            }
        }*/
        DrawPath();
        return hasAtLeastOnePath;
    }

    
    /*public static bool EnemyHasAtLeastOnePath()
    {
        return hasAtLeastOnePath;
    }*/

    void DrawPath()
    {
        NavMesh.CalculatePath(enemyTarget.transform.position, enemySpawnPoint.transform.position, NavMesh.AllAreas, pathToDraw);
        myLineRenderer.positionCount = pathToDraw.corners.Length;
        myLineRenderer.SetPositions(pathToDraw.corners);

        Debug.Log(string.Join(", ", pathToDraw.corners));

        if (pathToDraw.corners.Length < 2)
        {
            return;
        }


        /*for (int i = 1; i < pathToDraw.corners.Length; i++)
        {
            Vector3 pointPosition = new Vector3(pathToDraw.corners[i].x, pathToDraw.corners[i].y, pathToDraw.corners[i].z);
            myLineRenderer.SetPosition(i, pointPosition);
        }*/
    }
}
