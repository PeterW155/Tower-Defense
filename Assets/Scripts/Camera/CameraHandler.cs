using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class CameraHandler : MonoBehaviour
{
    public Camera cineCamera;
    public Transform cameraParent;
    public Transform cameraYRotate;
    public Transform cameraXRotate;
    public Transform cameraZoom;
    [Space]
    public bool lockMouseWhileRotating = true;
    public bool lockLookX = true;
    [Tooltip("Only necessary if lock look X is false.")]
    public Vector2 minMaxLookX;
    [Range(1, 5)]
    public float movementSensetivity = 2.5f;
    [Range(1, 50)]
    public float movementDrag = 16f;
    [Range(1, 5)]
    public float rotationSensetivity = 2.5f;
    [Range(1, 50)]
    public float rotationDrag = 10f;
    [Range(1, 5)]
    public float zoomSensetivity = 2.5f;
    [Range(1, 50)]
    public float zoomDrag = 10f;
    public Vector2 zoomMinMax = new Vector2(2, 20);

    [HideInInspector]
    public PlayerInput playerInput;
    [Space]
    [Header("Controls")]
    [StringInList(typeof(PropertyDrawersHelper), "AllActionMaps")] public string cameraActionMap;
    [StringInList(typeof(PropertyDrawersHelper), "AllActionMaps")] public string mainActionMap;
    [Space]
    [StringInList(typeof(PropertyDrawersHelper), "AllPlayerInputs")] public string moveControl;
    private InputAction _move;
    [StringInList(typeof(PropertyDrawersHelper), "AllPlayerInputs")] public string lookControl;
    private InputAction _look;
    [StringInList(typeof(PropertyDrawersHelper), "AllPlayerInputs")] public string lookAltControl;
    private InputAction _lookAlt;
    [StringInList(typeof(PropertyDrawersHelper), "AllPlayerInputs")] public string rotateControl;
    private InputAction _rotate;
    [StringInList(typeof(PropertyDrawersHelper), "AllPlayerInputs")] public string rotateAltControl;
    private InputAction _rotateAlt;
    [StringInList(typeof(PropertyDrawersHelper), "AllPlayerInputs")] public string zoomControl;
    private InputAction _zoom;
    [StringInList(typeof(PropertyDrawersHelper), "AllPlayerInputs")] public string zoomAltControl;
    private InputAction _zoomAlt;
    [StringInList(typeof(PropertyDrawersHelper), "AllPlayerInputs")] public string activateControl;
    private InputAction _activate;
    [StringInList(typeof(PropertyDrawersHelper), "AllPlayerInputs")] public string deactivateControl;
    private InputAction _deactivate;

    private Rigidbody _rigidbodyParent;
    private Rigidbody _rigidbodyYRotate;
    private Vector2 warpPosition;
    private float zoomPosZ;
    private float zoomTarget;
    private float zoomTime;
    [Space]
    [Header("Debug")]
    #pragma warning disable 0414
    [ReadOnly] [SerializeField] private bool cameraAltActive;
    #pragma warning restore 0414
    [ReadOnly] [SerializeField] private Vector2 move;
    [ReadOnly] [SerializeField] private Vector2 look;
    [ReadOnly] [SerializeField] private float lookAlt;
    [ReadOnly] [SerializeField] private float zoom;

    private InputActionMap[] disabledActionMaps;

    float defaultDrag = 0.95f;

    //might be better to separate the player input component from the camera handler object in the future
    private static CameraHandler _instance;
    public static CameraHandler Instance { get { return _instance; } }

    void Awake()
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

        //set variables
        if (!cameraParent.TryGetComponent<Rigidbody>(out _rigidbodyParent))
            throw new Exception("Camera parent missing parent rigidbody.");
        if (!cameraYRotate.TryGetComponent<Rigidbody>(out _rigidbodyYRotate))
            throw new Exception("Camera rotate missing y rotate rigidbody.");

        cameraAltActive = false;
        zoomPosZ = cameraZoom.localPosition.z;
        zoomTarget = zoomPosZ;

        playerInput = GetComponent<PlayerInput>();

        //initialize inputs
        _move = playerInput.actions[moveControl];
        _look = playerInput.actions[lookControl];
        _lookAlt = playerInput.actions[lookAltControl];
        _rotate = playerInput.actions[rotateControl];
        _rotateAlt = playerInput.actions[rotateAltControl];
        _zoom = playerInput.actions[zoomControl];
        _zoomAlt = playerInput.actions[zoomAltControl];
        _activate = playerInput.actions[activateControl];
        _deactivate = playerInput.actions[deactivateControl];
        ControlChange(null);

        //other stuff
        _rigidbodyYRotate.maxAngularVelocity = 100;
    }

    private void OnEnable()
    {
        _activate.started += EnableCameraControls;

        _deactivate.performed += DisableCameraControls;

        playerInput.onControlsChanged += ControlChange;
    }

    private void OnDisable()
    {
        _activate.started -= EnableCameraControls;

        _deactivate.performed -= DisableCameraControls;

        playerInput.onControlsChanged -= ControlChange;
    }

    private void ControlChange(PlayerInput input)
    {
        //Debug.Log("Controls Changed");

        foreach (var b in _activate.bindings)
        {
            int id = _activate.bindings.IndexOf(x => x == b);

            if (_deactivate.bindings.ElementAtOrDefault(id) != null)
            {
                //Debug.Log("added");
                _deactivate.AddBinding(b);
            }
            else
            {
                //Debug.Log("changed");
                _deactivate.ChangeBinding(id).To(b);
            }
        }
    }

    private void EnableCameraControls(InputAction.CallbackContext context)
    {
        disabledActionMaps = playerInput.actions.actionMaps.Where(x => (x.enabled && x.name != mainActionMap)).ToArray(); //get all currently active maps
        //enable action map
        playerInput.actions.FindActionMap(cameraActionMap).Enable();
        //disable other action maps
        foreach(InputActionMap actionMap in disabledActionMaps)
            actionMap.Disable();
        
        cameraAltActive = true;
    }

    private void DisableCameraControls(InputAction.CallbackContext context)
    {
        //disable action map
        playerInput.actions.FindActionMap(cameraActionMap).Disable();
        //enable previously disabled action maps
        foreach (InputActionMap actionMap in disabledActionMaps)
            actionMap.Enable();

        cameraAltActive = false;
    }

    private void Update()
    {
        if (_rotate.WasPressedThisFrame() || _rotateAlt.WasPressedThisFrame() || _zoomAlt.WasPressedThisFrame())
            warpPosition = Mouse.current.position.ReadValue();
        move = _move.ReadValue<Vector2>();
        look = _look.ReadValue<Vector2>();
        lookAlt = _lookAlt.ReadValue<float>();
        zoom = _zoom.ReadValue<Vector2>().normalized.y;
        
        //look X (optional)
        if (!lockLookX && (_rotateAlt.IsPressed() || _rotate.IsPressed()))
        {
            cameraXRotate.localEulerAngles += new Vector3(10f * -look.y * rotationSensetivity * Time.unscaledDeltaTime, 0, 0);

            Debug.Log(cameraXRotate.localEulerAngles.x);
            //check if camera is near x limits
            if (cameraXRotate.localEulerAngles.x > minMaxLookX.y && cameraXRotate.localEulerAngles.x < minMaxLookX.x)
            {
                Debug.Log("too high");
                float target = Mathf.Abs(minMaxLookX.y - cameraXRotate.localEulerAngles.x) > Mathf.Abs(minMaxLookX.x - cameraXRotate.localEulerAngles.x) ? minMaxLookX.x : minMaxLookX.y;
                cameraXRotate.localEulerAngles = new Vector3(target, 0, 0);
            }
        }

        //camera zoom alt
        if (_zoomAlt.IsPressed() && !_rotateAlt.IsPressed()) //if zoom is pressed and rotate isn't so that both aren't active at once
        {
            if (lockMouseWhileRotating)
                Mouse.current.WarpCursorPosition(warpPosition);

            zoomTime = 0;
            zoomPosZ = cameraZoom.localPosition.z;
            Vector2 lookMinMax = new Vector2(look.x > look.y ? look.y : look.x, look.x > look.y ? look.x : look.y);
            zoomTarget = Mathf.Clamp(zoomTarget + Mathf.Clamp(look.x + look.y, lookMinMax.x, lookMinMax.y) * zoomSensetivity * 2f * Time.unscaledDeltaTime, -zoomMinMax.y, -zoomMinMax.x);
        }
        //camera zoom
        else if (!_zoomAlt.IsPressed() && zoom != 0)
        {
            zoomTime = 0;
            zoomPosZ = cameraZoom.localPosition.z;
            //float zoomAmount = Time.deltaTime * 40f * zoomSensetivity; //this is for scroll wheel
            //zoomTarget = Mathf.Clamp(zoomTarget + (look.y > 0 ? zoomAmount : -zoomAmount), -zoomMinMax.y, -zoomMinMax.x);
            float valueAdjustment = playerInput.currentControlScheme == "Controller" ? 10f : 120f;
            zoomTarget = Mathf.Clamp(zoomTarget + zoom * zoomSensetivity * valueAdjustment * Time.unscaledDeltaTime, -zoomMinMax.y, -zoomMinMax.x);
        }
        if (zoomTime < (5 / zoomDrag))
        {
            zoomTime += Time.deltaTime;
        }
        else
        {
            zoomTime = (5 / zoomDrag);
        }

        cameraZoom.localPosition = new Vector3(0, 0, Mathf.Lerp(zoomPosZ, zoomTarget, Mathf.SmoothStep(0f, 1f, Mathf.Pow(zoomTime / (5 / zoomDrag), 0.4f))));
    }

    private void FixedUpdate()
    {
        //camera movement
        Vector2 p_move = move.normalized;
        Vector3 rotateOffset = cameraYRotate.TransformDirection(new Vector3(p_move.x, 0, p_move.y));
        p_move = new Vector2(rotateOffset.x, rotateOffset.z).normalized;

        if (p_move.magnitude > 0)
        {
            _rigidbodyParent.AddForce(movementSensetivity * 6f * new Vector3(p_move.x, 0, p_move.y), ForceMode.Impulse);
            _rigidbodyParent.velocity *= defaultDrag;
        }
        else
        {
            _rigidbodyParent.velocity *= (1f - (movementDrag / 100));
        }

        //camera rotation
        if (_rotateAlt.IsPressed() || _rotate.IsPressed())
        {
            if (lockMouseWhileRotating)
                Mouse.current.WarpCursorPosition(warpPosition);

            if (look.magnitude > 0)
            {
                _rigidbodyYRotate.angularVelocity = new Vector3(0, 0.21f * look.x * rotationSensetivity, 0); //this is main horizontal rotate
            }
            else
            {
                _rigidbodyYRotate.angularVelocity *= (1f - (rotationDrag / 100));
            }
        }
        else if (lookAlt != 0)
        {
            _rigidbodyYRotate.AddRelativeTorque(new Vector3(0, 0.06f * lookAlt * rotationSensetivity, 0), ForceMode.Impulse);
            _rigidbodyYRotate.angularVelocity *= defaultDrag;
        }
        //rotation drag
        else
        {
            _rigidbodyYRotate.angularVelocity *= (1f - (rotationDrag / 100));
        }
    }
}
