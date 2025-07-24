using System.Collections.Generic;
using UnityEngine;

public class WeaponTestInput : MonoBehaviour
{
    public EquipLogic equipLogic;
    public GameObject pistolPrefab;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            equipLogic.EquipWeapon(pistolPrefab);
        }

        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            equipLogic.UnequipWeapon();
        }
    }
}
