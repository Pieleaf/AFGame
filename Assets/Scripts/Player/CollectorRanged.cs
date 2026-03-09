using UnityEngine;

public abstract class CollectorRanged : MonoBehaviour
{
    public bool active = true;
    public bool linkToShop = false;

    public float upgradeRangeMin = 0f;
    public float upgradeRangeMax = 150f;
    public float upgradeRangeInc = 30f;

    protected Collector collector;
    protected SphereCollider collid;

    public float currentRange => collid.radius;

    protected virtual void Awake()
    {
        if (collector == null)
            collector = GetComponentInParent<Collector>();

        collid = GetComponent<SphereCollider>();
    }

    protected virtual void Start()
    {
        if (linkToShop)
            collid.radius = upgradeRangeMin;
    }

    public virtual void SetActive(bool active)
    {
        this.active = active;
        collid.enabled = active;
    }

    public virtual void UpgradeRange()
    {
        if (collid.radius >= upgradeRangeMax)
            return;

        collid.radius = Mathf.Clamp(collid.radius + upgradeRangeInc,
            upgradeRangeMin, upgradeRangeMax);
    }
}