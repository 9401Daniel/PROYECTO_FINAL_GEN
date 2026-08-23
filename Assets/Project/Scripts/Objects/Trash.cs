using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class Trash : Interactable
{
    [SerializeField] private MissionDetails missionDetails;
    [SerializeField] private Material outlineMaterial;
    private bool outlineMode = false;

    protected override void Awake()
    {
        base.Awake();
        TurnOnOutlineMode();
        if (outlineMaterial == null)
        {
            Debug.LogError("Outline material is not assigned in Trash script.");
        }
    }
    protected override void Interact(InputAction.CallbackContext context)
    {
        HidePrompt();
        print("Interact Trash.");
        if (missionDetails == null)
        {
            Debug.LogError("MissionDetails reference is not set in Trash script.");
        }
        else
        {
            missionDetails.AddCount();
        }
        active = false;
        FinishInteraction();
        TurnOffOutlineMode();
    }

    public override void ResetInteraction()
    {
        base.ResetInteraction();
        TurnOnOutlineMode();
    }

    private void TurnOnOutlineMode()
    {
        if (outlineMode) return;
        outlineMode = true;
        for (int i = 0; i < transform.childCount; i++)
        {
            if (!transform.GetChild(i).TryGetComponent(out MeshRenderer meshRenderer)) continue;
            if (meshRenderer.materials.Length < 1 || meshRenderer.materials[meshRenderer.materials.Length - 1] == outlineMaterial) continue;
            Material[] currentMaterials = meshRenderer.materials;
            Material[] newMaterials = new Material[currentMaterials.Length + 1];
            Array.Copy(currentMaterials, newMaterials, currentMaterials.Length);
            newMaterials[currentMaterials.Length] = outlineMaterial;
            meshRenderer.materials = newMaterials;
        }
    }

    private void TurnOffOutlineMode()
    {
        if (!outlineMode) return;
        outlineMode = false;
        for (int i = 0; i < transform.childCount; i++)
        {
            if (!transform.GetChild(i).TryGetComponent(out MeshRenderer meshRenderer)) continue;
            Material[] currentMaterials = meshRenderer.materials;
            if (currentMaterials.Length != 2) continue; // Assuming the outline material is always the last one, we expect exactly 2 materials: the original and the outline.
            Material[] newMaterials = new Material[currentMaterials.Length - 1];
            Array.Copy(currentMaterials, newMaterials, currentMaterials.Length - 1);
            meshRenderer.materials = newMaterials;
        }
    }
}
