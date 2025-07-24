using UnityEngine;

public class EquipLogic : MonoBehaviour
{
    public Transform cameraTransform; // Main camera
    public Vector3 positionOffset = new Vector3(0.5f, -0.5f, 1.0f);
    public Vector3 rotationOffset = Vector3.zero;
    public GameObject weaponPrefab; // Default weapon prefab
    public GameObject currentWeapon;

    // Call this to equip a weapon prefab
    public void EquipWeapon(GameObject weaponPrefab)
    {
        // Remove current weapon if any
        if (currentWeapon != null)
        {
            Destroy(currentWeapon);
        }

        // Spawn new weapon
        currentWeapon = Instantiate(weaponPrefab, cameraTransform);
        currentWeapon.name = currentWeapon.name.Replace("(Clone)", "").Trim();
        currentWeapon.transform.localPosition = positionOffset;
        currentWeapon.transform.localEulerAngles = rotationOffset;
    }

    // Call this to unequip current weapon
    public void UnequipWeapon()
    {
        if (currentWeapon != null)
        {
            Destroy(currentWeapon);
            currentWeapon = null;
        }
    }
}
