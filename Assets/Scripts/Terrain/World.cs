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

[ExecuteAlways]
public class World : MonoBehaviour
{
    public delegate void WorldEvent();
    public static event WorldEvent ChunkUpdated;
    public static event WorldEvent WorldLoaded;

    public int mapSizeInChunks = 6;
    public int chunkSize = 16, chunkHeight = 100;
    public int border = 12;
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

    private static World _instance;
    public static World Instance { get { return _instance; } }

    private void OnEnable()
    {
        #if UNITY_EDITOR
        EditorApplication.playModeStateChanged += SaveTemp;
        if (!EditorApplication.isPlayingOrWillChangePlaymode) //this will load the editor for non-play mode
            LoadWorld();
        #endif
    }
    private void OnDisable()
    {
        taskTokenSource.Cancel();
        #if UNITY_EDITOR
        EditorApplication.playModeStateChanged -= SaveTemp;
        #endif
    }

    private void Awake()
    {
        if (Application.isPlaying)
        {
            LoadWorld(); //called during build play
        }

        // If an instance of this already exists and it isn't this one
        if (_instance != null && _instance != this)
        {
            // We destroy this instance
            Destroy(this.gameObject);
        }
        else
        {
            // Make this the instance
            _instance = this;
        }
    }

    #if UNITY_EDITOR
    private void SaveTemp(PlayModeStateChange change)
    {
        switch (change)
        {
            case PlayModeStateChange.ExitingEditMode:
                SaveWorld(false, true);
                break;
            case PlayModeStateChange.EnteredPlayMode:
                //LoadWorld(false, true); //this loads the editor version of play mode (only called if this scene is first)
                break;
        }
    }
    #endif

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
                border = this.border,
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

        try
        {
            meshDataDictionary = await CreateMeshDataAsync(dataToRender);
        }
        catch (Exception)
        {
            Debug.LogError("Task canceled");
            Time.timeScale = 1;
            #if UNITY_EDITOR
            if (editorUpdate != null)
                StopCoroutine(editorUpdate);
            #endif
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
                ChunkData newData = terrainGenerator.GenerateChunkData(data, mapSeedOffset, worldData);
                dictionary.TryAdd(pos, newData);
            }
            return dictionary;
        },
        taskTokenSource.Token
        );
    }

    #if UNITY_EDITOR
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
    #endif

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
            if (ChunkUpdated != null)
                ChunkUpdated();
            if (WorldLoaded != null)
                WorldLoaded();
        }
        Time.timeScale = 1;
        #if UNITY_EDITOR
        if (editorUpdate != null)
            StopCoroutine(editorUpdate);
        #endif
    }

    private void CreateChunk(WorldData worldData, Vector3Int position, MeshData meshData, bool loadOnly)
    {
        ChunkRenderer chunkRenderer = worldRenderer.RenderChunk(worldData, position, meshData);
        if (!loadOnly)
            worldData.chunkDictionary.Add(position, chunkRenderer);

    }

    internal bool IsBlockModifiable(Vector3Int blockWorldPos)
    {
        Vector3Int pos = Chunk.ChunkPositionFromBlockCoords(this, blockWorldPos.x, blockWorldPos.y, blockWorldPos.z);
        ChunkData containerChunk = null;

        worldData.chunkDataDictionary.TryGetValue(pos, out containerChunk);

        if (containerChunk != null)
        {
            Vector3Int blockPos = Chunk.GetBlockInChunkCoordinates(containerChunk, new Vector3Int(blockWorldPos.x, blockWorldPos.y, blockWorldPos.z));

            if (containerChunk.unmodifiableColumns.Contains(new Vector2Int(blockPos.x, blockPos.z))) //check if column is unmodifiable
                return false;
            else
                return true;
        }
        else
            return false;
    }

    internal BlockType GetBlock(RaycastHit hit, bool place = false)
    {
        ChunkRenderer chunk = hit.collider.GetComponent<ChunkRenderer>();
        if (chunk == null)
            return BlockType.Nothing;

        Vector3Int pos = Vector3Int.RoundToInt(GetBlockPos(hit, place));

        return WorldDataHelper.GetBlock(chunk.ChunkData.worldReference, pos);
    }

    internal bool GetBlockVolume(Vector3Int corner1, Vector3Int corner2, bool checkEmpty) //if it is checking empty: true if empty, false if any blocks || if checking filled: true if filled, false if any empty
    {
        Vector3Int size = corner2 - corner1;

        for (int x = 0; (size.x > 0 ? x <= size.x : x >= size.x); x += (size.x > 0 ? 1 : -1))
        {
            for (int y = 0; (size.y > 0 ? y <= size.y : y >= size.y); y += (size.y > 0 ? 1 : -1))
            {
                for (int z = 0; (size.z > 0 ? z <= size.z : z >= size.z); z += (size.z > 0 ? 1 : -1))
                {
                    if (!IsBlockModifiable(new Vector3Int(corner1.x + x, corner1.y + y, corner1.z + z))) //check if column is unmodifiable
                        return false;

                    BlockType block = GetBlockFromChunkCoordinates(null, corner1.x + x, corner1.y + y, corner1.z + z);
                    if (checkEmpty ? //whether to be checking for emptiness or filled
                        block != BlockType.Nothing && block != BlockType.Air : //if the block is anything but empty
                        block == BlockType.Nothing || block == BlockType.Air || block == BlockType.Barrier)  //if the block is empty
                        return false;
                }
            }
        }
        return true;
    }

    internal bool SetBlock(RaycastHit hit, BlockType blockType, bool place = false)
    {
        HashSet<ChunkRenderer> updateChunks = new HashSet<ChunkRenderer>();

        Vector3Int pos = Vector3Int.RoundToInt(GetBlockPos(hit, place));

        Vector3Int chunkPos = Chunk.ChunkPositionFromBlockCoords(this, pos.x, pos.y, pos.z);

        ChunkRenderer chunk = WorldDataHelper.GetChunk(this, chunkPos);
        if (chunk == null)
            return false;

        WorldDataHelper.SetBlock(chunk.ChunkData.worldReference, pos, blockType);
        chunk.ModifiedByThePlayer = true;
        updateChunks.Add(chunk);

        if (Chunk.IsOnEdge(chunk.ChunkData, pos))
        {
            List<ChunkData> neighbourDataList = Chunk.GetEdgeNeighbourChunk(chunk.ChunkData, pos);
            foreach (ChunkData neighbourData in neighbourDataList)
            {
                //neighbourData.modifiedByThePlayer = true;
                if (neighbourData != null)
                {
                    ChunkRenderer chunkToUpdate = WorldDataHelper.GetChunk(neighbourData.worldReference, neighbourData.worldPosition);
                    if (chunkToUpdate != null)
                        updateChunks.Add(chunkToUpdate);
                }
            }

        }

        foreach (ChunkRenderer cr in updateChunks)
            cr.UpdateChunk();
        if (ChunkUpdated != null)
            ChunkUpdated();
        return true;
    }

    internal bool SetBlockVolume(Vector3Int corner1, Vector3Int corner2, BlockType blockType)
    {
        Vector3Int size = corner2 - corner1;

        HashSet<ChunkRenderer> updateChunks = new HashSet<ChunkRenderer>();

        for (int x = 0; (size.x > 0 ? x <= size.x : x >= size.x); x += (size.x > 0 ? 1 : -1))
        {
            for (int y = 0; (size.y > 0 ? y <= size.y : y >= size.y); y += (size.y > 0 ? 1 : -1))
            {
                for (int z = 0; (size.z > 0 ? z <= size.z : z >= size.z); z += (size.z > 0 ? 1 : -1))
                {
                    Vector3Int pos = Vector3Int.RoundToInt(new Vector3(corner1.x + x, corner1.y + y, corner1.z + z));
                    Vector3Int chunkPos = Chunk.ChunkPositionFromBlockCoords(this, corner1.x + x, corner1.y + y, corner1.z + z);

                    ChunkRenderer chunk = WorldDataHelper.GetChunk(this, chunkPos);
                    if (chunk == null)
                        return false;

                    updateChunks.Add(chunk);

                    WorldDataHelper.SetBlock(chunk.ChunkData.worldReference, pos, blockType);
                    chunk.ModifiedByThePlayer = true;

                    if (Chunk.IsOnEdge(chunk.ChunkData, pos))
                    {
                        List<ChunkData> neighbourDataList = Chunk.GetEdgeNeighbourChunk(chunk.ChunkData, pos);
                        foreach (ChunkData neighbourData in neighbourDataList)
                        {
                            //neighbourData.modifiedByThePlayer = true;
                            if (neighbourData != null)
                            {
                                ChunkRenderer chunkToUpdate = WorldDataHelper.GetChunk(neighbourData.worldReference, neighbourData.worldPosition);
                                if (chunkToUpdate != null)
                                    updateChunks.Add(chunkToUpdate);
                            }
                        }

                    }
                }
            }
        }
        foreach (ChunkRenderer cr in updateChunks)
            cr.UpdateChunk();
        if (ChunkUpdated != null)
            ChunkUpdated();
        return true;
    }

    public void SetModifiability(RaycastHit hit, bool unlock)
    {
        Vector3Int blockWorldPos = Vector3Int.RoundToInt(GetBlockPos(hit));

        Vector3Int chunkPos = Chunk.ChunkPositionFromBlockCoords(this, blockWorldPos.x, blockWorldPos.y, blockWorldPos.z);

        ChunkRenderer chunk = WorldDataHelper.GetChunk(this, chunkPos);

        Vector3Int blockPos = Chunk.GetBlockInChunkCoordinates(chunk.ChunkData, new Vector3Int(blockWorldPos.x, blockWorldPos.y, blockWorldPos.z));

        if (chunk != null)
        {
            Vector2Int columnPos = new Vector2Int(blockPos.x, blockPos.z);
            if (unlock && chunk.ChunkData.unmodifiableColumns.Contains(columnPos))
            {
                chunk.ChunkData.unmodifiableColumns.Remove(columnPos);
                chunk.UpdateChunk();
                if (ChunkUpdated != null)
                    ChunkUpdated();
            }
            else if (!unlock)
            {
                chunk.ChunkData.unmodifiableColumns.Add(columnPos);
                chunk.UpdateChunk();
                if (ChunkUpdated != null)
                    ChunkUpdated();
            }
        }
    }

    public Vector3 GetBlockPos(RaycastHit hit, bool place = false, int[] baseWidthLength = null )
    {
        Vector3 pos = new Vector3(
             GetBlockPositionIn(hit.point.x, hit.normal.x, place),
             GetBlockPositionIn(hit.point.y, hit.normal.y, place),
             GetBlockPositionIn(hit.point.z, hit.normal.z, place)
             );

        if (baseWidthLength != null)
        {
            float x, y, z;
            if (baseWidthLength[0] % 2 != 1)
            {
                x = pos.x + 0.5f;
                x = Mathf.Round(x) - 0.5f;
            }
            else
                x = Mathf.Round(pos.x);

            if (baseWidthLength[1] % 2 != 1)
            {
                z = pos.z + 0.5f;
                z = Mathf.Round(z) - 0.5f;
            }
            else
                z = Mathf.Round(pos.z);

            y = pos.y;
            y = Mathf.Round(y);

            pos = new Vector3(x, y, z);

            return pos;
        }
        return Vector3Int.RoundToInt(pos);
    }

    private float GetBlockPositionIn(float pos, float normal, bool place)
    {
        float halfway = Mathf.Abs(pos % 1);
        if (0.49f < halfway && halfway < 0.51f)
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

    private static string worldAssetPath = "/Resources/Worlds/";
    private static string fileType = "txt";

    private static string defaultAssetFolder = "temp";
    [SerializeField]
    [HideInInspector]
    private string customAssetPath = "/Resources/Worlds/new_world"; //this needs to be changed

    //SAVE and LOAD methods
    #if UNITY_EDITOR
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
    #endif
    public void LoadWorld(bool loadAs = false, bool loadTemp = false)
    {
        #if UNITY_EDITOR
        if (loadAs)
        {
            customAssetPath = Path.GetDirectoryName(EditorUtility.OpenFilePanel("Get World Data File", Application.dataPath + worldAssetPath, fileType));
            customAssetPath = customAssetPath.Replace("\\", "/");
            if (customAssetPath.StartsWith(Application.dataPath))
                customAssetPath = customAssetPath.Substring(Application.dataPath.Length);
        }
        #endif
        string assetName = Path.GetFileNameWithoutExtension(loadTemp ? worldAssetPath + defaultAssetFolder : customAssetPath);
        string assetPath = loadTemp ? worldAssetPath + defaultAssetFolder : customAssetPath;

        //assetName = "new_world";
        //assetPath = "/Terrain/Worlds/new_world";
        assetPath = assetPath + "/";

        Debug.Log("Load from: " + Application.dataPath + assetPath + assetName + "." + fileType);

        GameObject gameObject = Resources.Load(assetPath.Replace("/Resources/", "") + assetName, typeof(GameObject)) as GameObject;

        if (gameObject != null)
        {
            BinaryFormatter bf = new BinaryFormatter();
            // 1. Construct a SurrogateSelector object
            SurrogateSelector ss = new SurrogateSelector();

            WorldDataSerializationSurrogate wdss = new WorldDataSerializationSurrogate();
            ss.AddSurrogate(typeof(WorldData), new StreamingContext(StreamingContextStates.All), wdss);

            // 2. Have the formatter use our surrogate selector
            bf.SurrogateSelector = ss;

            TextAsset textAsset = Resources.Load(assetPath.Replace("/Resources/", "") + assetName, typeof(TextAsset)) as TextAsset;
            Stream stream = new MemoryStream(textAsset.bytes);
            //FileStream file = File.Open(Application.dataPath + assetPath + assetName + "." + fileType, FileMode.Open);

            WorldData worldDataTemp = (WorldData)bf.Deserialize(stream);
            //file.Close();

            worldData = new WorldData
            {
                chunkHeight = worldDataTemp.chunkHeight,
                chunkSize = worldDataTemp.chunkSize,
                mapSizeInChunks = worldDataTemp.mapSizeInChunks,
                border = worldDataTemp.border,
                mapSeedOffset = worldDataTemp.mapSeedOffset,
                chunkDataDictionary = new Dictionary<Vector3Int, ChunkData>(worldDataTemp.chunkDataDictionary),
                chunkDictionary = new Dictionary<Vector3Int, ChunkRenderer>(worldDataTemp.chunkDictionary)
            };

            //GameObject gameObject = PrefabUtility.LoadPrefabContents(Application.dataPath + assetPath + assetName + ".prefab");
            //GameObject gameObject = Instantiate(worldPrefab, transform);
            worldRenderer.chunkPool.Clear();
            worldRenderer.DeleteRenderers();
            worldRenderer.LoadRenderersFromPrefab(gameObject, this, ref worldData.chunkDictionary, ref worldData.chunkDataDictionary);

            chunkSize = worldData.chunkSize;
            chunkHeight = worldData.chunkHeight;
            mapSizeInChunks = worldData.mapSizeInChunks;
            mapSeedOffset = worldData.mapSeedOffset;


            /*if (Application.isPlaying)
            {
                Destroy(gameObject);
            }
            else
            {
                DestroyImmediate(gameObject);
            }*/

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
    public int border;
    public Vector2Int mapSeedOffset;
}
