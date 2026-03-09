#if UNITY_EDITOR
using NUnit.Framework;
# endif
using UnityEngine;

public class Orb : MonoBehaviour
{
    [Header("General Settings")]
    public OrbState startState = OrbState.Collectable;
    public bool alwaysUpdate = false;
    public float scaleSpeed = 0.5f;

    public float defaultAudioRange = 2.5f;
    public float defaultAudioVolume = -1f;

    [Tooltip("Orb color is selected by mixing two colors within this list.")]
    public ColorSettings[] colorSelection;

    [Header("Collectable Settings")]
    public float collectableSize = 10f;

    [Header("Snake Limb Settings")]
    public float limbSize = 12f;
    public float limbEmissionMod = 3f;

    public ColorSettings collisionColor;
    public float collisionFadeTime = 2f;

    [Header("General Readonly")]
    [ReadOnly]
    public ColorSettings defaultColor;
    [ReadOnly]
    public float radius;
    [ReadOnly]
    public bool changingScale = false;
    [ReadOnly]
    public OrbState _state = OrbState.Inactive;

    [Header("Collectable Readonly")]
    [ReadOnly]
    public bool beingGrabbed = false;
    [ReadOnly]
    public float shrinkage = 1f;

    [Header("Snake Limb Readonly")]
    [ReadOnly]
    public int pathIndex = -1;
    [ReadOnly]
    public bool isHarmlessLimb = false;
    [ReadOnly]
    public float targetRadius = -1f;

    [ReadOnly]
    public bool justCollided = false;
    [ReadOnly]
    public float collidedTime = -1f;

    Collider collid;
    Renderer rend;
    AudioSource audioSource;
    ColorSettings currentColor;

    public OrbState state
    {
        get => _state;
        set
        {
            ChangeState(value);
        }
    }

    private void Awake()
    {
        //Debug.Log($"{this} awake");

        rend = GetComponent<MeshRenderer>();
        audioSource = GetComponent<AudioSource>();
        collid = GetComponent<Collider>();

        defaultAudioRange = defaultAudioRange >= 0 ? defaultAudioRange : audioSource.maxDistance;
        defaultAudioVolume = defaultAudioVolume >= 0 ? defaultAudioVolume : audioSource.volume;

        defaultColor = ColorSettings.RandomMix(colorSelection);
        changingScale = false;
        isHarmlessLimb = false;
        justCollided = false;
        beingGrabbed = false;

        ChangeState(startState);

        if (!changingScale || targetRadius == -1f)
        {
            changingScale = false;
            targetRadius = radius;
        }
    }

    private void Start()
    {
        audioSource.outputAudioMixerGroup = AudioManager.Instance.mixer.FindMatchingGroups("SFX")[0];
    }

    public void ChangeState(OrbState newState)
    {
        var prevState = state;
        _state = newState;
        //Debug.Log($"{this} ChangeState({newState})");

        if (prevState == state)
            return;

        shrinkage = 1f;
        beingGrabbed = false;

        if (state == OrbState.InSnake)
        {
            rend.enabled = true;
            audioSource.mute = false;
            collid.enabled = true;

            transform.localScale = Vector3.zero;
            targetRadius = limbSize / 2;
            changingScale = true;

            currentColor = defaultColor.Clone();
            currentColor.emissionIntensity = defaultColor.emissionIntensity * limbEmissionMod;
        }
        else if (state == OrbState.Collectable)
        {
            rend.enabled = true;
            currentColor = defaultColor.Clone();
            transform.localScale = Vector3.zero;

            collid.enabled = true;
            audioSource.mute = true;
            audioSource.volume = 0;

            targetRadius = collectableSize / 2;
            changingScale = true;
        }
        else if (state == OrbState.InSnakeHidden)
        {
            rend.enabled = false;
            audioSource.mute = true;
            audioSource.volume = 0;
            collid.enabled = false;

            transform.localScale = Vector3.zero;
            transform.position = Vector3.zero;
            changingScale = false;
        }
        else if (state == OrbState.Inactive)
        {
            rend.enabled = false;
            audioSource.mute = true;
            audioSource.volume = 0;
            collid.enabled = false;

            transform.localScale = Vector3.zero;
            changingScale = false;
        }

        currentColor.ApplyToMaterial(rend.material);

        UpdateAudioVisuals();
    }

    private void UpdateAudioVisuals()
    {

        #if UNITY_EDITOR
        Assert.IsFalse(rend.enabled && currentColor == null);
        # endif

        HandleScale();

        if (justCollided && rend.enabled)
        {
            // If collided, change to collision color and fade back to normal:

            var newColor = currentColor.Clone();

            var t = (Time.time - collidedTime) / collisionFadeTime;
            newColor = ColorSettings.Lerp(
                collisionColor, newColor, t);

            if (t >= 1)
            {
                collidedTime = -1f;
                justCollided = false;
            }
            newColor.ApplyToMaterial(rend.material);
        }

        if (!audioSource.mute)
        {
            audioSource.maxDistance = defaultAudioRange * radius;
            audioSource.volume = Mathf.MoveTowards(audioSource.volume,
                defaultAudioVolume, defaultAudioVolume / 10 * Time.deltaTime);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        //Debug.Log($"{this} (state: {state}) trigger activated  by: {other}");

        var player = other.GetComponent<PlayerController>();
        var playerCollector = other.GetComponent<Collector>();

        if (player == null && playerCollector == null)
            return;

        if (state == OrbState.Collectable)
        {
            Collect();
        }
        else if (state == OrbState.InSnake)
        {
            if (player != null && !isHarmlessLimb)
            {
                player.HitOwnLimb(this);
            }
        }
    }

    public void Collect()
    {
        Debug.Log($"{this} ({state}): Collect()");
        if (state == OrbState.InSnake || state == OrbState.InSnakeHidden)
            return;

        Debug.Log($"{GameManager.Instance}");

        GameManager.Instance.OrbCollected(this);
    }

    private void Update()
    {
        if (state == OrbState.Inactive)
            return;

        if (alwaysUpdate || changingScale || justCollided)
            UpdateAudioVisuals();
    }

    private void HandleScale()
    {
        if (state == OrbState.InSnakeHidden || state == OrbState.Inactive)
            return;
        
        var ts = targetRadius * 2 * shrinkage * Vector3.one;

        if (changingScale)
        {
            transform.localScale = Vector3.MoveTowards(
                transform.localScale,
                ts,
                scaleSpeed);

            if (Vector3.Equals(transform.localScale, ts))
                changingScale = false;
        }
        else
        {
            transform.localScale = ts;
        }

        radius = rend.bounds.size.x / 2;
    }

    internal void Destroy()
    {
        Debug.Log($"{this} ({state}): Destroy()");

        EffectsManager.Instance.OrbBurst(this);

        transform.parent = null;
        state = OrbState.Inactive;
        Destroy(this, 10);

    }

    internal void OnCollided(PlayerController player)
    {
        if (state == OrbState.InSnake)
        {
            justCollided = true;
            collidedTime = Time.time;
        }
    }

    public void SetShrinkage(float shrinkage)
    {
        this.shrinkage = shrinkage;
        UpdateAudioVisuals();
    }

    public void SetBeingGrabbed(bool grabbed)
    {
        if (beingGrabbed == grabbed) return;

        beingGrabbed = grabbed;
        collid.enabled = grabbed;
        if (!grabbed)
            SetShrinkage(1f);
    }
}

public enum OrbState
{
    Inactive, Collectable, InSnake, InSnakeHidden
}