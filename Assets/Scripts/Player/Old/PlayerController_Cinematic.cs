using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class PlayerController_Cinematic : MonoBehaviour
{
    public float moveMaxSpeed = 5f;
    public float yawMaxHandling = 5f;
    public float climbSpeedModifier = 4f;

    public float pitchHandling = 50f;

    public AnimationCurve moveSpeedCurve = AnimationCurve.Linear(0, 0, 1, 1);
    public AnimationCurve yawSpeedCurve = AnimationCurve.Linear(0, 0, 1, 1);

    public AnimationCurve yawFalloffByMomentum = AnimationCurve.Linear(0, 0, 1, 0.4f);

    public float moveAcceleration = 0.5f;
    public float rotationAcceleration = 0.5f;

    public float momentum = 0f;
    public float rotationMomentum = 0f;

    private Vector2 moveInput;
    private float climbInput;

    public float speed = 0f;
    public float yawSpeed = 0f;
    public float pitchSpeed = 0f;

    public Vector3 velocity;

    private void Awake()
    {
        Debug.Log($"Player Awake");
    }
    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
        Debug.Log($"Moved: {moveInput}");
        Debug.Log($"momentum: {momentum}");
        Debug.Log($"rotationMomentum: {rotationMomentum}");
    }
    public void OnClimb(InputValue value)
    {
        climbInput = value.Get<float>();
        Debug.Log($"Climbed: {climbInput}");
    }

    private void Update()
    {
        momentum         = Mathf.MoveTowards(momentum,         moveInput.y, moveAcceleration     * Time.deltaTime);
        rotationMomentum = Mathf.MoveTowards(rotationMomentum, moveInput.x, rotationAcceleration * Time.deltaTime);

        speed = (
            Mathf.Sign(momentum)
            * moveSpeedCurve.Evaluate(Mathf.Abs( momentum))
            * moveMaxSpeed
        );
        yawSpeed = (
            Mathf.Sign(rotationMomentum)
            * yawSpeedCurve.Evaluate(MathF.Abs(rotationMomentum))
            * yawMaxHandling * 
            (1 - yawFalloffByMomentum.Evaluate(momentum))
        );

        var forward = speed * (1 - Mathf.Abs(climbInput) * 0.5f) * Time.deltaTime;
        var up = speed * climbInput * 0.5f * climbSpeedModifier * Time.deltaTime;
        var yaw = yawSpeed * Time.deltaTime;

        velocity = transform.rotation * new Vector3(0, up, forward);
        transform.position += velocity;

        pitchSpeed = momentum * -climbInput * pitchHandling;

        Vector3 euler = transform.localEulerAngles;
        euler.y += yaw;
        euler.x = Mathf.MoveTowardsAngle(euler.x, momentum * -climbInput * 55, pitchHandling * Time.deltaTime);
        transform.localEulerAngles = euler;
    }

    static public float Keep0(float f)
    {
        return Mathf.Abs(f) < 0.001 ? 0 : f;
    }
}
