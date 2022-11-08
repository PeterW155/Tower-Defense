using System.Runtime.Serialization;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WorldDataSerializationSurrogate : ISerializationSurrogate
{
    // Method called to serialize a WorldData object
    public void GetObjectData(System.Object obj,
                              SerializationInfo info, StreamingContext context)
    {

        WorldData worldData = (WorldData)obj;
        int[,] chunkArray = new int[worldData.chunkDataDictionary.Keys.Count(), 3];
        for (int i = 0; i < worldData.chunkDataDictionary.Keys.Count(); i++)
        {
            chunkArray[i, 0] = worldData.chunkDataDictionary.Keys.ElementAt(i).x;
            chunkArray[i, 1] = worldData.chunkDataDictionary.Keys.ElementAt(i).y;
            chunkArray[i, 2] = worldData.chunkDataDictionary.Keys.ElementAt(i).z;
        }
        info.AddValue("chunkKeys", chunkArray);
        info.AddValue("chunkHeight", worldData.chunkHeight);
        info.AddValue("chunkSize", worldData.chunkSize);
        info.AddValue("border", worldData.border);
        info.AddValue("mapSizeInChunks", worldData.mapSizeInChunks);
        info.AddValue("mapSeedOffsetX", worldData.mapSeedOffset.x);
        info.AddValue("mapSeedOffsetY", worldData.mapSeedOffset.y);
    }

    // Method called to deserialize a WorldData object
    public System.Object SetObjectData(System.Object obj,
                                       SerializationInfo info, StreamingContext context,
                                       ISurrogateSelector selector)
    {

        WorldData worldData = (WorldData)obj;
        worldData.chunkDictionary = new Dictionary<Vector3Int, ChunkRenderer>();
        worldData.chunkDataDictionary = new Dictionary<Vector3Int, ChunkData>();
        int[,] chunkArray = (int[,])info.GetValue("chunkKeys", typeof(int[,]));
        for (int i = 0; i < chunkArray.GetLength(0); i++)
        {
            Vector3Int v3 = new Vector3Int();
            v3.x = chunkArray[i, 0];
            v3.y = chunkArray[i, 1];
            v3.z = chunkArray[i, 2];
            worldData.chunkDictionary.Add(v3, null);
            worldData.chunkDataDictionary.Add(v3, null);
        }
        worldData.chunkHeight = (int)info.GetValue("chunkHeight", typeof(int));
        worldData.chunkSize = (int)info.GetValue("chunkSize", typeof(int));
        worldData.border = (int)info.GetValue("border", typeof(int));
        worldData.mapSizeInChunks = (int)info.GetValue("mapSizeInChunks", typeof(int));
        worldData.mapSeedOffset.x = (int)info.GetValue("mapSeedOffsetX", typeof(int));
        worldData.mapSeedOffset.y = (int)info.GetValue("mapSeedOffsetY", typeof(int));
        obj = worldData;
        return obj;   // Formatters ignore this return value //Seems to have been fixed!
    }
}
