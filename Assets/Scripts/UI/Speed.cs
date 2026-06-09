using UnityEngine;

public class Speed : MonoBehaviour
{
    public PlayerStats ps;
    public TMPro.TextMeshProUGUI text;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        text = gameObject.GetComponent<TMPro.TextMeshProUGUI>();

        text.text = "Speed: " + null;
    }

    // Update is called once per frame
    void Update()
    {
        text.text = "Speed: " + Mathf.Round(ps.speed).ToString();
    }
}
