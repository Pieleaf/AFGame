using UnityEngine;
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

    private void Awake()
    {
        Instance = this;

        collectPitchRange = collectPitchRange != null ? collectPitchRange : Vector2.one;
    }

    public void PlayCollect()
    {
        var src = MakeSource();
        Debug.Log($"{this} PlayCollect(): {src != null}");

        if (src == null)
            return;


        src.volume = collectVolume;
        src.pitch = Random.Range(collectPitchRange[0], collectPitchRange[1]);

        src.PlayOneShot(collectSFX);
        Destroy(src, collectSFX.length);
    }

    public void PlayBounce()
    {
        var src = MakeSource();
        Debug.Log($"{this} PlayBounce(): {src != null}");

        if (src == null)
            return;


        src.volume = bounceVolume;
        src.pitch = bouncePitch;

        src.PlayOneShot(bounceSFX);
        Destroy(src, bounceSFX.length);
    }

    public void PlayPurchase()
    {
        var src = MakeSource();
        Debug.Log($"{this} PlayPurchase(): {src != null}");

        if (src == null)
            return;


        src.volume = purchaseVolume;
        src.pitch = purchasePitch;

        src.PlayOneShot(purchaseSFX);
        Destroy(src, purchaseSFX.length);
    }

    private AudioSource MakeSource()
    {
        if (lastSfxTime + sfxCooldown > Time.time)
            return null;

        lastSfxTime = Time.time;

        var src = gameObject.AddComponent<AudioSource>();

        src.outputAudioMixerGroup = AudioManager.Instance.mixer.FindMatchingGroups("SFX")[0];

        return src;

    }
}
