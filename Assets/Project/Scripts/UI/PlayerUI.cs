using TMPro;
using UnityEngine;

public class PlayerUI : MonoBehaviour
{
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private BombThrower bombThrower;
    [SerializeField] private TextMeshProUGUI attemptsText;
    [SerializeField] private TextMeshProUGUI cooldownText;

    void Start()
    {
        playerStats.OnAttemptChanged += UpdateAttemptsUI;
        bombThrower.CooldownLoggerEvent += UpdateCooldownUI;
        attemptsText.text = playerStats.CurrentAttempts.ToString();
        cooldownText.text = "Q";
    }

    private void UpdateAttemptsUI()
    {
        attemptsText.text = playerStats.CurrentAttempts.ToString();
    }

    private void UpdateCooldownUI()
    {
        if (bombThrower.CurrentCharges == 0)
        {
            cooldownText.gameObject.transform.parent.gameObject.SetActive(false);
            return;
        }
        cooldownText.gameObject.transform.parent.gameObject.SetActive(true);
        if (bombThrower.CooldownTimer <= 0)
        {
            cooldownText.text = "Q";
            return;
        }
        cooldownText.text = bombThrower.CooldownTimer.ToString("F0");
    }

    public void SetActive(bool isActive)
    {
        gameObject.SetActive(isActive);
    }

}
