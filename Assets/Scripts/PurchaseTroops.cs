using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PurchaseTroops : MonoBehaviour
{
    public Transform troop1Prefab;
    public Transform troop2Prefab;

    public Transform spawnPoint;

    [Space]
    [StringInList(typeof(PropertyDrawersHelper), "AllActionMaps")] public string unitActionMap;

    [HideInInspector] public bool menuActive;

    private PlayerInput _playerInput;

    private void Start()
    {
        _playerInput = CameraHandler.Instance.playerInput;
    }

    public void SpawnTroop1()
    {
        if(PlayerStats.Instance.money >= 150)
        {
            Instantiate(troop1Prefab, spawnPoint);
            PlayerStats.Instance.money -= 150;
        }
    }

    public void SpawnTroop2()
    {
        if (PlayerStats.Instance.money >= 200)
        {
            Instantiate(troop2Prefab, spawnPoint);
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
