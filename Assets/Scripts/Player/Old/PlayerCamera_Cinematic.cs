using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamera_Cinematic : MonoBehaviour
{

    public AnimationCurve cameraSmoothing = AnimationCurve.EaseInOut(0, 0, 1, 1);

    public float maxPositionLag = 2f;
    public float maxRotationY = 45f;
    public float maxRotationX = 45f;

    public float followTime = 5f;
    public float rotationTimeY = 5f;
    public float rotationTimeX = 5f;

    public float wiggleMagnitude = 60f;
    public float maxMovementX = 3.5f;
    private float newXPos;

    PlayerController_Cinematic player;
    Vector3 startPos;
    Vector3 startRot;

    private Vector3 smoothVelocityP;
    private float smoothVelocityRY;
    private float smoothVelocityRX;

    void Start()
    {
        player = GetComponentInParent<PlayerController_Cinematic>();
        startPos = transform.localPosition;
        startRot = transform.localEulerAngles;

        smoothVelocityP = new Vector3();

        Debug.Log($"player: {player}");
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


        //       if (transform.localPosition != startPos)
        var offset = transform.InverseTransformVector(
            -player.velocity * maxPositionLag
        );

        var offsetDegree = player.momentum * player.rotationMomentum;
        newXPos = MathF.Sign(offsetDegree) * cameraSmoothing.Evaluate(offsetDegree) * maxMovementX;
        offset.x += newXPos;

        transform.localPosition = Vector3.SmoothDamp(
            transform.localPosition,
            startPos + offset,
            ref smoothVelocityP,
            followTime
            );

        //        if (transform.localEulerAngles != startRot)
        // var rotationOffset = startRot;
        // rotationOffset.y -= player.yawSpeed * maxRotationLag;

        float currentY = transform.localEulerAngles.y;

        var newY = Mathf.SmoothDampAngle(
            currentY,
            startRot.y + player.yawSpeed * maxRotationY,
            ref smoothVelocityRY,
            rotationTimeY);

        float currentX = transform.localEulerAngles.x;

        var newX = Mathf.SmoothDampAngle(
            currentX,
            startRot.x + player.pitchSpeed * maxRotationX,
            ref smoothVelocityRX,
            rotationTimeX);

       /* Debug.Log($"(cam) player.yawSpeed: {player.yawSpeed}");
        Debug.Log($"(cam) player.pitchSpeed: {player.pitchSpeed}");
        Debug.Log($"(cam) offset: {offset}");
        Debug.Log($"(cam) newX: {newX}");
        Debug.Log($"(cam) newY: {newY}");*/

        transform.localRotation = Quaternion.Euler(newX, newY, 0f);


        //  var wiggleX = player.yawSpeed + player.velocity.magnitude;

        //Quaternion.Lerp(transform.localEulerAngles, rotationOffset, followSpeed * Time.deltaTime);
        //       Quaternion.RotateTowards(transform.rotation, player.transform.rotation, followSpeed * Time.deltaTime);


       // var rm = (player.velocity.magnitude + Mathf.Abs(newY)) / wiggleMagnitude;
       // wiggleX = Mathf.Sign(newY) * cameraSmoothing.Evaluate(player.momentum * player.rotationMomentum) * wiggleAmount;

     //   transform.localPosition = new Vector3(startPos.x + wiggleX, startPos.y, startPos.z);
    }

    static float UseCurve(AnimationCurve curve, float x)
    {
        return Mathf.Sign(x) * curve.Evaluate(x);
    }

}
