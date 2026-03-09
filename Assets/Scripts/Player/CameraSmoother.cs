using UnityEngine;

public class CameraSmoother : MonoBehaviour
{
    // Unused, see PlayerCamera

    public Transform targetCam;

    public float posSpeed = 1.0f;
    public float angleSpeed = 1.0f;

    private void Awake()
    {
        transform.position = targetCam.position;
        transform.rotation = targetCam.rotation;
    }

    void Update()
    {
        if (targetCam == null)
            return;

        transform.position = Vector3.MoveTowards(
            transform.position, 
            targetCam.position, 
            posSpeed);

        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            targetCam.rotation,
            angleSpeed);
    }
}
