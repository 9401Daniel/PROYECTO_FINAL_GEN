using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FlowManager : MonoBehaviour
{
    public static FlowManager Instance { get; private set; }
    private readonly List<string> _scenesWithLoading = new() { "Central Park", "Downtown", "Chinatown" };
    public static string targetScene;

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
}
