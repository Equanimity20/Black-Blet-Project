using UnityEngine;
using TMPro;

public class Timer : MonoBehaviour
{
    public TextMeshProUGUI timerText;
    private float elapsedTime = 0f;

    public float currentTimerValue;

    void Update()
    {
        elapsedTime += Time.deltaTime;

        int minutes = Mathf.FloorToInt(elapsedTime / 60f);
        int seconds = Mathf.FloorToInt(elapsedTime % 60f);
        int milliseconds = Mathf.FloorToInt((elapsedTime * 100f) % 100f); // Two digits: 0–99

        if (minutes > 0)
        {
            // Show mm:ss:ms (with 2-digit ms)
            timerText.text = string.Format("{0:00}:{1:00}:{2:00}", minutes, seconds, milliseconds);
        }
        else
        {
            // Show ss:ms (with 2-digit ms)
            timerText.text = string.Format("{0:00}:{1:00}", seconds, milliseconds);
        }

        // Update the current timer value
        currentTimerValue = elapsedTime;
    }
}