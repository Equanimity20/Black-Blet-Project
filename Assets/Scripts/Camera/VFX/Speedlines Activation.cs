using UnityEngine;

public class SpeedlinesActivation : MonoBehaviour
{
    public PlayerMovementAdvanced pm;
    public PlayerStats ps;
    public ParticleSystem speedlines;

    private bool speedlinesPlaying = false; // Track state — only call Play/Stop on transitions

    void Start()
    {
        speedlines.Stop();
        speedlinesPlaying = false;
    }

    void Update()
    {
        bool shouldPlay = ShouldSpeedlinesPlay();

        if (shouldPlay && !speedlinesPlaying)
        {
            speedlines.Play();
            speedlinesPlaying = true;
        }
        else if (!shouldPlay && speedlinesPlaying)
        {
            speedlines.Stop();
            speedlinesPlaying = false;
        }
    }

    private bool ShouldSpeedlinesPlay()
    {
        if (ps.speed < 10f)
            return false;

        return pm.state == PlayerMovementAdvanced.MovementState.sprinting ||
               pm.state == PlayerMovementAdvanced.MovementState.wallrunning ||
               pm.state == PlayerMovementAdvanced.MovementState.dashing ||
               pm.state == PlayerMovementAdvanced.MovementState.sliding;
    }
}
