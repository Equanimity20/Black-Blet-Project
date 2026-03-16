using UnityEngine;
public class PiviotPointPos : MonoBehaviour
{
    public GameObject pivotPoint;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        pivotPoint = gameObject.transform.parent.gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        pivotPoint.transform.position = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y, gameObject.transform.position.z);
    }
}
