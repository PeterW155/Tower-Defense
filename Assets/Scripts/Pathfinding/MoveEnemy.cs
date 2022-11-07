using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using System.Linq;

public class MoveEnemy : MonoBehaviour
{

    public Transform[] destinations;
    public NavMeshAgent agent;

    private LineRenderer myLineRenderer;

    //public Transform player;
    public Transform targetObject;
    [SerializeField] private GameObject targetMarkerPrefab;
    [SerializeField] private Transform visualObjectsParent;

    public LayerMask whatIsGround, whatIsPlayer;

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
    public Transform firePoint;

    //States
    public float sightRange, attackRange;
    public bool playerInSightRange, playerInAttackRange;

    private void Awake()
    {
        targetObject = GameObject.Find("END").transform;
        agent = GetComponent<NavMeshAgent>();
        myLineRenderer = GetComponent<LineRenderer>();

        myLineRenderer.startWidth = 0.15f;
        myLineRenderer.endWidth = 0.15f;
        myLineRenderer.positionCount = 0;

        StartCoroutine(CheckPlayerInSight());
    }

    private void Update()
    {
        float dist = Vector3.Distance(transform.position, targetObject.transform.position);
        if(dist <= 1)
        {
            Destroy(gameObject);
            PlayerStats.Instance.lives--;
        }
        else if (agent.hasPath)
        {
            DrawPath();
        }
    }

    private IEnumerator CheckPlayerInSight()
    {
        for (; ;) 
        {
            playerInSightRange = Physics.CheckSphere(transform.position, sightRange, whatIsPlayer);

            if (playerInSightRange) playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, whatIsPlayer);
            else playerInAttackRange = false;

            if (playerInSightRange && !playerInAttackRange) ChasePlayer(); // Prioritize chase player
            else if (playerInSightRange && playerInAttackRange) AttackPlayer(GetNearestPlayer()); // Then attacking player (probably need to prioritize this later on)
            else if (!playerInSightRange && !playerInAttackRange) MoveToTarget(); // Then move to target

            yield return new WaitForSeconds(0.5f);
        }
    }

    private void MoveToTarget()
    {
        targetMarkerPrefab.transform.SetParent(visualObjectsParent);
        targetMarkerPrefab.SetActive(true);
        targetMarkerPrefab.transform.position = targetObject.transform.position;
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
        agent.SetDestination(GetNearestPlayer().position);
    }

    private Transform GetNearestPlayer()
    {
        Collider[] playerUnits = Physics.OverlapSphere(transform.position, sightRange, whatIsPlayer);
        Transform nearestPlayerPosition = playerUnits.OrderBy(x => Vector3.Distance(x.transform.position, transform.position)).FirstOrDefault().transform;
        return nearestPlayerPosition;
    }

    private void AttackPlayer(Transform playerObject)
    {
        agent.SetDestination(transform.position);

        transform.LookAt(playerObject);

        if (!alreadyAttacked)
        {
            //Rigidbody rb = Instantiate(projectile, transform.position, Quaternion.identity).GetComponent<Rigidbody>();
            //rb.AddForce(transform.forward * 32f, ForceMode.Impulse);
            //rb.AddForce(transform.up * 8f, ForceMode.Impulse);

            GameObject bulletObj = (GameObject)Instantiate(projectile, firePoint.position, firePoint.rotation);
            Bullet bulletS = bulletObj.GetComponent<Bullet>();
            
            if(bulletS != null)
            {
                bulletS.Seek(playerObject);
            }

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

    // Draws the path the player will take to reach its destination
    void DrawPath()
    {
        myLineRenderer.positionCount = agent.path.corners.Length;
        myLineRenderer.SetPosition(0, transform.position);

        if (agent.path.corners.Length < 2)
        {
            return;
        }

        for (int i = 1; i < agent.path.corners.Length; i++)
        {
            Vector3 pointPosition = new Vector3(agent.path.corners[i].x, agent.path.corners[i].y, agent.path.corners[i].z);
            myLineRenderer.SetPosition(i, pointPosition);
        }
    }

    /*private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);
    }*/
}
