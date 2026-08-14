using UnityEngine;

public class LookingCamera : MonoBehaviour
{
    private Camera mainCamera;
    void Start()
    {
        if (gameObject.activeSelf)
        {
            mainCamera ??= Camera.main;
            if (mainCamera != null)
                transform.rotation = Quaternion.LookRotation(transform.position - mainCamera.transform.position);
        }
    }
}
