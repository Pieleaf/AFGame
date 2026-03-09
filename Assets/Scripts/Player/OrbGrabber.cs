using System;
using System.Collections.Generic;
using UnityEngine;

public class OrbGrabber : CollectorRanged
{
    // Takes control of any orbs that hits the collider and moves them towards us

    public float grabTime = 1f;
    public AnimationCurve grabCurve = CurvePresets.InverseLog(); // Distance over time

    public float orbShrinkTowards = 0.05f;
    public float collectAtDist = 1f; // Collects when the orb's edge touches this radius

    [ReadOnly]
    public List<GrabData> grabbing;

    protected override void Awake()
    {
        base.Awake();
        grabbing = new List<GrabData>();
    }
    public override void SetActive(bool active)
    {
        base.SetActive(active);
        if (!active)
            StopAllGrabs();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!active)
            return;

        var orb = other.GetComponent<Orb>();
        if (orb != null)
        {
            StartGrab(orb);
        }
    }

    private void StartGrab(Orb orb)
    {
        if (orb == null || orb.state != OrbState.Collectable 
            || orb.beingGrabbed || orb.radius < 0.1f)
            return;

        Debug.Log($"Grab started on {orb}");

        grabbing.Add(new GrabData { 
            orb = orb, 
            startPos = orb.transform.position, 
            grabbedAt = Time.time });
        orb.SetBeingGrabbed(true);
    }

    private void StopAllGrabs()
    {
        if (grabbing.Count == 0) return;

        foreach (var grab in grabbing)
        {
            grab.orb.SetBeingGrabbed(false);
        }
    }

    void Update()
    {
        UpdateGrabs();
    }

    private void UpdateGrabs()
    {
        grabbing ??= new List<GrabData>();

        if (!active && grabbing.Count > 0)
        {
            StopAllGrabs();
            return;
        }

        for (int i = grabbing.Count - 1; i >= 0; i--)
        {
            GrabData grab = grabbing[i];
            Orb orb = grab.orb;

            if (orb == null || orb.state != OrbState.Collectable || !orb.beingGrabbed)
            {
                grabbing.RemoveAt(i);
                continue;
            }

            var t = (Time.time - grab.grabbedAt) / grabTime;
            var distStd = grabCurve.Evaluate(t);
            var newDist = distStd * (currentRange + orb.radius);

            if (newDist <= collectAtDist)
            {
                CollectGrabbed(i);
                continue;
            }

            orb.transform.position = Vector3.Lerp(
                collector.transform.position, grab.startPos, distStd);

            orb.SetShrinkage(Mathf.Lerp(orbShrinkTowards, 1f, distStd));
        }
    }

    private void CollectGrabbed(int i)
    {
        if (i < 0 || i >= grabbing.Count)
            throw new IndexOutOfRangeException($"Index ({i}) out of range: [0, {grabbing.Count}]");

        var orb = grabbing[i].orb;
        grabbing.RemoveAt(i);
        orb.Collect();
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 1f, 1f, 0.5f);
        Gizmos.DrawWireSphere(transform.position, collectAtDist);
    }

    [Serializable]
    public struct GrabData
    {
        public Orb orb;
        public Vector3 startPos;
        public float grabbedAt; // Time when grab started
    }
}
