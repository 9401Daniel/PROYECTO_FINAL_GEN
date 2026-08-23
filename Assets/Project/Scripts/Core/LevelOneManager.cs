using System.Collections;
using UnityEngine;

public class LevelOneManager : MonoBehaviour
{
    [SerializeField] private MissionDetails missionDetails;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(Fade.Instance.FadeIn());
        missionDetails.OnMissionCountChanged += ValidateMission;
        DialogueManager.Instance.ShowDialogue();
    }

    private void ValidateMission()
    {
        if (missionDetails.MissionCompleted)
        {
            Debug.Log("Mission completed!");
            StartCoroutine(GoToMinigame());
        }
    }

    private IEnumerator GoToMinigame()
    {
        StartCoroutine(Fade.Instance.FadeOut());
        yield return new WaitForSeconds(1f);
        FlowManager.Instance.GoToScene("Minigame 1");
    }
}
