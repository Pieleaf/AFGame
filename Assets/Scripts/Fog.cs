using System;
using UnityEngine;

public class Fog : MonoBehaviour
{
    float speed = .1f;
    float yMod = 0.5f;

    void Update()
    {
        transform.position = transform.position + new Vector3(
            (speed + MathF.Abs(transform.position.y + 2) * yMod) * Time.deltaTime, 0, 0);
    }
}
