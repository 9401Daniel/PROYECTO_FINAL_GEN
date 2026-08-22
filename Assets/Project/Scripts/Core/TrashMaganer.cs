using UnityEngine;
using System.Collections.Generic;
public class TrashMaganer : MonoBehaviour
{
    [SerializeField] private List<Trash> trashList = new();
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (trashList.Count == 0)
        {
            trashList.AddRange(GameObject.FindObjectsByType<Trash>(FindObjectsSortMode.None));
        }
    }

    public void ResetTrash()
    {
        foreach (var trash in trashList)
        {
            trash.ResetInteraction();
        }
    }
}
