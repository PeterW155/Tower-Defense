using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavMeshUpdate : MonoBehaviour
{
    void OnEnable()
    {
        World.ChunkUpdated += Test;
    }

    void OnDisable()
    {
        World.ChunkUpdated -= Test;
    }

    void Test()
    {
        Debug.Log("chunk updated");
    }
}
