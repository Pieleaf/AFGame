using UnityEngine;

public class TutorialSpot : MonoBehaviour
{
    private void Start()
    {
        var collid = GetComponent<SphereCollider>();
        Collider[] overlaps = Physics.OverlapSphere(transform.position, collid.radius);

        foreach (var o in overlaps)
        {
            if (o.GetComponent<PlayerController>())
            {
                UIManager.Instance.SetShowTutorial(true);
                break;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        UIManager.Instance.SetShowTutorial(true);
    }
    private void OnTriggerExit(Collider other)
    {
        UIManager.Instance.SetShowTutorial(false);
    }
}
