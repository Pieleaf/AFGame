using UnityEngine;

public class Collector : MonoBehaviour
{
    Collider collid;

    CollectorRanged[] collectorRangeds;

    private void Awake()
    {
        collectorRangeds = GetComponentsInChildren<CollectorRanged>();
        collid = GetComponent<Collider>();
    }
        
    public void SetActive(bool active)
    {
        collid.enabled = active;

        foreach (var coll in collectorRangeds)
        {
            coll.SetActive(active);
        }
    }

    public void UpgradeRange()
    {
        foreach (var coll in collectorRangeds)
        {
            coll.UpgradeRange();
        }
    }
}
