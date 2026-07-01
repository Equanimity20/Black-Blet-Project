using UnityEngine;

public class GameManager : MonoBehaviour
{
    public int MaxFrameRate;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Application.targetFrameRate = MaxFrameRate;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
