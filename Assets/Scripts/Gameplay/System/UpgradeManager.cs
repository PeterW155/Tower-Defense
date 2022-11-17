using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class UpgradeManager : MonoBehaviour
{
    public GameObject upgradeMenu;
    public GameObject infoText;
    private int cost;
    private int lvl;
    private string turretType;
    private GameObject target;

    [Header("Controls")]
    private PlayerInput _playerInput;
    [StringInList(typeof(PropertyDrawersHelper), "AllPlayerInputs")] public string selectionControl;
    private InputAction _selection;

    private static UpgradeManager _instance;
    public LayerMask towerMask;

    public static UpgradeManager Instance { get { return _instance; } }

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
        _selection = _playerInput.actions[selectionControl];
    }

    private void OnEnable()
    {
        _selection.performed += OnClick;
    }

    private void OnDisable()
    {
        _selection.performed -= OnClick;
    }

    private void OnClick(InputAction.CallbackContext context)
    {
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        RaycastHit hit;

        if (!CanvasHitDetector.Instance.IsPointerOverUI() && Physics.Raycast(ray, out hit, Mathf.Infinity, towerMask))
        {
            hit.transform.parent.GetComponent<TowerData>().BeginUpgrade();
        }
    }

    public void GetInfo(int cost, GameObject target, int currentLevel, string turretType)
    {
        this.cost = cost;
        this.lvl = currentLevel;
        this.turretType = turretType;
        this.target = target;
        UpdateInfo();
    }

    private void UpdateInfo()
    {
        infoText.GetComponent<Text>().text = "Turret Type: " + turretType + "\nCurrent Level: " + lvl + "\n<b>Cost To Level-Up: $" + cost + " </b>";
    }

    public void UpgradeTarget()
    {
        if (PlayerStats.Instance.money >= cost && target != null)
        {
            target.GetComponent<TowerData>().Upgrade();
            PlayerStats.Instance.GetComponent<PlayerStats>().money -= cost;
            upgradeMenu.SetActive(false);
        }
    }

    public void Cancel()
    {
        upgradeMenu.SetActive(false);
    }

}
