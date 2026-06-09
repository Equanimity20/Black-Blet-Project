using UnityEngine;

public class SpeedlinesActivation : MonoBehaviour
{
    public PlayerMovementAdvanced pm;
    public PlayerStats ps;
    public ParticleSystem speedlines;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        speedlines.Stop();
    }

    // Update is called once per frame
    void Update()
    {

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
            ps.speed >= 10f)
            {
                speedlines.Play();
            }
        }

        if (ps.speed < 10f)
        {
            speedlines.Stop();
        }
    }
}
