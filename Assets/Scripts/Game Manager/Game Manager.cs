using UnityEngine;

public class GameManager : MonoBehaviour
{
    public int MaxFrameRate = 144;

    void Awake()
    {
        QualitySettings.vSyncCount = 0; // Must be 0 or targetFrameRate is ignored
        Application.targetFrameRate = MaxFrameRate;
    }

    void Update()
    {
        
    }
}
