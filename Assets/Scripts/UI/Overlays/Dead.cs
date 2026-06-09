using UnityEngine;

public class Dead : MonoBehaviour
{

    public PlayerStats ps;
    public GameObject overlay;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        overlay.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (ps.Dead)
        {
            OnEnable();
        }
    }

    void OnEnable()
    {
        overlay.SetActive(true);
    }

    void OnDisable()
    {
        overlay.SetActive(false);
    }
}
