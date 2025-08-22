using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class BasicEnemy : MonoBehaviour, IDamageable
{
    [Header("Enemy Settings")]
    private UnityEngine.AI.NavMeshAgent enemy;
    public Transform target;
    public Health hp;
    public bool CanAttack = true;
    public bool seenPlayer = false;
    public float viewDistance;
    public float ViewConeAngularWidth;
    public float EnemyHealth = 1f;
    private float timeSinceLastSeen = Mathf.Infinity;

    [Header("Ragdoll Settings")]
    public List<Rigidbody> rb;
    public List<Collider> colliders;
    public List<CharacterJoint> joints;
    public bool isRagdollOn = false;

    void Start()
    {
        enemy = gameObject.GetComponent<UnityEngine.AI.NavMeshAgent>();
        ToggleRagdoll(false, Vector3.zero); // Disable ragdoll
        rb = new List<Rigidbody>(GetComponentsInChildren<Rigidbody>());
        colliders = new List<Collider>(GetComponentsInChildren<Collider>());
        joints = new List<CharacterJoint>(GetComponentsInChildren<CharacterJoint>());
    }

    void Update()
    {
        if (!isRagdollOn)
        {
            timeSinceLastSeen += Time.deltaTime;

            Vector3 origin = enemy.transform.position;
            Vector3 directionToTarget = (target.position - origin).normalized;
            float distanceToTarget = Vector3.Distance(origin, target.position);
            float angleToTarget = Vector3.Angle(enemy.transform.forward, directionToTarget);

            bool isInView = distanceToTarget < viewDistance && angleToTarget < ViewConeAngularWidth;
            bool isVisible = false;

            if (isInView)
            {
                // Perform raycast to check line of sight
                RaycastHit hit;
                if (Physics.Raycast(origin, directionToTarget, out hit, viewDistance))
                {
                    if (hit.transform == target)
                    {
                        isVisible = true;
                    }
                }
            }

            if (isVisible)
            {
                if (!seenPlayer)
                {
                    Debug.Log("Player spotted!");
                }

                seenPlayer = true;
                timeSinceLastSeen = 0f;
            }
            else if (seenPlayer && timeSinceLastSeen >= 10f)
            {
                seenPlayer = false;
                Debug.Log("Lost sight of player after 10 seconds.");
            }

            // Attack if close enough
            if (distanceToTarget < 1f && CanAttack)
            {
                Attack();
                CanAttack = false;
            }

            if (EnemyHealth <= 0)
            {
                ToggleRagdoll(true, Vector3.zero);
            }

            // --- Visualization ---
            Vector3 forward = enemy.transform.forward;
            Vector3 leftBoundary = Quaternion.AngleAxis(-ViewConeAngularWidth, Vector3.up) * forward;
            Vector3 rightBoundary = Quaternion.AngleAxis(ViewConeAngularWidth, Vector3.up) * forward;

            Debug.DrawRay(origin, forward * viewDistance, Color.green);
            Debug.DrawRay(origin, leftBoundary * viewDistance, Color.yellow);
            Debug.DrawRay(origin, rightBoundary * viewDistance, Color.yellow);
        }
            
    }

    void FixedUpdate()
    {
        if(!isRagdollOn)
        {
            if (seenPlayer)
            {
                enemy.SetDestination(target.position);
            }
        }
    }

    void Attack()
    {
        hp.damageValue += 5f;
        StartCoroutine(WaitAndContinue());
    }

    private IEnumerator WaitAndContinue()
    {
        yield return new WaitForSeconds(1f);
        CanAttack = true;
    }

    public void ToggleRagdoll(bool RagdollOn, Vector3 forceDirection)
    {
        isRagdollOn = RagdollOn;

        foreach (var col in colliders)
        {
            if (col != null)
            {
                col.enabled = RagdollOn;
            }
        }

        foreach (var rigidbody in rb)
        {
            if (rigidbody != null)
            {
                rigidbody.isKinematic = !RagdollOn;
                if (RagdollOn)
                {
                    float knockbackForce = 25f; // Adjust as needed
                    rigidbody.AddForce(forceDirection * knockbackForce, ForceMode.Impulse);
                }
            }
        }

        if (enemy != null)
        {
            enemy.enabled = !RagdollOn;
        }
    }

    public void TakeDamage(float damage, Vector3 hitDirection)
    {
            EnemyHealth -= damage;
            if (EnemyHealth <= 0)
            {
                ToggleRagdoll(true, hitDirection);
            }
    }
}