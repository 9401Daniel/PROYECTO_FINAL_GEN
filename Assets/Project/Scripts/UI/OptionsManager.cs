using UnityEngine;
using UnityEngine.UI;

public class OptionsManager : MonoBehaviour
{
    [SerializeField] private Slider masterVolume;
    [SerializeField] private Slider musicVolume;
    [SerializeField] private Slider sfxVolume;

    private bool isLoaded = false;

    void Start()
    {
        LoadVolumeSettings();
    }

    private void LoadVolumeSettings()
    {
        isLoaded = false; // Bloqueamos temporalmente para que no interprete esto como un input del usuario

        float master = PlayerPrefs.GetFloat("MasterVolume", 1f);
        float music = PlayerPrefs.GetFloat("MusicVolume", 1f);
        float sfx = PlayerPrefs.GetFloat("SfxVolume", 1f);

        // Asignamos el valor visual a los sliders
        if (masterVolume != null)
        {
            masterVolume.value = master;
        }
        if (musicVolume != null)
        {
            musicVolume.value = music;
        }
        if (sfxVolume != null)
        {
            sfxVolume.value = sfx;
        }

        // Aplicamos al AudioManager
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMasterVolume(master);
            AudioManager.Instance.SetMusicVolume(music);
            AudioManager.Instance.SetAmbienceVolume(music);
            AudioManager.Instance.SetSfxVolume(sfx);
        }

        isLoaded = true; // Ya podemos escuchar al usuario
    }

    public void OnMasterVolumeChanged(float value)
    {
        if (!isLoaded) return; // Si no ha terminado de cargar, ignoramos

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMasterVolume(value);
        }

        PlayerPrefs.SetFloat("MasterVolume", value);
        PlayerPrefs.Save();
    }

    public void OnMusicVolumeChanged(float value)
    {
        if (!isLoaded) return;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMusicVolume(value);
            AudioManager.Instance.SetAmbienceVolume(value); // Mantiene el ambiente ligado a la música
        }

        PlayerPrefs.SetFloat("MusicVolume", value);
        PlayerPrefs.Save();
    }

    public void OnSfxVolumeChanged(float value)
    {
        if (!isLoaded) return;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetSfxVolume(value);
        }

        PlayerPrefs.SetFloat("SfxVolume", value);
        PlayerPrefs.Save();
    }

}