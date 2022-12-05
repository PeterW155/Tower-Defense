using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.AI;

public class PlayerUnitArmyNew : MonoBehaviour
{
    public LayerMask ground;
    private PlayerInput playerInput;
    [StringInList(typeof(PropertyDrawersHelper), "AllPlayerInputs")] public string destinationControl;
    private InputAction destination;
    private Transform parent;
    private static Transform unitParent;
    static Transform troopParent;
    Vector3 target;

    private void Awake() {
        parent = new GameObject("Unit Parent").transform;
        unitParent = new GameObject("Selected Parent").transform;
        troopParent = GameObject.Find("Troops").transform;
        unitParent.transform.parent = troopParent;

        playerInput = FindObjectOfType<PlayerInput>();
        destination = playerInput.actions[destinationControl];
    }

    public static void AddUnitToSelectedParent(GameObject unitToAdd)
    {
        unitToAdd.transform.parent = troopParent;
        unitToAdd.transform.parent = unitParent;
    }

    public static void RemoveUnitFromSelectedParent(GameObject unitToRemove)
    {
        unitToRemove.transform.parent = troopParent;
    }

    private void OnEnable()
    {
        destination.performed += OnRightClick;
    }

    private void OnDisable()
    {
        destination.performed -= OnRightClick;
    }

    private void OnRightClick(InputAction.CallbackContext context)
    {
        Vector3 mouse = Mouse.current.position.ReadValue(); // Get the mouse Position
        Ray castPoint = Camera.main.ScreenPointToRay(mouse); // Cast a ray to get where the mouse is pointing at
        RaycastHit hit; // Stores the position where the ray hit.
        if (Physics.Raycast(castPoint, out hit, Mathf.Infinity, ground)) // If the raycast doesn't hit a wall
        {
            target = hit.point; // Move the target to the mouse position
            GameObject closestUnit = FindClosestUnitToTarget(target);
            float distanceX = target.x - closestUnit.transform.position.x; // Find distance for x
            float distanceZ = target.z - closestUnit.transform.position.z; // Find distance for z
            Debug.Log("Closest Unit: " + closestUnit.transform.position + ", DistanceX: " + distanceX + ", DistanceZ: " + distanceZ);
            // Debug.Log("Length of selected unit: " + unitParent.transform.childCount);
            SetDestinationForEachUnit(distanceX, distanceZ);
        }
    }

    private void SetDestinationForEachUnit(float distanceX, float distanceZ)
    {
        foreach (Transform unit in unitParent)
        {
            float unitDestinationX = unit.position.x + distanceX; // Set destination x
            float unitDestinationZ = unit.position.z + distanceZ; // Set destination z
            Vector3 unitDestination = new Vector3(unitDestinationX, unit.position.y, unitDestinationZ); // Set Vector3 destination
            unit.GetComponent<MovePlayerNew>().MoveToDestination(unitDestination); // Apply destination to each unit
            Debug.Log("Unit Position: " + unit.position + ", Destination: " + target + ", DistanceX: " + distanceX + ", DistanceZ: " + distanceZ);
        }
    }

    private GameObject FindClosestUnitToTarget(Vector3 target)
    {
        float shortestDistanceX = Mathf.Infinity;
        float shortestDistanceZ = Mathf.Infinity;
        GameObject unitClosestToTarget = new GameObject();
        foreach (Transform unit in unitParent)
        {
            float unitDistanceX = target.x - unit.position.x;
            float unitDistanceZ = target.z - unit.position.z;
            Debug.Log(unitDistanceX + " " + unitDistanceZ);
            if (unitDistanceX < shortestDistanceX && unitDistanceZ < shortestDistanceZ)
            {
                unitClosestToTarget = unit.gameObject;
            }
        }
        return unitClosestToTarget; 
    }

}
