using UnityEngine;

public class BulletTracer : MonoBehaviour
{
    private LineRenderer line;

    public void Init(Vector3 start, Vector3 end, float duration)
    {
        line = GetComponent<LineRenderer>();
        line.SetPosition(0, start);
        line.SetPosition(1, end);
        Destroy(gameObject, duration); // auto destroy
    }
}
