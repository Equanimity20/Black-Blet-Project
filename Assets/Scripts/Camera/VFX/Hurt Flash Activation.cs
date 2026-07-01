using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class HurtFlashActivation : MonoBehaviour
{
    public Volume volume;
    private Vignette vignette;
    public PlayerStats health;

    public float intensity;
    private float maxIntensity = 0.5f;
    private float healthValue;

    void Start()
    {
        if (volume.profile.TryGet(out vignette))
        {
            intensity = 0f;
        }
    }

    void Update()
    {
        if (vignette != null)
        {
            healthValue = health.currentHealth;
            intensity = maxIntensity * (1 - healthValue / health.maxHealth);
            vignette.intensity.Override(intensity);
        }
    }
}
