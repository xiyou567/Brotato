using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    private const string KEY_MUSIC = "Brotato_MusicVolume";
    private const string KEY_SFX = "Brotato_SfxVolume";

    private AudioSource musicSource;
    private AudioSource sfxSource;

    public float MusicVolume { get; private set; } = 1f;
    public float SfxVolume { get; private set; } = 1f;

    private void Awake()
    {
        // 单例，跨场景常驻
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // 场景里可能只挂了一个 AudioSource，缺的补上
        AudioSource[] srcs = GetComponents<AudioSource>();
        if (srcs.Length >= 1) musicSource = srcs[0];
        else musicSource = gameObject.AddComponent<AudioSource>();
        if (srcs.Length >= 2) sfxSource = srcs[1];
        else sfxSource = gameObject.AddComponent<AudioSource>();

        // 纯 2D 游戏，关掉空间衰减，音量不随听者距离变化
        musicSource.spatialBlend = 0f;
        sfxSource.spatialBlend = 0f;

        MusicVolume = PlayerPrefs.GetFloat(KEY_MUSIC, 1f);
        SfxVolume = PlayerPrefs.GetFloat(KEY_SFX, 1f);
        ApplyVolumes();
    }

    private void Start()
    {
        // Awake 里被判重、已 Destroy 的那一份也会走到 Start，
        // 此时两个音源都还是 null，直接播会 NRE
        if (Instance != this) return;

        PlayMusic("背景音乐");
    }

    public void PlaySound(string clipName)
    {
        AudioClip clip = Resources.Load<AudioClip>("Music/" + clipName);
        if (clip != null)
            sfxSource.PlayOneShot(clip);
    }

    public void PlayMusic(string clipName)
    {
        AudioClip clip = Resources.Load<AudioClip>("Music/" + clipName);
        if (clip == null) return;
        musicSource.clip = clip;
        musicSource.loop = true;
        musicSource.Play();
    }

    public void SetMusicVolume(float v)
    {
        MusicVolume = Mathf.Clamp01(v);
        PlayerPrefs.SetFloat(KEY_MUSIC, MusicVolume);
        ApplyVolumes();
        PlayerPrefs.Save();
    }

    public void SetSfxVolume(float v)
    {
        SfxVolume = Mathf.Clamp01(v);
        PlayerPrefs.SetFloat(KEY_SFX, SfxVolume);
        ApplyVolumes();
        PlayerPrefs.Save();
    }

    private void ApplyVolumes()
    {
        if (musicSource != null) musicSource.volume = MusicVolume;
        if (sfxSource != null) sfxSource.volume = SfxVolume;
    }
}
