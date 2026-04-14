using UnityEngine;

public class CameraZone : MonoBehaviour
{
    [SerializeField] private Camera zoneCamera;
    private static Camera currentCamera;

    private void OnTriggerEnter(Collider other)
    {
        // if the object entering the trigger is not the player, do nothing
        if (!other.CompareTag("Player")) return;

        // if there is a current camera, disable it before enabling the new one
        if (currentCamera != null)
            currentCamera.enabled = false;

        currentCamera = zoneCamera;
        currentCamera.enabled = true;
    }

    private void OnTriggerExit(Collider other)
    {   
        // se same as enter but for exit
        if (!other.CompareTag("Player")) return;

        if (currentCamera != null)
            currentCamera.enabled = false;

        currentCamera = null;
    }
}