using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Fade : MonoBehaviour
{
    [SerializeField] private float fadeDuration = 0.5f;
    private Image fadePanel;

    public static Fade Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        fadePanel = transform.GetChild(0).GetComponent<Image>();
        StartCoroutine(FadeIn());
    }

    public IEnumerator FadeIn()
    {
        fadePanel.gameObject.SetActive(true);
        fadePanel.color = new Color(0f, 0f, 0f, 1f); // Init opaque

        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            fadePanel.color = new Color(0f, 0f, 0f, 1f - (t / fadeDuration));
            yield return null;
        }

        fadePanel.color = new Color(0f, 0f, 0f, 0f);
        fadePanel.gameObject.SetActive(false);
    }

    public IEnumerator FadeOut()
    {
        fadePanel.gameObject.SetActive(true);
        fadePanel.color = new Color(0f, 0f, 0f, 0f); // Init transparent

        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            fadePanel.color = new Color(0f, 0f, 0f, t / fadeDuration);
            yield return null;
        }

        fadePanel.color = new Color(0f, 0f, 0f, 1f);
    }
}
