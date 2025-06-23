using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthBar : MonoBehaviour
{
    public GameObject healthBar;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    void HealthBarLogic(float damageValue)
    {
        RectTransform rectTransform = healthBar.GetComponent<RectTransform>();
        float newWidth = Mathf.Max(0, rectTransform.sizeDelta.x - damageValue);
        rectTransform.sizeDelta = new Vector2(newWidth, rectTransform.sizeDelta.y);
    }
}
