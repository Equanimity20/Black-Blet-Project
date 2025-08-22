using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class HurtFlashActivation : MonoBehaviour
{
    public PostProcessVolume volume;
    public Vignette vignette;
    public Health health;

    public float intensity;
    private float maxIntensity = 0.5f;
    private float healthValue;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        volume.profile.TryGetSettings(out vignette);

        if (vignette)
        {
            intensity = 0f;
        }
    }

    // Update is called once per frame
    void Update()
    {
        vignette.intensity.value = intensity;

        healthValue = health.currentHealth;

        intensity = maxIntensity * (1 - healthValue / health.maxHealth);
    }
}
