using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{

    public Transform target;
    public bool smoothCameraMovement;
    public float smoothSpeed = 0.125f;
    public Vector3 offset; // offset of the camera (this is determined manually in the Unity Editor)

    private void FixedUpdate()
    {
        Vector3 desiredPosition = target.position + offset; // desired camera position relative to the player 
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed); // smoothed/linearly interpolated version of desiredPosition 
                                                                                                   // Lerp stands for linear interpolation 
        Vector3 newPosition;
        if(smoothCameraMovement)
        {
            newPosition = smoothedPosition;
        } else
        {
            newPosition = desiredPosition;
        }

        transform.position = newPosition;
        transform.LookAt(target);
    }
}
