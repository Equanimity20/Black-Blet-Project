using System;
using System.Collections;
using UnityEngine;

public class BasicEnemy : MonoBehaviour, IDamageable
{
    [Header("Enemy Settings")]
    private UnityEngine.AI.NavMeshAgent enemy;
    public Transform target;
    public PlayerStats ps;
    public bool CanAttack = true;
    public bool seenPlayer = false;
    public float viewDistance;
    public float ViewConeAngularWidth;
    public float EnemyHealth = 1f;
    public PlayerCam cam;
    private float timeSinceLastSeen = Mathf.Infinity;

    [Header("Explosion Settings")]
    public float radius = 2.5f;
    public int maxHits = 25;
    public float MaxDmg = 50f;
    public float MinDmg = 1f;
    public float explosionForce;
    public LayerMask HitLayer;
    public LayerMask BlockLayer;
    public GameObject explosionEffectParent;

    [Header("Camera Shake Settings")]
    public float shakeDuration;
    public float shakeStrength;

    private Collider[] Hits;

    void Start()
    {
        enemy = gameObject.GetComponent<UnityEngine.AI.NavMeshAgent>();
        explosionEffectParent.SetActive(false);
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
            print("Lost sight of player after 10 seconds.");
        }

        // Attack if close enough
        if (distanceToTarget < 1f && CanAttack)
        {
            Attack();
            CanAttack = false;
        }

        // --- Visualization ---
        Vector3 forward = enemy.transform.forward;
        Vector3 leftBoundary = Quaternion.AngleAxis(-ViewConeAngularWidth, Vector3.up) * forward;
        Vector3 rightBoundary = Quaternion.AngleAxis(ViewConeAngularWidth, Vector3.up) * forward;

        Debug.DrawRay(origin, forward * viewDistance, Color.green);
        Debug.DrawRay(origin, leftBoundary * viewDistance, Color.yellow);
        Debug.DrawRay(origin, rightBoundary * viewDistance, Color.yellow); 
    }

    public void DestroyEnemy()
    {
        Destroy(gameObject);
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
        ps.damageValue += 5f;
        StartCoroutine(WaitAndContinue());
    }

    private IEnumerator WaitAndContinue()
    {
        yield return new WaitForSeconds(1f);
        CanAttack = true;
    }

    public void ExplodeOnDeath()
    {
        if (explosionEffectParent != null)
        {
            foreach (ParticleSystem ps in explosionEffectParent.GetComponentsInChildren<ParticleSystem>())
            {
                //play particle systems
                if (ps == null || ps.gameObject == null) continue;
                GameObject instance = Instantiate(ps.gameObject, transform.position, transform.rotation);
                ParticleSystem instancePs = instance.GetComponent<ParticleSystem>();
                if (instancePs != null)
                {
                    instancePs.Play();
                    Destroy(instance, instancePs.main.duration);
                }
                else
                {
                    Destroy(instance);
                }
            }
        }

        //Create overlapsphere to detect objects in explosion radius
        int hits = Physics.OverlapSphereNonAlloc(transform.position, radius, Hits, HitLayer.value);

        for (int i = 0; i < hits; i++)
        {
            if(Hits[i].attachedRigidbody == gameObject.GetComponent<Rigidbody>()) continue;

            //Find rigidbody
            if (Hits[i].attachedRigidbody != null)
            {
                //Calculate shake strength and duration based on distance from explosion
                float distance = Vector3.Distance(transform.position, Hits[i].transform.position);
                Vector3 dir = (Hits[i].transform.position - transform.position).normalized;
                shakeStrength = Mathf.Clamp01(1 - (distance / radius)) * (explosionForce / 5000) * 0.1f;
                shakeDuration = Mathf.Clamp01(1 - (distance / radius)) * 1.5f;

                //check if blocked
                if (!Physics.Raycast(transform.position, dir, distance, BlockLayer.value))
                {
                    //Calculate force
                    Vector3 force = dir * (explosionForce / 10) / distance;
                    //Apply explosion force
                    if(force != Vector3.zero)
                        Hits[i].attachedRigidbody.AddForce(force, ForceMode.Force);
                    //Shake camera
                    if (Hits[i].GetComponent<Collider>().CompareTag("Player"))
                        cam.DoShake(shakeDuration, shakeStrength);
                }
            }
            if (Hits[i].GetComponent<Collider>() != null)
            {
                //Calculate damage to deal
                var damageable = Hits[i].GetComponentInParent<IDamageable>();
                float distance = Vector3.Distance(transform.position, Hits[i].transform.position);
                float damageToDeal = Mathf.Lerp(MaxDmg, MinDmg, distance / radius);
                Vector3 dir = Vector3.zero;
                if (damageable != null)
                {
                    //deal damage
                    ps.damageValue += damageToDeal;
                    damageable.TakeDamage(damageToDeal, dir);
                }
            }
        }
        DestroyEnemy();
    }

    public void TakeDamage(float damage, Vector3 hitDirection)
    {
        EnemyHealth -= damage;
        if (EnemyHealth <= 0)
        {
            ExplodeOnDeath();
        }
    }
}