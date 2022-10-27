using UnityEngine;
using UnityEngine.InputSystem;

public class UnitClick : MonoBehaviour
{
    public GameObject groundMarker;

    public LayerMask clickableLayer;
    public LayerMask ground;

    [Header("Controls")]
    private PlayerInput _playerInput;
    [StringInList(typeof(PropertyDrawersHelper), "AllActionMaps")] public string mainActionMap;
    [StringInList(typeof(PropertyDrawersHelper), "AllPlayerInputs")] public string rightClickControl;
    private InputAction _rightClick;
    [StringInList(typeof(PropertyDrawersHelper), "AllPlayerInputs")] public string leftClickControl;
    private InputAction _leftClick;
    [StringInList(typeof(PropertyDrawersHelper), "AllPlayerInputs")] public string leftShiftControl;
    private InputAction _leftShift;
    
    // Start is called before the first frame update
    private void Awake()
    {
        _playerInput = FindObjectOfType<PlayerInput>();

        _rightClick = _playerInput.actions[rightClickControl];
        _leftClick = _playerInput.actions[leftClickControl];
        _leftShift = _playerInput.actions[leftShiftControl];
    }

    public void EnableMain()
    {
        _playerInput.actions.FindActionMap(mainActionMap).Enable();
    }

    public void DisableMain()
    {
        _playerInput.actions.FindActionMap(mainActionMap, true).Disable();
    }

    private void OnEnable()
    {
        _rightClick.performed += OnRightClick;
        _leftClick.performed += OnLeftClick;
    }

    private void OnDisable()
    {
        _rightClick.performed -= OnRightClick;
        _leftClick.performed -= OnLeftClick;
    }

    private void OnLeftClick(InputAction.CallbackContext context)
    {
        groundMarker.SetActive(false);
        RaycastHit hit = new RaycastHit();
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, clickableLayer))
        {
            // If we hit a clickable object
            if (_leftShift.WasPressedThisFrame())
            {
                // Shift clicked
                UnitSelections.Instance.ShiftClickSelect(hit.collider.gameObject);
            }
            else
            {
                // Normal Clicked
                UnitSelections.Instance.ClickSelect(hit.collider.gameObject);
            }
        }
        else
        {
            // If we didn't and we're not shift clicking
            if (!_leftShift.WasPressedThisFrame())
            {
                UnitSelections.Instance.DeselectAll();
            }
        }
    }

    private void OnRightClick(InputAction.CallbackContext context)
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, ground))
        {
            groundMarker.transform.position = hit.point;
            groundMarker.SetActive(false);
            groundMarker.SetActive(true);
        }
    }
}
