using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Source")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource ambienceSource;

    [Header("Audio Mixer")]
    [Tooltip("Parámetros expuestos requeridos: MasterVol, MusicVol, AmbienceVol, SFXVol")]
    [SerializeField] private AudioMixer audioMixer;

    [Header("Audio Clips")]
    [SerializeField] private AudioClip backgroundMusic;
    [SerializeField] private AudioClip sfxAmbience;
    [SerializeField] private AudioClip sfxClickButton;
    [SerializeField] private AudioClip sfxPlayerDamage;
    [SerializeField] private AudioClip sfxPlayerDeath;
    [SerializeField] private AudioClip sfxStartGame;

    [Header("Balance de Ambiente")]
    [Tooltip("El ambiente es solo de apoyo, así que suena a este % del volumen de música (0.5 = mitad de fuerte)")]
    [Range(0f, 1f)]
    [SerializeField] private float ambienceRelativeLevel = 0.35f;

    [Header("Música por Escena")]
    [Tooltip("Escribe el nombre EXACTO de la escena (como aparece en Build Settings) y arrastra la canción que debe sonar ahí. Si una escena no está en esta lista, sigue sonando la que ya estaba.")]
    [SerializeField] private List<SceneMusic> sceneMusicList = new();

    [Serializable]
    private class SceneMusic
    {
        public string sceneName;
        public AudioClip musicClip;
    }

    void Awake()
    {
        //PlayerPrefs.DeleteAll();      //<--- Descomentar en caso de que se corrompan las player prefs
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
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
    }

    void Start()
    {
        LoadVolumeSettings();

        PlayBackgroundMusic(backgroundMusic);
        PlayAmbienceSound(sfxAmbience);
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SceneMusic match = sceneMusicList.Find(entry => entry.sceneName == scene.name);

        if (match != null && match.musicClip != null)
            PlayBackgroundMusic(match.musicClip);
    }

    // ----- Música y ambiente -----

    public void PlayBackgroundMusic(AudioClip clip)
    {

        if (clip == null || musicSource == null) 
        {
            return;
        }
        if (musicSource.clip == clip && musicSource.isPlaying) 
        {
            return;
        }

        musicSource.clip = clip;
        musicSource.loop = true;
        musicSource.Play();
    }

    public void PlayAmbienceSound(AudioClip clip)
    {
        if (clip == null || ambienceSource == null) return;
        if (ambienceSource.clip == clip && ambienceSource.isPlaying) return;

        ambienceSource.clip = clip;
        ambienceSource.loop = true;
        ambienceSource.Play();
    }

    // ----- SFX -----
    // Método genérico + wrappers con nombre, para que sea fácil de llamar
    // desde otros scripts sin exponer AudioClips fuera del AudioManager.

    private void PlaySfx(AudioClip clip)
    {
        if (sfxSource != null && clip != null)
            sfxSource.PlayOneShot(clip);
    }

    public void PlaySFXClickButton()  => PlaySfx(sfxClickButton);
    public void PlaySFXPlayerDamage() => PlaySfx(sfxPlayerDamage);
    public void PlaySFXPlayerDeath()  => PlaySfx(sfxPlayerDeath);
    public void PlaySFXStartGame()    => PlaySfx(sfxStartGame);

    // ----- Volumen (llamado desde OptionsManager) -----
    // El slider manda un valor lineal 0-1; el AudioMixer trabaja en decibelios,
    // por eso la conversión con Log10. -80dB se considera silencio total.
    public void SetMasterVolume(float value)
    {
        SetMixerVolume("MasterVol", value);
        PlayerPrefs.SetFloat("MasterVolume", value);
    }

    public void SetMusicVolume(float value)
    {
        // Música y ambiente comparten el mismo slider en Options, pero el ambiente
        // se escala por ambienceRelativeLevel para que siempre suene más bajo (es solo apoyo).
        SetMixerVolume("MusicVol", value);
        PlayerPrefs.SetFloat("MusicVolume", value);
    }

    public void SetSfxVolume(float value)
    {
        SetMixerVolume("SFXVol", value);
        PlayerPrefs.SetFloat("SfxVolume", value);
    }

    public void SetAmbienceVolume(float value)
    {
        SetMixerVolume("AmbienceVol", value * ambienceRelativeLevel);
    }

    private void SetMixerVolume(string exposedParameter, float linearValue)
    {
        if (audioMixer == null) 
        {
            Debug.Log("Mixer nulo");
            return;
        }
        
        float dB;
        // Si el valor del slider es exactamente 0 o cercano a 0, aplicamos silencio absoluto (-80dB)
        if (linearValue <= 0.0001f)
        {
            dB = -80f;
        }
        else
        {
            dB = Mathf.Log10(linearValue) * 20f;
        }

        audioMixer.SetFloat(exposedParameter, dB);
    }

    private void LoadVolumeSettings()
    {
        float masterVol = PlayerPrefs.GetFloat("MasterVolume", 1f);
        float musicVol = PlayerPrefs.GetFloat("MusicVolume", 1f);
        float sfxVol = PlayerPrefs.GetFloat("SfxVolume", 1f);

        SetMixerVolume("MasterVol", masterVol);
        SetMixerVolume("MusicVol", musicVol);
        SetMixerVolume("AmbienceVol", musicVol * ambienceRelativeLevel);
        SetMixerVolume("SFXVol", sfxVol);
    }
}