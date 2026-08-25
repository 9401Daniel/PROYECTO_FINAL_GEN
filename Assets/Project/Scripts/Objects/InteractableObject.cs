using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InteractableObject : Interactable
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
            Debug.LogError("Outline material is not assigned in InteractableObject script.");
        }
    }
    protected override void Interact(InputAction.CallbackContext context)
    {
        HidePrompt();
        print("Interact InteractableObject.");
        if (missionDetails == null)
        {
            Debug.LogError("MissionDetails reference is not set in InteractableObject script.");
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
            if (!transform.GetChild(i).TryGetComponent(out MeshRenderer meshRendererC)) continue;
            if (meshRendererC.materials.Length < 1 || meshRendererC.materials[meshRendererC.materials.Length - 1] == outlineMaterial) continue;
            Material[] currentMaterials = meshRendererC.materials;
            Material[] newMaterials = new Material[currentMaterials.Length + 1];
            Array.Copy(currentMaterials, newMaterials, currentMaterials.Length);
            newMaterials[currentMaterials.Length] = outlineMaterial;
            meshRendererC.materials = newMaterials;
        }
        if (TryGetComponent(out MeshRenderer meshRendererP))
        {
            if (meshRendererP.materials.Length < 1 || meshRendererP.materials[meshRendererP.materials.Length - 1] == outlineMaterial) return;
            Material[] currentMaterials = meshRendererP.materials;
            Material[] newMaterials = new Material[currentMaterials.Length + 1];
            Array.Copy(currentMaterials, newMaterials, currentMaterials.Length);
            newMaterials[currentMaterials.Length] = outlineMaterial;
            meshRendererP.materials = newMaterials;
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
        if (TryGetComponent(out MeshRenderer meshRendererP))
        {
            Material[] currentMaterials = meshRendererP.materials;
            if (currentMaterials.Length != 2) return; // Assuming the outline material is always the last one, we expect exactly 2 materials: the original and the outline.
            Material[] newMaterials = new Material[currentMaterials.Length - 1];
            Array.Copy(currentMaterials, newMaterials, currentMaterials.Length - 1);
            meshRendererP.materials = newMaterials;

        }
    }
}
