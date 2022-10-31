using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;
using UnityEngine;
using UnityEngine.InputSystem;

public class MovePlayer : MonoBehaviour
{
    public NavMeshAgent agent;
    public LayerMask ground;

    public LayerMask clickableLayer;
    private PlayerInput _playerInput;
    [StringInList(typeof(PropertyDrawersHelper), "AllActionMaps")] public string mainActionMap;
    [StringInList(typeof(PropertyDrawersHelper), "AllPlayerInputs")] public string rightClickControl;
    private InputAction _rightClick;

    bool walkPointSet;
    Vector3 target;
    
    // Start is called before the first frame update
    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        _playerInput = FindObjectOfType<PlayerInput>();
        _rightClick = _playerInput.actions[rightClickControl];
    }

    // Update is called once per frame
    /*void Update()
    {
        if (Input.GetMouseButtonDown(1)) // If the player has right clicked
        {
            Vector3 mouse = Input.mousePosition; // Get the mouse Position
            Ray castPoint = Camera.main.ScreenPointToRay(mouse); // Cast a ray to get where the mouse is pointing at
            RaycastHit hit; // Stores the position where the ray hit.
            if (Physics.Raycast(castPoint, out hit, Mathf.Infinity, ground)) // If the raycast doesn't hit a wall
            {
                target = hit.point; // Move the target to the mouse position
                agent.SetDestination(target);
            }
        }
    }*/

    private void OnEnable()
    {
        _rightClick.performed += OnRightClick;
    }

    private void OnDisable()
    {
        _rightClick.performed -= OnRightClick;
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
}
