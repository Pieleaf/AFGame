using UnityEngine;

[ExecuteAlways]
public class SkySpiral : MonoBehaviour
{
    public float rotationSpeed = 2.0f;

    void Update()
    {
        transform.eulerAngles = new Vector3 (
            transform.eulerAngles.x, 
            transform.eulerAngles.y - (rotationSpeed*Time.deltaTime), 
            transform.eulerAngles.z);
    }
}
