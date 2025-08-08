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
        if (equipLogic.CurrentWeapon != null)
            gameObject.GetComponent<TextMeshProUGUI>().text = equipLogic.CurrentWeaponName;
        else
            gameObject.GetComponent<TextMeshProUGUI>().text = null;
    }
}
