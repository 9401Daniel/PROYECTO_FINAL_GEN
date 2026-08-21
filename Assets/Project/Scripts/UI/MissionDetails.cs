using UnityEngine;
using TMPro;
using System;

public class MissionDetails : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI missionText;
    [SerializeField] private string missionTextContent;
    [SerializeField] private int missionMaxCount;
    private int initialCount = 0;

    public event Action OnMissionCountChanged;
    public bool MissionCompleted => initialCount == missionMaxCount;

    public void AddCount()
    {
        initialCount++;
        missionText.text = missionTextContent + ": " + initialCount + "/" + missionMaxCount;
        OnMissionCountChanged?.Invoke();
    }

}
