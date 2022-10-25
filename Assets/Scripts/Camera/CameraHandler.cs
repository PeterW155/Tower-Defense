using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class CameraHandler : MonoBehaviour
{
    public Camera cineCamera;
    public Transform cameraParent;
    public Transform cameraRotate;
    public Transform cameraZoom;
    [Space]
    [Range(1, 5)]
    public float movementSensetivity = 2.5f;
    [Range(1, 25)]
    public float movementDrag = 16f;
    [Range(1, 5)]
    public float rotationSensetivity = 2.5f;
    [Range(1, 25)]
    public float rotationDrag = 10f;
    [Range(1, 5)]
    public float zoomSensetivity = 2.5f;
    public Vector2 zoomMinMax = new Vector2(2, 20);

    [Space]
    [Header("Controls")]
    public PlayerInput _playerInput;
    [StringInList(typeof(PropertyDrawersHelper), "AllActionMaps")] public string cameraActionMap;
    [StringInList(typeof(PropertyDrawersHelper), "AllPlayerInputs")] public string moveControl;
    private InputAction _move;
    [StringInList(typeof(PropertyDrawersHelper), "AllPlayerInputs")] public string lookControl;
    private InputAction _look;
    [StringInList(typeof(PropertyDrawersHelper), "AllPlayerInputs")] public string rotateControl;
    private InputAction _rotate;
    [StringInList(typeof(PropertyDrawersHelper), "AllPlayerInputs")] public string zoomControl;
    private InputAction _zoom;

    private Rigidbody _rigidbodyParent;
    private Rigidbody _rigidbodyRotate;
    private Vector2 warpPosition;
    private float zoomPosZ;
    private float zoomTarget;
    private float zoomTime;
    [Space]
    [Header("Debug")]
    [ReadOnly] [SerializeField] private Vector2 move;
    [ReadOnly] [SerializeField] private Vector2 look;
    [ReadOnly] [SerializeField] private float zoom;

    void Awake()
    {
        if (!cameraParent.TryGetComponent<Rigidbody>(out _rigidbodyParent))
            throw new Exception("Camera parent missing rigidbody.");
        if (!cameraRotate.TryGetComponent<Rigidbody>(out _rigidbodyRotate))
            throw new Exception("Camera rotate missing rigidbody.");

        zoomPosZ = cameraZoom.localPosition.z;
        zoomTarget = zoomPosZ;

        _playerInput = GetComponent<PlayerInput>();

        _playerInput.actions.FindActionMap(cameraActionMap).Enable();

        //initialize inputs
        _move = _playerInput.actions[moveControl];
        _look = _playerInput.actions[lookControl];
        _rotate = _playerInput.actions[rotateControl];
        _zoom = _playerInput.actions[zoomControl];
    }

    private void Update()
    {
        if (_rotate.WasPressedThisFrame())
            warpPosition = Mouse.current.position.ReadValue();
        move = _move.ReadValue<Vector2>();
        look = _look.ReadValue<Vector2>();
        zoom = _zoom.ReadValue<float>();

        //camera zoom
        if (zoom != 0)
        {
            zoomTime = 0;
            zoomPosZ = cameraZoom.localPosition.z;
            float zoomAmount = Time.deltaTime * 40f * zoomSensetivity;
            zoomTarget = Mathf.Clamp(zoomTarget + (zoom > 0 ? zoomAmount : -zoomAmount), -zoomMinMax.y, -zoomMinMax.x);
        }
        if (zoomTime < 0.8f)
        {
            zoomTime += Time.deltaTime;
        }
        cameraZoom.localPosition = new Vector3(0, 0, Mathf.Lerp(zoomPosZ, zoomTarget, Mathf.SmoothStep(0f, 1f, Mathf.Pow(zoomTime/0.8f, 0.4f))));
    }

    private void FixedUpdate()
    {
        float defaultDrag = 0.95f;

        //camera movement
        Vector2 p_move = move.normalized;
        Vector3 rotateOffset = cameraRotate.TransformDirection(new Vector3(p_move.x, 0, p_move.y));
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
        if (_rotate.IsPressed())
        {
            Mouse.current.WarpCursorPosition(warpPosition);

            if (look.magnitude > 0)
            {
                _rigidbodyRotate.AddRelativeTorque(new Vector3(0, 0.035f * look.x * rotationSensetivity, 0), ForceMode.Impulse);
                _rigidbodyRotate.angularVelocity *= defaultDrag;
            }
            else
            {
                _rigidbodyRotate.angularVelocity *= (1f - (rotationDrag / 100));
            }
        }
        //rotation drag
        else
        {
            _rigidbodyRotate.angularVelocity *= (1f - (rotationDrag / 100));
        }
    }
}
