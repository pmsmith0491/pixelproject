using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spriteify;

[ExecuteInEditMode]
public class SpriteifyManager : MonoBehaviour
{
    /* CI: The manager script for the Spriteify Asset
     *  The MainCamera is using an Orthographic Projection. 
     *  spriteTargets is a valid List of GameObjectMaterialPair objects (a tuple (x,y) where x is a GameObject and y is a Material)
     *  Renders spriteTargets as programatically generated pixelArt
     */


    [SerializeField]
    List<GameObjectMaterialPair> spriteTargets; // (GameObject, Material) pairs where each GameObject is a pixelation target and the material
                                                // will contain the render texture of the temporary pixel cam looking at only the pixelated GameObject

    // infallable fields: ResolutionX, ResolutionY should be consistent. _BoxSize should change depending on the object.
    // Position obviously changes and will be set in FixedUpdate. 

    /*
     * 
     *  UNITY MONO BEHAVIOR METHODS 
     * 
     */


    //PRE: The first Update call is made
    //POST: each material is given the SimplePixelation Shader and a defaul box size of 8
    private void Start()
    {
        // if Standby and Pixel layers don't exist, creat them. Put each GameObject inside of the Standby layer

        foreach(GameObjectMaterialPair pair in spriteTargets)
        {
            pair.material = new Material(Shader.Find("Custom/SimplePixelation"));
            pair.material.SetFloat("_BoxSize", 8);
        }
    }


    //PRE: The script instance has been loaded because the SpriteifyManager has become active 
    //POST: The spriteTargets are pixelated in the Viewport
    private void Awake()
    {
        SetAllPairsToLayer("Standby");
        // initialize variable pointing to main camera
        // cull Standby and Pixel layers from camera 
        // get texture from the camera 
        var pixelTexture = CreatePixelTexture(); // texture that is the same dimensions as the screen,
                                                 // contains color and depth data of all pixels of objects that need to be pixelated 

        // Create a material in the package that has overlay as its shader, send pixelTexture and mainCamTexture into it as its parameters
        // blit from the Camera to the material, don't use the Camera's texture, and return the texture that is the combination of the main and pixel textures which will be blitted to the screen
    }


    //PRE: The current fixed-rate frame has updated 
    //POST: The position of the spriteTarget[i].gameObject in Viewport space has been injected into spriteTarget[i].material where 0 <= i < spriteTargets.length 
    private void FixedUpdate()
    {
        // For each pair, set the material's pixel origin to the origin of the GameObject.transform in normalized Viewport Space snapped to the Pixel Grid (this is why we need to use orthographic projection, this effect does not hold on perspective).

    }


    /* 
     * 
     *  SPRITEMANAGER FUNCTIONS 
     * 
     */


    // culls Standby and Pixel layers from the main camera 
    private void CullStandbyLayerFromMainCam()
    {
        // get main camera, cull Standby and Pixel Layers
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



    //PRE: The spriteTargets are not pixelated in the Viewport
    //POST: Returns a renderTexture where the spriteTargets are pixelated independent of other objects 
    private RenderTexture CreatePixelTexture()
    {

        var result = new RenderTexture(Screen.width, Screen.height, 8);
        Debug.Assert(result.Create(), "failed to create render texture for camera");


        //  update the spritePositions and snap them to the pixel grid. 
        foreach (GameObjectMaterialPair pair in spriteTargets)
        {
            SetPairToLayer(pair, "Pixel");
             
            // for every spriteTarget, create a render texture, pixelate the render texture, combine the render textures
            
            SetPairToLayer(pair, "Standby");
        }
        return result;
    }

}
