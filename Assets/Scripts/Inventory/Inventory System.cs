using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventorySystem : MonoBehaviour
{
    [Serializable]
    public class InventoryItem
    {
        public string itemName;
        public GameObject prefab;
        public Sprite iconSprite; // Keep as Sprite
        public ItemType itemType;
        [HideInInspector] public GameObject instantiatedObject;
        [HideInInspector] public int slotIndex = -1; // -1 means not in toolbar
        [HideInInspector] public bool newItem = false; // For UI Updates
    }

    [Header("Toolbar Settings")]
    [SerializeField] private int toolbarSlots = 9; // Only toolbar slots available

    [Header("UI References")]
    public Transform toolbarParent; // Parent object for toolbar slot UI elements

    [Serializable]
    public class ToolbarSlotUI // Array of RawImage components for each toolbar slot
    {
        public RawImage slotImage;
        public Image iconImage;
    }
    public ToolbarSlotUI[] toolbarSlotsUI;
    private float originalY;

    [Header("Available Items")]
    public List<InventoryItem> availableItems = new();

    [Header("Equipment Settings")]
    public Transform equipmentParent; // Where equipped items appear (like camera transform)
    public Vector3 positionOffset = new Vector3(0.5f, -0.5f, 1.0f);
    public Vector3 rotationOffset = Vector3.zero;
    public enum ItemType { Gun, Melee, Throwable, Powerup }

    // Toolbar slots - tracks what's in each slot
    private GameObject[] toolbarSlotObjects;
    public InventoryItem[] slotContents;
    [HideInInspector] public int publicSlotIndex;

    // Currently equipped item
    private GameObject currentEquippedItem;
    private int currentEquippedSlot = -1;

    // Properties for external access
    public int ToolbarSlots => toolbarSlots;
    public GameObject CurrentEquippedItem => currentEquippedItem;
    public int CurrentEquippedSlot => currentEquippedSlot;
    public string CurrentEquippedItemName => currentEquippedItem != null ? currentEquippedItem.name : "None";

    void Awake()
    {
        InitializeToolbar();
        InitializeItemIcons();
        HideIconImageOnStart();
    }

    void Start()
    {
        originalY = toolbarSlotsUI[0].slotImage.transform.position.y;
    }

    void Update()
    {
        HandleInput();
    }

    private void HideIconImageOnStart()
    {
        for (int i = 0; i < toolbarSlotsUI.Length; i++)
        {
            toolbarSlotsUI[i].iconImage.color = new Color(1f, 1f, 1f, 0f); // Hide icon
        }
    }

    private void InitializeToolbar()
    {
        toolbarSlotObjects = new GameObject[toolbarSlots];
        slotContents = new InventoryItem[toolbarSlots];
    }

    private void InitializeItemIcons()
    {
        // Initialize toolbar slot images
        if (toolbarSlotsUI != null)
        {
            for (int i = 0; i < toolbarSlotsUI.Length && i < toolbarSlots; i++)
            {
                if (toolbarSlotsUI[i].iconImage != null)
                {
                    // Start with empty/transparent slots
                    toolbarSlotsUI[i].iconImage.sprite = null;
                    Color slotColor = toolbarSlotsUI[i].iconImage.color;
                    slotColor.a = 0.3f; // Dim empty slots
                    toolbarSlotsUI[i].iconImage.color = slotColor;
                }
            }
        }
    }

    private void HandleInput()
    {
        // Handle toolbar hotkeys (1-9, 0)
        if (Input.inputString.Length > 0)
        {
            char inputChar = Input.inputString[0];

            // Handle number keys for toolbar
            if (char.IsDigit(inputChar))
            {
                int keyPressed = inputChar == '0' ? 10 : (inputChar - '0'); // Convert '0' to 10, others to 1-9
                int slotIndex = keyPressed - 1; // Convert to 0-based index

                if (slotIndex >= 0 && slotIndex < toolbarSlots)
                {
                    publicSlotIndex = slotIndex;
                    HandleToolbarInput(slotIndex);
                }
            }
        }
    }

    private void HandleToolbarInput(int slotIndex)
    {
        if (IsSlotOccupied(slotIndex))
        {
            // If slot has item, equip it (or unequip if already equipped)
            if (currentEquippedSlot == slotIndex)
            {
                UnequipItem();
                ChangeSelectedSlot(-1); // Deselect all slots
            }
            else
            {
                EquipItemFromSlot(slotIndex);
            }
        }
    }

    // Add item to specific toolbar slot
    public bool AddItemToSlot(InventoryItem item, int slotIndex)
    {
        if (!IsValidSlotIndex(slotIndex) || IsSlotOccupied(slotIndex))
            return false;

        slotContents[slotIndex] = item;
        item.slotIndex = slotIndex;
        item.instantiatedObject = Instantiate(item.prefab);
        item.instantiatedObject.SetActive(false); // Start inactive until equipped


        // Update the UI slot image
        UpdateSlotUI(slotIndex);

        return true;
    }

    // Helper method to create InventoryItem from GameObject
    public InventoryItem CreateInventoryItem(GameObject prefab, Sprite iconSprite, string itemName, ItemType itemType)
    {
        InventoryItem newItem = new InventoryItem();
        newItem.prefab = prefab;
        newItem.iconSprite = iconSprite;
        newItem.itemName = itemName;
        newItem.itemType = itemType;
        newItem.slotIndex = -1;
        newItem.newItem = true; // Mark as new item for UI updates

        return newItem;
    }

    // Overloaded helper method with automatic name from GameObject
    public InventoryItem CreateInventoryItem(GameObject prefab, Sprite iconSprite, ItemType itemType)
    {
        string itemName = prefab != null ? prefab.name : "Unknown Item";
        return CreateInventoryItem(prefab, iconSprite, itemName, itemType);
    }

    // Try to add item to first available toolbar slot
    public bool AddItemToToolbar(InventoryItem item)
    {
        for (int i = 0; i < toolbarSlots; i++)
        {
            if (!IsSlotOccupied(i))
            {
                return AddItemToSlot(item, i);
            }
        }

        Debug.Log($"Toolbar full! Cannot pick up {item.itemName}");
        return false; // Toolbar full
    }

    // Remove item from specific slot
    public InventoryItem RemoveItemFromSlot(int slotIndex)
    {
        if (!IsValidSlotIndex(slotIndex) || !IsSlotOccupied(slotIndex))
            return null;

        if (toolbarSlotsUI[slotIndex].iconImage == null)
        {
            toolbarSlotsUI[slotIndex].iconImage.enabled = false;
        }

        InventoryItem item = slotContents[slotIndex];
        slotContents[slotIndex] = null;
        item.slotIndex = -1;

        // If this was the equipped item, unequip it
        if (slotIndex == currentEquippedSlot)
            UnequipItem();

        // Update the UI slot image
        UpdateSlotUI(slotIndex);

        Debug.Log($"Removed {item.itemName} from toolbar slot {slotIndex + 1}");
        return item;
    }

    // Drop item from toolbar slot (for when player wants to make space)
    public bool DropItemFromSlot(int slotIndex)
    {
        InventoryItem item = RemoveItemFromSlot(slotIndex);
        if (item != null)
        {
            Debug.Log($"Dropped {item.itemName}");
            // Here you could spawn the item back into the world
            // SpawnItemInWorld(item);
            return true;
        }
        return false;
    }

    // Equip item from toolbar slot
    public bool EquipItemFromSlot(int slotIndex)
    {
        if (!IsValidSlotIndex(slotIndex) || !IsSlotOccupied(slotIndex))
            return false;

        InventoryItem item = slotContents[slotIndex];

        if (currentEquippedSlot == slotIndex)
        {
            return true; // Already equipped
        }

        // Update all slot UI to show equipped state
        UpdateAllSlotsUI();

        UnequipItem();

        // Get proper equip orientation from item if it implements IPickUpItem
        rotationOffset = (item.prefab != null && item.prefab.GetComponent<IPickUpItem>() != null) ? item.prefab.GetComponent<IPickUpItem>().SetProperEquipOrientation() : Vector3.zero;

        // Equip new item
        if (item.prefab != null && equipmentParent != null)
        {
            currentEquippedItem = item.instantiatedObject;
            currentEquippedItem.SetActive(true);
            currentEquippedItem.transform.SetParent(Camera.main.transform);
            currentEquippedItem.GetComponent<Rigidbody>().isKinematic = true;
            foreach (var collider in currentEquippedItem.GetComponents<BoxCollider>())
                collider.enabled = false;
            currentEquippedItem.name = item.instantiatedObject.name;
            currentEquippedItem.transform.localPosition = positionOffset;
            currentEquippedItem.transform.localEulerAngles = rotationOffset;
            currentEquippedSlot = slotIndex;
        }

        // Update UI to highlight equipped item
        UpdateAllSlotsUI();

        //Change selected slot
        ChangeSelectedSlot(slotIndex);

        return true;
    }

    // Unequip current item
    public void UnequipItem()
    {
        if (currentEquippedItem != null)
            currentEquippedItem.SetActive(false);

        currentEquippedItem = null;
        currentEquippedSlot = -1;

        // Update UI to remove highlight
        UpdateAllSlotsUI();
    }

    // Move/swap items between toolbar slots
    public bool MoveItem(int fromSlot, int toSlot)
    {
        if (!IsValidSlotIndex(fromSlot) || !IsValidSlotIndex(toSlot))
            return false;

        if (!IsSlotOccupied(fromSlot))
            return false;

        // If destination slot is occupied, swap items
        if (IsSlotOccupied(toSlot))
        {
            InventoryItem tempItem = slotContents[toSlot];
            slotContents[toSlot] = slotContents[fromSlot];
            slotContents[fromSlot] = tempItem;

            slotContents[toSlot].slotIndex = toSlot;
            slotContents[fromSlot].slotIndex = fromSlot;

            Debug.Log($"Swapped items between slots {fromSlot + 1} and {toSlot + 1}");
        }
        else
        {
            // Move item to empty slot
            slotContents[toSlot] = slotContents[fromSlot];
            slotContents[fromSlot] = null;
            slotContents[toSlot].slotIndex = toSlot;

            Debug.Log($"Moved {slotContents[toSlot].itemName} from slot {fromSlot + 1} to slot {toSlot + 1}");
        }

        // Update equipped slot if necessary
        if (currentEquippedSlot == fromSlot)
            currentEquippedSlot = toSlot;
        else if (currentEquippedSlot == toSlot && IsSlotOccupied(fromSlot))
            currentEquippedSlot = fromSlot;

        return true;
    }

    // Utility methods
    public bool IsValidSlotIndex(int slotIndex)
    {
        return slotIndex >= 0 && slotIndex < toolbarSlots;
    }

    public bool IsSlotOccupied(int slotIndex)
    {
        return IsValidSlotIndex(slotIndex) && slotContents[slotIndex] != null;
    }

    public InventoryItem GetItemInSlot(int slotIndex)
    {
        if (!IsValidSlotIndex(slotIndex))
            return null;
        return slotContents[slotIndex];
    }

    public int GetFirstEmptySlot()
    {
        for (int i = 0; i < toolbarSlots; i++)
        {
            if (!IsSlotOccupied(i))
                return i;
        }
        return -1; // No empty slots
    }

    public int GetItemCount()
    {
        int count = 0;
        for (int i = 0; i < toolbarSlots; i++)
        {
            if (IsSlotOccupied(i))
                count++;
        }
        return count;
    }

    public bool IsToolbarFull()
    {
        return GetItemCount() >= toolbarSlots;
    }

    // Check if player can pick up an item (has space in toolbar)
    public bool CanPickupItem()
    {
        return !IsToolbarFull();
    }

    // UI Management Methods
    private void UpdateSlotUI(int slotIndex)
    {
        if (toolbarSlotsUI == null || slotIndex < 0 || slotIndex >= toolbarSlotsUI.Length)
        {
            Debug.Log("Invalid slot index");
            return;
        }

        ToolbarSlotUI slot = toolbarSlotsUI[slotIndex];

        if (IsSlotOccupied(slotIndex))
        {
            InventoryItem item = slotContents[slotIndex];
            slot.iconImage.sprite = item.iconSprite;
            slot.iconImage.color = new Color(1f, 1f, 1f, slotIndex == currentEquippedSlot ? 1f : 0.7f); // Highlight if equipped
        }
        else
        {
            slot.iconImage.sprite = null;
            slot.iconImage.color = new Color(1f, 1f, 1f, 0f); // Hide icon
        }
    }

    private void UpdateAllSlotsUI()
    {
        for (int i = 0; i < toolbarSlotsUI.Length; i++)
        {
            UpdateSlotUI(i);
        }
    }

    // Get list of all items currently in toolbar (useful for saving/loading)
    public List<InventoryItem> GetAllToolbarItems()
    {
        List<InventoryItem> items = new List<InventoryItem>();
        for (int i = 0; i < toolbarSlots; i++)
        {
            if (IsSlotOccupied(i))
            {
                items.Add(slotContents[i]);
            }
        }
        return items;
    }

    // Clear all items from toolbar
    public void ClearToolbar()
    {
        for (int i = 0; i < toolbarSlots; i++)
        {
            if (IsSlotOccupied(i))
            {
                RemoveItemFromSlot(i);
            }
        }
        UnequipItem();
        Debug.Log("Toolbar cleared");
    }

    // Debug method to print toolbar contents
    public void PrintToolbarContents()
    {
        Debug.Log($"=== TOOLBAR ({toolbarSlots} slots) ===");
        for (int i = 0; i < toolbarSlots; i++)
        {
            if (IsSlotOccupied(i))
            {
                string equipped = (i == currentEquippedSlot) ? " [EQUIPPED]" : "";
                Debug.Log($"Slot {i + 1}: {slotContents[i].itemName}{equipped}");
            }
            else
            {
                Debug.Log($"Slot {i + 1}: Empty");
            }
        }
        Debug.Log($"Toolbar Usage: {GetItemCount()}/{toolbarSlots}");
    }

    public void ChangeSelectedSlot(int slotIndex)
    {
        // Raise selected slot
        for (int i = 0; i < toolbarSlotsUI.Length; i++)
        {
            //create variables
            Vector3 currentPos = toolbarSlotsUI[i].slotImage.transform.position;
            Vector3 slotTargetPos;

            Vector3 iconCurrentPos = toolbarSlotsUI[i].iconImage.transform.position;
            Vector3 iconTargetPos;

            if (i == slotIndex && currentEquippedItem.activeSelf)
            {
                // Raise selected slot
                slotTargetPos = new Vector3(currentPos.x, originalY + 10f, currentPos.z);
                iconTargetPos = new Vector3(iconCurrentPos.x, originalY + 10f, iconCurrentPos.z);
            }
            else
            {
                // Lower other slots to default position
                slotTargetPos = new Vector3(currentPos.x, originalY - 10f, currentPos.z);
                iconTargetPos = new Vector3(iconCurrentPos.x, originalY - 10f, iconCurrentPos.z);
            }

            // Smoothly move towards target
            toolbarSlotsUI[i].slotImage.transform.position = Vector3.Lerp(currentPos, slotTargetPos, Time.deltaTime * 1000f);
            toolbarSlotsUI[i].iconImage.transform.position = Vector3.Lerp(iconCurrentPos, iconTargetPos, Time.deltaTime * 1000f);
        }

        currentEquippedSlot = slotIndex;
    }
}