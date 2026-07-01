using System;
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
        public Sprite iconSprite;
        public ItemType itemType;
        [HideInInspector] public GameObject instantiatedObject;
        [HideInInspector] public int slotIndex = -1;
        [HideInInspector] public bool newItem = false;
    }

    [Header("Toolbar Settings")]
    [SerializeField] private int toolbarSlots = 9;

    [Header("UI References")]
    public Transform toolbarParent;

    [Serializable]
    public class ToolbarSlotUI
    {
        public RawImage slotImage;
        public Image iconImage;
    }
    public ToolbarSlotUI[] toolbarSlotsUI;
    private float originalY;

    [Header("Available Items")]
    public List<InventoryItem> availableItems = new();

    [Header("Equipment Settings")]
    public Transform equipmentParent;
    public Vector3 positionOffset = new Vector3(0.5f, -0.5f, 1.0f);
    public Vector3 rotationOffset = Vector3.zero;
    public enum ItemType { Gun, Melee, Throwable, Powerup }

    private GameObject[] toolbarSlotObjects;
    public InventoryItem[] slotContents;
    [HideInInspector] public int publicSlotIndex;

    private GameObject currentEquippedItem;
    private int currentEquippedSlot = -1;

    [Header("External Access")]
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
            toolbarSlotsUI[i].iconImage.color = new Color(1f, 1f, 1f, 0f);
        }
    }

    private void InitializeToolbar()
    {
        toolbarSlotObjects = new GameObject[toolbarSlots];
        slotContents = new InventoryItem[toolbarSlots];
    }

    private void InitializeItemIcons()
    {
        if (toolbarSlotsUI != null)
        {
            for (int i = 0; i < toolbarSlotsUI.Length && i < toolbarSlots; i++)
            {
                if (toolbarSlotsUI[i].iconImage != null)
                {
                    toolbarSlotsUI[i].iconImage.sprite = null;
                    Color slotColor = toolbarSlotsUI[i].iconImage.color;
                    slotColor.a = 0.3f;
                    toolbarSlotsUI[i].iconImage.color = slotColor;
                }
            }
        }
    }

    private void HandleInput()
    {
        if (Input.inputString.Length > 0)
        {
            char inputChar = Input.inputString[0];

            if (char.IsDigit(inputChar))
            {
                int keyPressed = inputChar == '0' ? 10 : (inputChar - '0');
                int slotIndex = keyPressed - 1;

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
            if (currentEquippedSlot == slotIndex)
            {
                UnequipItem();
                ChangeSelectedSlot(-1);
            }
            else
            {
                EquipItemFromSlot(slotIndex);
            }
        }
    }

    public bool AddItemToSlot(InventoryItem item, int slotIndex)
    {
        if (!IsValidSlotIndex(slotIndex) || IsSlotOccupied(slotIndex))
            return false;
        
        slotContents[slotIndex] = item;
        item.slotIndex = slotIndex;
        
        if (item.instantiatedObject != null)
        {
            Destroy(item.instantiatedObject);
        }
        
        item.instantiatedObject = Instantiate(item.prefab);
        item.instantiatedObject.SetActive(false);

        UpdateSlotUI(slotIndex);

        return true;
    }

    public InventoryItem CreateInventoryItem(GameObject prefab, Sprite iconSprite, string itemName, ItemType itemType)
    {
        InventoryItem newItem = new InventoryItem();
        newItem.prefab = prefab;
        newItem.iconSprite = iconSprite;
        newItem.itemName = itemName;
        newItem.itemType = itemType;
        newItem.slotIndex = -1;
        newItem.newItem = true;

        return newItem;
    }

    public InventoryItem CreateInventoryItem(GameObject prefab, Sprite iconSprite, ItemType itemType)
    {
        string itemName = prefab != null ? prefab.name : "Unknown Item";
        return CreateInventoryItem(prefab, iconSprite, itemName, itemType);
    }

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
        return false;
    }

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

        if (slotIndex == currentEquippedSlot)
            UnequipItem();

        UpdateSlotUI(slotIndex);

        return item;
    }

    public bool DropItemFromSlot(int slotIndex)
    {
        InventoryItem item = RemoveItemFromSlot(slotIndex);
        if (item != null)
        {
            GameObject droppedItem = Instantiate(item.prefab, Camera.main.transform.position + Camera.main.transform.forward * 1.5f, Quaternion.identity);
            
            var pickupScript = droppedItem.GetComponent<IPickUpItem>();
            if (pickupScript != null)
            {
                MonoBehaviour scriptComponent = pickupScript as MonoBehaviour;
                if (scriptComponent != null)
                {
                    var prefabField = scriptComponent.GetType().GetField("PrefabgameObject");
                    if (prefabField != null)
                    {
                        prefabField.SetValue(scriptComponent, item.prefab);
                    }
                    
                    var iconField = scriptComponent.GetType().GetField("Icon");
                    if (iconField != null)
                    {
                        iconField.SetValue(scriptComponent, item.iconSprite);
                    }
                }
            }
            
            Vector3 throwForce = Camera.main.transform.forward * 1.5f + Vector3.up * 1f + UnityEngine.Random.insideUnitSphere;
            Vector3 TorqueForce = new Vector3(UnityEngine.Random.Range(-10f, 10f), UnityEngine.Random.Range(-10f, 10f), UnityEngine.Random.Range(-10f, 10f));
            droppedItem.GetComponent<Rigidbody>().AddTorque(TorqueForce, ForceMode.Impulse);
            droppedItem.GetComponent<Rigidbody>().AddForce(throwForce, ForceMode.Impulse);
            return true;
        }
        return false;
    }

    public bool EquipItemFromSlot(int slotIndex)
    {
        if (!IsValidSlotIndex(slotIndex) || !IsSlotOccupied(slotIndex))
            return false;

        InventoryItem item = slotContents[slotIndex];

        if (currentEquippedSlot == slotIndex)
            return true;
        
        UpdateAllSlotsUI();
        UnequipItem();

        // Get rotation from the instantiated object rather than prefab —
        // prefab may be a destroyed scene reference (Unity's null check returns true for destroyed objects)
        rotationOffset = (item.instantiatedObject != null && item.instantiatedObject.GetComponent<IPickUpItem>() != null)
            ? item.instantiatedObject.GetComponent<IPickUpItem>().SetProperEquipOrientation()
            : Vector3.zero;

        // FIX: was "item.prefab != null" — if PrefabgameObject pointed to a scene object that was
        // destroyed after pickup, Unity's null check returns true and this block was silently skipped,
        // preventing the item from ever being equipped. Use item.instantiatedObject instead,
        // which is always a live scene object created by AddItemToSlot.
        if (item.instantiatedObject != null && equipmentParent != null)
        {
            currentEquippedItem = item.instantiatedObject;
            currentEquippedItem.SetActive(true);
            currentEquippedItem.transform.SetParent(equipmentParent);

            var rb = currentEquippedItem.GetComponent<Rigidbody>();
            if (rb != null) rb.isKinematic = true;

            foreach (var collider in currentEquippedItem.GetComponents<Collider>())
                collider.enabled = false;

            currentEquippedItem.name = item.instantiatedObject.name;
            currentEquippedItem.transform.localEulerAngles = rotationOffset;
            currentEquippedItem.transform.localPosition = positionOffset;
            currentEquippedSlot = slotIndex;
        }

        UpdateAllSlotsUI();
        ChangeSelectedSlot(slotIndex);

        return true;
    }

    public void UnequipItem()
    {
        if (currentEquippedItem != null)
            currentEquippedItem.SetActive(false);

        currentEquippedItem = null;
        currentEquippedSlot = -1;

        UpdateAllSlotsUI();
        ResetToolbarHeights();
    }

    public bool MoveItem(int fromSlot, int toSlot)
    {
        if (!IsValidSlotIndex(fromSlot) || !IsValidSlotIndex(toSlot))
            return false;

        if (!IsSlotOccupied(fromSlot))
            return false;

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
            slotContents[toSlot] = slotContents[fromSlot];
            slotContents[fromSlot] = null;
            slotContents[toSlot].slotIndex = toSlot;

            Debug.Log($"Moved {slotContents[toSlot].itemName} from slot {fromSlot + 1} to slot {toSlot + 1}");
        }

        if (currentEquippedSlot == fromSlot)
            currentEquippedSlot = toSlot;
        else if (currentEquippedSlot == toSlot && IsSlotOccupied(fromSlot))
            currentEquippedSlot = fromSlot;

        UpdateSlotUI(fromSlot);
        UpdateSlotUI(toSlot);

        return true;
    }

    public bool IsValidSlotIndex(int slotIndex)
    {
        return slotIndex >= 0 && slotIndex < toolbarSlots;
    }

    public bool IsSlotOccupied(int slotIndex)
    {
        return IsValidSlotIndex(slotIndex) && slotContents[slotIndex] != null && slotContents[slotIndex].itemName != null;
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
        return -1;
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

    public bool CanPickupItem()
    {
        return !IsToolbarFull();
    }

    private void UpdateSlotUI(int slotIndex)
    {
        if (toolbarSlotsUI == null || slotIndex < 0 || slotIndex >= toolbarSlotsUI.Length)
            return;

        ToolbarSlotUI slot = toolbarSlotsUI[slotIndex];

        if (IsSlotOccupied(slotIndex))
        {
            InventoryItem item = slotContents[slotIndex];
            slot.iconImage.sprite = item.iconSprite;
            slot.iconImage.color = new Color(1f, 1f, 1f, slotIndex == currentEquippedSlot ? 1f : 0.7f);
        }
        else
        {
            slot.iconImage.sprite = null;
            slot.iconImage.color = new Color(1f, 1f, 1f, 0f);
        }
    }

    private void UpdateAllSlotsUI()
    {
        for (int i = 0; i < toolbarSlotsUI.Length; i++)
        {
            UpdateSlotUI(i);
        }
    }

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
    }

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
        for (int i = 0; i < toolbarSlotsUI.Length; i++)
        {
            Vector3 currentPos = toolbarSlotsUI[i].slotImage.transform.position;
            Vector3 slotTargetPos;

            Vector3 iconCurrentPos = toolbarSlotsUI[i].iconImage.transform.position;
            Vector3 iconTargetPos;

            List<InventoryItem> item = new List<InventoryItem>(slotContents)
            {
                slotContents[i]
            };

            if(i == slotIndex)
            {
                if (currentEquippedItem == item[i]?.instantiatedObject)
                {
                    slotTargetPos = new Vector3(currentPos.x, originalY + 10f, currentPos.z);
                    iconTargetPos = new Vector3(iconCurrentPos.x, originalY + 10f, iconCurrentPos.z);
                }
                else
                {
                    slotTargetPos = new Vector3(currentPos.x, originalY, currentPos.z);
                    iconTargetPos = new Vector3(iconCurrentPos.x, originalY, iconCurrentPos.z);
                }
            
                toolbarSlotsUI[i].slotImage.transform.position = Vector3.Lerp(currentPos, slotTargetPos, Time.deltaTime * 1000f);
                toolbarSlotsUI[i].iconImage.transform.position = Vector3.Lerp(iconCurrentPos, iconTargetPos, Time.deltaTime * 1000f);
            }
        }

        currentEquippedSlot = slotIndex;
    }

    void ResetToolbarHeights()
    {
        for (int i = 0; i < toolbarSlotsUI.Length; i++)
        {
            Vector3 currentPos = toolbarSlotsUI[i].slotImage.transform.position;
            Vector3 slotTargetPos = new Vector3(currentPos.x, originalY, currentPos.z);
            toolbarSlotsUI[i].slotImage.transform.position = Vector3.Lerp(currentPos, slotTargetPos, Time.deltaTime * 1000f);

            Vector3 iconCurrentPos = toolbarSlotsUI[i].iconImage.transform.position;
            Vector3 iconTargetPos = new Vector3(iconCurrentPos.x, originalY, iconCurrentPos.z);
            toolbarSlotsUI[i].iconImage.transform.position = Vector3.Lerp(iconCurrentPos, iconTargetPos, Time.deltaTime * 1000f);
        }
    }
}