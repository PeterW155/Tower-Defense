using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PurchaseTroops : MonoBehaviour
{
    public Transform troop1Prefab;
    public Transform troop2Prefab;
    public Transform troopParent;

    public Transform spawnPoint;

    [Space]
    [StringInList(typeof(PropertyDrawersHelper), "AllActionMaps")] public string unitActionMap;

    [HideInInspector] public bool menuActive;

    private PlayerInput _playerInput;

    private static PurchaseTroops _instance;
    public static PurchaseTroops Instance { get { return _instance; } }

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
    }

    private void Start()
    {
        _playerInput = CameraHandler.Instance.playerInput;
    }

    public void SpawnTroop1()
    {
        if(PlayerStats.Instance.money >= 150)
        {
            Instantiate(troop1Prefab, spawnPoint, troopParent);
            PlayerStats.Instance.money -= 150;
        }
    }

    public void SpawnTroop2()
    {
        if (PlayerStats.Instance.money >= 200)
        {
            Instantiate(troop2Prefab, spawnPoint, troopParent);
            PlayerStats.Instance.money -= 200;
        }
    }

    public void MenuEnabled()
    {
        menuActive = true;
        _playerInput.actions.FindActionMap(unitActionMap, true).Enable();
    }
    public void MenuDisabled()
    {
        menuActive = false;
        _playerInput.actions.FindActionMap(unitActionMap, true).Disable();
    }
}
