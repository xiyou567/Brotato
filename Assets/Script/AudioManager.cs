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

        AudioSource[] srcs = GetComponents<AudioSource>();
        if (srcs.Length >= 1) musicSource = srcs[0];
        else musicSource = gameObject.AddComponent<AudioSource>();
        if (srcs.Length >= 2) sfxSource = srcs[1];
        else sfxSource = gameObject.AddComponent<AudioSource>();

        musicSource.spatialBlend = 0f;
        sfxSource.spatialBlend = 0f;

        MusicVolume = PlayerPrefs.GetFloat(KEY_MUSIC, 1f);
        SfxVolume = PlayerPrefs.GetFloat(KEY_SFX, 1f);
        ApplyVolumes();
    }

    private void Start()
    {

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
