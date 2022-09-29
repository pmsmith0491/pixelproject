using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class BoxBlur : MonoBehaviour
{
    public Material mat;

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        // source is the full rendered scene that you would normally
        // send directly to the monitor. We are intercepting this so we can
        // do a bit more work, before passing it on. 
        // For BoxBlur, we need to Blit for 2 passes.
        var temporaryTexture = RenderTexture.GetTemporary(source.width, source.height);     
        Graphics.Blit(source, temporaryTexture, mat, 0);    
        Graphics.Blit(temporaryTexture, destination, mat, 1);     
        RenderTexture.ReleaseTemporary(temporaryTexture);
        
    }
}
