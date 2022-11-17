using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.AI;

public class PlayerUnitArmy : MonoBehaviour
{
    //public static PlayerUnitArmy Instance { get; internal set; }

    private FormationBase _formation;

    public FormationBase Formation {
        get {
            if (_formation == null) _formation = GetComponent<FormationBase>();
            return _formation;
        }
        set => _formation = value;
    }
    private readonly List<GameObject> _spawnedUnits = new List<GameObject>();
    private List<Vector3> _points = new List<Vector3>();
    [SerializeField] private float _unitSpeed = 2;
    Vector3 target;
    NavMeshAgent agent;
    public LayerMask ground;
    private Transform _parent;
    private GameObject dummyGameObject;
    private static Transform _unitParent;
    static Transform troopParent;
    //private static List<GameObject> selectedUnit = new List<GameObject>();

    private PlayerInput _playerInput;
    [StringInList(typeof(PropertyDrawersHelper), "AllPlayerInputs")] public string destinationControl;
    private InputAction _destination;

    private void Awake() {
        _parent = new GameObject("Unit Parent").transform;
        _unitParent = new GameObject("Selected Parent").transform;
        troopParent = GameObject.Find("Troops").transform;
        _unitParent.transform.parent = troopParent;

        _playerInput = FindObjectOfType<PlayerInput>();
        _destination = _playerInput.actions[destinationControl];
    }

    private void Update() {

    }

    public static void AddUnitToSelectedParent(GameObject unitToAdd)
    {
        unitToAdd.transform.parent = troopParent;
        unitToAdd.transform.parent = _unitParent;
        //selectedUnit.Add(unitToAdd);
        // foreach (Transform child in _unitParent)
        // {
        //     if (child.gameObject.tag == "Player")
        //     {
        //         selectedUnit.Add(child.gameObject);
        //     }
        // }
    }

    public static void RemoveUnitFromSelectedParent(GameObject unitToRemove)
    {
        unitToRemove.transform.parent = troopParent;
        //selectedUnit.Remove(unitToRemove);
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
            GameObject closestUnit = FindClosestUnitToTarget(target);
            float distanceX = target.x - closestUnit.transform.position.x;
            float distanceZ = target.z - closestUnit.transform.position.z;
            Debug.Log("Length of selected unit: " + _unitParent.transform.childCount);
            SetDestinationForEachUnit(distanceX, distanceZ);
        }
    }

    private void SetDestinationForEachUnit(float distanceX, float distanceZ)
    {
        foreach (Transform unit in _unitParent)
        {
            float unitDestinationX = unit.position.x + distanceX;
            float unitDestinationZ = unit.position.z + distanceZ;
            Vector3 unitDestination = new Vector3(unitDestinationX, unit.position.y, unitDestinationZ);
            //Debug.Log("unitDestination: " + unitDestination);
            //unit.GetComponent<MovePlayer>().MoveToDestination(unitDestination);
            //unit.GetComponent<MovePlayer>().MoveToDestination(target);
        }
    }

    private GameObject FindClosestUnitToTarget(Vector3 target)
    {
        float shortestDistanceX = 0.0f;
        float shortestDistanceZ = 0.0f;
        GameObject unitClosestToTarget = new GameObject();
        foreach (Transform unit in _unitParent)
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

    private void SetFormation() {
        _points = Formation.EvaluatePoints().ToList();

        if (_points.Count > _spawnedUnits.Count) {
            var remainingPoints = _points.Skip(_spawnedUnits.Count);
            Spawn(remainingPoints);
        }
        else if (_points.Count < _spawnedUnits.Count) {
            Kill(_spawnedUnits.Count - _points.Count);
        }

        for (var i = 0; i < _spawnedUnits.Count; i++) {
            _spawnedUnits[i].transform.position = Vector3.MoveTowards(_spawnedUnits[i].transform.position, transform.position + _points[i], _unitSpeed * Time.deltaTime);
        }
    }

    private void Spawn(IEnumerable<Vector3> points) {
        foreach (var pos in points) {
            //var unit = Instantiate(_unitPrefab, transform.position + pos, Quaternion.identity, _parent);
            //_spawnedUnits.Add(unit);
        }
    }

    private void Kill(int num) {
        for (var i = 0; i < num; i++) {
            var unit = _spawnedUnits.Last();
            _spawnedUnits.Remove(unit);
            Destroy(unit.gameObject);
        }
    }
}
