using UnityEngine;

public class Wind : MonoBehaviour
{
    // Changes pitch/volume based on height, then adds based on speed

    public bool muted = false;

    public float pitchSpeedChange = 0.3f; // At max speed
    public float volumeSpeedChange = 0.04f;

    public float pitchTopHeight = 1f; // At 0 speed
    public float pitchBotHeight = 0.5f;

    public float volumeTopHeight = 0.03f;
    public float volumeBotHeight = 0.03f;

    public float heightModTop = 75f;
    public float heightModBot = 0f;

    // Y: 74 -> 1 p. 0.03 v
    // Y: 0 -> .5 p. 0.03 v

    AudioSource windSource;
    PlayerController player;

    void Awake()
    {
        windSource = GetComponent<AudioSource>();
        player = GetComponentInParent<PlayerController>();

        windSource.mute = true;
    }

    private void Start()
    {
        windSource.outputAudioMixerGroup = AudioManager.Instance.mixer.FindMatchingGroups("SFX")[0];
    }

    void Update()
    {
        windSource.mute = muted;

        var speedMod = Mathf.Clamp01(player.speed / player.maxSpeed);
        var heightMod = Mathf.Clamp01((player.transform.position.y - heightModBot) / heightModTop);

        var volume = volumeBotHeight + heightMod * volumeTopHeight;
        var pitch = pitchBotHeight + heightMod * pitchTopHeight;

        volume += volumeSpeedChange * speedMod;
        pitch += pitchSpeedChange * speedMod;

        windSource.volume = volume;
        windSource.pitch = pitch;
    }
}
