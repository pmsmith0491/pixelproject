// DEPRECATED 
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class RenderTextureManager : MonoBehaviour
{
    /* 
     Attach to a camera:
     Creates a render texture containing the camera's view
     */

    [SerializeField] private Material materialToSet;
    [SerializeField] private Material pixelPostProcess;
    [SerializeField] private Transform pixelationTarget; // Transform of object to be pixelated to keep track of individual object in coordinate space
    [SerializeField] private string texName;
    [SerializeField] private string pixelShaderTargetField;

    private void Awake()
    {
        var tex = new RenderTexture(Screen.width, Screen.height, 8);      
        Debug.Assert(tex.Create(), "failed to create render texture for camera");

        Camera cam = GetComponent<Camera>(); // the pixel camera
        Vector3 viewportPosition = cam.WorldToViewportPoint(pixelationTarget.position);
        
        cam.targetTexture = tex;
        materialToSet.SetTexture(texName, tex);     
    }

    private void FixedUpdate()
    {
        SetTargetPosInPixelPostProcess();
    }

    private void SetTargetPosInPixelPostProcess()
    {
        Camera cam = GetComponent<Camera>(); // the pixel camera
        Vector3 viewportPosition = cam.WorldToViewportPoint(pixelationTarget.position);
        SnapTargetToPixelGrid(viewportPosition, cam);
        Vector4 viewportPositionAsVec4 = new Vector4(viewportPosition.x, viewportPosition.y, viewportPosition.z, 0);
        pixelPostProcess.SetVector(pixelShaderTargetField, viewportPositionAsVec4);
    }

    private void SnapTargetToPixelGrid(Vector3 targetViewportPos, Camera cam)
    {
        float pixelSizeX = 1 / Screen.width;
        float pixelSizeY = 1 / Screen.height;
        float snappedPosX = Mathf.Floor(targetViewportPos.x * Screen.width)/Screen.width;
        float snappedPosY = Mathf.Floor(targetViewportPos.y * Screen.height) / Screen.height;
        Vector3 snappedPos = new Vector3(snappedPosX, snappedPosY, targetViewportPos.z);
        pixelationTarget.position = cam.ViewportToWorldPoint(snappedPos);
    }

}
