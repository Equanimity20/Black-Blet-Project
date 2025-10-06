using JetBrains.Annotations;
using UnityEngine;

public class InventoryInputSystem : MonoBehaviour
{
    public InventorySystem invsys;
    public Transform orientation;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, orientation.transform.forward, out hit, 5f))
        {
            if (hit.collider.isTrigger)
            {
                if (hit.collider.GetComponent<IPickUpItem>() != null)
                {
                    // If we hit a pick up item, we can interact with it
                    if (Input.GetKeyDown(KeyCode.F))
                    {
                        hit.collider.GetComponent<IPickUpItem>().PickUp();
                    }
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            invsys.DropItemFromSlot(invsys.CurrentEquippedSlot);
        }
    }
}
