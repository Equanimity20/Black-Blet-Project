using UnityEngine;
using TMPro;
using System.Collections;

public class PlayerStats : MonoBehaviour
{
    public float speed = 0f;
    private Vector3 lastPosition;

    [Header("Health")]
    public float maxHealth = 100f;
    public float currentHealth;
    private float lastHealth = -1f; // Track changes — only update UI when health changes

    [Header("Damage Value")]
    public float damageValue;

    [Header("References")]
    public TextMeshProUGUI healthText;
    public PlayerMovementAdvanced pm;
    public PlayerCam cam;

    [Header("Respawn/Death")]
    public bool Dead;

    [Header("Damage Tilt")]
    float tiltAmount;

    void Start()
    {
        currentHealth = maxHealth;
        pm.enabled = true;
        UpdateHealthText();
    }

    void Update()
    {
        speed = Vector3.Distance(transform.position, lastPosition) / Time.deltaTime;
        lastPosition = transform.position;

        // Only rebuild the string and set text when health actually changes
        if (currentHealth != lastHealth)
        {
            UpdateHealthText();
            lastHealth = currentHealth;
        }

        if (damageValue > 0)
        {
            TakeDamage(damageValue);
            damageValue = 0;
        }

        if (currentHealth == 0 && !Dead)
        {
            Dead = true;
            pm.enabled = false;
        }
    }

    private void UpdateHealthText()
    {
        healthText.text = Mathf.CeilToInt(currentHealth).ToString();
    }

    public void TakeDamage(float damageAmount)
    {
        currentHealth -= damageAmount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        tiltAmount = Random.Range(0, 1f) < 0.5f ? 5f : -5f;

        StartCoroutine(DamageTilt());
    }

    private IEnumerator DamageTilt()
    {
        cam.DoTilt(tiltAmount, 0.1f);
        yield return new WaitForSeconds(0.1f);
        cam.DoTilt(0f, 0.25f);
    }
}
