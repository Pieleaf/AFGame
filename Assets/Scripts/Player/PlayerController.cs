using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Flying - Movement")]
    public float maxSpeed = 50f;
    public float maxReverseSpeed = 10f;
    public float moveAcceleration = 2f;

    public AnimationCurve moveSpeedCurve = CurvePresets.Log();

    public float climbSpeedMod = -0.2f; // Add this as mod to speed when climbing
    public float diveSpeedMod  = 0.5f;  // Add this as mod to speed when diving
    public float glideModifier = 0.3f;  // How slowly to change speed to 0 when input is blank

    [Header("Flying - Handling/Rotation")]
    public float yawMaxSpeed = 180f;
    public float pitchMaxAngle = 40f;
    public float pitchMinAngle = -45f;

    public AnimationCurve pitchCurve = AnimationCurve.Linear(0, 0, 1, 1);
    public AnimationCurve yawSpeedCurve = CurvePresets.Log();
    public AnimationCurve yawFalloffCurve = CurvePresets.Exp();

    public float yawAcceleration = 5f;
    public float pitchAcceleration = 2.5f;

    public float pitchGlideModifier = 0.3f;  // How slowly to change pitch to 0 when input is blank
    public float yawFalloffByMomentum = 0.6f; // How much to lower handling at higher speeds

    public float leanAmount = 45f; // Amount to lean when player turns left/right
    public float leanSpeed = 4.5f;


    [Header("Tumbling")]

    public float bounceForcePos = 30f;
    public float bounceForceAngle = 360f;
    public float bounceForceLift = 0f;
    public ForceMode bounceForceMode = ForceMode.VelocityChange;

    public float tumbleDuration = 3f;
    public float recoverySpeed = 180f; // Degrees per second to rotate back to default

    [Header("Misc")]

    public Transform witchModel;
    public AudioListener listener;

    [ReadOnly]
    public WitchState state = WitchState.Inactive;
    [ReadOnly]
    public float speedStd = 0f;
    [ReadOnly]
    public float pitchStd = 0f;
    [ReadOnly]
    public float yawSpeedStd = 0f;

    [ReadOnly]
    public float speed = 0f;
    [ReadOnly]
    public float yawSpeed = 0f;
    [ReadOnly]
    public float climbSpeed = 0f;


    [ReadOnly]
    public float yaw;
    [ReadOnly]
    public float pitch;
    [ReadOnly]
    public float lean;
    [ReadOnly]
    public Vector3 velocity;
    [ReadOnly]
    public Vector3 tumbleAngle;
    [ReadOnly]
    public Collector collector;
    [ReadOnly]
    public OrbVacuum vacuum;

    [ReadOnly]
    public Vector2 lookInput;
    [ReadOnly]
    public bool clickInput;

    private Vector2 moveInput;
    private float climbInput;
    private Quaternion targetRotation;

    private Collider collid;
    private Rigidbody rb;

    private List<GameObject> children;
    private float tumbleStartTime = -1f;

    private void Awake()
    {
        Debug.Log($"Player {this} Awake");

        collid = GetComponent<Collider>();
        rb = GetComponent<Rigidbody>();

        yaw = transform.rotation.eulerAngles.y;
        targetRotation = transform.rotation;

        children = new List<GameObject>();
        for (int i = 0; i < transform.childCount; i++)
        {
            var child = transform.GetChild(i);
            if (child != null && child.gameObject.activeSelf)
                children.Add(child.gameObject);
        }

        collector = GetComponentInChildren<Collector>();
        vacuum = GetComponentInChildren<OrbVacuum>();
        listener = GetComponentInChildren<AudioListener>();
    }

    public void ChangeState(WitchState newState)
    {
        Debug.Log($"Player ChangeState({newState}) (prev: {state})");
        if (state == newState)
            return;

        var prevState = state;
        state = newState;

        if (newState == WitchState.Inactive)
        {
            SetChildrenActive(false);
            collid.enabled = false;
            rb.Sleep();
        }
        else
        {
            if (prevState == WitchState.Inactive)
            {
                SetChildrenActive(true);
                collid.enabled = true;
                rb.WakeUp();
            }

            if (newState == WitchState.Playable)
            {
                rb.useGravity = false;
                collector.SetActive(true);
                vacuum.SetActive(true);
            }
            else if (newState == WitchState.Tumbling)
            {
                rb.useGravity = true;
                collector.SetActive(false);
                vacuum.SetActive(false);
            }
        }
    }

    private void SetChildrenActive(bool active)
    {
        foreach (GameObject child in children)
        {
            child.SetActive(active);
        }
    }

    public void OnShop()
    {
        UIManager.Instance.OnShop();
        clickInput = false;
    }

    public void OnPause()
    {
        UIManager.Instance.OnPause();
        clickInput = false;
    }

    public void OnClick(InputValue value)
    {
        clickInput = value.isPressed;
    }

    public void OnLook(InputValue value)
    {
        lookInput = value.Get<Vector2>();
        //Debug.Log($"Lookinput: {lookInput}");
    }

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
       // Debug.Log($"Moved: {moveInput}");
    }
    public void OnClimb(InputValue value)
    {
        climbInput = value.Get<float>();
        //Debug.Log($"Climbed: {climbInput}");
    }

    private void Update()
    {
        if (state == WitchState.Playable)
        {
            HandleFlying();
        }
        if (state == WitchState.Tumbling)
        {
            HandleTumbling();
        }
    }

    private void HandleTumbling()
    {
        var tumbleStop = tumbleStartTime + tumbleDuration;

        if (Time.time >= tumbleStop + 180 / recoverySpeed || tumbleStartTime < 0)
        {
            StopTumble();
            return;
        }

        // Turn off speed when tumbling
        speed = Mathf.MoveTowards(speed, 0, 10 * Time.deltaTime);
        yawSpeed = Mathf.MoveTowards(yawSpeed, 0, 10 * Time.deltaTime);

        if (Time.time >= tumbleStop) 
        {
            // Gradual recovery:
            witchModel.localRotation = Quaternion.RotateTowards(
                witchModel.localRotation, 
                Quaternion.identity, 
                recoverySpeed * Time.deltaTime);

            if (witchModel.localRotation.Equals(Quaternion.identity))
                StopTumble();
                    
        } else // Tumbling, not recovering
        {
            // Calculate spin:
            witchModel.localRotation = (
                witchModel.localRotation
                * Quaternion.Euler(tumbleAngle * Time.deltaTime));
        }
    }

    private void HandleFlying()
    {

        // Standardized speed calculation (from 0 to 1)

        var accelerationMod = moveInput.y == 0 ? glideModifier : 1;
        var pitchAccMod = climbInput == 0 ? pitchGlideModifier : 1;

        speedStd = Mathf.MoveTowards(speedStd, moveInput.y, moveAcceleration * accelerationMod * Time.deltaTime);
        yawSpeedStd = Mathf.MoveTowards(yawSpeedStd, moveInput.x, yawAcceleration * Time.deltaTime);
        //climbMomentum = Mathf.MoveTowards(climbMomentum, climbInput, climbAcceleration * Time.deltaTime);
        pitchStd = Mathf.MoveTowards(pitchStd, -climbInput, pitchAcceleration * pitchAccMod * Time.deltaTime);

        // Actual speed:

        speed = (
            Mathf.Sign(speedStd)
            * moveSpeedCurve.Evaluate(Mathf.Abs(speedStd))
            * (speedStd > 0 ? maxSpeed : maxReverseSpeed)
        );
        yawSpeed = (
            Mathf.Sign(yawSpeedStd)
            * yawSpeedCurve.Evaluate(MathF.Abs(yawSpeedStd))
            * yawMaxSpeed *
            (1 - yawFalloffCurve.Evaluate(speedStd) * yawFalloffByMomentum * 2)
        );

        // Rotation:

        pitch = (
            Mathf.Sign(pitchStd)
            * pitchCurve.Evaluate(MathF.Abs(pitchStd))
            * (pitchStd < 0 ? pitchMaxAngle : -pitchMinAngle)
        );

        lean = -yawSpeed / yawMaxSpeed * leanAmount;

        yaw += yawSpeed * Time.deltaTime;

        targetRotation = Quaternion.Euler(pitch, yaw, 0);
        rb.MoveRotation(targetRotation);

        var leanInterp = Mathf.LerpAngle(
            witchModel.localEulerAngles.z,
            lean, leanSpeed * Time.deltaTime);

        witchModel.localEulerAngles = new Vector3(0, 0, leanInterp);

        // Position:

        if (climbInput > 0)
            speed += speed * Mathf.Abs(pitchStd) * climbSpeedMod;
        else
            speed += speed * Mathf.Abs(pitchStd) * diveSpeedMod;

        velocity = transform.rotation * new Vector3(0, 0, speed);

        rb.linearVelocity = velocity;
    }

    internal void HitOwnLimb(Orb orb)
    {
        if (state == WitchState.Inactive)
            return;

        Debug.Log($"Player hit own snake limb: {orb}");

        SnakeManager.Instance.SnakeHit();
        orb.OnCollided(this);
        BounceOff(orb.transform);
    }

    private void BounceOff(Transform source)
    {
        Debug.Log($"BounceOff({source})");
        StartTumble();

        rb.linearVelocity = Vector3.zero;
        rb.AddExplosionForce(
            bounceForcePos, source.position, 0, bounceForceLift, bounceForceMode);
        tumbleAngle = new Vector3(
                UnityEngine.Random.Range(-bounceForceAngle, bounceForceAngle),
                UnityEngine.Random.Range(-bounceForceAngle, bounceForceAngle),
                UnityEngine.Random.Range(-bounceForceAngle, bounceForceAngle));

        SFXPlayer.Instance.PlayBounce();
    }

    private void StartTumble()
    {
        Debug.Log($"StartTumble()");
        tumbleStartTime = Time.time;
        ChangeState(WitchState.Tumbling);
    }

    private void StopTumble()
    {
        Debug.Log($"StopTumble()");
        tumbleStartTime = -1f;
        ChangeState(WitchState.Playable);
    }


    internal void CollectPrey(Prey prey)
    {
        GameManager.Instance.PreyCollected(prey);
    }

    internal void Activate()
    {
        ChangeState(WitchState.Playable);
    }
}

public enum WitchState
{
    Playable, Tumbling, Inactive
}

