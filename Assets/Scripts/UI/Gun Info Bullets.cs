using UnityEngine;
using System.Reflection;
using TMPro;

public class GunInfoBullets : MonoBehaviour
{
    public EquipLogic equipLogic;

    public int currentAmmo;
    public int reserveAmmo;

    private GameObject lastWeapon;
    private MonoBehaviour gunScript;
    private FieldInfo ammoField;
    private FieldInfo reserveField;

    void Update()
    {
        GameObject weapon = equipLogic.CurrentWeapon;

        // Only update if weapon changed
        if (weapon != lastWeapon)
        {
            lastWeapon = weapon;
            gunScript = null;
            ammoField = null;
            reserveField = null;

            if (weapon != null)
            {
                // Find the first custom script with currentAmmo and reserveAmmo
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
        }

        // Sync ammo values if we found a gun script
        if (gunScript != null)
        {
            // Read values from weapon script
            currentAmmo = (int)ammoField.GetValue(gunScript);
            reserveAmmo = (int)reserveField.GetValue(gunScript);

            gameObject.GetComponent<TextMeshProUGUI>().text = currentAmmo + " / " + reserveAmmo;
        }
    }
}
