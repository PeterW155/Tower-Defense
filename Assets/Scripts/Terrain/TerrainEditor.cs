using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[RequireComponent(typeof(World))]
[ExecuteInEditMode]
public class TerrainEditor : MonoBehaviour
{
    [SerializeField]
    private LayerMask groundMask;
    public BlockType blockType = BlockType.Dirt;

    [HideInInspector]
    public World world;

    public void ModifyTerrain(RaycastHit hit, BlockType blockType = BlockType.Air, bool place = false)
    {
        world.SetBlock(hit, blockType, place);
    }
}
