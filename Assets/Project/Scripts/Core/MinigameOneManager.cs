using UnityEngine;
using System.Collections;

public class MinigameOneManager : MonoBehaviour
{
    [SerializeField] private UICounter counter;
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
            StartCoroutine(GoToHome());
        }
    }

    private IEnumerator GoToHome()
    {
        StartCoroutine(Fade.Instance.FadeOut());
        yield return new WaitForSeconds(1f);
        FlowManager.Instance.GoToScene("Menu");
    }

}
