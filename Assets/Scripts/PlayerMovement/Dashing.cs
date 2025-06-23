using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dashing : MonoBehaviour
{
    [Header("References")]
    public Transform orientation;
    public Transform playerCam;
    private Rigidbody rb;
    private Collider col;
    private PhysicMaterial mat;
    private PlayerMovementAdvanced pm;

    [Header("Dashing")]
    public float dashForce;
    public float dashUpwardForce;
    public float maxDashYSpeed;
    public float dashDuration;

    [Header("CameraEffects")]
    public PlayerCam cam;
    public float dashFov;

    [Header("Settings")]
    public bool useCameraForward = true;
    public bool allowAllDirections = true;
    public bool disableGravity = false;
    public bool resetVel = true;

    [Header("Cooldown")]
    public float dashCd;
    private float dashCdTimer;

    [Header("Input")]
    public KeyCode dashKey = KeyCode.E;

    private float originalDrag;
    private float originalDynamicFriction;
    private float originalStaticFriction;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        pm = GetComponent<PlayerMovementAdvanced>();
        col = GetComponentInChildren<Collider>();
        mat = col.material;
        originalDrag = rb.drag;
        originalDynamicFriction = mat.dynamicFriction;
        originalStaticFriction = mat.staticFriction;
    }

    private void Update()
    {
        if (Input.GetKeyDown(dashKey))
            Dash();

        if (dashCdTimer > 0)
            dashCdTimer -= Time.deltaTime;
    }

    private void Dash()
    {
        if (dashCdTimer > 0)
            return;

        dashCdTimer = dashCd;
        rb.drag = 0f;
        mat.dynamicFriction = 0f;
        mat.staticFriction = 0f;

        pm.dashing = true;
        pm.maxYSpeed = maxDashYSpeed;

        cam.DoFov(dashFov, 0.25f);

        Vector3 inputDirection = GetInputDirectionFromCamera();
        if (inputDirection.magnitude < 0.1f)
            inputDirection = orientation.forward;

        if (disableGravity)
            rb.useGravity = false;

        Vector3 dashDirection = inputDirection.normalized;
        Vector3 forceToApply = dashDirection * dashForce;
        
        // Air dash: use AddForce
        rb.AddForce(forceToApply, ForceMode.VelocityChange);

        Invoke(nameof(ResetDash), dashDuration);
    }

    private Vector3 delayedForceToApply;
    private void DelayedDashForce()
    {
        if (resetVel)
            rb.velocity = Vector3.zero;

        rb.AddForce(delayedForceToApply, ForceMode.Impulse);
    }

    private void ResetDash()
    {
        rb.drag = originalDrag;
        mat.dynamicFriction = originalDynamicFriction;
        mat.staticFriction = originalStaticFriction;

        pm.dashing = false;
        pm.maxYSpeed = 0;

        cam.DoFov(80f, 0.25f);

        if (disableGravity)
            rb.useGravity = true;
    }

   private Vector3 GetInputDirectionFromCamera()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        // Get forward and right vectors
        Vector3 camForward = playerCam.forward;
        Vector3 camRight = playerCam.right;

        // Zero out vertical tilt only if player is NOT pressing vertical input
        if (vertical == 0)
            camForward.y = 0f;

        camForward.Normalize();
        camRight.y = 0f;
        camRight.Normalize();

        // Combine input
        Vector3 direction = (camForward * vertical + camRight * horizontal);

        // Allow vertical dashing if player is looking strongly up or down
        if (vertical != 0)
        {
            float verticalLookDot = Vector3.Dot(playerCam.forward.normalized, Vector3.up);
            
            // Only allow strong vertical dashes if camera is tilted up/down enough
            if (Mathf.Abs(verticalLookDot) > 0.75f)
            {
                direction = playerCam.forward * vertical + playerCam.right * horizontal;
            }
        }

        return direction.normalized;
    }
}