using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;
using UnityEngine;
using UnityEngine.InputSystem;

public class MovePlayer : MonoBehaviour
{
    public NavMeshAgent agent;
    public LayerMask ground;
    public int health;

    private PlayerInput _playerInput;
    [StringInList(typeof(PropertyDrawersHelper), "AllActionMaps")] public string mainActionMap;
    [StringInList(typeof(PropertyDrawersHelper), "AllPlayerInputs")] public string destinationControl;
    private InputAction _destination;

    Vector3 target;
    
    // Start is called before the first frame update
    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        _playerInput = FindObjectOfType<PlayerInput>();
        _destination = _playerInput.actions[destinationControl];
    }

    private void OnEnable()
    {
        _destination.performed += OnRightClick;
    }

    private void OnDisable()
    {
        _destination.performed -= OnRightClick;
    }

    private void OnRightClick(InputAction.CallbackContext context)
    {
        Vector3 mouse = Mouse.current.position.ReadValue(); // Get the mouse Position
        Ray castPoint = Camera.main.ScreenPointToRay(mouse); // Cast a ray to get where the mouse is pointing at
        RaycastHit hit; // Stores the position where the ray hit.
        if (Physics.Raycast(castPoint, out hit, Mathf.Infinity, ground)) // If the raycast doesn't hit a wall
        {
            target = hit.point; // Move the target to the mouse position
            agent.SetDestination(target);
        }
    }

    public void TakeDamage(int damage)
    {
        health -= damage;

        if(health <= 0)
        {
            Destroy(agent.gameObject);
        }
    }
}
