using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMovement : MonoBehaviour
{

    public float speed = 10f;
    private Transform target;
    private int wavepointIndex = 0;

    // Start is called before the first frame update
    void Start()
    {
        target = Waypoints.waypoints[0];

        // Call the pathfinding algorithm A*
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 dir = target.position - transform.position;
        transform.Translate(dir.normalized * speed * Time.deltaTime, Space.World);

        if (Vector3.Distance(transform.position, target.position) <= .2f)
        {
            GetNextWaypoint();
        }

        // If terrain is changed, call the pathfinding algorithm again
    }

    void GetNextWaypoint()
    {
        if (wavepointIndex >= Waypoints.waypoints.Length-1)
        {
            Destroy(gameObject);
            return;
        }

        wavepointIndex++;
        target = Waypoints.waypoints[wavepointIndex];
    }

    void AStarPathFinding()
    {
        /* 
        OPEN_LIST
        CLOSED_LIST
        ADD start_cell to OPEN_LIST

        LOOP
            current_cell = cell in OPEN_LIST with the lowest F_COST
            REMOVE current_cell from OPEN_LIST
            ADD current_cell to CLOSED_LIST

        IF current_cell is finish_cell
            RETURN

        FOR EACH adjacent_cell to current_cell
            IF adjacent_cell is unwalkable OR adjacent_cell is in CLOSED_LIST
                SKIP to the next adjacent_cell

            IF new_path to adjacent_cell is shorter OR adjacent_cell is not in OPEN_LIST
                SET F_COST of adjacent_cell
                SET parent of adjacent_cell to current_cell
                IF adjacent_cell is not in OPEN_LIST
                    ADD adjacent_cell to OPEN_LIST
        */

        
    }
}
