using UnityEngine;

public class LookingCamera : MonoBehaviour
{
    private Camera mainCamera;
    void Start()
    {
        if (gameObject.activeSelf)
        {
            mainCamera ??= Camera.main;
        }
    }

    void Update()
    {
        if (mainCamera != null && gameObject.activeSelf)
            transform.rotation = Quaternion.LookRotation(transform.position - mainCamera.transform.position);
    }
}
