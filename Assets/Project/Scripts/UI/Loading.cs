using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Loading : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Slider sliderProgress;
    [SerializeField] private TextMeshProUGUI tmpPorcentaj;


    private void Start()
    {
        StartCoroutine(Fade.Instance.FadeIn());
        StartCoroutine(CargarEscenaAsync());
    }

    private IEnumerator CargarEscenaAsync()
    {
        string scene = FlowManager.targetScene;

        if (string.IsNullOrEmpty(scene))
        {
            Debug.LogWarning("Target scene is not set. Returning to Menu.");
            SceneManager.LoadScene("Menu");
            yield break;
        }

        AsyncOperation operation = SceneManager.LoadSceneAsync(scene);
        operation.allowSceneActivation = false;

        while (!operation.isDone)
        {
            // Unity hide the real progress at 0.9 until it allows activation.
            // So we need to clamp the value to 0.9 and then map it to 0-1 range for our slider.
            float progresoReal = Mathf.Clamp01(operation.progress / 0.9f);

            sliderProgress.value = progresoReal;
            tmpPorcentaj.text = "Loading... " + Mathf.RoundToInt(progresoReal * 100f);

            if (operation.progress >= 0.9f)
            {
                sliderProgress.value = 1f;
                tmpPorcentaj.text = "Loading... 100%";
                operation.allowSceneActivation = true;
            }

            yield return null;
        }
    }
}
