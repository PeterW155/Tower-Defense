using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(World))]
public class TerrainEditor : MonoBehaviour
{
    [SerializeField]
    private LayerMask groundMask;
    [SerializeField]
    private List<BlockType> blockModifyBlacklist = new BlockType[] { BlockType.Bedrock, BlockType.Barrier }.ToList();
    public BlockType blockType = BlockType.Dirt;
    [HideInInspector]
    public BlockType playModeBlockType;

    [Space]
    public GameObject placeProxy;
    public GameObject removeProxy;

    [Space]
    public int cost;

    private PlayerInput _playerInput;
    [Space]
    [Header("Controls")]
    [StringInList(typeof(PropertyDrawersHelper), "AllActionMaps")] public string editingActionMap;
    [Space]
    [StringInList(typeof(PropertyDrawersHelper), "AllPlayerInputs")] public string clickControl;
    private InputAction _click;
    [StringInList(typeof(PropertyDrawersHelper), "AllPlayerInputs")] public string removeControl;
    private InputAction _remove;

    [HideInInspector]
    public World world;
    [HideInInspector]
    public bool editing;

    private Coroutine editCoroutine;

    private static TerrainEditor _instance;
    public static TerrainEditor Instance { get { return _instance; } }

    private void Awake()
    {
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

        _playerInput = FindObjectOfType<PlayerInput>();

        _click = _playerInput.actions[clickControl];
        _remove = _playerInput.actions[removeControl];

        editing = false;
    }

    public void ModifyTerrain(RaycastHit hit, BlockType blockType = BlockType.Air, bool place = false)
    {
        if (world.GetBlock(hit, place) != BlockType.Bedrock)
            world.SetBlock(hit, blockType, place);
    }

    public void ModifyModifiabilityEditor(RaycastHit hit, bool unlock)
    {
        world.SetModifiability(hit, unlock);
    }

    public void EnableTerrainEditing()
    {
        editing = true;
        editCoroutine = StartCoroutine(Editing());
        if (CameraHandler.Instance.cameraAltActive)
        {
            CameraHandler.Instance.disabledActionMaps = new InputActionMap[] { _playerInput.actions.FindActionMap(editingActionMap, true) };
        }
        else
            _playerInput.actions.FindActionMap(editingActionMap, true).Enable();
    }
    public void DisableTerrainEditing()
    {
        editing = false;
        if (editCoroutine != null)
            StopCoroutine(editCoroutine);
        if (CameraHandler.Instance.cameraAltActive)
        {
            CameraHandler.Instance.disabledActionMaps = EditButtons.Instance.popupHandler.disabledActionMaps;
        }
        else
            _playerInput.actions.FindActionMap(editingActionMap, true).Disable();
    }

    private IEnumerator Editing()
    {
        for (; ; )
        {
            if (playModeBlockType != BlockType.Nothing)
            {
                Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
                RaycastHit hit;

                if (!CanvasHitDetector.Instance.IsPointerOverUI() && Physics.Raycast(ray, out hit, Mathf.Infinity, groundMask))
                {
                    Vector3 pos;

                    if (_remove.IsPressed())
                    {
                        placeProxy.SetActive(false);
                        removeProxy.SetActive(true);

                        pos = world.GetBlockPos(hit);
                        removeProxy.transform.position = pos;

                        if (_click.WasPerformedThisFrame() && cost <= PlayerStats.Instance.money) //removed
                        {
                            StartCoroutine(PlacingTerrain(hit, BlockType.Air));
                            
                        }
                    }
                    else
                    {
                        placeProxy.SetActive(true);
                        removeProxy.SetActive(false);

                        pos = world.GetBlockPos(hit, true);
                        placeProxy.transform.position = pos;

                        if (_click.WasPerformedThisFrame() && cost <= PlayerStats.Instance.money) //placed
                        {
                            StartCoroutine(PlacingTerrain(hit, playModeBlockType, true));
                        }
                    }
                }
                else
                {
                    placeProxy.SetActive(false);
                    removeProxy.SetActive(false);
                }

            }
            yield return null;
        }
    }

    private IEnumerator PlacingTerrain(RaycastHit hit, BlockType blockType, bool place = false)
    {
        //fill with dummy
        BlockType origional = world.GetBlock(hit, place);
        Vector3Int blockPos = Vector3Int.RoundToInt(world.GetBlockPos(hit, place));

        if (world.IsBlockModifiable(blockPos) && !blockModifyBlacklist.Contains(world.GetBlock(hit, place)) //check if column is modifiable, if its not in the blacklist,
            && (place || WorldDataHelper.GetBlock(world, blockPos + Vector3Int.up) != BlockType.Barrier))   //and not destroying blocks below towers (barriers)
        {
            ModifyTerrain(hit, BlockType.Barrier, place);
            yield return 1;

            //check if path valid
            bool pathValid = EnemyTargetPathChecker.Instance.CheckPathFromTargetToEnemy();

            //spawn tower
            if (pathValid && cost <= PlayerStats.Instance.money)
            {
                ModifyTerrain(hit, blockType, place);

                //remove money from player
                PlayerStats.Instance.money -= cost; //update money
            }
            else //otherwise remove barriers
            {
                ModifyTerrain(hit, origional, place);
            }
        }
        yield return null;
    }
}
