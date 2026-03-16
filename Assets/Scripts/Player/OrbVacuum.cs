using UnityEngine;

public class OrbVacuum : CollectorRanged
{
    // Old method that applies a velocity to all orbs in the collider

    public float maxAttraction = 4f;
    public float minAttraction = 1f;
    public float attractionTransitionWindow = 4f;
    public AnimationCurve attractionCurve = CurvePresets.Exp();

    public float orbShrinkTowards = 0f; // Target scale to shrink orb by when it gets closer

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Start()
    {
        base.Start();
    }

    private void OnTriggerStay(Collider other)
    {
        if (!active)
            return;

        Orb orb = other.GetComponent<Orb>();
        if (orb == null)
            return;

        //Debug.Log($"Vacuum triggered: {other} (orb: {orb})");

        if (orb == null || orb.state != OrbState.Collectable)
            return;

        Vector3 toMe = collector.transform.position - orb.transform.position;

        float dist = toMe.magnitude;
        float attraction = maxAttraction;
        if (dist == 0f)
        {
            //Debug.Log($"{this} freezing orb {orb} at collector");
            orb.transform.position = collector.transform.position;
            return;
        }

        if (attractionTransitionWindow > 0f)
        {
            float attractStd = Mathf.Clamp01((currentRange - dist) / attractionTransitionWindow);

            if (attractStd == 0)
                attraction = minAttraction;

            else if (attractStd < 1f)
            {
                var attrCurved = attractionCurve.Evaluate(attractStd);
                attraction = Mathf.Lerp(minAttraction, maxAttraction, attrCurved);
            }
        }
        if (orbShrinkTowards != 1f)
        {
            var shrink = Mathf.Lerp(orbShrinkTowards, 1f, dist / currentRange);
            orb.SetShrinkage(shrink);
        }
        //Debug.Log($"Sucking orb {orb} with force {attraction} at distance {dist}");

        var speed = attraction * Time.deltaTime;
        if (speed > dist)
            orb.transform.position = collector.transform.position;
        else
            orb.transform.position += speed * toMe.normalized;
    }

}
