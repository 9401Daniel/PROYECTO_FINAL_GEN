using UnityEngine;
using UnityEngine.UI;

public class GameOver : MonoBehaviour
{
    public static GameOver Instance { get; private set; }
    [SerializeField] private Button RetryButton;
    [SerializeField] private Button MainMenuButton;
    private GameObject gameOverPanel;

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
        gameOverPanel = transform.GetChild(0).gameObject;
        HideGameOver();
    }

    public void ShowGameOver()
    {
        gameOverPanel.SetActive(true);
    }

    public void HideGameOver()
    {
        gameOverPanel.SetActive(false);
        SaveManager.Instance.ResetProgress();
    }

    private void Start()
    {
        RetryButton.onClick.AddListener(Retry);
        MainMenuButton.onClick.AddListener(MainMenu);
    }

    private void Retry()
    {
        HideGameOver();
        FlowManager.Instance.GoToScene("Level 1");
    }

    private void MainMenu()
    {
        HideGameOver();
        FlowManager.Instance.GoToScene("Menu");
    }

}
