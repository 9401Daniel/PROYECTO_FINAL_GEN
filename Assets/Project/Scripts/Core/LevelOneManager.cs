using UnityEngine;

public class LevelOneManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(Fade.Instance.FadeIn());
    }

    // Update is called once per frame
    void Update()
    {

    }
}
