using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FlowManager : MonoBehaviour
{
    public static FlowManager Instance { get; private set; }
    public readonly List<string> _scenesWithLoading = new() { "Level 1", "Level 2", "Chinatown" }; // Add the names of the scenes that require a loading screen here.
    public static string targetScene = "Menu";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        //-----------
    }

    public void GoToScene(string sceneName)
    {
        if (_scenesWithLoading.Contains(sceneName))
        {
            targetScene = sceneName;
            SceneManager.LoadScene("Loading");
            return;
        }
        else
        {
            SceneManager.LoadScene(sceneName);

        }
    }

    public void ExitGame()
    {
        Application.Quit();
    }

}
