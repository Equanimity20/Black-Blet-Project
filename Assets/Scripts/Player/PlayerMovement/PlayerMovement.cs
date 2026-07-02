using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovementAdvanced : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed;
    public float walkSpeed;
    public float sprintSpeed;
    public float dashSpeed;
    public float dashSpeedChangeFactor;
    public float slideSpeed;
    public float wallrunSpeed;
    public float climbSpeed;

    public float maxYSpeed;

    public float groundDrag;

    [Header("Jumping")]
    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    bool readyToJump;

    [Header("Crouching")]
    public float crouchSpeed;
    public float crouchYScale;
    private float startYScale;

    [Header("Coyote Time")]
    public float coyoteTime = 0.2f;
    private float coyoteTimeCounter;

    [Header("Jump Buffer")]
    public float jumpBufferTime = 0.2f;
    private float jumpBufferCounter;

    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.LeftShift;
    public KeyCode crouchKey = KeyCode.LeftControl;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsGround;
    public bool grounded;

    [Header("Slope Handling")]
    public float maxSlopeAngle;
    private RaycastHit slopeHit;
    private bool exitingSlope;

    // Cached slope result - computed once per Update, reused everywhere
    private bool cachedOnSlope;

    [Header("References")]
    public Transform orientation;

    float horizontalInput;
    float verticalInput;

    Vector3 moveDirection;
    public Rigidbody rb;
    public PlayerCam cam;

    [Header("State")]
    public MovementState state;
    public enum MovementState
    {
        idle,
        walking,
        sprinting,
        wallrunning,
        climbing,
        crouching,
        dashing,
        sliding,
        air
    }

    public bool sliding;
    public bool wallrunning;
    public bool climbing;
    public bool dashing;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        readyToJump = true;
        cam.DoFov(80f, 0.1f);

        startYScale = transform.localScale.y;
    }

    private void Update()
    {
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f, whatIsGround);

        // Cache OnSlope once per frame — used by SpeedControl, MovePlayer, StateHandler
        cachedOnSlope = ComputeOnSlope();

        MyInput();
        StateHandler();
        ViewBobbing();

        if (state != MovementState.dashing && state != MovementState.wallrunning && state != MovementState.sliding)
            SpeedControl();

        if (state == MovementState.walking || state == MovementState.sprinting || state == MovementState.crouching)
            rb.linearDamping = groundDrag;
        else
            rb.linearDamping = 0;

        if (grounded)
            coyoteTimeCounter = coyoteTime;
        else
            coyoteTimeCounter -= Time.deltaTime;

        if (state != lastState)
            cam.DoFov(80, 0.25f);
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    private void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        if (Input.GetKeyDown(jumpKey))
            jumpBufferCounter = jumpBufferTime;
        else
            jumpBufferCounter -= Time.deltaTime;

        if (jumpBufferCounter > 0 && coyoteTimeCounter > 0 && readyToJump)
        {
            readyToJump = false;
            Jump();
            Invoke(nameof(ResetJump), jumpCooldown);
            jumpBufferCounter = 0f;
        }

        if (Input.GetKeyDown(crouchKey))
        {
            transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
            rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
        }

        if (Input.GetKeyUp(crouchKey))
            transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
    }

    private float desiredMoveSpeed;
    private float lastDesiredMoveSpeed;
    private MovementState lastState;
    private bool keepMomentum;

    private void StateHandler()
    {
        if (dashing)
        {
            state = MovementState.dashing;
            desiredMoveSpeed = dashSpeed;
            speedChangeFactor = dashSpeedChangeFactor;
        }
        else if (climbing)
        {
            state = MovementState.climbing;
            desiredMoveSpeed = climbSpeed;
        }
        else if (wallrunning)
        {
            state = MovementState.wallrunning;
            desiredMoveSpeed = wallrunSpeed;
        }

        if (sliding)
        {
            state = MovementState.sliding;
            desiredMoveSpeed = (cachedOnSlope && rb.linearVelocity.y < 0.1f) ? slideSpeed : sprintSpeed;
        }
        else if (Input.GetKey(crouchKey))
        {
            state = MovementState.crouching;
            cam.DoFov(75f, 0.25f);
            desiredMoveSpeed = crouchSpeed;
        }
        else if (grounded && Input.GetKey(sprintKey))
        {
            state = MovementState.sprinting;
            cam.DoFov(85f, 0.25f);
            desiredMoveSpeed = sprintSpeed;
        }
        else if (grounded && (horizontalInput != 0 || verticalInput != 0))
        {
            state = MovementState.walking;
            desiredMoveSpeed = walkSpeed;
        }
        else if (grounded && horizontalInput == 0 && verticalInput == 0)
        {
            state = MovementState.idle;
            desiredMoveSpeed = 0;
        }
        else
        {
            state = MovementState.air;
            desiredMoveSpeed = (desiredMoveSpeed < sprintSpeed) ? walkSpeed : sprintSpeed;
        }

        bool desiredMoveSpeedHasChanged = desiredMoveSpeed != lastDesiredMoveSpeed;
        if (lastState == MovementState.dashing)
            keepMomentum = true;

        if (desiredMoveSpeedHasChanged)
        {
            StopAllCoroutines();
            if (keepMomentum)
                StartCoroutine(SmoothlyLerpMoveSpeed());
            else
                moveSpeed = desiredMoveSpeed;
        }

        lastDesiredMoveSpeed = desiredMoveSpeed;
        lastState = state;

        if (Mathf.Abs(desiredMoveSpeed - lastDesiredMoveSpeed) > 4f && moveSpeed != 0)
        {
            StopAllCoroutines();
            StartCoroutine(SmoothlyLerpMoveSpeed());
        }
        else
        {
            moveSpeed = desiredMoveSpeed;
        }

        lastDesiredMoveSpeed = desiredMoveSpeed;
    }

    private float speedChangeFactor;
    private IEnumerator SmoothlyLerpMoveSpeed()
    {
        float time = 0;
        float difference = Mathf.Abs(desiredMoveSpeed - moveSpeed);
        float startValue = moveSpeed;
        float boostFactor = speedChangeFactor;

        while (time < difference)
        {
            moveSpeed = Mathf.Lerp(startValue, desiredMoveSpeed, time / difference);
            time += Time.deltaTime * boostFactor;
            yield return null;
        }

        moveSpeed = desiredMoveSpeed;
        speedChangeFactor = 1f;
        keepMomentum = false;
    }

    private void MovePlayer()
    {
        if (state == MovementState.dashing)
            return;

        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        // Use cached slope result — no extra raycast here
        if (cachedOnSlope && !exitingSlope)
        {
            rb.AddForce(GetSlopeMoveDirection(moveDirection) * moveSpeed * 20f, ForceMode.Force);

            if (rb.linearVelocity.y > 0)
                rb.AddForce(Vector3.down * 80f, ForceMode.Force);
        }
        else if (grounded)
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
        }
        else
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);
        }

        rb.useGravity = !cachedOnSlope;
    }

    private void SpeedControl()
    {
        // Use cached slope result — no extra raycast here
        if (cachedOnSlope && !exitingSlope)
        {
            if (rb.linearVelocity.magnitude > moveSpeed)
                rb.linearVelocity = rb.linearVelocity.normalized * moveSpeed;
        }
        else
        {
            Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            if (flatVel.magnitude > moveSpeed)
            {
                Vector3 limitedVel = flatVel.normalized * moveSpeed;
                rb.linearVelocity = new Vector3(limitedVel.x, rb.linearVelocity.y, limitedVel.z);
            }
        }

        if (maxYSpeed != 0 && rb.linearVelocity.y > maxYSpeed)
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, maxYSpeed, rb.linearVelocity.z);
    }

    private void Jump()
    {
        exitingSlope = true;
        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    private void ResetJump()
    {
        readyToJump = true;
        exitingSlope = false;
    }

    // Renamed to ComputeOnSlope — only called once per frame from Update
    private bool ComputeOnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f + 0.3f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }
        return false;
    }

    // Public accessor uses the cached value — no extra raycast
    public bool OnSlope()
    {
        return cachedOnSlope;
    }

    public Vector3 GetSlopeMoveDirection(Vector3 direction)
    {
        return Vector3.ProjectOnPlane(direction, slopeHit.normal).normalized;
    }

    public void ViewBobbing()
    {
        if (state == MovementState.walking || state == MovementState.sprinting || state == MovementState.wallrunning || state == MovementState.climbing)
        {
            cam.bobFrequency = 14f;
            cam.bobHeight = 0.05f;
            cam.DoBobbing(cam.bobFrequency, cam.bobHeight);
        }
        else if (state == MovementState.crouching)
        {
            cam.bobFrequency = 8f;
            cam.bobHeight = 0.025f;
            cam.DoBobbing(cam.bobFrequency, cam.bobHeight);
        }
        else
        {
            cam.StopBobbing();
        }
    }
}
