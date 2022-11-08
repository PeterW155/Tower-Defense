using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
public class ChunkRenderer : MonoBehaviour
{
    MeshFilter meshFilter;
    MeshCollider meshCollider;
    public bool showGizmo = false;

    public ChunkData ChunkData;

    public bool ModifiedByThePlayer
    {
        get
        {
            return ChunkData.modifiedByThePlayer;
        }
        set
        {
            ChunkData.modifiedByThePlayer = value;
        }
    }

    public void InitializeChunk(ChunkData data)
    {
        this.ChunkData = data;
    }

    private void RenderMesh(MeshData meshData)
    {
        if (meshFilter == null)
            meshFilter = GetComponent<MeshFilter>();
        if (meshCollider == null)
            meshCollider = GetComponent<MeshCollider>();

        Mesh mesh = new Mesh();
        mesh.subMeshCount = 2;
        mesh.vertices = meshData.vertices.Concat(meshData.unmodifiableMesh.vertices).ToArray();

        mesh.SetTriangles(meshData.triangles.ToArray(), 0);
        mesh.SetTriangles(meshData.unmodifiableMesh.triangles.Select(val => val + meshData.vertices.Count).ToArray(), 1);

        mesh.uv = meshData.uv.Concat(meshData.unmodifiableMesh.uv).ToArray();
        mesh.RecalculateNormals();

        meshFilter.sharedMesh = mesh;

        //collision meshes
        Mesh collisionMesh = new Mesh();

        collisionMesh.vertices = meshData.colliderVertices.Concat(meshData.unmodifiableMesh.colliderVertices).ToArray();
        collisionMesh.triangles = meshData.colliderTriangles.Concat(meshData.unmodifiableMesh.colliderTriangles.Select(val => val + meshData.colliderVertices.Count)).ToArray();
        collisionMesh.RecalculateNormals();

        meshCollider.sharedMesh = collisionMesh;
    }

    public void UpdateChunk()
    {
        RenderMesh(Chunk.GetChunkMeshData(ChunkData));
    }

    public void UpdateChunk(MeshData data)
    {
        RenderMesh(data);
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (showGizmo)
        {
            if (ChunkData != null)
            {
                if (Selection.activeObject == gameObject)
                    Gizmos.color = new Color(0, 1, 0, 0.4f);
                else
                    Gizmos.color = new Color(1, 0, 1, 0.4f);

                Gizmos.DrawCube(transform.position + new Vector3(ChunkData.chunkSize / 2f, ChunkData.chunkHeight / 2f, ChunkData.chunkSize / 2f), new Vector3(ChunkData.chunkSize, ChunkData.chunkHeight, ChunkData.chunkSize));
            }
        }
    }
#endif
}
