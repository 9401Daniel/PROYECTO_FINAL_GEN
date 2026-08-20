using UnityEngine;
using TMPro;

public class UICounter : MonoBehaviour
{
    [SerializeField] private int initialCount;
    [SerializeField] private string countText;
    private TextMeshProUGUI textMeshPro;

    private void Awake()
    {
        textMeshPro = GetComponent<TextMeshProUGUI>();
        textMeshPro.text = countText + ": " + initialCount;
    }

    public void UpdateRollbackCount()
    {
        initialCount--;
        textMeshPro.text = countText + ": " + initialCount;
    }

    public void UpdateForwardCount()
    {
        initialCount++;
        textMeshPro.text = countText + ": " + initialCount;
    }
}
