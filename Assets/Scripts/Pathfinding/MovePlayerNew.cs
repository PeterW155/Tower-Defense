using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MovePlayerNew : MonoBehaviour
{
    public NavMeshAgent agent;

    private void Start()
    {
        
    }

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }
    
    public void MoveToDestination(Vector3 destination)
    {
        agent.SetDestination(destination);
    }
}
