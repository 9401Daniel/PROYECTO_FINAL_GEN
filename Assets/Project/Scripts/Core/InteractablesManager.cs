using UnityEngine;
using System.Collections.Generic;
public class InteractablesManager : MonoBehaviour
{
    private List<Interactable> interactables = new();
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (interactables.Count == 0)
        {
            interactables.AddRange(FindObjectsByType<Interactable>(FindObjectsSortMode.None));
        }
    }

    public void ResetItems()
    {
        foreach (var item in interactables)
        {
            item.ResetInteraction();
        }
    }
}
