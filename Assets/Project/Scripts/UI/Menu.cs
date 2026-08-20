using UnityEngine;
using UnityEngine.UI;

public class Menu : MonoBehaviour
{
    [SerializeField] private Button startGameButton;
    [SerializeField] private Button howToPlayButton;
    [SerializeField] private Button optionsButton;
    [SerializeField] private Button creditsButton;

    private void Start()
    {
        startGameButton.onClick.AddListener(StartGame);
        howToPlayButton.onClick.AddListener(HowToPlay);
        optionsButton.onClick.AddListener(Options);
        creditsButton.onClick.AddListener(Credits);
    }

    private void StartGame()
    {
        print("StartGame");
        StartCoroutine(Fade.Instance.FadeOut());
        // Logica para obtener el ultimo nivel completado
        // FlowManager.Instance.GoToScene("");
    }

    private void HowToPlay()
    {
        print("HowToPlay");
        //TODO: Canvas
    }

    private void Options()
    {
        print("Options");
        //TODO: Canvas
    }

    private void Credits()
    {
        print("Credits");
        //TODO: Canvas
    }
}
