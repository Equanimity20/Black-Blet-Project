using UnityEngine;
using System.Collections;

public class GrappleGunLogic : MonoBehaviour, IPickUpItem
{
    [Header("References")]
    public GameObject inventoryManager;
    public InventorySystem invSys;
    public GameObject Player;
    public Sprite Icon;
    public GameObject PrefabgameObject;
    public Vector3 properEquipOrientation;
    public bool isGrappling;
    public bool canGrapple = true;
    public bool grappleDebouncer = false;
    public GameObject ShootPoint;
    public Transform ShootPointTransform;
    public LineRenderer grappleLineRenderer;
    
    [Header("Grapple Settings")]
    public float grappleSpeed = 50f;
    public float maxGrappleDistance = 100f;
    public LayerMask grappleableLayers;
    public float grappleCooldown = 3f;
    private float grappleCooldownTimer = 0f;
    
    private Vector3 hitPointVector3;
    private Camera playerCamera;
    private Material lineMaterial; // Store the material

    void Start()
    {
        inventoryManager = GameObject.FindGameObjectWithTag("Inventory Manager");
        invSys = inventoryManager.GetComponent<InventorySystem>();
        Player = GameObject.FindGameObjectWithTag("Player");
        ShootPointTransform = ShootPoint.transform;
        
        // Create a material at runtime
        CreateLineMaterial();
        
        grappleLineRenderer.positionCount = 1;
        
        playerCamera = Camera.main;
        if (playerCamera == null)
        {
            playerCamera = Player.GetComponentInChildren<Camera>();
        }
        
        if (grappleableLayers.value == 0)
        {
            grappleableLayers = ~0;
        }

        canGrapple = true;
    }

    void CreateLineMaterial()
    {
        // Create a new material with an Unlit shader
        lineMaterial = new Material(Shader.Find("Unlit/Color"));
        lineMaterial.color = Color.gray;
        
        // Assign to line renderer
        grappleLineRenderer.material = lineMaterial;
        grappleLineRenderer.startColor = Color.gray;
        grappleLineRenderer.endColor = Color.gray;
        grappleLineRenderer.startWidth = 0.1f;
        grappleLineRenderer.endWidth = 0.1f;
    }

    void Update()
    {
        // Re-assign material if it gets lost
        if (grappleLineRenderer != null && (grappleLineRenderer.material == null || grappleLineRenderer.material.shader == null))
        {
            CreateLineMaterial();
        }
        
        if (ShootPoint != null)
        {
            ShootPointTransform = ShootPoint.transform;
            grappleLineRenderer.SetPosition(0, ShootPointTransform.position);
        }

        //handle grapple cooldown
        if (grappleCooldownTimer > 0f)
        {
            grappleCooldownTimer -= Time.deltaTime;
            if (grappleCooldownTimer <= 0f)
                canGrapple = true;
        }

        if(invSys.CurrentEquippedItem == gameObject)
        {
            if (Input.GetMouseButtonDown(0) && grappleCooldownTimer <= 0f)
            { 
                Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                
                if (Physics.Raycast(ray, out hit, maxGrappleDistance, grappleableLayers))
                {
                    hitPointVector3 = hit.point;
                    isGrappling = true;
                    grappleDebouncer = false;
                }
                else
                {
                    Debug.LogWarning("No grapple point found!");
                }
            }

            if (Input.GetMouseButtonUp(0))
            {
                ResetGrapple();
                StartGrappleCooldown();
            }

            if(canGrapple && isGrappling)
            {
                GrappleLogic();
            }

            if(!canGrapple)
            {
                if (!grappleDebouncer)
                {
                    grappleDebouncer = true;
                    StartCoroutine(GrappleGunSpinny());
                }
            }
        }
    }

    void ResetGrapple()
    {
        isGrappling = false;
        grappleDebouncer = false;
        canGrapple = false;

        if (grappleLineRenderer != null)
            grappleLineRenderer.positionCount = 1;
    }
    
    void StartGrappleCooldown()
    {
        grappleCooldownTimer = grappleCooldown;
    }

    void OnDisable()
    {
        isGrappling = false;
        grappleDebouncer = false;
        if (grappleLineRenderer != null)
            grappleLineRenderer.positionCount = 1;
    }

    void OnEnable()
    {
        canGrapple = true;
        grappleCooldownTimer = 0f;
        
        // Recreate material when enabled (in case it was lost)
        if (grappleLineRenderer != null)
        {
            Invoke(nameof(EnsureLineMaterial), 0.1f);
        }
    }

    void EnsureLineMaterial()
    {
        if (grappleLineRenderer != null)
        {
            CreateLineMaterial();
        }
    }

    public void GrappleLogic()
    {
        Vector3 startPointVector3 = Player.transform.position;
        Vector3 direction = hitPointVector3 - startPointVector3;
        float distance = Vector3.Distance(startPointVector3, hitPointVector3);

        Debug.DrawRay(startPointVector3, direction, Color.yellow);

        // Set line renderer to show the grapple line
        grappleLineRenderer.positionCount = 2;
        grappleLineRenderer.SetPosition(0, ShootPointTransform.position);
        grappleLineRenderer.SetPosition(1, hitPointVector3);

        if(distance > 1f)
        {
            Player.transform.position = Vector3.MoveTowards(Player.transform.position, hitPointVector3, grappleSpeed * Time.deltaTime);
        }
        else
        {
            Vector3 finalPos = hitPointVector3 - direction.normalized;
            Player.transform.position = finalPos;
        }
    }

    IEnumerator GrappleGunSpinny()
    {
        float elapsed = 0f;
        Quaternion startRotation = transform.localRotation;

        // Spin 360 degrees around the Z axis over the full grappleCooldown
        while (elapsed < grappleCooldown)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / grappleCooldown);
            transform.localRotation = startRotation * Quaternion.Euler(-(t * 360f), 0f, 0f);
            yield return null;
        }

        // Snap back to original rotation when done
        transform.localRotation = startRotation;
    }

    public void PickUp()
    {
        if (invSys.CanPickupItem())
        {
            InventorySystem.InventoryItem newItem = invSys.CreateInventoryItem(PrefabgameObject, Icon, "Grapple Gun", InventorySystem.ItemType.Gun);
            invSys.AddItemToToolbar(newItem);
        }
    }

    public Vector3 SetProperEquipOrientation()
    {
        properEquipOrientation = new Vector3(0f, 0f, 0f);
        return properEquipOrientation;
    }
}