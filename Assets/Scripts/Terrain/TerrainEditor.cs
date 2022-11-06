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

    private CanvasHitDetector chd;

    private Coroutine editCoroutine;

    private void Awake()
    {
        _playerInput = FindObjectOfType<PlayerInput>();

        _click = _playerInput.actions[clickControl];
        _remove = _playerInput.actions[removeControl];

        editing = false;

        chd = FindObjectOfType<CanvasHitDetector>();
    }

    public void ModifyTerrainEditor(RaycastHit hit, BlockType blockType = BlockType.Air, bool place = false)
    {
        if (world.GetBlock(hit, place) != BlockType.Bedrock)
            world.SetBlock(hit, blockType, place);
    }

    public void ModifyTerrain(RaycastHit hit, BlockType blockType = BlockType.Air, bool place = false)
    {
            if (!blockModifyBlacklist.Contains(world.GetBlock(hit, place)) 
                && (place || WorldDataHelper.GetBlock(world, Vector3Int.RoundToInt(world.GetBlockPos(hit, place)) + Vector3Int.up) != BlockType.Barrier))
                world.SetBlock(hit, blockType, place);
    }

    public void EnableTerrainEditing()
    {
        editing = true;
        editCoroutine = StartCoroutine(Editing());
    }
    public void DisableTerrainEditing()
    {
        editing = false;
        if (editCoroutine != null)
            StopCoroutine(editCoroutine);
    }

    public void EnableMap()
    {
        _playerInput.actions.FindActionMap(editingActionMap, true).Enable();
    }

    public void DisableMap()
    {
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

                if (!chd.IsPointerOverUI() && Physics.Raycast(ray, out hit, Mathf.Infinity, groundMask))
                {
                    Vector3 pos;

                    if (_remove.IsPressed())
                    {
                        placeProxy.SetActive(false);
                        removeProxy.SetActive(true);

                        pos = world.GetBlockPos(hit);
                        removeProxy.transform.position = pos;

                        if (cost <= PlayerStats.Instance.money && _click.WasPerformedThisFrame()) //removed
                        {
                            ModifyTerrain(hit);
                            PlayerStats.Instance.money -= cost; //update money
                        }
                    }
                    else
                    {
                        placeProxy.SetActive(true);
                        removeProxy.SetActive(false);

                        pos = world.GetBlockPos(hit, true);
                        placeProxy.transform.position = pos;

                        if (cost <= PlayerStats.Instance.money && _click.WasPerformedThisFrame()) //placed
                        {
                            ModifyTerrain(hit, playModeBlockType, true);
                            PlayerStats.Instance.money -= cost; //update money
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
}
