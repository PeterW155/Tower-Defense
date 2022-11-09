using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class TowerEditor : MonoBehaviour
{
    public World world;
    [SerializeField]
    private LayerMask groundMask;
    [SerializeField]
    private LayerMask towerMask;
    [Space]
    public Transform towerParent;

    [Space]
    public Material placeMaterial;
    public Material removeMaterial;

    [Space]
    [Header("Controls")]
    private PlayerInput _playerInput;
    [StringInList(typeof(PropertyDrawersHelper), "AllActionMaps")] public string editingActionMap;
    [StringInList(typeof(PropertyDrawersHelper), "AllPlayerInputs")] public string clickControl;
    private InputAction _click;
    [StringInList(typeof(PropertyDrawersHelper), "AllPlayerInputs")] public string removeControl;
    private InputAction _remove;
    
    [Space]
    [Header("Debug")]
    [ReadOnly] public GameObject selectedTower;
    private TowerData td;

    private List<TowerData> tdList;
    private Renderer[] renderers;
    private bool proxiesActive;
    private bool materialActive;

    [HideInInspector]
    public bool editing;

    private CanvasHitDetector chd;

    private Coroutine editCoroutine;

    private void Awake()
    {
        _playerInput = FindObjectOfType<PlayerInput>();

        _click = _playerInput.actions[clickControl];
        _remove = _playerInput.actions[removeControl];

        tdList = new List<TowerData>();

        editing = false;

        chd = FindObjectOfType<CanvasHitDetector>();
    }

    public void EnableTowerEditing()
    {
        editing = true;
        editCoroutine = StartCoroutine(Editing());
        _playerInput.actions.FindActionMap(editingActionMap, true).Enable();
    }
    public void DisableTowerEditing()
    {
        editing = false;
        if (editCoroutine != null)
            StopCoroutine(editCoroutine);
        _playerInput.actions.FindActionMap(editingActionMap, true).Disable();

        if (proxiesActive)
        {
            proxiesActive = false;
            foreach (TowerData m_td in tdList)
            {
                m_td.main.SetActive(true);
                m_td.proxy.SetActive(false);
            }
        }
    }

    public void NewSelectedTower(GameObject prefab)
    {
        if (selectedTower != null)
            Destroy(selectedTower);
        selectedTower = Instantiate(prefab);
        td = selectedTower.GetComponent<TowerData>();
        td.main.SetActive(false);
        td.proxy.SetActive(true);
        selectedTower.SetActive(false);
        materialActive = false;
        renderers = td.proxy.GetComponentsInChildren<Renderer>(true);
    }

    private IEnumerator Editing()
    {
        for (; ; )
        {
            if (selectedTower != null)
            {
                Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
                RaycastHit hit;

                if (_remove.IsPressed()) //removeing towers section
                {
                    selectedTower.SetActive(false);

                    if (!proxiesActive) // acitvate proxies
                    {
                        proxiesActive = true;
                        foreach (TowerData m_td in tdList)
                        {
                            m_td.main.SetActive(false);
                            m_td.proxy.SetActive(true);
                        }
                    }

                    if (!chd.IsPointerOverUI() && Physics.Raycast(ray, out hit, Mathf.Infinity, towerMask))
                    {
                        //tower removal
                        if (_click.WasPerformedThisFrame())
                        {
                            TowerData n_td = hit.transform.GetComponentInParent<TowerData>(true);

                            //reimburse player
                            PlayerStats.Instance.money += n_td.cost;

                            tdList.Remove(n_td);
                            //fill with air/remove barriers
                            Vector3 center = n_td.transform.position + new Vector3(0, n_td.size.y / 2f, 0);
                            Vector3Int corner1 = Vector3Int.RoundToInt(center - ((Vector3)n_td.size / 2f - Vector3.one / 2f));
                            Vector3Int corner2 = Vector3Int.RoundToInt(center + ((Vector3)n_td.size / 2f - Vector3.one / 2f));
                            world.SetBlockVolume(corner1, corner2, BlockType.Air);

                            Destroy(n_td.gameObject);
                        }
                    }
                }
                else //placing towers section
                {
                    if (proxiesActive) //turn proxies back to normal towers if remove button released
                    {
                        proxiesActive = false;
                        foreach (TowerData m_td in tdList)
                        {
                            m_td.main.SetActive(true);
                            m_td.proxy.SetActive(false);
                        }
                    }
                    if (!chd.IsPointerOverUI() && Physics.Raycast(ray, out hit, Mathf.Infinity, groundMask))
                    {
                        selectedTower.SetActive(true);
                        Vector3 pos = world.GetBlockPos(hit, true, new int[2]{td.size.z, td.size.z}) + new Vector3(0, -0.5f, 0);
                        selectedTower.transform.position = pos;
                        //check for valid placement
                        TowerData m_td = selectedTower.GetComponent<TowerData>();
                        //check valid ground first
                        Vector3 g_center = pos + new Vector3(0, -0.5f, 0);
                        Vector3Int g_corner1 = Vector3Int.RoundToInt(new Vector3(g_center.x - ((m_td.size.x / 2f) - 0.5f), g_center.y, g_center.z - ((m_td.size.x / 2f) - 0.5f)));
                        Vector3Int g_corner2 = Vector3Int.RoundToInt(new Vector3(g_center.x + ((m_td.size.x / 2f) - 0.5f), g_center.y, g_center.z + ((m_td.size.x / 2f) - 0.5f)));
                        //check valid empty space
                        Vector3 center = pos + new Vector3(0, m_td.size.y / 2f, 0);
                        Vector3Int corner1 = Vector3Int.RoundToInt(center - ((Vector3)m_td.size / 2f - Vector3.one / 2f));
                        Vector3Int corner2 = Vector3Int.RoundToInt(center + ((Vector3)m_td.size / 2f - Vector3.one / 2f));

                        if (world.GetBlockVolume(g_corner1, g_corner2, false) && world.GetBlockVolume(corner1, corner2, true)) //check ground then empty
                        {
                            if (!materialActive)
                            {
                                foreach (Renderer r in renderers)//show place proxy material
                                    r.material = placeMaterial;
                                materialActive = true;
                            }

                            if (_click.WasPerformedThisFrame() && m_td.cost <= PlayerStats.Instance.money) //tower placed
                            {
                                StartCoroutine(PlacingTower(selectedTower, corner1, corner2, pos));
                            }
                        }
                        else //if space is invalid show red proxy material
                        {
                            if (materialActive)
                            {
                                foreach (Renderer r in renderers)//show remove proxy material
                                    r.material = removeMaterial;
                                materialActive = false;
                            }
                        }
                    }
                    else
                        selectedTower.SetActive(false);
                }
            }
            yield return null;
        }
    }

    private IEnumerator PlacingTower(GameObject selectedTower, Vector3Int corner1, Vector3Int corner2, Vector3 pos)
    {
        //instantiate tower
        GameObject newTower = Instantiate(selectedTower, pos, Quaternion.identity, towerParent);
        TowerData n_td = newTower.GetComponent<TowerData>();
        n_td.main.SetActive(false);
        n_td.proxy.SetActive(false);
        yield return 1;

        //check if path valid
        bool pathValid = EnemyTargetPathChecker.Instance.CheckPathFromTargetToEnemy();

        //spawn tower
        if (pathValid && n_td.cost <= PlayerStats.Instance.money)
        {
            world.SetBlockVolume(corner1, corner2, BlockType.Barrier); //spawn barriers

            tdList.Add(n_td);
            foreach (Renderer r in n_td.proxy.GetComponentsInChildren<Renderer>(true))
                r.material = removeMaterial;
            Debug.Log("placed");
            n_td.main.SetActive(true);
            n_td.proxy.SetActive(false);

            //remove money from player
            PlayerStats.Instance.money -= n_td.cost;
        }
        else //otherwise remove tower
        {
            Destroy(newTower);
        }
    }
}
