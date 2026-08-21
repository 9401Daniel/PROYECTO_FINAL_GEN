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

        // Back buttons
        howToPlayBackButton.onClick.AddListener(BackToMainMenu);
        optionsBackButton.onClick.AddListener(BackToMainMenu);
        creditsBackButton.onClick.AddListener(BackToMainMenu);

        // Estado inicial
        ShowMainMenu();
    }

    private void StartGame()
    {
        print("StartGame");
        // Logica para obtener el ultimo nivel completado
        StartCoroutine(StartGameCoroutine());
    }

    private IEnumerator StartGameCoroutine()
    {
        StartCoroutine(Fade.Instance.FadeOut());
        yield return new WaitForSeconds(1f);
        FlowManager.Instance.GoToScene("Level 1");
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

        panelToShow.SetActive(true);
    }
}
