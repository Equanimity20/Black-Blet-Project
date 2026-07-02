using UnityEngine;

public class Dead : MonoBehaviour
{
    public PlayerStats ps;
    public GameObject overlay;

    void Start()
    {
        overlay.SetActive(false);
    }

    void Update()
    {
        // Only trigger once when player dies — don't call SetActive every frame
        if (ps.Dead && !overlay.activeSelf)
        {
            overlay.SetActive(true);
        }
    }
}
