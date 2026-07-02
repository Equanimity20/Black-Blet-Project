using UnityEngine;
using System.Reflection;
using TMPro;

public class GunInfoBullets : MonoBehaviour
{
    public InventorySystem invSys;

    public int currentAmmo;
    public int reserveAmmo;

    public GameObject ammoIcon;

    private GameObject lastWeapon;
    private MonoBehaviour gunScript;
    private FieldInfo ammoField;
    private FieldInfo reserveField;

    private TextMeshProUGUI tmp; // Cached — was calling GetComponent multiple times per frame
    private int lastCurrentAmmo = -1;
    private int lastReserveAmmo = -1;

    private InventorySystem.InventoryItem item => invSys.slotContents[invSys.publicSlotIndex];

    void Start()
    {
        tmp = gameObject.GetComponent<TextMeshProUGUI>();
        tmp.text = null;
        ammoIcon.SetActive(false);
    }

    void Update()
    {
        GameObject weapon = invSys.CurrentEquippedItem;

        if (weapon != null && item != null)
        {
            if (weapon != lastWeapon && item.itemType == InventorySystem.ItemType.Gun)
            {
                lastWeapon = weapon;
                ammoIcon.SetActive(true);
                gunScript = null;
                ammoField = null;
                reserveField = null;
                lastCurrentAmmo = -1;
                lastReserveAmmo = -1;

                foreach (var script in weapon.GetComponents<MonoBehaviour>())
                {
                    var type = script.GetType();
                    var ca = type.GetField("currentAmmo");
                    var ra = type.GetField("reserveAmmo");

                    if (ca != null && ra != null)
                    {
                        gunScript = script;
                        ammoField = ca;
                        reserveField = ra;
                        break;
                    }
                }
            }
            else if (weapon != lastWeapon)
            {
                // Switched to a non-gun item
                lastWeapon = weapon;
                gunScript = null;
                ammoIcon.SetActive(false);
                tmp.text = null;
                lastCurrentAmmo = -1;
                lastReserveAmmo = -1;
            }

            if (gunScript != null)
            {
                int newCurrentAmmo = (int)ammoField.GetValue(gunScript);
                int newReserveAmmo = (int)reserveField.GetValue(gunScript);

                // Only rebuild the string when values actually change
                if (newCurrentAmmo != lastCurrentAmmo || newReserveAmmo != lastReserveAmmo)
                {
                    currentAmmo = newCurrentAmmo;
                    reserveAmmo = newReserveAmmo;
                    tmp.text = currentAmmo + " / " + reserveAmmo;
                    lastCurrentAmmo = newCurrentAmmo;
                    lastReserveAmmo = newReserveAmmo;
                }
            }
            else
            {
                if (tmp.text != null)
                    tmp.text = null;
            }
        }
        else if (lastWeapon != null)
        {
            // Weapon unequipped — clear UI once
            lastWeapon = null;
            gunScript = null;
            ammoIcon.SetActive(false);
            tmp.text = null;
        }
    }
}
