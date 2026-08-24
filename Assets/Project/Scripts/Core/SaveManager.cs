using UnityEngine;

public static class SaveManager
{
    private const string KEY_LAST_LEVEL_COMPLETED = "LastLevelCompleted";
    private const string KEY_ATTEMPTS_LEFT = "AttemptsRemaining";
    private const int DEFAULT_ATTEMPTS = 3;

    // 0 = ningún nivel completado, 1 = Nivel 1 completo, 2 = Nivel 2 completo, etc.
    public static int LastLevelCompleted
    {
        get => PlayerPrefs.GetInt(KEY_LAST_LEVEL_COMPLETED, 0);
        set { PlayerPrefs.SetInt(KEY_LAST_LEVEL_COMPLETED, value); PlayerPrefs.Save(); }
    }

    public static int AttemptsRemaining
    {
        get => PlayerPrefs.GetInt(KEY_ATTEMPTS_LEFT, DEFAULT_ATTEMPTS);
        set { PlayerPrefs.SetInt(KEY_ATTEMPTS_LEFT, Mathf.Max(0, value)); PlayerPrefs.Save(); }
    }

    public static void ResetProgress()
    {
        PlayerPrefs.DeleteKey(KEY_LAST_LEVEL_COMPLETED);
        PlayerPrefs.DeleteKey(KEY_ATTEMPTS_LEFT);
        PlayerPrefs.Save();
    }
}