#if UNITY_EDITOR
using NUnit.Framework;
# endif
using UnityEngine;

//[ExecuteAlways]
public class Prey : MonoBehaviour
{
    public bool catchable = false;
    public bool randomizeStart = true;

    public float speed = 50;
    public float turnSpeed = .4f;

    public float yawMod = 360f;
    public float pitchMod = 360f;
    public float leanMod = 1f;

    public float maxPitch = 30f;

    public float movNoiseOffset = 0f;
    public float movNoiseScale = 0.002f;

    public float timeUntilReturn = 50f; // Seconds until prey fully turns back towards cage

    [ReadOnly]
    public bool _active = true;
    [ReadOnly]
    public bool escaped = false;
    [ReadOnly]
    public float escapeTime = 0;

    Rigidbody rb;
    Vector3 cageCenter;
    Collider collid;
    Renderer rend;

    int i;

    public Quaternion directionMod; // Used to steer prey back into cage

    public bool active { get => _active;
        set
        {
            _active = value;

            collid.enabled = _active;
            rend.enabled   = _active;
            if (_active)
                rb.WakeUp();
            else
                rb.Sleep();
        }
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rend = GetComponent<Renderer>();
        collid = GetComponent<Collider>();
    }

    void Start()
    {
        #if UNITY_EDITOR
        Assert.IsTrue(transform.parent.GetComponent<PreyCage>() != null, $"{this} lacks a cage");
        # endif

        active = active; // Force update components;
        cageCenter = transform.parent.position;
        directionMod = Quaternion.identity;

        movNoiseOffset += Random.Range(0, 100);

        if (randomizeStart)
        {
            var euler = transform.rotation.eulerAngles;
            euler.x = Random.Range(0, 360);
            euler.y = Random.Range(-maxPitch, maxPitch);
            transform.rotation = Quaternion.Euler(euler);
        }

        // Todo: Handle if prey didn't start within cage
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PreyCage>() != null)
        {
            //Debug.Log($"Prey returned: {this}");
            escaped = false;
            escapeTime = 0;
        }

        if (catchable)
        {
            var collector = other.GetComponent<Collector>();
            if (collector != null)
            {
                Debug.Log($"Prey {this} triggered by collector: {other}");

                GameManager.Instance.player.CollectPrey(this);
            }

            var player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                Debug.Log($"Prey {this} triggered by player: {other}");

                player.CollectPrey(this);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<PreyCage>() != null)
        {
            //Debug.Log($"Prey escaped: {this}");
            escaped = true;
        }
    }

    void Update()
    {
        if (!active)
            return;

        i++;
        var noiseI = (i + movNoiseOffset) * movNoiseScale;

        var xNoise = Mathf.PerlinNoise1D(noiseI) - 0.45f;
        var yNoise = Mathf.PerlinNoise1D(noiseI+100 * movNoiseScale) - 0.45f;
        //var zNoise = Mathf.PerlinNoise1D(noiseI+200 * movNoiseScale) - 0.45f;

        var yaw = xNoise * yawMod;
        var pitch = yNoise * pitchMod;
        var roll = xNoise * leanMod;

        var newDirection = Quaternion.Euler(pitch, yaw, roll);

        if (escaped)
        {
            // The longer the prey is away from the cage, the stronger it feels the urge to return

            escapeTime += Time.deltaTime;

            var toCage = Quaternion.LookRotation((transform.position - cageCenter).normalized);

            var newDirectionMod = Quaternion.Inverse(newDirection) * toCage;

            directionMod = Quaternion.Lerp(directionMod, 
                newDirectionMod, escapeTime / timeUntilReturn);
        }

        newDirection *= directionMod;

        var euler = newDirection.eulerAngles;
//        Debug.Log($"{this} {euler} {newDirection}");

        // Limit pitch using euler:
        if (euler.x > 180f) euler.x -= 360f;
        euler.x *= maxPitch / 90;

        //      Debug.Log($"{this} 222222 {euler} {newDirection}");

        newDirection = Quaternion.Euler(euler);

        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            newDirection,
            turnSpeed * (escaped ? 10 : 1) * Time.deltaTime
        );

        rb.linearVelocity = transform.rotation * new Vector3(0, 0, -speed);
    }

    internal void Destroy()
    {
        EffectsManager.Instance.PreyBurst(this);
        active = false;
    }
}
