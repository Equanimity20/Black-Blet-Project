using UnityEngine;

public class SpeedlinesActivation : MonoBehaviour
{
    public PlayerMovementAdvanced pm;
    public ParticleSystem speedlines;
    public float currentSpeed;
    public Vector3 lastPosition;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        speedlines.Stop();
    }

    // Update is called once per frame
    void Update()
    {
        // Distance moved since last frame
        float distance = Vector3.Distance(transform.position, lastPosition);

        // Speed = distance / time
        currentSpeed = distance / Time.deltaTime;

        // Store this frame's position for next frame
        lastPosition = transform.position;

        if (Input.GetKeyDown(KeyCode.E))
        {
            speedlines.Play();
        }
        
        if (pm.state != PlayerMovementAdvanced.MovementState.air && pm.state != PlayerMovementAdvanced.MovementState.walking)
        {
            if (pm.state == PlayerMovementAdvanced.MovementState.sprinting ||
            pm.state == PlayerMovementAdvanced.MovementState.wallrunning ||
            pm.state == PlayerMovementAdvanced.MovementState.dashing ||
            pm.state == PlayerMovementAdvanced.MovementState.sliding &&
            currentSpeed >= 10f)
            {
                speedlines.Play();
            }
        }

        if (currentSpeed < 10f)
        {
            speedlines.Stop();
        }
    }
}
