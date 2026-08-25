using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private BombThrower bombThrower;
    [SerializeField] private TextMeshProUGUI attemptsText;
    [SerializeField] private TextMeshProUGUI cooldownText;
    [SerializeField] private Button menuButton;
    private Image iconImage;
    private GameObject parentObject;

    void Start()
    {
        playerStats.OnAttemptChanged += UpdateAttemptsUI;
        bombThrower.CooldownLoggerEvent += UpdateCooldownUI;
        attemptsText.text = playerStats.CurrentAttempts.ToString();
        cooldownText.text = "Q";
        parentObject = cooldownText.gameObject.transform.parent.gameObject;
        iconImage = parentObject.GetComponentInChildren<Image>();
    }

    private void UpdateAttemptsUI()
    {
        attemptsText.text = playerStats.CurrentAttempts.ToString();
    }

    private void UpdateCooldownUI()
    {
        print("Current charges: " + bombThrower.CurrentCharges);
        if (bombThrower.CurrentCharges == 0)
        {
            parentObject.SetActive(false);
            return;
        }
        parentObject.SetActive(true);
        if (bombThrower.CooldownTimer <= 0)
        {
            cooldownText.text = "Q";
            iconImage.color = new Color(1f, 1f, 1f, 1f); // Set the icon color to white
            return;
        }
        cooldownText.text = bombThrower.CooldownTimer.ToString("F0");
        iconImage.color = new Color(0f, 0f, 0f, 0.5f); // Set the icon color to semi-transparent black
    }

    public void SetActive(bool isActive)
    {
        gameObject.SetActive(isActive);
    }

    private void GoToMenu()
    {
        FlowManager.Instance.GoToScene("Menu");
    }

}
