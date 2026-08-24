using UnityEngine;
using System.Collections;

public class MinigameManager : MonoBehaviour
{
    [SerializeField] private UICounter counter;
    [SerializeField] private string nextSceneName;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(Fade.Instance.FadeIn());
        counter.OnCountChanged += ValidateMission;
    }
    private void ValidateMission()
    {
        if (counter.GameCompleted)
        {
            Debug.Log("Game completed!");
            DialogueManager.Instance.ShowDialogue();
            DialogueManager.Instance.OnDialogueManagerEnded += GoToNext;
        }
    }

    private void OnDisable()
    {
        DialogueManager.Instance.OnDialogueManagerEnded -= GoToNext;
    }

    private void GoToNext()
    {
        StartCoroutine(GoToNextCoroutine());
    }
    private IEnumerator GoToNextCoroutine()
    {
        StartCoroutine(Fade.Instance.FadeOut());
        yield return new WaitForSeconds(1f);
        FlowManager.Instance.GoToScene(nextSceneName);
    }

}
