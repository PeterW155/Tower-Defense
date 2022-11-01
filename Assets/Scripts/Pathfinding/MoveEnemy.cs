using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class MoveEnemy : MonoBehaviour
{

    public Transform[] destinations;
    public NavMeshAgent agent;

    public Transform player;
    public Transform targetObject;

    public LayerMask whatIsGround, whatIsPlayer;
    private PlayerInput _playerInput;
    [StringInList(typeof(PropertyDrawersHelper), "AllActionMaps")] public string mainActionMap;
    [StringInList(typeof(PropertyDrawersHelper), "AllPlayerInputs")] public string rightClickControl;
    private InputAction _rightClick;

    public float health;

    //Patroling
    public Vector3 walkPoint;
    Vector3 target;
    bool walkPointSet;
    public float walkPointRange;

    //Attacking
    public float timeBetweenAttacks;
    bool alreadyAttacked;
    public GameObject projectile;

    //States
    public float sightRange, attackRange;
    public bool playerInSightRange, playerInAttackRange;

    private void Awake()
    {
        player = GameObject.Find("Player").transform;
        targetObject = GameObject.Find("EnemyTarget").transform;
        agent = GetComponent<NavMeshAgent>();

        _playerInput = FindObjectOfType<PlayerInput>();
        _rightClick = _playerInput.actions[rightClickControl];
    }

    // Update is called once per frame
    void Update()
    {
        playerInSightRange = Physics.CheckSphere(transform.position, sightRange, whatIsPlayer);
        playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, whatIsPlayer);
        //if (playerInSightRange && !playerInAttackRange) ChasePlayer(); // Prioritize chase player
        //else if (playerInSightRange && playerInAttackRange) AttackPlayer(); // Then attacking player (probably need to prioritize this later on)
        //else if (!playerInSightRange && !playerInAttackRange) MoveToTarget(); // Then move to target

        if (playerInSightRange) ChasePlayer(); // Prioritize chase player
        else if (!playerInSightRange) MoveToTarget(); // Then move to target
    }

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
        if (Physics.Raycast(castPoint, out hit, Mathf.Infinity, whatIsGround)) // If the raycast doesn't hit a wall
        {
            target = hit.point; // Move the target to the mouse position
        }
    }

    private void MoveToTarget()
    {
        //Debug.Log("Moving to target");
        agent.SetDestination(targetObject.position);
    }

    private void Patroling()
    {
        if (!walkPointSet) SearchWalkPoint();

        if (walkPointSet)
            agent.SetDestination(walkPoint);

        Vector3 distanceToWalkPoint = transform.position - walkPoint;

        if (distanceToWalkPoint.magnitude < 1f)
        {
            walkPointSet = false;
        }
    }

    private void SearchWalkPoint()
    {
        float randomZ = Random.Range(-walkPointRange, walkPointRange);
        float randomX = Random.Range(-walkPointRange, walkPointRange);

        walkPoint = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);

        if (Physics.Raycast(walkPoint, -transform.up, 2f, whatIsGround))
        {
            walkPointSet = true;
        }
    }

    private void ChasePlayer()
    {
        //Debug.Log("Chasing Player");
        agent.SetDestination(GetNearestPlayer());
    }

    Vector3 GetNearestPlayer()
    {
        Vector3 nearestPlayerPosition = new Vector3(Mathf.Infinity, Mathf.Infinity, Mathf.Infinity);
        GameObject nearestPlayerObject; 
        float nearestPlayerPositionSquared = Mathf.Infinity;
        Collider[] playerUnits = Physics.OverlapSphere(transform.position, sightRange, whatIsPlayer);
        foreach (Collider playerUnit in playerUnits)
        {
            Debug.Log("Hit colliders: " + playerUnit.gameObject.name);
            GameObject playerUnitObject = playerUnit.transform.gameObject;
            Vector3 playerUnitPosition = playerUnitObject.transform.position;

            Vector3 distanceToPlayerUnit = playerUnitPosition - transform.position;
            float distanceToPlayerUnitSquared = distanceToPlayerUnit.sqrMagnitude;
            Debug.Log("Distance Squared: " + distanceToPlayerUnitSquared);
            Debug.Log("Nearest Distance Squared: " + nearestPlayerPositionSquared);
            if (distanceToPlayerUnitSquared < nearestPlayerPositionSquared)
            {
                nearestPlayerPositionSquared = distanceToPlayerUnitSquared;
                nearestPlayerPosition = playerUnitPosition;
            }
        }
        return nearestPlayerPosition;
    }

    private void AttackPlayer()
    {
        //Debug.Log("Attacking Player");
        agent.SetDestination(transform.position);

        transform.LookAt(player);

        if (!alreadyAttacked)
        {
            Rigidbody rb = Instantiate(projectile, transform.position, Quaternion.identity).GetComponent<Rigidbody>();
            rb.AddForce(transform.forward * 32f, ForceMode.Impulse);
            rb.AddForce(transform.up * 8f, ForceMode.Impulse);

            alreadyAttacked = true;
            Invoke(nameof(ResetAttack), timeBetweenAttacks);
        }
    }

    private void ResetAttack()
    {
        alreadyAttacked = false;
    }

    public void TakeDamage(int damage)
    {
        health -= damage;

        if (health <= 0)
        {
            Invoke(nameof(DestroyEnemy), 0.5f);
        }
    }

    private void DestroyEnemy()
    {
        Destroy(gameObject);
    }

    /*private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);
    }*/
}
