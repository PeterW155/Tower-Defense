using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.AI.Navigation;

public class ChunkBaker : MonoBehaviour
{
    private List<GameObject> chunkList = new List<GameObject>();
    private NavMeshSurface chunkSurface;
    private GameObject worldRenderer;
    
    // Start is called before the first frame update
    void Start()
    {
        worldRenderer = GameObject.Find("WorldRenderer");
        //worldRenderer = this.gameObject;
        //GetAllChunks();
        //BakeAllChunks();
    }

    private void OnEnable()
    {
        World.ChunkUpdated += BakeChunk;
    }

    private void OnDisable()
    {
        World.ChunkUpdated -= BakeChunk;
    }

    private void BakeChunk(ChunkRenderer chunkRenderer)
    {
        chunkRenderer.GetComponent<NavMeshSurface>().BuildNavMesh();
        Debug.Log("Bake Chunk");
    }

    private void GetAllChunks()
    {
        foreach (Transform chunk in worldRenderer.transform)
        {
            Debug.Log("Getting chunk: " + chunk.name);
            //chunk.GetComponent<NavMeshSurface>().BuildNavMesh();
            chunkList.Add(chunk.gameObject);
        }
    }

    private void BakeAllChunks()
    {
        foreach (GameObject chunk in chunkList)
        {
            Debug.Log("Baking chunk: " + chunk.name);
            chunk.GetComponent<NavMeshSurface>().BuildNavMesh();
        }
    }
}
