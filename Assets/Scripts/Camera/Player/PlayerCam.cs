using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Unity.VisualScripting;

public class PlayerCam : MonoBehaviour
{
    public float sensX;
    public float sensY;

    public Transform orientation;
    public Transform camHolder;

    public float xRotation;
    public float yRotation;

    public float bobFrequency;
    public float bobHeight;

    private float originalYPos;
    private Camera cam; // Cached — was calling GetComponent every DoFov call

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        originalYPos = camHolder.localPosition.y;
        cam = GetComponent<Camera>();
    }

    private void Update()
    {
        float mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * sensX;
        float mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * sensY;

        yRotation += mouseX;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        camHolder.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        orientation.rotation = Quaternion.Euler(0, yRotation, 0);
    }

    public void DoFov(float endValue, float duration)
    {
        cam.DOFieldOfView(endValue, duration);
    }

    public void DoTilt(float zTilt, float duration)
    {
        transform.DOLocalRotate(new Vector3(0, 0, zTilt), duration);
    }

    public void DoBobbing(float bobFrequency, float bobHeight)
    {
        gameObject.transform.localPosition = new Vector3(0, Mathf.Sin(bobFrequency * Time.time) * bobHeight, 0);
    }

    public void StopBobbing()
    {
        gameObject.transform.localPosition = new Vector3(0, originalYPos, 0);
    }

    public void DoShake(float duration, float strength)
    {
        StartCoroutine(Shake(duration, strength));
    }

    public IEnumerator Shake(float duration, float strength)
    {
        Transform target = this.transform;
        float elapsed = 0f;
        float strengthVelocity = 0f;
        Vector3 originalTransform = target.localPosition;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f);
            float y = Random.Range(-1f, 1f);
            float z = Random.Range(-1f, 1f);

            strength = Mathf.SmoothDamp(strength, 0f, ref strengthVelocity, 1.5f);
            target.localPosition = originalTransform + new Vector3(x, y, z) * strength;

            elapsed += Time.deltaTime;
            yield return null;
        }

        target.localPosition = originalTransform;
    }
}
