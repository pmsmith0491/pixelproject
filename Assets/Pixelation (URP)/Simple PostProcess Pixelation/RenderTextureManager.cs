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
    [SerializeField] private string texName;

    private void Awake()
    {
        var tex = new RenderTexture(Screen.width, Screen.height, 8);      
        Debug.Assert(tex.Create(), "failed to create render texture for camera");

        Camera cam = GetComponent<Camera>(); // the pixel camera

        cam.targetTexture = tex;
        materialToSet.SetTexture(texName, tex);
       
    }
}
