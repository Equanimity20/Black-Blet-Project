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
    // Start is called before the first frame update
    void Start()
    {
        currentHealth = maxHealth;
        pm.enabled = true;
    }

    // Update is called once per frame
    void Update()
    {
        speed = Vector3.Distance(transform.position, lastPosition) / Time.deltaTime; 
        lastPosition = transform.position;

        healthText.text = currentHealth.ToString(); ;

        if (damageValue > 0)
        {
            TakeDamage(damageValue);
            damageValue = 0;
        }

        if (currentHealth == 0)
        {
            Dead = true;
            pm.enabled = false;
        }
    }

    public void TakeDamage(float damageAmount)
    {
        currentHealth -= damageAmount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth); // Prevent health going below 0 or above max

        float randomNumber = Random.Range(0, 1f);
        
        if (randomNumber < 0.5f)
        {
            tiltAmount = 5f;
        }
        else
        {
            tiltAmount = -5f;
        }

        StartCoroutine(DamageTilt());
    }

    private IEnumerator DamageTilt()
    {
        cam.DoTilt(tiltAmount, 0.1f);
        yield return new WaitForSeconds(0.1f);
        cam.DoTilt(0f, 0.25f);
    }
}
