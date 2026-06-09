using System.Runtime.Serialization;
using UnityEngine;

public class Lava : MonoBehaviour
{
    public PlayerStats ps;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            ps.damageValue += 1f;
        }
    }
}
