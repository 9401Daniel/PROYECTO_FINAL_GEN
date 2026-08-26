using System.Collections;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [SerializeField] private MissionDetails missionDetails;
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private string minigameSceneName;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(Fade.Instance.FadeIn());
        missionDetails.OnMissionCountChanged += ValidateMission;
        playerStats.OnAttemptChanged += CheckGameOver;
        DialogueManager.Instance.ShowDialogue();
    }

    private void ValidateMission()
    {
        if (missionDetails.MissionCompleted)
        {
            Debug.Log("Mission completed!");
            SaveManager.Instance.LastCheckpoint = true;
            StartCoroutine(GoToMinigame());
        }
    }

    private void OnDisable()
    {
        missionDetails.OnMissionCountChanged -= ValidateMission;
        playerStats.OnAttemptChanged -= CheckGameOver;
    }

    private void CheckGameOver()
    {
        if (playerStats.CurrentAttempts <= 0)
        {
            playerStats.SetMoving(false);
            GameOver.Instance.ShowGameOver();
        }
    }

    private IEnumerator GoToMinigame()
    {
        StartCoroutine(Fade.Instance.FadeOut());
        yield return new WaitForSeconds(1f);
        FlowManager.Instance.GoToScene(minigameSceneName);
    }
}
