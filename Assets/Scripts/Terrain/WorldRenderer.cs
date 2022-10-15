using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class WorldRenderer : MonoBehaviour
{
    public GameObject chunkPrefab;
    public Queue<ChunkRenderer> chunkPool = new Queue<ChunkRenderer>();

    public void Clear(WorldData worldData)
    {
        foreach (var item in worldData.chunkDictionary.Values)
        {
            #if UNITY_EDITOR
            DestroyImmediate(item.gameObject);
            #else
            Destroy(item.gameObject);
            #endif

        }
        chunkPool.Clear();
    }

    public void DeleteRenderers()
    {
        if (chunkPool == null || chunkPool.Count() <= 0)
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                #if UNITY_EDITOR
                DestroyImmediate(transform.GetChild(i).gameObject);
                #else
                Destroy(transform.GetChild(i).gameObject);
                #endif
            }
    }

    public void LoadRenderersFromPrefab(GameObject gameObject, World world, ref Dictionary<Vector3Int, ChunkRenderer> chunkDictionary, ref Dictionary<Vector3Int, ChunkData> chunkDataDictionary)
    {
        foreach (Transform child in gameObject.transform)
        {
            GameObject newChild = Instantiate(child.gameObject, transform);
            Vector3Int pos = Vector3Int.FloorToInt(newChild.transform.position);
            newChild.name = "Chunk" + pos;
            if (chunkDictionary.ContainsKey(pos))
            {
                ChunkRenderer cr = newChild.GetComponent<ChunkRenderer>();
                chunkPool.Enqueue(cr);
                cr.ChunkData.worldReference = world;
                chunkDictionary[pos] = cr;
                chunkDataDictionary[pos] = cr.ChunkData;
            }
            else
                throw new Exception("Corrupted world data. Chunk positions do not match.");
            newChild.SetActive(false);
        }
    }

    internal ChunkRenderer RenderChunk(WorldData worldData, Vector3Int position, MeshData meshData)
    {
        ChunkRenderer newChunk = null;
        if (chunkPool.Count > 0)
        {
            newChunk = chunkPool.Dequeue();
            newChunk.transform.position = position;
        }
        else
        {
            GameObject chunkObject = Instantiate(chunkPrefab, position, Quaternion.identity, transform);
            newChunk = chunkObject.GetComponent<ChunkRenderer>();
        }

        newChunk.InitializeChunk(worldData.chunkDataDictionary[position]);
        newChunk.UpdateChunk(meshData);
        newChunk.gameObject.SetActive(true);
        return newChunk;
    }

    public void RemoveChunk(ChunkRenderer chunk)
    {
        chunk.gameObject.SetActive(false);
        chunkPool.Enqueue(chunk);
        Debug.Log(chunkPool.Count());
    }
}