using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public Rigidbody rb;
    public float speed = 10f;
    public bool Grounded;
    Vector2 rotation = Vector2.zero;
    [Header("Camera")]
    public Camera cam;
    public Vector3 HeadCameraOffset = new Vector3(0, 1, 0);
    public float Sensitivity = 2f;
    [Header("Jump")]
    public float jumpHeight = 5f;
    public float jumpStrength = 5f;
    public bool canJump = true;
    [Header("Coyote Time")]
    public float coyoteTime = 0.2f;
    private float coyoteTimer = 0f;
    [Header("Input Buffer")]
    private float jumpBufferTime = 0.1f;
    private float jumpBufferCounter = 0f;
    [Header("Slide")]
    private bool isSliding = false;
    private float slideSpeed = 15f; // Initial slide speed
    private float slideSpeedDecay = 5f; // How quickly sliding slows down
    private float currentSlideSpeed;
    public float slideSpeedGain = 1.5f; // New variable for slide velocity gain

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 move;
        int w, a, s, d;
        if(Input.GetKey("w")) w = 1; else w = 0;
        if(Input.GetKey("a")) a = 1; else a = 0;
        if(Input.GetKey("s")) s = 1; else s = 0;
        if(Input.GetKey("d")) d = 1; else d = 0;

        move.y = w - s;
        move.x = d - a;

        // Check if two movement keys are pressed
        float currentSpeed = speed;
        if ((w == 1 || s == 1) && (a == 1 || d == 1))
        {
            currentSpeed = speed * 1.2f; // 20% speed boost when moving diagonally
        }

        rotation.y += Input.GetAxis("Mouse X") * Sensitivity;
        rotation.x += Input.GetAxis("Mouse Y") * Sensitivity;

        cam.transform.eulerAngles = new Vector2(-rotation.x, rotation.y);
        cam.transform.position = transform.position + HeadCameraOffset;

        transform.eulerAngles = new Vector2(0, rotation.y);
        Vector3 moveDirection = transform.right * move.x + transform.forward * move.y;

        // Separate ground and wall checks
        RaycastHit hit;
        bool isGrounded = Physics.Raycast(transform.position, -transform.up, out hit, 1.2f);
        bool isAgainstWall = Physics.SphereCast(transform.position, 0.5f, moveDirection, out hit, 0.6f);
        Grounded = isGrounded;
        
        if (isGrounded) {
            coyoteTimer = coyoteTime;
        }
        else if (isAgainstWall && Input.GetButton("Jump")) {
            coyoteTimer = coyoteTime;
        }
        else 
        {
            coyoteTimer -= Time.deltaTime;
        }

        if (Input.GetButtonDown("Jump"))
        {
            jumpBufferCounter = jumpBufferTime;
        }
        else
        {
            jumpBufferCounter -= Time.deltaTime;
        }

        // Modified jump condition to allow jumping during slide
        if(jumpBufferCounter > 0f && (isGrounded || (isAgainstWall && Input.GetButton("Jump")) || coyoteTimer > 0 || isSliding))
        {
            Jump();
            jumpBufferCounter = 0f;
            coyoteTimer = 0f;
        }

        // Handle sliding
        float transitionSpeed = 5f; // Same transition speed as crouch
        float oldHeight, newHeight;

        if (Input.GetKeyDown("c") && !isSliding && move.magnitude > 0)
        {
            isSliding = true;
            currentSlideSpeed = speed * slideSpeedGain;
            oldHeight = transform.localScale.y;
            newHeight = crouchHeight;
            transform.position += Vector3.up * (oldHeight - newHeight) * 0.5f;
            transform.localScale = new Vector3(transform.localScale.x, newHeight, transform.localScale.z);
            HeadCameraOffset.y = newHeight;
        }

        Vector3 targetVelocity;
        if (isSliding)
        {
            currentSlideSpeed = Mathf.Max(0, currentSlideSpeed - slideSpeedDecay * Time.deltaTime);
            targetVelocity = moveDirection.normalized * currentSlideSpeed;
            
            // Start standing up 0.2 seconds before slide ends
            float timeUntilSlideEnds = currentSlideSpeed / slideSpeedDecay;
            if (timeUntilSlideEnds <= 0.5f)
            {
                oldHeight = transform.localScale.y;
                newHeight = Mathf.Lerp(transform.localScale.y, normalHeight, Time.deltaTime * transitionSpeed);
                transform.position += Vector3.up * (newHeight - oldHeight) * 0.5f;
                transform.localScale = new Vector3(transform.localScale.x, newHeight, transform.localScale.z);
                HeadCameraOffset.y = Mathf.Lerp(HeadCameraOffset.y, normalHeight, Time.deltaTime * transitionSpeed);
            }
            
            // Modified slide cancel condition to not cancel on jump
            if (currentSlideSpeed <= 0)
            {
                isSliding = false;
            }
        }
        else
        {
            targetVelocity = moveDirection.normalized * currentSpeed;
            // Smooth transition to normal height when not sliding
            if (transform.localScale.y < normalHeight)
            {
                oldHeight = transform.localScale.y;
                newHeight = Mathf.Lerp(transform.localScale.y, normalHeight, Time.deltaTime * transitionSpeed);
                transform.position += Vector3.up * (newHeight - oldHeight) * 0.5f;
                transform.localScale = new Vector3(transform.localScale.x, newHeight, transform.localScale.z);
                HeadCameraOffset.y = Mathf.Lerp(HeadCameraOffset.y, normalHeight, Time.deltaTime * transitionSpeed);
            }
        }

        rb.velocity = new Vector3(targetVelocity.x, rb.velocity.y, targetVelocity.z);
    }
    void Jump()
    {
        rb.velocity = new Vector3(rb.velocity.x, jumpStrength, rb.velocity.z);
    }
}
