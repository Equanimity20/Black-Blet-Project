using System.Collections.Generic;
using UnityEngine;

public class EquipLogic : MonoBehaviour
{
    public enum WeaponSlot { Primary, Secondary, Melee, Utility }

    [System.Serializable]
    public class WeaponEntry
    {
        public WeaponSlot slot;
        public GameObject prefab;
    }

    [Header("Weapon Slots")]
    public List<WeaponEntry> weaponList = new List<WeaponEntry>();

    public Transform cameraTransform;
    public Vector3 positionOffset = new Vector3(0.5f, -0.5f, 1.0f);
    public Vector3 rotationOffset = Vector3.zero;

    private Dictionary<WeaponSlot, GameObject> weaponPrefabs;
    private GameObject currentWeapon;
    private WeaponSlot? currentSlot = null;

    // Add this property to allow other scripts to check what's equipped:
    public GameObject CurrentWeapon => currentWeapon;
    public WeaponSlot? CurrentSlot => currentSlot;
    public string CurrentWeaponName => currentWeapon != null ? currentWeapon.name : "None";

    void Awake()
    {
        weaponPrefabs = new Dictionary<WeaponSlot, GameObject>();
        foreach (var entry in weaponList)
        {
            if (entry.prefab != null && !weaponPrefabs.ContainsKey(entry.slot))
                weaponPrefabs.Add(entry.slot, entry.prefab);
        }
    }

    public void EquipWeapon(WeaponSlot slot, WeaponEntry prefab = null)
    {
        if (!weaponPrefabs.ContainsKey(slot))
        {
            return;
        }

        if (currentSlot == slot) return; // Already equipped

        UnequipWeapon();

        var prefabClone = weaponPrefabs[slot];
        currentSlot = slot;
        currentWeapon = Instantiate(prefabClone, cameraTransform);
        currentWeapon.name = prefabClone.name;
        currentWeapon.transform.localPosition = positionOffset;
        currentWeapon.transform.localEulerAngles = rotationOffset;
    }

    public void UnequipWeapon()
    {
        if (currentWeapon != null)
            Destroy(currentWeapon);

        currentWeapon = null;
        currentSlot = null;
    }
}
