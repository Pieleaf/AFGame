using NUnit.Framework;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;
using static UnityEngine.SpriteMask;

public class SFXPlayer : MonoBehaviour
{
    public static SFXPlayer Instance;

    public AudioClip collectSFX;
    public float collectVolume = 0.1f;
    public Vector2 collectPitchRange;

    public AudioClip bounceSFX;
    public float bouncePitch = 1f;
    public float bounceVolume = 0.2f;

    public AudioClip purchaseSFX;
    public float purchasePitch = 1f;
    public float purchaseVolume = 0.2f;

    public float sfxCooldown = 0.1f;

    float lastSfxTime = -99f;

    public int sourcesLength = 10;
    private AudioSource[] sources;
    private int sourcesI = -1;

    private void Awake()
    {
        Instance = this;

        collectPitchRange = collectPitchRange != null ? collectPitchRange : Vector2.one;

        UpdateSourcePool();
    }

    private void UpdateSourcePool()
    {
        if (sources != null && sources.Length == sourcesLength)
            return;

        var sourcesOld = sources != null ? (AudioSource[])sources.Clone() : new AudioSource[0];
        sources = new AudioSource[sourcesLength];

        // Reuse old sources if exists:
        for (int i = 0; i < sources.Length; i++)
        {
            if (sourcesOld.Length > i)
                sources[i] = sourcesOld[i];
            else
                sources[i] = MakeSource();
        }

        // Delete old unused sources:
        for (int i = sources.Length; i < sourcesOld.Length; i++)
        {
            Destroy(sourcesOld[i]);
        }

        Debug.Log($"{this} UpdateSourcePool(): Making new pool of {sources.Length} audiosources"
            + (sourcesOld.Length > 0 ? $" (old sources: {sourcesOld.Length})" : ""));
    }

    private AudioSource GetSource()
    {
        sourcesI = (sourcesI + 1) % sources.Length;

        return sources[sourcesI];
    }

    private AudioSource MakeSource()
    {
        var src = gameObject.AddComponent<AudioSource>();
        src.outputAudioMixerGroup = AudioManager.Instance.mixer.FindMatchingGroups("SFX")[0];
        return src;
    }

    public void PlayCollect()
    {
        var pitch = Random.Range(collectPitchRange[0], collectPitchRange[1]);

        Play(collectSFX, collectVolume, pitch);
    }

    public void PlayBounce()
    {
        Play(bounceSFX, bounceVolume, bouncePitch);
    }

    public void PlayPurchase()
    {
        Play(purchaseSFX, purchaseVolume, purchasePitch);

    }

    private void Play(AudioClip sfx, float volume=1f, float pitch=1f)
    {
        if (!CanPlay())
            return;

        AudioSource src = GetSource();

        if (src == null)
            return;

        Debug.Log($"{this} Play({sfx}, {volume}, {pitch}): src {src}, sourcesI {sourcesI}, sources {sources}");

        src.volume = volume;
        src.pitch = pitch;

        src.PlayOneShot(sfx);

        lastSfxTime = Time.time;
    }

    private bool CanPlay()
    {
        return lastSfxTime + sfxCooldown < Time.time;
    }

    private void FixedUpdate()
    {
    }
}
