// COPYRIGHT 2022 Peter Smith
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spriteify;

[ExecuteInEditMode]
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
     *  spriteTargets is a valid List of GameObjectMaterialPair objects (a tuple (x,y) where x is a GameObject and y is a Material)
     *  
     *  There exist layers with name "Pixel" and "Standby"
     *  
     *  There exists a GameObject tag with name "PixelCamera".
     *  
     */


    [SerializeField] private List<GameObjectMaterialPair> spriteTargets; // (GameObject, Material) pairs where each GameObject is a pixelation target and the material
                                                // will contain the render texture of the temporary pixel cam looking at only the pixelated GameObject
    
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
        SetAllPairsToLayer("Standby");  // Put each GameObject inside of the Standby layer
        foreach (GameObjectMaterialPair pair in spriteTargets)
        {
            pair.material = new Material(Shader.Find("Custom/SimplePixelation"));
            pair.material.SetFloat("_BoxSize", 8);
        }
        // ASSERT: Every Material in spriteTargets uses the simple pixelation shader 
    }


    //PRE: This object satisfies the CI. The script instance has been loaded because the SpriteifyManager has become active.
    //POST: The spriteTargets are pixelated in the Viewport 
    private void Awake()
    {
         
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

        // create a render texture and set it as the camera's view. Then, set it as the CameraTexture of Overlay.shader 
        //  then set pixelTexture as PixelTexture of Overlay.shader 
        
        // create the blit render passes here and programatically insert them into the render pipeline (making sure not to add duplicates).
    }


    //PRE: This object satisfies the CI. The current fixed-rate frame has updated 
    //POST: The position of the spriteTarget[i].gameObject in Viewport space has been injected into spriteTarget[i].material where 0 <= i < spriteTargets.length 
    private void FixedUpdate()
    {
        // For each pair, set the material's pixel origin to the origin of the GameObject.transform in
        // normalized Viewport Space snapped to the Pixel Grid
        // (this is why we need to use orthographic projection, this effect does not work with perspective due to the nature of perspective projections).
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
    private void SetAllPairsToLayer(string layerName)
    {
        int layerNameAsInt = LayerMask.NameToLayer(layerName);
        foreach(GameObjectMaterialPair pair in spriteTargets)
        {
            pair.gameObject.layer = layerNameAsInt;
        }
    }

    
    //PRE: This object satisfies the CI, layerName is a well defined string that is a name of an existing layer in the Unity Project
    //POST: spriteTargets.gameObject.layer is set to the layer with layerName
    private void SetPairToLayer(GameObjectMaterialPair pair, string layerName)
    {
        int layerNameAsInt = LayerMask.NameToLayer(layerName);
        pair.gameObject.layer = layerNameAsInt;
    }


    //PRE: This object satisfies the CI. The spriteTargets are not pixelated in the Viewport
    //POST: Returns a renderTexture where the spriteTargets are pixelated independent of other objects 
    private RenderTexture CreatePixelTexture(Camera pixelCam)
    {

        var result = new RenderTexture(Screen.width, Screen.height, 8);
        Debug.Assert(result.Create(), "failed to create render texture for camera");


        //  update the spritePositions and snap them to the pixel grid. 
        foreach (GameObjectMaterialPair pair in spriteTargets)
        {
            SetPairToLayer(pair, "Pixel");
            // ASSERT: pair is in the Pixel layer

            // for every spriteTarget, create a render texture, pixelate the render texture, combine the render textures
            var tex = new RenderTexture(Screen.width, Screen.height, 8);
            pixelCam.targetTexture = tex;
            pair.material.SetTexture("_MainTex", tex);

            // create a new material with Overlay as its shader and set tex to its PixelTexture and result as its input 
            //
            //          Come up with a better plan for this.      
            //          Since we have direct access to textures, we don't have to blit them. We just create a temporary material, give it two textures (tex and result) and return the combination of the two textures.
            //
            //          The overlay shader will handle the z depth of the textures. 


            SetPairToLayer(pair, "Standby");
            // ASSERT: pair is now in the Standby layer
        }
        return result;
    }
}
