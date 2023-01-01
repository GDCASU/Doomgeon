using System.Collections;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Enemy will follow the player if it gets close enough to it.
/// 
/// Author: Alben Trang
/// </summary>
public class EnemyFollow : MonoBehaviour
{
    // Some code by Brackeys from here: "ENEMY AI - Making an RPG in Unity (E10)"
    // https://www.youtube.com/watch?v=xppompv1DBg&list=PLPV2KyIb3jR4KLGCCAciWQ5qHudKtYeP7&index=11

    public float lookRadius = 10.0f;
    public float rotationSpeed = 5.0f;
    public float attackCooldown = 1.0f;
    public int damage = 1;

    private Transform playerTarget;
    private Damageable playerDamageable;
    private NavMeshAgent agent;
    private bool canAttack;

    private void Start()
    {
        playerTarget = GameObject.FindWithTag("Player").transform;
        playerDamageable = playerTarget.GetComponent<Damageable>();
        agent = GetComponent<NavMeshAgent>();
        canAttack = true;
    }

    private void Update()
    {
        float distanceFromTarget = Vector3.Distance(playerTarget.position, transform.position);

        if (distanceFromTarget <= lookRadius)
        {
            agent.SetDestination(playerTarget.position);

            if (distanceFromTarget <= agent.stoppingDistance)
            {
                // Attack the player continuously when they are close.
                if (canAttack)
                {
                    playerDamageable.ReceiveDamage(damage);
                    canAttack = false;
                    StartCoroutine(AttackCooldown());
                }

                // Rotate to always face the player when they are close.
                FaceTarget();
            }
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
        Gizmos.DrawWireSphere(transform.position, lookRadius);
    }
}
