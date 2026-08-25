using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }
    private const string KEY_LAST_LEVEL_COMPLETED = "LastLevelCompleted";
    private const string KEY_ATTEMPTS_LEFT = "AttemptsRemaining";
    private const string KEY_LAST_CHECKPOINT = "LastCheckpoint";
    private const int DEFAULT_ATTEMPTS = 5;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    // 0 = ningún nivel completado, 1 = Nivel 1 completo, 2 = Nivel 2 completo, etc.
    public int LastLevelCompleted
    {
        get => PlayerPrefs.GetInt(KEY_LAST_LEVEL_COMPLETED, 0);
        set
        {
            PlayerPrefs.SetInt(KEY_LAST_LEVEL_COMPLETED, value);
            PlayerPrefs.Save();
        }
    }

    public int AttemptsRemaining
    {
        get => PlayerPrefs.GetInt(KEY_ATTEMPTS_LEFT, 0);
        set
        {
            PlayerPrefs.SetInt(KEY_ATTEMPTS_LEFT, Mathf.Clamp(value, 0, DEFAULT_ATTEMPTS)); PlayerPrefs.Save();
        }
    }

    public bool LastCheckpoint
    {
        get => PlayerPrefs.GetInt(KEY_LAST_CHECKPOINT, 0) == 1;
        set
        {
            PlayerPrefs.SetInt(KEY_LAST_CHECKPOINT, value ? 1 : 0); PlayerPrefs.Save();
        }
    }

    public void ResetProgress()
    {
        PlayerPrefs.DeleteKey(KEY_LAST_LEVEL_COMPLETED);
        PlayerPrefs.DeleteKey(KEY_ATTEMPTS_LEFT);
        PlayerPrefs.DeleteKey(KEY_LAST_CHECKPOINT);
        PlayerPrefs.Save();
    }
}