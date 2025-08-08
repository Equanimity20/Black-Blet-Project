using UnityEngine;

public class WeaponTestInput : MonoBehaviour
{
    public EquipLogic equipLogic;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
            equipLogic.EquipWeapon(EquipLogic.WeaponSlot.Primary);
        if (Input.GetKeyDown(KeyCode.Alpha2))
            equipLogic.EquipWeapon(EquipLogic.WeaponSlot.Secondary);
        if (Input.GetKeyDown(KeyCode.Alpha3))
            equipLogic.EquipWeapon(EquipLogic.WeaponSlot.Melee);
        if (Input.GetKeyDown(KeyCode.Alpha4))
            equipLogic.EquipWeapon(EquipLogic.WeaponSlot.Utility);
    }
}
