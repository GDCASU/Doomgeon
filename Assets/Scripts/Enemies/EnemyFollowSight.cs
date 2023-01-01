using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyFollowSight : MonoBehaviour
{
    public Transform lookPoint;
    public float lookDistance = 10.0f;
    public float lookRadius = 10.0f;
    public float tooCloseRadius = 3.0f;
    public float rotationSpeed = 5.0f;
    public float attackCooldown = 1.0f;
    public int damage = 1;

    private Transform playerTarget;
    private Damageable playerDamageable;
    private NavMeshAgent agent;
    private Vector3 lastSeenPlayerPos;
    private bool canAttack;
    private bool foundSomething;
    private bool firstSpottedPlayer;
    private RaycastHit hit;

    private void Start()
    {
        playerTarget = GameObject.FindWithTag("Player").transform;
        playerDamageable = playerTarget.GetComponent<Damageable>();
        agent = GetComponent<NavMeshAgent>();
        lastSeenPlayerPos = Vector3.zero;
        canAttack = true;
        firstSpottedPlayer = false;
    }

    private void Update()
    {
        float distanceFromTarget = Vector3.Distance(playerTarget.position, transform.position);
        foundSomething = Physics.SphereCast(lookPoint.position, lookRadius, lookPoint.forward, out hit, lookDistance, Constants.LayerMask.Protagonist);

        if (distanceFromTarget <= agent.stoppingDistance)
        {
            // Rotate to always face the player when they are close.
            FaceTarget();

            // Attack the player continuously when they are close.
            if (canAttack)
            {
                playerDamageable.ReceiveDamage(damage);
                canAttack = false;
                StartCoroutine(AttackCooldown());
            }
        }
        else if ((foundSomething && hit.collider.CompareTag("Player")) || (distanceFromTarget <= tooCloseRadius))
        {
            // Rotate to always face the player when they are close.
            FaceTarget();

            firstSpottedPlayer = true;
            lastSeenPlayerPos = playerTarget.position;
            agent.SetDestination(lastSeenPlayerPos);
        }
        else if (firstSpottedPlayer)
        {
            agent.SetDestination(lastSeenPlayerPos);
        }
    }

    private void FaceTarget()
    {
        Vector3 dirToTarget = (playerTarget.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(dirToTarget.x, 0, dirToTarget.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
    }

    private IEnumerator AttackCooldown()
    {
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(lookPoint.position, lookPoint.forward * lookDistance);
    }
}
