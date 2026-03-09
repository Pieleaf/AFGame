using UnityEngine;

public class ObjectBobber : MonoBehaviour
{

    public float bobTime = 1f;
    public float bobMagnitude = 0.03f;

    Vector3 startPos;

    void Start()
    {
        startPos = transform.localPosition;
    }

    void Update()
    {
        transform.localPosition = new Vector3(
            startPos.x,
            startPos.y + Mathf.Sin(Time.time * bobTime) * bobMagnitude,
            startPos.z);
    }
}
