using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sliding : MonoBehaviour
{
    [Header("References")]
    public Transform orientation;
    public Transform playerObj;
    public PlayerCam cam;
    private Rigidbody rb;
    private PlayerMovementAdvanced pm;
    private PlayerStats ps;

    [Header("Sliding")]
    public float maxSlideTime;
    public float slideForce;
    private float slideTimer;
    private float entrySpeedCap;

    public float slideYScale;
    private float startYScale;

    [Header("Input")]
    public KeyCode slideKey = KeyCode.LeftControl;
    private float horizontalInput;
    private float verticalInput;


    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        pm = GetComponent<PlayerMovementAdvanced>();
        ps = GetComponent<PlayerStats>();

        startYScale = playerObj.localScale.y;
    }

    private void Update()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        if (Input.GetKeyDown(slideKey) && (horizontalInput != 0 || verticalInput != 0))
            StartSlide();

        if (Input.GetKeyUp(slideKey) && pm.sliding)
            StopSlide();

        if (ps.speed >= 50f)
            slideForce = 1f;
        else
            slideForce = 1.1f;
    }

    private void FixedUpdate()
    {
        if (pm.sliding)
            SlidingMovement();
    }

    private void StartSlide()
    {
        pm.sliding = true;
        entrySpeedCap = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z).magnitude;

        playerObj.localScale = new Vector3(playerObj.localScale.x, slideYScale, playerObj.localScale.z);
        rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);

        float randomValue = Random.Range(0f, 1f);
        if(randomValue < 0.5f)
        {
            cam.DoTilt(-5f, 0.25f);
        }
        else
        {
            cam.DoTilt(5f, 0.25f);
        }

        slideTimer = maxSlideTime;
    }

    private void SlidingMovement()
    {
        Vector3 inputDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        // sliding normal
        if(!pm.OnSlope() || rb.linearVelocity.y > -0.1f)
        {
            rb.AddForce(inputDirection.normalized * slideForce, ForceMode.Force);

            slideTimer -= Time.deltaTime;
        }

        // sliding down a slope
        else
        {
            rb.AddForce(pm.GetSlopeMoveDirection(inputDirection) * slideForce, ForceMode.Force);
        }

        Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        if (flatVel.magnitude > entrySpeedCap)
        {
            Vector3 limited = flatVel.normalized * entrySpeedCap;
            rb.linearVelocity = new Vector3(limited.x, rb.linearVelocity.y, limited.z);
        }

        if (slideTimer <= 0)
            StopSlide();
    }

    private void StopSlide()
    {
        pm.sliding = false;

        playerObj.localScale = new Vector3(playerObj.localScale.x, startYScale, playerObj.localScale.z);
        cam.DoTilt(0f, 0.25f);
    }
}
