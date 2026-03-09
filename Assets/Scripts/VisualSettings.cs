using System;
using UnityEngine;

[Serializable]
//[CreateAssetMenu(fileName = "VisualSettings", menuName = "ScriptableObjects/VisualSettings", order = 1)]
public class VisualSettings// : ScriptableObject
{
//    [Header("Color Settings")]
    public Color color = Color.white;

  //  [Header("Emission Settings")]
    public bool emissionEnabled = false;
    public Color emissionColor = Color.white;
    public float emissionIntensity = 1f;

    public float mixPriority = 1f;

    public VisualSettings()
    {
    }

    public VisualSettings(VisualSettings vs)
    {
        color = vs.color;
        emissionEnabled = vs.emissionEnabled;
        emissionColor = vs.emissionColor;
        emissionIntensity = vs.emissionIntensity;
    }

    public override string ToString()
    {
        return $"VisualSettings(color: {color}, emissionEnabled: {emissionEnabled}, " +
            $"emissionColor: {emissionColor}, emissionIntensity: {emissionIntensity}, mixPriority: {mixPriority})";
    }

    public void ApplyToMaterial(Material target)
    {
        target.color = color;

        if (emissionEnabled)
            target.EnableKeyword("_EMISSION");
        else
            target.DisableKeyword("_EMISSION");

        target.SetColor("_EmissionColor", emissionColor * emissionIntensity);
    }

    public static VisualSettings RandomMix(VisualSettings[] list)
    {
        //Debug.Log($"RandomMix({list} length {list.Length})");
        if (list.Length == 0) { 
          //  Debug.Log($"-> null");
            return null;
        }
        else if (list.Length == 1)
        {
            //Debug.Log($"-> {list[0]}");
            return list[0];
        }
        else
        {
            int iA = UnityEngine.Random.Range(0, list.Length - 1);
            int iB = UnityEngine.Random.Range(0, list.Length - 2);
            iB = iB >= iA ? iB + 1 : iB;

            var vsA = list[iA];
            var vsB = list[iB];

            var rand = UnityEngine.Random.value;

            if (vsA.mixPriority != vsB.mixPriority && vsA.mixPriority + vsB.mixPriority > 0 )
            {
                // Standardize ratio to [-1, 1] and use this to lerp rand
                var weight = 2 * vsB.mixPriority / (vsA.mixPriority + vsB.mixPriority) - 1;
                var target = vsA.mixPriority > vsB.mixPriority ? 0 : 1;

                rand = Mathf.LerpUnclamped(rand, target, Mathf.Abs(weight));
            }

            var mix = VisualSettings.Lerp(vsA, vsB, rand);

           // Debug.Log($"-> {mix}");
            return mix;
        }
    }

    public static VisualSettings Lerp(VisualSettings a, VisualSettings b, float t)
    {
        //Debug.Log($"VisualSettings.LerpHueIntensity(\n{a}, \n{b}, \n{t})");

        if (a == null)
            throw new ArgumentNullException("a is null");
        if (b == null)
            throw new ArgumentNullException("b is null");

        t = Mathf.Clamp01(t);
        if (t == 0) return a;
        if (t == 1) return b;

        VisualSettings newVis = new();

        newVis.color = LerpColor(a.color, b.color, t);
        newVis.emissionEnabled = (t <= 0.5f) ? a.emissionEnabled : b.emissionEnabled;
        newVis.emissionColor = LerpColor(a.emissionColor, b.emissionColor, t);
        newVis.emissionIntensity = Mathf.Lerp(a.emissionIntensity, b.emissionIntensity, t);

        //Debug.Log ($"-> {newVis}");
        return newVis;
    }

    public static Color LerpColor(Color a, Color b, float t)
    {
        Color c = new();

        Color.RGBToHSV(a, out float aH, out float aS, out float aV);
        Color.RGBToHSV(b, out float bH, out float bS, out float bV);

        var hue = Mathf.Lerp(aH, bH, t);
        var sat = Mathf.Lerp(aS, bS, t);
        var val = Mathf.Lerp(aV, bV, t);
        var alp = Mathf.Lerp(a.a, b.a, t);

        c = Color.HSVToRGB(hue, sat, val);
        c.a = alp;

        return c;
    }
}
