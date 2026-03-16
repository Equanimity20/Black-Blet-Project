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
    public static bool isThrown = false;

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

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        inventoryManager = GameObject.FindGameObjectWithTag("Inventory Manager");
        invSys = inventoryManager.GetComponent<InventorySystem>();
        pm = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovementAdvanced>();

        explosionEffectParent.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (invSys.CurrentEquippedItem == PrefabgameObject && Input.GetMouseButtonUp(0))
        {
            ThrowDynamite();
        }
    }

    void Awake()
    {
        Hits = new Collider[maxHits];
    }

    void ThrowDynamite()
    {
        //Make new instance of dynamite and set settings/create force
        GameObject dynamite = Instantiate(gameObject, transform.position, transform.rotation);
        dynamite.GetComponent<Rigidbody>().isKinematic = false;
        dynamite.GetComponent<Collider>().enabled = true;
        Vector3 throwDirection = new Vector3(pm.cam.transform.forward.x, pm.cam.transform.forward.y + 0.1f, pm.cam.transform.forward.z).normalized;
        float throwForce = 25f;
        //Apply force to dynamite
        dynamite.GetComponent<Rigidbody>().AddForce(throwDirection * throwForce, ForceMode.Impulse);
        isThrown = true;
        //Remove dynamite from inventory
        invSys.RemoveItemFromSlot(invSys.CurrentEquippedSlot);
    }

    void OnCollisionEnter(Collision collision)
    {
        if(isThrown)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                //Ignore collision with player
                Physics.IgnoreCollision(collision.collider, col);
            }
            else if (isThrown)
            {
                //Explode on impact
                Destroy(gameObject);
                Explode();
            }
        }
    }

    void Explode()
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
                    damageable.TakeDamage(damageToDeal, dir);
                }
            }
        }
    }
    
    void OnDrawGizmos()
    {
        // Draw the wire sphere in the Scene view
        Gizmos.color = Color.red; // Choose a color for the gizmo
        Gizmos.DrawWireSphere(transform.position, radius);
    }

    public void PickUp()
    {
        if (invSys.CanPickupItem())
        {
            //Add dynamite to inventory
            InventorySystem.InventoryItem newItem = invSys.CreateInventoryItem(PrefabgameObject, Icon, "Dynamite", InventorySystem.ItemType.Throwable);
            invSys.AddItemToToolbar(newItem);
        }
    }

    public Vector3 SetProperEquipOrientation()
    {
        properEquipOrientation = new Vector3(0f, 0f, 0f);
        return properEquipOrientation;
    }

    public void TakeDamage(float damage, Vector3 hitDirection)
    {
        rb.AddForce(hitDirection * 25f, ForceMode.Impulse);
    }
}
