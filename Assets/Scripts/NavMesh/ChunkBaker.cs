using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using NavMeshBuilder = UnityEngine.AI.NavMeshBuilder;
using Unity.AI.Navigation;

public class ChunkBaker : MonoBehaviour
{
    [SerializeField]
    private NavMeshSurface Surface; // To bake navmesh
    [SerializeField]
    private GameObject UnitGameObject; // To track player's location to update navmesh base on its straight distance from the target
    [SerializeField]
    private GameObject World; // To get chunk data
    [SerializeField]
    private float UpdateRate = 0.1f;
    [SerializeField]
    private float MovementThreshold = 3;
    [SerializeField]
    private Vector3 NavMeshSize = new Vector3(20, 20, 20);

    private Vector3 WorldAnchor;
    private NavMeshData NavMeshData;
    private List<NavMeshBuildSource> Sources = new List<NavMeshBuildSource>();
    private List<GameObject> Chunks = new List<GameObject>();
    private List<GameObject> ChunkPath = new List<GameObject>();
    private List<GameObject> ChunkStartAndEnd = new List<GameObject>();

    private int chunkSize;
    
    private void Start()
    {
        chunkSize = World.GetComponent<World>().chunkSize;
        //Debug.Log("Hi");
        GetChunks();
        //Debug.Log("Hello");
        //StartCoroutine(CheckGameObjectMovement());
    }

    private void Update()
    {
        //Debug.Log("Hello World");
        FindStartAndEndChunk();
        //Debug.Log("Chunk Start Location: " + ChunkStartAndEnd[0]);
        //Debug.Log("Chunk End Location: " + ChunkStartAndEnd[1]);
    }

    private IEnumerator CheckGameObjectMovement()
    {
        WaitForSeconds Wait = new WaitForSeconds(UpdateRate);
        for (; ;)
        {
            if (GameObject.Find(UnitGameObject.name))
            {
                BuildNavMesh(true);
            }
            
            else if (Vector3.Distance(WorldAnchor, UnitGameObject.transform.position) > MovementThreshold)
            {
                BuildNavMesh(true);
                WorldAnchor = UnitGameObject.transform.position;
            }

            yield return Wait;
        }
    }

    private void BuildNavMesh(bool Async)
    {
        Bounds navMeshBounds = new Bounds(UnitGameObject.transform.position, NavMeshSize);
        List<NavMeshBuildMarkup> markups = new List<NavMeshBuildMarkup>();

        List<NavMeshModifier> modifiers;
        if (Surface.collectObjects == CollectObjects.Children)
        {
            modifiers = new List<NavMeshModifier>(Surface.GetComponentsInChildren<NavMeshModifier>());
        }
        else
        {
            modifiers = NavMeshModifier.activeModifiers;
        }

        for (int i = 0; i < modifiers.Count; i++)
        {
            if (((Surface.layerMask & (1 << modifiers[i].gameObject.layer)) == 1) && modifiers[i].AffectsAgentType(Surface.agentTypeID))
            {
                markups.Add(new NavMeshBuildMarkup()
                {
                    root = modifiers[i].transform,
                    overrideArea = modifiers[i].overrideArea,
                    area = modifiers[i].area,
                    ignoreFromBuild = modifiers[i].ignoreFromBuild
                });
            }
        }

        if (Surface.collectObjects == CollectObjects.Children)
        {
            NavMeshBuilder.CollectSources(Surface.transform, Surface.layerMask, Surface.useGeometry, Surface.defaultArea, markups, Sources);
        }
        else
        {
            NavMeshBuilder.CollectSources(navMeshBounds, Surface.layerMask, Surface.useGeometry, Surface.defaultArea, markups, Sources);
        }

        Sources.RemoveAll(source => source.component != null && source.component.gameObject.GetComponent<NavMeshAgent>() != null);

        if (Async)
        {
            NavMeshBuilder.UpdateNavMeshDataAsync(NavMeshData, Surface.GetBuildSettings(), Sources, new Bounds(UnitGameObject.transform.position, NavMeshSize));
        }
    }

    private void GetChunks()
    {
        foreach (Transform chunk in World.transform.Find("WorldRenderer"))
        {
            Chunks.Add(chunk.gameObject);
        }
    }

    private void FindStartAndEndChunk()
    {
        //Debug.Log("Hello");
        //Debug.Log(UnitGameObject.transform.position);
        if (UnitGameObject != null)
        {
            FindWhichChunkGameObjectIsIn(UnitGameObject.transform.position);
        }
        //Debug.Log(UnitGameObject.GetComponent<MovePlayer>().target);
        if (UnitGameObject.tag == "Player")
        {
            FindWhichChunkGameObjectIsIn(UnitGameObject.GetComponent<MovePlayer>().target);
        }
        else if (UnitGameObject.tag == "Enemy")
        {
            FindWhichChunkGameObjectIsIn(UnitGameObject.GetComponent<MoveEnemy>().target);
        }
    }

    private void FindWhichChunkGameObjectIsIn(Vector3 currentObjectPosition)
    {
        //float x = UnitGameObject.transform.position.x;
        //float z = UnitGameObject.transform.position.z;
        foreach (GameObject chunk in Chunks)
        {
            string chunkCoordName = chunk.name.Substring(chunk.name.LastIndexOf('(') + 1).Remove(chunk.name.Substring(chunk.name.LastIndexOf('(') + 1).Length - 1, 1);
            string[] chunkCoordList = chunkCoordName.Split(',');
            Vector3 chunkPosition = new Vector3(int.Parse(chunkCoordList[0]), int.Parse(chunkCoordList[1]), int.Parse(chunkCoordList[2]));

            if (CheckGameObjectInChunk(currentObjectPosition, chunkPosition))
            {
                Debug.Log("Chunk: " + chunkCoordName);
                ChunkStartAndEnd.Add(chunk);
                break;
            }
        }
    }

    private bool CheckGameObjectInChunk(Vector3 currentObjectPosition, Vector3 chunkPosition)
    {
        return (currentObjectPosition.x > chunkPosition.x && 
            currentObjectPosition.x < chunkPosition.x + chunkSize && 
            currentObjectPosition.z > chunkPosition.z &&
            currentObjectPosition.z < chunkPosition.z + chunkSize);
    }
}
