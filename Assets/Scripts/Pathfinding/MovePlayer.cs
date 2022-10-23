using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovePlayer : MonoBehaviour
{
    public UnityEngine.AI.NavMeshAgent agent;
    public LayerMask hitLayers;

    bool walkPointSet;
    Vector3 target;
    
    // Start is called before the first frame update
    private void Awake()
    {
        agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // If the player has left clicked
        {
            Vector3 mouse = Input.mousePosition; // Get the mouse Position
            Ray castPoint = Camera.main.ScreenPointToRay(mouse); // Cast a ray to get where the mouse is pointing at
            RaycastHit hit; // Stores the position where the ray hit.
            if (Physics.Raycast(castPoint, out hit, Mathf.Infinity, hitLayers)) // If the raycast doesn't hit a wall
            {
                target = hit.point; // Move the target to the mouse position
                agent.SetDestination(target);
            }
        }
    }
}