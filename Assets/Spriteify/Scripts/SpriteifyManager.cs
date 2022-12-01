// COPYRIGHT 2022 Peter Smith
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spriteify;

public class SpriteifyManager : MonoBehaviour
{
    /* Class Invariant: 
     * 
     *  Description: The manager script for the Spriteify Asset
     *  -------------
     *  
     *  Desired Outcome: Renders GameObjects within spriteTargets as programatically generated pixelArt
     *  ---------------- 
     *  
     *  The MainCamera is using an Orthographic Projection. 
     *  spriteTargets is a valid List of GameObjects
     *  
     *  There exist layers with name "Pixel" and "Standby"
     *  
     *  There exists a GameObject tag with name "PixelCamera".
     *  
     */

   [SerializeField]
    private List<GameObjectMaterialPair> spriteTargets; // all objects that need to be pixelated 

    [SerializeField]
    private Material overlayMaterial;

    private const int MAX_LAYERS = 31; // The largest layer index in Unity.


    // SimplePixelation.shader properties:
    // fields that should not change per object:    ResolutionX, ResolutionY.
    // fields that can change per object:           _BoxSize, _PixelationTargetPos.

    /*
     * 
     *  UNITY MONO BEHAVIOR METHODS 
     * 
     */


    //PRE: The first Update call is made
    //POST: each material is given the SimplePixelation Shader and a defaul box size of 8
    //      This object satisfies the CI. 
    private void Start()
    {
        SetAllTargetsToLayer("Standby");  // Put each GameObject inside of the Standby layer 
        foreach (GameObjectMaterialPair pair in spriteTargets)
        {
            pair.material = new Material(Shader.Find("Spriteify/SimplePixelation"));
            pair.material.SetFloat("_BoxSize", 8);
        }
    }


    //PRE: This object satisfies the CI. The current fixed-rate frame has updated 
    //POST: The sprite targets are pixelated in the viewport of display 1 
    private void FixedUpdate()
    {
       /* foreach (GameObjectMaterialPair pair in spriteTargets)
        {
            SetTargetPosInPixelPostProcess(Camera.main, pair);
        }*/
    }


    //PRE: This object satisfies the CI. 
    //POST: The sprite targets are pixelated in the viewport of display 1 
    private void Awake()
    {
        // For each pair, set the material's pixel origin to the origin of the GameObject.transform in
        // normalized Viewport Space snapped to the Pixel Grid
        // (this is why we need to use orthographic projection, this effect does not work with perspective due to the nature of perspective projections).
        Camera mainCam = Camera.main; // Main Camera 

        CullLayerFromCam(mainCam, "Pixel");
        CullLayerFromCam(mainCam, "Standby");
        //ASSERT: Pixel and Standby layers are excluded from main camera's rendering

        Camera pixelCam; // Duplicate of MainCamera but it has a transparent background and only renders the "Pixel" layer.

        GameObject pixelCamGameObject = GameObject.FindGameObjectWithTag("PixelCamera");

        if (pixelCamGameObject == null)
        {
            //ASSERT: The scene does NOT contain a pixelCam. 
            pixelCam = CreatePixelCam();
        }
        else
        {
            pixelCam = pixelCamGameObject.GetComponent<Camera>();
        }
        //ASSERT: The pixelCam is initialized and within the scene. 

        var pixelTexture = CreatePixelTexture(pixelCam); // texture that is the same dimensions as the screen,
                                                         // contains color and depth data of all pixels of objects that need to be pixelated 

        overlayMaterial.SetTexture("_OverlayTex", pixelTexture);

        pixelTexture.Release();
        //ASSERT: We have given pixel texture to the overlay tex and no longer need to allocate the memory containing the texture
    }

    /* 
     * 
     *  SPRITEIFY_MANAGER FUNCTIONS 
     * 
     */

    //PRE: This object satisfies the CI. There does not exist a camera called PixelCam with tag "PixelCam"
    //POST: Scene contains a new camera called PixelCam with tag "PixelCam" such that:
    //      The camera is a copy of the main camera but culls everything except the pixel layer
    //      The camera clears to a solid color set to (0,0,0,0) (transparent black)
    //      The camera outputs to display 2
    // Note: This function could be retooled to be a "create camera" function, but in this instance there is no need. 
    private Camera CreatePixelCam()
    {
        GameObject pixelCamObject = new GameObject("PixelCamera");
        pixelCamObject.AddComponent<Camera>();
        Camera pixelCam = pixelCamObject.GetComponent<Camera>();
        pixelCam.CopyFrom(Camera.main);
        pixelCam.tag = "PixelCamera";
        pixelCam.clearFlags = CameraClearFlags.SolidColor;
        pixelCam.backgroundColor = new Color(0.0f, 0.0f, 0.0f, 0.0f);
        pixelCam.targetDisplay = 1; // Camera outputs to display 2
        CullAllLayersFromMaskExcept(pixelCam, "Pixel");
        pixelCam.transform.SetParent(Camera.main.transform);
        // ASSERT: pixelCam only renders the Pixel layer with a transparent black background 
        return (pixelCam);
    }


    //PRE: This object satisfies the CI. cam is a well defined Camera object, layerName is a well defined string and there exists a Layer with name = layerName 
    //POST: cam excludes layer with name layerName from rendering.
    private void CullLayerFromCam(Camera cam, string layerName)
    {
        cam.cullingMask &= ~(1 << LayerMask.NameToLayer(layerName));// the cullingmask is a 32 bitfield where each bit represents a layer the camera either renders (1) or doesn't render (0).
                                                                    // if we don't want the camera to render a layer, we need to flip the bit in the cullingMask of the layer
       // ASSERT: cam's culling mask excludes layerName from rendering. 
    }


    //PRE: This object satisfies the CI. cam is a well defined Camera object, layerName is a well defined string and there exists a Layer with the name layerName 
    //POST: cam includes layerName in its culling mask
    private void AddLayerToCamCullingMask(Camera cam, string layerName)
    {
        cam.cullingMask |= (1 << LayerMask.NameToLayer(layerName));
        // ASSERT: cam's culling mask includes layerName, meaning layerName will render on cam 
    }


    //PRE: This object satisfies the CI. cam is a well defined Camera object, layerName is a well defined string and there exists a layer with the name layerName
    //POST: cam excludes all layers in its culling mask except for layerName
    private void CullAllLayersFromMaskExcept(Camera cam, string layerName)
    {
        int layerNameAsInt = LayerMask.NameToLayer(layerName);

        for (int i = 0; i < MAX_LAYERS; i++)
        {
            if (i != layerNameAsInt)
            {
                CullLayerFromCam(cam, LayerMask.LayerToName(i));
            }
            else
            {
                AddLayerToCamCullingMask(cam, LayerMask.LayerToName(i));
            }
        }
    }


    //PRE: This object satisfies the CI, layerName is a well defined string that is a name of an existing layer in the Unity Project
    //POST: spriteTargets.gameObject[i] where 0 <= i < spriteTargets.length are set to the layer with layerName
    private void SetAllTargetsToLayer(string layerName)
    {
        foreach(GameObjectMaterialPair target in spriteTargets)
        {
            SetGameObjectToLayer(target.gameObject, layerName);
        }
    }

    
    //PRE: layerName is a well defined string that is a name of an existing layer in the Unity Project
    //POST: spriteTargets.gameObject.layer is set to the layer with layerName
    private void SetGameObjectToLayer(GameObject target, string layerName)
    {
        SetChildrenToLayer(target, layerName);
    }


    //PRE: obj is a well defined GameObject. layerName is a well defined string.
    //POST: obj and all children of obj are set to the layer "layerName". 
    private void SetChildrenToLayer(GameObject obj, string layerName)
    {
        if(obj != null)
        {  
            int layerNameAsInt = LayerMask.NameToLayer(layerName);
            obj.layer = layerNameAsInt;

            foreach(Transform child in obj.transform)
            {
                if(child != null)
                {
                    SetChildrenToLayer(child.gameObject, layerName);
                }
            }
        } 
    }

    //PRE: This object satisfies the CI. cam is a well defined Camera. pixelationTarget is a well defined GameObjectMaterialPair in spriteTargets 
    //POST: pixelationTarget.material's _PixelationTargetPos field is populated with cam's ViewPort position of pixelationTarget snapped to the Pixel Grid 
    private void SetTargetPosInPixelPostProcess(Camera cam, GameObjectMaterialPair pixelationTarget)
    {
        Vector3 viewportPosition = cam.WorldToViewportPoint(pixelationTarget.gameObject.transform.position);
        SnapTargetToPixelGrid(viewportPosition, cam, pixelationTarget.gameObject);
        Vector4 viewportPositionAsVec4 = new Vector4(viewportPosition.x, viewportPosition.y, viewportPosition.z, 0);
        pixelationTarget.material.SetVector("_PixelationTargetPos", viewportPositionAsVec4);
    }


    //PRE: This object satisfies the CI.
    //      targetViewportPos is a valid Vector3, cam is a valid Camera, and pixelationTarget is a valid GameObject 
    //      In ViewPort space, the pixelationTarget's position is not necessarily inside of a specific pixel
    //POST: pixelationTarget's position is snapped to a specific pixel in Viewport Space
    private void SnapTargetToPixelGrid(Vector3 targetViewportPos, Camera cam, GameObject pixelationTarget)
    {
        float pixelSizeX = 1 / Screen.width; // the width of a pixel in the viewport 
        float pixelSizeY = 1 / Screen.height; // the height of a pixel in the viewport 
        float snappedPosX = Mathf.Floor(targetViewportPos.x * Screen.width) / Screen.width; // snap the pixelationTarget's x position to a specific pixel
        float snappedPosY = Mathf.Floor(targetViewportPos.y * Screen.height) / Screen.height; // snap the pixelationTarget's y position to a specific pixel
        Vector3 snappedPos = new Vector3(snappedPosX, snappedPosY, targetViewportPos.z);
        pixelationTarget.transform.position = cam.ViewportToWorldPoint(snappedPos);
    }


    //PRE: This object satisfies the CI. The spriteTargets are not pixelated in the Viewport
    //POST: Returns a renderTexture where the spriteTargets are pixelated independent of other objects 
    private RenderTexture CreatePixelTexture(Camera pixelCam)
    {
        var result = new RenderTexture(Screen.width, Screen.height, 8); // contains ALL pixelated objects in a single texture
        var pixelTex = new RenderTexture(Screen.width, Screen.height, 8);
        pixelCam.targetTexture = pixelTex;

        foreach(GameObjectMaterialPair pair in spriteTargets) {
            SetGameObjectToLayer(pair.gameObject, "Pixel");

            Graphics.Blit(pixelTex, pixelTex, pair.material); // Copy pixelTex into MainTex of pair.material and copy the result back into pixelTex
            pixelCam.targetTexture = null;
            pixelCam.Render();
            
            Material overlayPixelAndResult = new Material(Shader.Find("Spriteify/OverlayTwoRenderTextures"));
            overlayPixelAndResult.SetTexture("_OverlayTex", pixelTex);
            Graphics.Blit(result, result, overlayPixelAndResult);
            
            SetGameObjectToLayer(pair.gameObject, "Standby");
        }
        pixelTex.Release();
        // We no longer need the pixelTex

        return result;
        /* Each GameObjectMaterialPair has a material with the shader SimplePixelation
         * 
         * PROCEDURE: Create Pixel Texture
         * 
         * rendertexture result
         * 
         * For each pair in spriteTargets {
         *      
         *      pair.layer = "Pixel"
         *      set all objects except pair to "Standby"
         * 
         *      Set pair.material.MainTex to pixelCam's texture
         *      create temporary texture pixtex 
         *      blit from pair.material to pixtex
         *      
         *      create temporary mat tempoverlay w/ shader "OverlayTwoTextures"
         *      Set mainTex of tempoverlay to result and overlay tex to pixtex 
         *      
         *      blit from tempoverlay to result 
         *      set pair to "Standby" 
         * }
         * 
         * set OverlayMaterial's overlay tex to result
         * set OverlayMaterial's mainTex to mainCam
         * 
         */
    }
}
