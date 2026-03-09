using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCamera : MonoBehaviour
{
    // Assumes a Camera child

    public AnimationCurve cameraSmoothing = CurvePresets.Log();

    public float defaultOffsetX = 0.5f;

    public float maxPositionLag = -1f;
    public float maxYaw = -40f;
    public float maxPitch = -25f;

    public float followTime = 0.2f;
    public float yawTime = 0.2f;
    public float pitchTime = 0f;

    public float maxMovementX = 1f;
    private float newPitchPos;

    public PlayerController player;
    public Transform pivot;
    public Vector3 startPos;

    public float mouseSensitivityX = 0.3f;
    public float mouseSensitivityY = 0.1f;
    public float mouseSnapBackSpeed = 400f;

    public bool mouseLook = true;
    [ReadOnly]
    public float mousePitch;
    [ReadOnly]
    public float mouseYaw;

    Vector3 startRot;

    Vector3 smoothVelocityP;
    float velocityYaw;
    float velocityPitch;

    void Start()
    {
        player = player != null ? player : GetComponentInParent<PlayerController>();

        pivot = pivot != null ? pivot : GetComponentInChildren<Camera>().transform;
        if (pivot.parent != transform)
            pivot = pivot.parent;
        
        startPos = transform.localPosition;
        startRot = pivot.localEulerAngles;

        smoothVelocityP = new Vector3();

        Debug.Log($"Playercamera Start. Player: {player}");
    }

    void Update()
    {
        /*var rm = player.rotationMomentum;
        wiggleX = Mathf.Sign(rm) * cameraSmoothing.Evaluate(Mathf.Abs(rm)) * wiggleAmount;
        
        transform.localPosition = new Vector3(startPos.x + wiggleX, startPos.y, startPos.z);
        
        var eu = transform.eulerAngles;
        eu.x = 0;
        transform.eulerAngles = eu;*/
    }

    private void LateUpdate()
    {
        if (player == null)
            return;

        // Offset
        // Move backwards as player moves forward:
        var offset = transform.InverseTransformVector(
            -player.velocity * maxPositionLag * Time.deltaTime
        );

        var offsetDegree = /*player.speedStd * */ player.yawSpeedStd;
        newPitchPos = UseCurve(cameraSmoothing, offsetDegree) * maxMovementX;

        // Move into the turn as player turns:
        offset.x += newPitchPos;

        // Move with or against the players forward movement:
        offset.z = -player.speedStd * maxPositionLag;

        // Sit at one side when not rotating:
        offset.x += (1-MathF.Abs(player.yawSpeedStd)) * defaultOffsetX;

        transform.localPosition = Vector3.SmoothDamp(
            transform.localPosition,
            startPos + offset,
            ref smoothVelocityP,
            followTime
            );

        // Auto-switch camera default side to the same side as most recent turn:
        if (MathF.Abs(transform.localPosition.x) > 0.1f)
        {
            defaultOffsetX = MathF.Abs(defaultOffsetX) * MathF.Sign(transform.localPosition.x);
        }
  
        // Yaw and Pitch 

        float currentYaw = pivot.localEulerAngles.y;

        var newYaw = Mathf.SmoothDampAngle(
            currentYaw,
            startRot.y + player.yawSpeed / player.yawMaxSpeed * maxYaw,
            ref velocityYaw,
            yawTime);

        float currentPitch = pivot.localEulerAngles.x;

        var newPitch = Mathf.SmoothDampAngle(
            currentPitch,
            startRot.x + player.pitch / player.pitchMaxAngle * maxPitch,
            ref velocityPitch,
            pitchTime);

        newYaw   = Keep0(newYaw);
        newPitch = Keep0(newPitch);

        if (newPitch != currentPitch || newYaw != currentYaw)
        {
            var euler = transform.eulerAngles;
            euler.x = 0;
            transform.eulerAngles = euler;

            if (player.clickInput)
                newPitch = 0;

            pivot.localEulerAngles = new Vector3(newPitch, newYaw, 0f);
        }

        if (player.clickInput || mousePitch != 0f || mouseYaw != 0f)
        {
            if (!player.clickInput)
            {
                mousePitch = Mathf.MoveTowards(mousePitch, 0f, mouseSnapBackSpeed * Time.deltaTime);
                mouseYaw = Mathf.MoveTowards(mouseYaw, 0f, mouseSnapBackSpeed * Time.deltaTime);
            }

            float mouseX = player.lookInput.x * mouseSensitivityX;
            float mouseY = player.lookInput.y * mouseSensitivityY;

            mouseYaw += mouseX;
            mousePitch -= mouseY;
            mousePitch = Mathf.Clamp(mousePitch, -90f, 90f);

            //Debug.Log($"Mouse look: {mouseX} {mouseY}");
            transform.localRotation = Quaternion.Euler(
                0f, mouseYaw, 0f);
            transform.rotation = Quaternion.Euler(
                mousePitch, 
                transform.eulerAngles.y, 
                transform.eulerAngles.z);
        }
    }

    public void SetMouseLook(bool mouseLook)
    {
        this.mouseLook = mouseLook;
        if (!mouseLook)
        {
            mousePitch = 0f;
            mouseYaw = 0f;
        }
    }

    static float UseCurve(AnimationCurve curve, float x)
    {
        return Mathf.Sign(x) * curve.Evaluate(MathF.Abs(x));
    }
    
    static public float Keep0(float f)
    {
        return Mathf.Abs(f) < 0.001 ? 0 : f;
    }
}
