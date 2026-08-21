
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
        {
            Vector3 dir = transform.position - mainCamera.transform.position;
            if (gameObject.tag == "Enemy")
            {
                dir.y = 0f;
            }
            dir.x = 0f;
            transform.rotation = Quaternion.LookRotation(dir);
        }
    }
}
