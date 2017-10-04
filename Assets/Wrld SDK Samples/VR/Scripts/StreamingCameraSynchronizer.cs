using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StreamingCameraSynchronizer : MonoBehaviour 
{
    [SerializeField]
    private Camera m_streamingCamera;

    void Update () 
    {
        m_streamingCamera.transform.position = transform.position;
        m_streamingCamera.transform.rotation = transform.rotation;
        m_streamingCamera.fieldOfView = Camera.main.fieldOfView;
    }
}
