using UnityEngine;
using UnityEngine.Audio;
using static UnityEngine.Rendering.DebugUI;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    public AudioMixer mixer;

    private void Awake()
    {
        Instance = this;

        SetMasterVolume(PlayerPrefs.GetFloat("MasterVolume", 1f));
        SetMusicVolume(PlayerPrefs.GetFloat("MusicVolume", 1f));
        SetSFXVolume(PlayerPrefs.GetFloat("SFXVolume", 1f));

    }

    public float ToDecibel(float value)
    {
        return Mathf.Log10(Mathf.Clamp(value, 0.0001f, 1f)) * 20f;
    }

    public float FromDecibel(float db)
    {
        return Mathf.Pow(10f, db / 20f);
    }

    public void SetMasterVolume(float value)
    {
        mixer.SetFloat("Master", ToDecibel(value));
        PlayerPrefs.SetFloat("MasterVolume", value);

    }

    public void SetSFXVolume(float value)
    {
        mixer.SetFloat("SFX", ToDecibel(value));
        PlayerPrefs.SetFloat("SFXVolume", value);
    }

    public void SetMusicVolume(float value)
    {
        mixer.SetFloat("Music", ToDecibel(value));
        PlayerPrefs.SetFloat("MusicVolume", value);
    }

    public float getMasterVolume()
    {
        mixer.GetFloat("Master", out float db);
        return FromDecibel(db);
    }

    public float getSFXVolume()
    {
        mixer.GetFloat("SFX", out float db);
        return FromDecibel(db);
    }

    public float getMusicVolume()
    {
        mixer.GetFloat("Music", out float db);
        return FromDecibel(db);
    }
}
