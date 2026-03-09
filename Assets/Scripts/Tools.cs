using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class CurvePresets
{
    public static AnimationCurve Log()
    {
        return new AnimationCurve(
            new Keyframe(0f, 0f, 2f, 2f),
            new Keyframe(1f, 1f)
        );
    }
    public static AnimationCurve Exp()
    {
        return new AnimationCurve(
            new Keyframe(0f, 0f, 0f, 0f),
            new Keyframe(1f, 1f, 2f, 2f)
        );
    }
    public static AnimationCurve InverseExp()
    {
        return new AnimationCurve(
            new Keyframe(0f, 1f, -2f, -2f),
            new Keyframe(1f, 0f, 0f, 0f)
        );
    }
    public static AnimationCurve InverseLog()
    {
        return new AnimationCurve(
            new Keyframe(0f, 1f),
            new Keyframe(1f, 0f, -2f, -2f)
        );
    }
}

public static class Tools
{
    public static float Keep0(float f)
    {
        return Mathf.Abs(f) < 0.001 ? 0 : f;
    }

    public static List<T> GetComponentsInScene<T>() where T : MonoBehaviour
    {
        var list = new List<T>();

        Scene scene = SceneManager.GetActiveScene();
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            var foundOrbs = root.GetComponentsInChildren<T>();
            list.AddRange(foundOrbs);
        }
        return list;
    }
}
