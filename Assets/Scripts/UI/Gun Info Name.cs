using UnityEngine;
using TMPro;

public class GunInfoName : MonoBehaviour
{
    public EquipLogic equipLogic;

    void Start()
    {
        gameObject.GetComponent<TextMeshProUGUI>().text = null;
    }

    void Update()
    {
        GameObject weapon = equipLogic.currentWeapon;

        if (weapon != null)
            gameObject.GetComponent<TextMeshProUGUI>().text = weapon.name;
        else
            gameObject.GetComponent<TextMeshProUGUI>().text = null;
    }
}
