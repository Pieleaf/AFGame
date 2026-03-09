using UnityEngine;

public class PreyCage : MonoBehaviour
{
    public bool active = true;
    public BoxCollider collid;

    private void Awake()
    {
        collid = GetComponent<BoxCollider>();
    }
}
