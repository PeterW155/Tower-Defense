using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

[ExecuteAlways]
public class World : MonoBehaviour
{
    public int mapSizeInChunks = 6;
    public int chunkSize = 16, chunkHeight = 100;
    [Space]
    public BlockDataManager blockDataManager;
    [Space]
    public GameObject chunkPrefab;
    public WorldRenderer worldRenderer;
    [Space]
    public TerrainGenerator terrainGenerator;
    public Vector2Int mapSeedOffset;

    CancellationTokenSource taskTokenSource = new CancellationTokenSource();

    public UnityEvent OnWorldCreated, OnNewChunksGenerated;

    public WorldData worldData;
    public bool IsWorldCreated { get; private set; }

    private Coroutine editorUpdate;


    private void OnEnable()
    {
        EditorApplication.playModeStateChanged += SaveTemp;
        if (!EditorApplication.isPlayingOrWillChangePlaymode)
            LoadWorld();
    }
    private void OnDisable()
    {
        taskTokenSource.Cancel();
        EditorApplication.playModeStateChanged -= SaveTemp;
    }

    private void Awake()
    {
        if (!Application.isEditor)
        {
            LoadWorld();
        }
    }
    private void SaveTemp(PlayModeStateChange change)
    {
        switch (change)
        {
            case PlayModeStateChange.ExitingEditMode:
                SaveWorld(false, true);
                break;
            case PlayModeStateChange.EnteredPlayMode:
                LoadWorld(false, true);
                break;
        }
    }


    public async void GenerateWorld(bool loadOnly = false)
    {
        #if UNITY_EDITOR
        blockDataManager.InitializeBlockDictionary();
        editorUpdate = StartCoroutine(EditorUpdate());
        #endif


        if (!loadOnly)
        {
            worldRenderer.chunkPool.Clear();
            worldRenderer.DeleteRenderers();

            worldData = new WorldData
            {
                chunkHeight = this.chunkHeight,
                chunkSize = this.chunkSize,
                mapSizeInChunks = this.mapSizeInChunks,
                mapSeedOffset = this.mapSeedOffset,
                chunkDataDictionary = new Dictionary<Vector3Int, ChunkData>(),
                chunkDictionary = new Dictionary<Vector3Int, ChunkRenderer>()
            };
        }

        await GenerateWorld(Vector3Int.zero, loadOnly);
    }

    private async Task GenerateWorld(Vector3Int position, bool loadOnly = false)
    {
        WorldGenerationData worldGenerationData = await Task.Run(() => GetPositionsFromCenter(), taskTokenSource.Token);

        if (!loadOnly)
        {
            terrainGenerator.GenerateBiomePoints(position, mapSizeInChunks, chunkSize, mapSeedOffset);


            foreach (Vector3Int pos in worldGenerationData.chunkPositionsToRemove)
            {
                WorldDataHelper.RemoveChunk(this, pos);
            }

            foreach (Vector3Int pos in worldGenerationData.chunkDataToRemove)
            {
                WorldDataHelper.RemoveChunkData(this, pos);
            }


            ConcurrentDictionary<Vector3Int, ChunkData> dataDictionary = null;

            try
            {
                dataDictionary = await CalculateWorldChunkData(worldGenerationData.chunkDataPositionsToCreate);
            }
            catch (Exception)
            {
                Debug.Log("Task canceled");
                return;
            }


            foreach (var calculatedData in dataDictionary)
            {
                worldData.chunkDataDictionary.Add(calculatedData.Key, calculatedData.Value);
            }
        }

        ConcurrentDictionary<Vector3Int, MeshData> meshDataDictionary = new ConcurrentDictionary<Vector3Int, MeshData>();

        List<ChunkData> dataToRender = worldData.chunkDataDictionary
            .Where(keyvaluepair => (loadOnly ? worldGenerationData.chunkPositionsToUpdate : worldGenerationData.chunkPositionsToCreate).Contains(keyvaluepair.Key))
            .Select(keyvalpair => keyvalpair.Value)
            .ToList();

        Debug.Log("runnning");
        meshDataDictionary = await CreateMeshDataAsync(dataToRender);
        try
        {
            
        }
        catch (Exception)
        {
            Debug.LogError("Task canceled");
            Time.timeScale = 1;
            StopCoroutine(editorUpdate);
            return;
        }

        StartCoroutine(ChunkCreationCoroutine(meshDataDictionary, loadOnly));
    }

    private Task<ConcurrentDictionary<Vector3Int, MeshData>> CreateMeshDataAsync(List<ChunkData> dataToRender)
    {
        ConcurrentDictionary<Vector3Int, MeshData> dictionary = new ConcurrentDictionary<Vector3Int, MeshData>();
        return Task.Run(() =>
        {

            foreach (ChunkData data in dataToRender)
            {
                if (taskTokenSource.Token.IsCancellationRequested)
                {
                    taskTokenSource.Token.ThrowIfCancellationRequested();
                }
                MeshData meshData = Chunk.GetChunkMeshData(data);
                dictionary.TryAdd(data.worldPosition, meshData);
            }

            return dictionary;
        }, taskTokenSource.Token
        );
    }

    private Task<ConcurrentDictionary<Vector3Int, ChunkData>> CalculateWorldChunkData(List<Vector3Int> chunkDataPositionsToCreate)
    {
        ConcurrentDictionary<Vector3Int, ChunkData> dictionary = new ConcurrentDictionary<Vector3Int, ChunkData>();

        return Task.Run(() =>
        {
            foreach (Vector3Int pos in chunkDataPositionsToCreate)
            {
                if (taskTokenSource.Token.IsCancellationRequested)
                {
                    taskTokenSource.Token.ThrowIfCancellationRequested();
                }
                ChunkData data = new ChunkData(chunkSize, chunkHeight, this, pos);
                ChunkData newData = terrainGenerator.GenerateChunkData(data, mapSeedOffset);
                dictionary.TryAdd(pos, newData);
            }
            return dictionary;
        },
        taskTokenSource.Token
        );
    }

    IEnumerator EditorUpdate()
    {
        Time.timeScale = 0;
        while (!Application.isPlaying)
        {
            EditorApplication.update += EditorApplication.QueuePlayerLoopUpdate;
            yield return null;
        }
        yield return null;
    }

    IEnumerator ChunkCreationCoroutine(ConcurrentDictionary<Vector3Int, MeshData> meshDataDictionary, bool loadOnly)
    {
        foreach (var item in meshDataDictionary)
        {
            CreateChunk(worldData, item.Key, item.Value, loadOnly);
            yield return 0;
        }
        if (IsWorldCreated == false)
        {
            IsWorldCreated = true;
            OnWorldCreated?.Invoke();
        }
        Time.timeScale = 1;
        StopCoroutine(editorUpdate);
    }

    private void CreateChunk(WorldData worldData, Vector3Int position, MeshData meshData, bool loadOnly)
    {
        ChunkRenderer chunkRenderer = worldRenderer.RenderChunk(worldData, position, meshData);
        if (!loadOnly)
            worldData.chunkDictionary.Add(position, chunkRenderer);

    }

    internal bool SetBlock(RaycastHit hit, BlockType blockType, bool place = false)
    {
        ChunkRenderer chunk = hit.collider.GetComponent<ChunkRenderer>();
        if (chunk == null)
            return false;

        Vector3Int pos = GetBlockPos(hit, place);

        Debug.Log(pos);

        WorldDataHelper.SetBlock(chunk.ChunkData.worldReference, pos, blockType);
        chunk.ModifiedByThePlayer = true;

        if (Chunk.IsOnEdge(chunk.ChunkData, pos))
        {
            List<ChunkData> neighbourDataList = Chunk.GetEdgeNeighbourChunk(chunk.ChunkData, pos);
            foreach (ChunkData neighbourData in neighbourDataList)
            {
                //neighbourData.modifiedByThePlayer = true;
                ChunkRenderer chunkToUpdate = WorldDataHelper.GetChunk(neighbourData.worldReference, neighbourData.worldPosition);
                if (chunkToUpdate != null)
                    chunkToUpdate.UpdateChunk();
            }

        }

        chunk.UpdateChunk();
        return true;
    }

    private Vector3Int GetBlockPos(RaycastHit hit, bool place = false)
    {
        Vector3 pos = new Vector3(
             GetBlockPositionIn(hit.point.x, hit.normal.x, place),
             GetBlockPositionIn(hit.point.y, hit.normal.y, place),
             GetBlockPositionIn(hit.point.z, hit.normal.z, place)
             );

        return Vector3Int.RoundToInt(pos);
    }

    private float GetBlockPositionIn(float pos, float normal, bool place)
    {
        if (Mathf.Abs(pos % 1) == 0.5f)
        {
            pos += place ? (normal / 2) : -(normal / 2);
        }


        return (float)pos;
    }


    private WorldGenerationData GetPositionsFromCenter()
    {
        List<Vector3Int> allChunkPositionsNeeded = WorldDataHelper.GetChunkPositionsAroundOrigin(this);

        List<Vector3Int> allChunkDataPositionsNeeded = WorldDataHelper.GetDataPositionsAroundOrigin(this);

        Vector3Int center = new Vector3Int(chunkSize * mapSizeInChunks, 0, chunkSize * mapSizeInChunks);
        List<Vector3Int> chunkPositionsToCreate = WorldDataHelper.SelectPositonsToCreate(worldData, allChunkPositionsNeeded, center);
        List<Vector3Int> chunkDataPositionsToCreate = WorldDataHelper.SelectDataPositonsToCreate(worldData, allChunkDataPositionsNeeded, center);

        List<Vector3Int> chunkPositionsToRemove = WorldDataHelper.GetUnnededChunks(worldData, allChunkPositionsNeeded);
        List<Vector3Int> chunkDataToRemove = WorldDataHelper.GetUnnededData(worldData, allChunkDataPositionsNeeded);

        WorldGenerationData data = new WorldGenerationData
        {
            chunkPositionsToCreate = chunkPositionsToCreate,
            chunkDataPositionsToCreate = chunkDataPositionsToCreate,
            chunkPositionsToRemove = chunkPositionsToRemove,
            chunkDataToRemove = chunkDataToRemove,
            chunkPositionsToUpdate = allChunkPositionsNeeded
        };
        return data;

    }

    internal BlockType GetBlockFromChunkCoordinates(ChunkData chunkData, int x, int y, int z)
    {
        Vector3Int pos = Chunk.ChunkPositionFromBlockCoords(this, x, y, z);
        ChunkData containerChunk = null;

        worldData.chunkDataDictionary.TryGetValue(pos, out containerChunk);

        if (containerChunk == null)
            return BlockType.Nothing;
        Vector3Int blockInCHunkCoordinates = Chunk.GetBlockInChunkCoordinates(containerChunk, new Vector3Int(x, y, z));
        return Chunk.GetBlockFromChunkCoordinates(containerChunk, blockInCHunkCoordinates);
    }

    public struct WorldGenerationData
    {
        public List<Vector3Int> chunkPositionsToCreate;
        public List<Vector3Int> chunkDataPositionsToCreate;
        public List<Vector3Int> chunkPositionsToRemove;
        public List<Vector3Int> chunkDataToRemove;
        public List<Vector3Int> chunkPositionsToUpdate;
    }

    public Vector3Int GetSurfaceHeightPosition(Vector2 nearestXZ)
    {
        Vector3Int blockPos = new Vector3Int(Mathf.RoundToInt(nearestXZ.x), 0, Mathf.RoundToInt(nearestXZ.y));
        for (int i = chunkHeight - 1; i > 0; i--)
        {
            BlockType block = GetBlockFromChunkCoordinates(null, blockPos.x, i, blockPos.y);
            if (block != BlockType.Nothing && block != BlockType.Air)
            {
                blockPos.y = i;
                break;
            }
        }

        return blockPos;
    }

    private static string worldAssetPath = "/Terrain/Worlds/";
    private static string fileType = "worlddata";

    private static string defaultAssetFolder = "temp";
    [SerializeField]
    [HideInInspector]
    private string customAssetPath;

    //SAVE and LOAD methods
    public void SaveWorld(bool saveAs = false, bool saveTemp = false)
    {
        if (saveAs)
        {
            customAssetPath = EditorUtility.SaveFilePanel("Create World Folder", Application.dataPath + worldAssetPath, "new_world", "folder");
            if (customAssetPath.StartsWith(Application.dataPath))
                customAssetPath = customAssetPath.Substring(Application.dataPath.Length);
            customAssetPath = Path.ChangeExtension(customAssetPath, "").TrimEnd('.');
        }
        string assetName = Path.GetFileNameWithoutExtension(saveTemp ? worldAssetPath + defaultAssetFolder : customAssetPath);
        string assetPath = saveTemp ? worldAssetPath + defaultAssetFolder : customAssetPath;
        if (!Directory.Exists("Assets" + assetPath))
            AssetDatabase.CreateFolder("Assets" + Path.GetDirectoryName(assetPath), assetName);

        assetPath = assetPath + "/";

        Debug.Log("Saved to: " + Application.dataPath + assetPath + assetName + "." + fileType);

        PrefabUtility.SaveAsPrefabAsset(worldRenderer.gameObject, Application.dataPath + assetPath + assetName + ".prefab", out bool success);
        Debug.Log(Application.dataPath + assetPath + assetName + ".prefab");

        BinaryFormatter bf = new BinaryFormatter();

        // 1. Construct a SurrogateSelector object
        SurrogateSelector ss = new SurrogateSelector();

        WorldDataSerializationSurrogate wdss = new WorldDataSerializationSurrogate();
        ss.AddSurrogate(typeof(WorldData), new StreamingContext(StreamingContextStates.All), wdss);

        // 2. Have the formatter use our surrogate selector
        bf.SurrogateSelector = ss;
        FileStream file = new FileStream(Application.dataPath + assetPath + assetName + "." + fileType, FileMode.Create, FileAccess.ReadWrite, FileShare.None) ; //you can call it anything you want
        bf.Serialize(file, worldData);
        file.Close();
        AssetDatabase.Refresh();
    }
    public void LoadWorld(bool loadAs = false, bool loadTemp = false)
    {
        if (loadAs)
        {
            customAssetPath = Path.GetDirectoryName(EditorUtility.OpenFilePanel("Get World Data File", Application.dataPath + worldAssetPath, fileType));
            customAssetPath = customAssetPath.Replace("\\", "/");
            if (customAssetPath.StartsWith(Application.dataPath))
                customAssetPath = customAssetPath.Substring(Application.dataPath.Length);
        }
        string assetName = Path.GetFileNameWithoutExtension(loadTemp ? worldAssetPath + defaultAssetFolder : customAssetPath);
        string assetPath = loadTemp ? worldAssetPath + defaultAssetFolder : customAssetPath;

        assetPath = assetPath + "/";

        Debug.Log("Load from: " + Application.dataPath + assetPath + assetName + "." + fileType);

        if (File.Exists(Application.dataPath + assetPath + assetName + "." + fileType))
        {
            BinaryFormatter bf = new BinaryFormatter();
            // 1. Construct a SurrogateSelector object
            SurrogateSelector ss = new SurrogateSelector();

            WorldDataSerializationSurrogate wdss = new WorldDataSerializationSurrogate();
            ss.AddSurrogate(typeof(WorldData), new StreamingContext(StreamingContextStates.All), wdss);

            // 2. Have the formatter use our surrogate selector
            bf.SurrogateSelector = ss;
            FileStream file = File.Open(Application.dataPath + assetPath + assetName + "." + fileType, FileMode.Open);
            WorldData worldDataTemp = (WorldData)bf.Deserialize(file);
            file.Close();

            worldData = new WorldData
            {
                chunkHeight = worldDataTemp.chunkHeight,
                chunkSize = worldDataTemp.chunkSize,
                mapSizeInChunks = worldDataTemp.mapSizeInChunks,
                mapSeedOffset = worldDataTemp.mapSeedOffset,
                chunkDataDictionary = new Dictionary<Vector3Int, ChunkData>(worldDataTemp.chunkDataDictionary),
                chunkDictionary = new Dictionary<Vector3Int, ChunkRenderer>(worldDataTemp.chunkDictionary)
            };

            GameObject gameObject = PrefabUtility.LoadPrefabContents(Application.dataPath + assetPath + assetName + ".prefab");
            worldRenderer.chunkPool.Clear();
            worldRenderer.DeleteRenderers();
            worldRenderer.LoadRenderersFromPrefab(gameObject, this, ref worldData.chunkDictionary, ref worldData.chunkDataDictionary);

            chunkSize = worldData.chunkSize;
            chunkHeight = worldData.chunkHeight;
            mapSizeInChunks = worldData.mapSizeInChunks;
            mapSeedOffset = worldData.mapSeedOffset;

            GenerateWorld(true);
        }
        else
            throw new Exception("Missing world data files! No world is loaded!");
    }
}

public struct WorldData
{
    public Dictionary<Vector3Int, ChunkData> chunkDataDictionary;
    public Dictionary<Vector3Int, ChunkRenderer> chunkDictionary;
    public int chunkSize;
    public int chunkHeight;
    public int mapSizeInChunks;
    public Vector2Int mapSeedOffset;
}
