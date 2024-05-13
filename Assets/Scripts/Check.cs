using Cinemachine;
using UnityEngine;

public class Check : MonoBehaviour
{
    void Start()
    {
        Camera[] allCameras = FindObjectsOfType<Camera>();
        foreach (Camera cam in allCameras)
        {
            if (cam == allCameras[0])
                continue;
            else
                Destroy(cam.gameObject);

            Debug.Log("Camera: " + cam.gameObject.name + " Check");
        }

        GameManager[] allGameManagers = FindObjectsOfType<GameManager>();
        foreach (GameManager gm in allGameManagers)
        {
            if (gm == allGameManagers[0])
                continue;
            else
                Destroy(gm.gameObject);

            Debug.Log("GameManager: " + gm.gameObject.name + " Check");
        }

        CinemachineVirtualCamera[] allCinemachineVirtualCameras = FindObjectsOfType<CinemachineVirtualCamera>();
        foreach (CinemachineVirtualCamera cmvc in allCinemachineVirtualCameras)
        {
            if (cmvc == allCinemachineVirtualCameras[0])
                continue;
            else
                Destroy(cmvc.gameObject);

            Debug.Log("CinemachineVirtualCamera: " + cmvc.gameObject.name + " Check");
        }
    }
}
