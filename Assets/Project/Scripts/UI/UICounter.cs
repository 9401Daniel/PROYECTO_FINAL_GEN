using UnityEngine;
using TMPro;
using System;

public class UICounter : MonoBehaviour
{
    [SerializeField] private int initialCount;
    [SerializeField] private string countText;
    private TextMeshProUGUI textMeshPro;
    public bool GameCompleted => initialCount == 0;
    public event Action OnCountChanged;

    private void Awake()
    {
        textMeshPro = GetComponent<TextMeshProUGUI>();
        textMeshPro.text = countText + ": " + initialCount;
    }

    public void UpdateRollbackCount()
    {
        initialCount--;
        textMeshPro.text = countText + ": " + initialCount;
        OnCountChanged?.Invoke();
    }
}
