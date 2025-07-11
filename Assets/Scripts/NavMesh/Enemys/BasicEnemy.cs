using System.Collections;
using UnityEngine;

public class BasicEnemy : MonoBehaviour
{
    private UnityEngine.AI.NavMeshAgent enemy;
    public Transform target;
    public Health hp;
    public float distance;
    public bool CanAttack = true;
    public bool seenPlayer = false;
    public float viewDistance;
    public float ViewConeAngularWidth;
    public float EnemyHealth = 1f;

    private float timeSinceLastSeen = Mathf.Infinity;

    void Start()
    {
        enemy = gameObject.GetComponent<UnityEngine.AI.NavMeshAgent>();
        SetRagdollState(false); // Disable ragdoll
    }

    void Update()
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
            SetRagdollState(true);
        }

        // --- Visualization ---
        Vector3 forward = enemy.transform.forward;
        Vector3 leftBoundary = Quaternion.AngleAxis(-ViewConeAngularWidth, Vector3.up) * forward;
        Vector3 rightBoundary = Quaternion.AngleAxis(ViewConeAngularWidth, Vector3.up) * forward;

        Debug.DrawRay(origin, forward * viewDistance, Color.green);
        Debug.DrawRay(origin, leftBoundary * viewDistance, Color.yellow);
        Debug.DrawRay(origin, rightBoundary * viewDistance, Color.yellow);
    }

    void FixedUpdate()
    {
        if (seenPlayer)
        {
            enemy.SetDestination(target.position);
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

    void SetRagdollState(bool isRagdoll)
    {
        Animator anim = GetComponent<Animator>();
        if (anim != null)
            anim.enabled = !isRagdoll;

        enemy.enabled = !isRagdoll; // Disable NavMeshAgent if ragdoll

        Rigidbody[] bodies = GetComponentsInChildren<Rigidbody>();
        foreach (Rigidbody rb in bodies)
        {
            if (rb.gameObject != this.gameObject) // Skip the root if needed
            {
                rb.isKinematic = !isRagdoll;
                rb.detectCollisions = isRagdoll;
            }
        }

        Collider[] colliders = GetComponentsInChildren<Collider>();
        foreach (Collider col in colliders)
        {
            if (col.gameObject != this.gameObject) // Skip main capsule collider
                col.enabled = isRagdoll;
        }
    }
}