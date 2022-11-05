using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.AI.Navigation;

public class ChunkBaker : MonoBehaviour
{
    private List<GameObject> chunkList = new List<GameObject>();
    private NavMeshSurface chunkSurface;
    private GameObject worldRenderer;
    public LayerMask ground;
    private float chunkSize;
    
    // Start is called before the first frame update
    void Start()
    {
        worldRenderer = GameObject.Find("WorldRenderer");
        chunkSize = GameObject.Find("World").GetComponent<World>().chunkSize;
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
        // Create NavMeshLink
        CreateNavMeshLink(chunkRenderer);
        Debug.Log("Bake Chunk");
    }

    private void CreateNavMeshLink(ChunkRenderer chunkRenderer)
    {   
        chunkRenderer.transform.GetChild(0).transform.position = new Vector3 (chunkRenderer.transform.position.x + chunkSize / 2, chunkRenderer.ChunkData.chunkHeight, chunkRenderer.transform.position.z);
        chunkRenderer.transform.GetChild(1).transform.position = new Vector3 (chunkRenderer.transform.position.x, chunkRenderer.ChunkData.chunkHeight, chunkRenderer.transform.position.z + chunkSize / 2);
        chunkRenderer.transform.GetChild(2).transform.position = new Vector3 (chunkRenderer.transform.position.x + chunkSize / 2, chunkRenderer.ChunkData.chunkHeight, chunkRenderer.transform.position.z + chunkSize);
        chunkRenderer.transform.GetChild(3).transform.position = new Vector3 (chunkRenderer.transform.position.x + chunkSize, chunkRenderer.ChunkData.chunkHeight, chunkRenderer.transform.position.z + chunkSize / 2);

        chunkRenderer.transform.GetChild(1).transform.Rotate(0.0f, 45.0f, 0.0f);
        chunkRenderer.transform.GetChild(3).transform.Rotate(0.0f, 45.0f, 0.0f);

        Debug.Log("Chunk: " + chunkRenderer.gameObject.name);
        /*foreach (Transform navMeshLink in chunkRenderer.transform)
        {
            RaycastHit hit;
            if (Physics.Raycast(navMeshLink.transform.position, Vector3.down, out hit, Mathf.Infinity, ground))
            {
               navMeshLink.transform.position = new Vector3 (navMeshLink.transform.position.x, hit.point.y, navMeshLink.transform.position.z);
               Debug.Log("Hit Location: " + hit.point.y);
            }
        }*/

        for (int i = 0; i < 4; i++)
        {
            Vector3 down = transform.TransformDirection(Vector3.down);
            Vector3 tempPosition = chunkRenderer.transform.GetChild(i).transform.position;
            RaycastHit hit;
            if (Physics.Raycast(chunkRenderer.transform.GetChild(i).transform.position, down, out hit, Mathf.Infinity, ground))
            {
               Vector3 newPosition = new Vector3 (tempPosition.x, hit.point.y, tempPosition.z);
               chunkRenderer.transform.GetChild(i).gameObject.SetActive(false);
               chunkRenderer.transform.GetChild(i).transform.position = newPosition;
               chunkRenderer.transform.GetChild(i).gameObject.SetActive(true);
               //Debug.Log("Hit Location: " + hit.point.y);
            }
            Debug.Log("New Height: " + chunkRenderer.transform.GetChild(i).transform.position);
        }

        /*Vector3 down = transform.TransformDirection(Vector3.down);
        RaycastHit hit;
        if (Physics.Raycast(chunkRenderer.transform.GetChild(0).transform.position, down, out hit, Mathf.Infinity, ground))
        {
            chunkRenderer.transform.GetChild(0).transform.position = new Vector3 (chunkRenderer.transform.GetChild(0).transform.position.x, hit.point.y, chunkRenderer.transform.GetChild(0).transform.position.z);
        }
        if (Physics.Raycast(chunkRenderer.transform.GetChild(1).transform.position, down, out hit, Mathf.Infinity, ground))
        {
            chunkRenderer.transform.GetChild(1).transform.position = new Vector3 (chunkRenderer.transform.GetChild(1).transform.position.x, hit.point.y, chunkRenderer.transform.GetChild(1).transform.position.z);
        }
        if (Physics.Raycast(chunkRenderer.transform.GetChild(2).transform.position, down, out hit, Mathf.Infinity, ground))
        {
            chunkRenderer.transform.GetChild(2).transform.position = new Vector3 (chunkRenderer.transform.GetChild(2).transform.position.x, hit.point.y, chunkRenderer.transform.GetChild(2).transform.position.z);
        }
        if (Physics.Raycast(chunkRenderer.transform.GetChild(3).transform.position, down, out hit, Mathf.Infinity, ground))
        {
            chunkRenderer.transform.GetChild(3).transform.position = new Vector3 (chunkRenderer.transform.GetChild(3).transform.position.x, hit.point.y, chunkRenderer.transform.GetChild(3).transform.position.z);
        }*/

        chunkRenderer.transform.GetChild(0).GetComponent<NavMeshLink>().width = chunkSize;
        chunkRenderer.transform.GetChild(1).GetComponent<NavMeshLink>().width = chunkSize;
        chunkRenderer.transform.GetChild(2).GetComponent<NavMeshLink>().width = chunkSize;
        chunkRenderer.transform.GetChild(3).GetComponent<NavMeshLink>().width = chunkSize;

        chunkRenderer.transform.GetChild(0).GetComponent<NavMeshLink>().UpdateLink();
        chunkRenderer.transform.GetChild(1).GetComponent<NavMeshLink>().UpdateLink();
        chunkRenderer.transform.GetChild(2).GetComponent<NavMeshLink>().UpdateLink();
        chunkRenderer.transform.GetChild(3).GetComponent<NavMeshLink>().UpdateLink();

        /*chunkRenderer.transform.GetChild(1).transform.eulerAngles = new Vector3 (
            chunkRenderer.transform.GetChild(1).transform.eulerAngles.x,
            chunkRenderer.transform.GetChild(1).transform.eulerAngles.y - 180,
            chunkRenderer.transform.GetChild(1).transform.eulerAngles.z
            );

        chunkRenderer.transform.GetChild(3).transform.eulerAngles = new Vector3 (
            chunkRenderer.transform.GetChild(3).transform.eulerAngles.x,
            chunkRenderer.transform.GetChild(3).transform.eulerAngles.y - 180,
            chunkRenderer.transform.GetChild(3).transform.eulerAngles.z
            );*/
    }

    /*private void GetAllChunks()
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
    }*/
}
