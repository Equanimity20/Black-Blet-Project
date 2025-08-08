using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using UnityEngine;

public class SilencedPistolLogic : MonoBehaviour
{
    [Header("References")]
    public EquipLogic equipLogic;
    public GameObject Camera;
    public Vector3 bulletVector;

    [Header("Weapon Settings")]
    public float damage = 10f;
    public float reloadTime = 1.5f;
    public int maxAmmoReserve = 60;
    public int maxAmmoClip = 10;
    public int currentAmmo;
    public int reserveAmmo;
    public bool isReloading = false;

    [Header("Visuals")]
    public GameObject MuzzleFlash;

    // Start is called before the first frame update
    void Start()
    {
        currentAmmo = maxAmmoClip;
        reserveAmmo = maxAmmoReserve;
        MuzzleFlash.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (equipLogic.CurrentWeapon == gameObject)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Shoot();
            }
            if (Input.GetKeyDown(KeyCode.R))
            {
                Reload();
            }
        }

    }

    void Shoot()
    {
        if (currentAmmo > 0)
        {
            MuzzleFlash.SetActive(true);
            Invoke("DisableMuzzleFlash", 0.1f);
            currentAmmo--;
            bulletVector = Camera.transform.forward;
            if (Physics.Raycast(Camera.transform.position, Camera.transform.forward, out RaycastHit hit, 500f))
            {
                Debug.DrawRay(Camera.transform.position, Camera.transform.forward * 500f, Color.red, 1f);
                if (hit.collider != null)
                {
                    var damageable = hit.collider.GetComponentInParent<IDamageable>();
                    if (damageable != null)
                    {
                        damageable.TakeDamage(damage, bulletVector);
                    }
                }
            }
        }
        else if (currentAmmo == 0 && reserveAmmo > 0 && !isReloading)
        {
            isReloading = true;
            StartCoroutine(ReloadCoroutine());
        }
    }

    void DisableMuzzleFlash()
    {
        MuzzleFlash.SetActive(false);
    }

    void Reload()
    {
        if (currentAmmo < maxAmmoClip && !isReloading && reserveAmmo > 0)
        {
            isReloading = true;
            StartCoroutine(ReloadCoroutine());
        }
    }

    IEnumerator ReloadCoroutine()
    {
        yield return new WaitForSeconds(reloadTime);
        int ReserveSubtractAmount = maxAmmoClip - currentAmmo;
        int ammoToReload = Mathf.Min(ReserveSubtractAmount, reserveAmmo);
        currentAmmo += ammoToReload;
        reserveAmmo -= ammoToReload;
        isReloading = false;
    }
}
