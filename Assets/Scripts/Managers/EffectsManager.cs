using System.Collections.Generic;
using UnityEngine;

public class EffectsManager : MonoBehaviour
{
    public static EffectsManager Instance;

    public ParticleSystem orbDestroyed;
    public ParticleSystem preyDestroyed;

    public float recentEffectsCoolDown = 0.5f; // Seconds to wait between overlapping effects
    public float recentEffectsOverlapDistance = 1f; // Distance to be considered overlapping
    public int recentEffectsCap = 5;

    [ReadOnly]
    public FIFOBuffer<EffectsEntry> recentEffects;

    PlayerController player;

    private void Awake()
    {
        Instance = this;
        recentEffects = new FIFOBuffer<EffectsEntry>(recentEffectsCap);
        player = FindAnyObjectByType<PlayerController>();

        Debug.Log($"SnakeManager Awake() with player: {player}");
    }

    public ParticleSystem PreyBurst(Prey prey)
    {
        ParticleSystem burst = MakeEffect(preyDestroyed, prey.transform.position);

        Debug.Log($"PreyBurst on {prey} at {prey.transform.position} (particles: {burst != null})");
        return burst;
    }

    public void OrbCollected(Orb orb)
    {
        var effect = OrbBurst(orb);
        if (effect != null)
            effect.transform.parent = player.transform;
    }
    public ParticleSystem OrbBurst(Orb orb)
    {
        ParticleSystem burst = MakeEffect(orbDestroyed, orb.transform.position);

        //Debug.Log($"OrbBurst on {orb} at {orb.transform.position} (particles: {burst != null})");
        if (burst != null)
        {
            burst.transform.localScale = orb.shrinkage * Vector3.one;
        }
        return burst;
    }

    public ParticleSystem MakeEffect(ParticleSystem prefab, Vector3 pos)
    {
        if (!ClearToGo(pos)) 
            return null;

        var effect = Instantiate(prefab);
        effect.transform.position = pos;

        recentEffects.Add(new EffectsEntry(effect, Time.time));
        return effect;
    }

    public bool ClearToGo(Vector3 pos)
    {
        var clearTime = Time.time - recentEffectsCoolDown;

        for (var i = 0; i < recentEffects.Count; i++) {
            var entry = recentEffects[i];

            if (entry.effect != null
                && entry.time > clearTime 
                && (Vector3.Distance(pos, entry.effect.transform.position)
                    < recentEffectsOverlapDistance)
                )
            {
                return false;
            }
        }
        return true;
    }
}

public struct EffectsEntry
{
    public ParticleSystem effect;
    public float time;

    public EffectsEntry(ParticleSystem effect, float time) : this()
    {
        this.effect = effect;
        this.time = time;
    }
}