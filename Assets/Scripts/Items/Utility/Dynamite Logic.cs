using UnityEngine;

public class DynamiteLogic : MonoBehaviour, IPickUpItem, IDamageable
{
    [Header("References")]
    private Rigidbody rb;
    private Collider col;
    private GameObject inventoryManager;
    public InventorySystem invSys;
    public PlayerMovementAdvanced pm;
    public PlayerCam cam;
    public Sprite Icon;
    public GameObject explosionEffectParent;
    public Vector3 properEquipOrientation;
    public GameObject PrefabgameObject;
    public bool isThrown = false;

    [Header("Explosion Settings")]
    public float radius = 2.5f;
    public int maxHits = 25;
    public float MaxDmg = 50f;
    public float MinDmg = 1f;
    public float explosionForce;
    public LayerMask HitLayer;
    public LayerMask BlockLayer;

    [Header("Camera Shake Settings")]
    public float shakeDuration;
    public float shakeStrength;

    private Collider[] Hits;

    void Awake()
    {
        Hits = new Collider[maxHits];
        // FIX: moved from Start() so col is never null when OnCollisionEnter fires
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
    }

    void Start()
    {
        inventoryManager = GameObject.FindGameObjectWithTag("Inventory Manager");
        invSys = inventoryManager.GetComponent<InventorySystem>();
        pm = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovementAdvanced>();

        if (explosionEffectParent != null)
            explosionEffectParent.SetActive(false);
    }

    void Update()
    {
        if (invSys != null && invSys.CurrentEquippedItem == gameObject && Input.GetMouseButtonUp(0))
        {
            ThrowDynamite();
        }
    }

    void ThrowDynamite()
    {
        GameObject dynamite = Instantiate(gameObject, transform.position, transform.rotation);
        Rigidbody dynamiteRb = dynamite.GetComponent<Rigidbody>();
        dynamiteRb.isKinematic = false;

        // FIX: was GetComponent (singular) — the inventory system disables ALL colliders
        // with GetComponents (plural), so re-enabling only the first one left the rest off,
        // meaning the thrown dynamite couldn't register collisions with the ground.
        foreach (var c in dynamite.GetComponents<Collider>())
            c.enabled = true;

        Vector3 throwDirection = new Vector3(
            pm.cam.transform.forward.x,
            pm.cam.transform.forward.y + 0.1f,
            pm.cam.transform.forward.z
        ).normalized;

        dynamiteRb.AddForce(throwDirection * 25f, ForceMode.Impulse);
        dynamite.GetComponent<DynamiteLogic>().isThrown = true;
        invSys.RemoveItemFromSlot(invSys.CurrentEquippedSlot);
    }

    void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Dynamite collided with: " + collision.gameObject.name);
        if (isThrown)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                // col is now always valid because it's assigned in Awake()
                Physics.IgnoreCollision(collision.collider, col);
            }
            else
            {
                Explode();
                Destroy(gameObject);
            }
        }
    }

    void Explode()
    {
        if (explosionEffectParent != null)
        {
            foreach (ParticleSystem ps in explosionEffectParent.GetComponentsInChildren<ParticleSystem>())
            {
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

        int hits = Physics.OverlapSphereNonAlloc(transform.position, radius, Hits, HitLayer.value);

        for (int i = 0; i < hits; i++)
        {
            if (Hits[i].attachedRigidbody == rb) continue;

            if (Hits[i].attachedRigidbody != null)
            {
                float distance = Vector3.Distance(transform.position, Hits[i].transform.position);
                Vector3 dir = (Hits[i].transform.position - transform.position).normalized;
                shakeStrength = Mathf.Clamp01(1 - (distance / radius)) * (explosionForce / 5000) * 0.1f;
                shakeDuration = Mathf.Clamp01(1 - (distance / radius)) * 1.5f;

                if (!Physics.Raycast(transform.position, dir, distance, BlockLayer.value))
                {
                    Vector3 force = dir * (explosionForce / 10) / distance;
                    if (force != Vector3.zero)
                        Hits[i].attachedRigidbody.AddForce(force, ForceMode.Force);
                    if (cam != null && Hits[i].GetComponent<Collider>().CompareTag("Player"))
                        cam.DoShake(shakeDuration, shakeStrength);
                }
            }

            if (Hits[i].GetComponent<Collider>() != null)
            {
                var damageable = Hits[i].GetComponentInParent<IDamageable>();
                float distance = Vector3.Distance(transform.position, Hits[i].transform.position);
                float damageToDeal = Mathf.Lerp(MaxDmg, MinDmg, distance / radius);
                if (damageable != null)
                {
                    damageable.TakeDamage(damageToDeal, Vector3.zero);
                }
            }
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);
    }

    public void PickUp()
    {
        Debug.Log("pickup called");
        if (invSys.CanPickupItem())
        {
            Debug.Log("can pickup item");
            InventorySystem.InventoryItem newItem = invSys.CreateInventoryItem(PrefabgameObject, Icon, "Dynamite", InventorySystem.ItemType.Throwable);
            invSys.AddItemToToolbar(newItem);
            Debug.Log("item added to inventory");
        }
    }

    public Vector3 SetProperEquipOrientation()
    {
        properEquipOrientation = new Vector3(-90f, 0f, 0f);
        return properEquipOrientation;
    }

    public void TakeDamage(float damage, Vector3 hitDirection)
    {
        rb.AddForce(hitDirection * 25f, ForceMode.Impulse);
    }
}