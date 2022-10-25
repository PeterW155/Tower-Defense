using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BedrockHandler : BlockLayerHandler
{
    protected override bool TryHandling(ChunkData chunkData, int x, int y, int z, int surfaceHeightNoise, Vector2Int mapSeedOffset)
    {
        if (y <= 1)
        {
            Vector3Int pos = new Vector3Int(x, y, z);
            Chunk.SetBlock(chunkData, pos, BlockType.Bedrock);
            return true;
        }
        return false;
    }
}
