using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Menu : MonoBehaviour
{
    [Header("Main Menu Buttons")]
    [SerializeField] private Button startGameButton;
    [SerializeField] private Button howToPlayButton;
    [SerializeField] private Button optionsButton;
    [SerializeField] private Button creditsButton;
    [SerializeField] private Button exitGameButton;

    [Header("Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject howToPlayPanel;
    [SerializeField] private GameObject optionsPanel;
    [SerializeField] private GameObject creditsPanel;

    [Header("Back Buttons")]
    [SerializeField] private Button howToPlayBackButton;
    [SerializeField] private Button optionsBackButton;
    [SerializeField] private Button creditsBackButton;

    private void Start()
    {
        // Main Menu
        startGameButton.onClick.AddListener(StartGame);
        howToPlayButton.onClick.AddListener(HowToPlay);
        optionsButton.onClick.AddListener(Options);
        creditsButton.onClick.AddListener(Credits);
        if (exitGameButton != null) exitGameButton.onClick.AddListener(ExitGame);
        // Back buttons
        howToPlayBackButton.onClick.AddListener(BackToMainMenu);
        optionsBackButton.onClick.AddListener(BackToMainMenu);
        creditsBackButton.onClick.AddListener(BackToMainMenu);

        // Estado inicial
        ShowMainMenu();
        GetSceneTarget();
        StartCoroutine(Fade.Instance.FadeIn());
    }

    private string GetSceneTarget()
    {
        int lastLevelCompleted = SaveManager.Instance.LastLevelCompleted;
        bool lastCheckpoint = SaveManager.Instance.LastCheckpoint;
        switch (lastLevelCompleted)
        {
            case 0:
                if (lastCheckpoint)
                {
                    print("Level 1, Minigame.");
                    return "Minigame 1";
                }
                else
                {
                    print("Start Level 1.");
                    return "Level 1";
                }
            case 1:
                if (lastCheckpoint)
                {
                    print("Level 2, Minigame.");
                    return "Minigame 2";
                }
                else
                {
                    print("Start Level 2.");
                    return "Level 2";
                }
            case 2:
                if (lastCheckpoint)
                {
                    print("Level 3, Minigame.");
                    return "Minigame 3";
                }
                else
                {
                    print("Start Level 3.");
                    return "Level 3";
                }
            default:
                print("Start Level 1.");
                return "Level 1";
        }
    }

    private void StartGame()
    {
        print("StartGame");
        string nextScene = GetSceneTarget();
        StartCoroutine(StartGameCoroutine(nextScene));
    }

    private IEnumerator StartGameCoroutine(string nextScene)
    {
        StartCoroutine(Fade.Instance.FadeOut());
        yield return new WaitForSeconds(1f);
        FlowManager.Instance.GoToScene(nextScene);
    }

    private void HowToPlay()
    {
        print("HowToPlay");
        ShowPanel(howToPlayPanel);
    }

    private void Options()
    {
        print("Options");
        ShowPanel(optionsPanel);
    }

    private void Credits()
    {
        print("Credits");
        ShowPanel(creditsPanel);
    }

    private void ExitGame()
    {
        print("ExitGame");
        FlowManager.Instance.ExitGame();
    }

    private void BackToMainMenu()
    {
        print("Back to Main Menu");
        ShowMainMenu();
    }

    private void ShowMainMenu()
    {
        mainMenuPanel.SetActive(true);
        howToPlayPanel.SetActive(false);
        optionsPanel.SetActive(false);
        creditsPanel.SetActive(false);
    }

    private void ShowPanel(GameObject panelToShow)
    {
        mainMenuPanel.SetActive(false);
        howToPlayPanel.SetActive(false);
        optionsPanel.SetActive(false);
        creditsPanel.SetActive(false);
        // Activar el panel correspondiente
        panelToShow.SetActive(true);
    }
}
