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
    private List<GameObject> spriteTargets; // all objects that need to be pixelated 

    [SerializeField]
    private Material overlayMaterial;

    [SerializeField]
    private Material pixelMaterial;


    [SerializeField]
    private Material pixelOverlayCreatorMat;



    private const int MAX_LAYERS = 31; // The largest layer index in Unity.

    [SerializeField]
    private bool snapObjects = false;

    Camera mainCam;
    Camera createPixelTex;
    Camera pixelCam;
    Camera pixelDepth;

    //RenderTexture mainCamTex;
    RenderTexture pixelTexCreator;
    RenderTexture pixelTexture;
    RenderTexture pixelDepthTex;
    

    /*
     * 
     *  UNITY MONO BEHAVIOR METHODS 
     * 
     */

    //PRE: This object satisfies the CI. 
    //POST: a clone of main camera called "pixelCam" that only looks at objects in the pixel layer is added to the scene
    //      pixelMaterial._MainTex is pixelCamera.targetTexture
    //      overlayMaterial._OverlayTex is pixelCamera.targetTexture
    private void Awake()
    {
        SetAllTargetsToLayer("Pixel");  // Put each GameObject inside of the Standby layer 

        mainCam = Camera.main; // Main Camera 
        CullLayerFromCam(mainCam, "Pixel");
        CullLayerFromCam(mainCam, "Standby");

        //ASSERT: Pixel and Standby layers are excluded from main camera's rendering

        pixelCam = CreateChildOfMainCam("PixelCamera", 1); // overlay camera 
        pixelDepth = CreateChildOfMainCam("PixelDepth", 2); // depth texture of overlay camera 
        createPixelTex = CreateChildOfMainCam("CreatePixelTexture", 3);

        CullAllLayersFromMaskExcept(pixelCam, "Pixel");
        CullAllLayersFromMaskExcept(pixelDepth, "Pixel");
        CullAllLayersFromMaskExcept(pixelDepth, "Nothing");

        pixelTexture = new RenderTexture(Screen.width, Screen.height, 8);
        pixelDepthTex = new RenderTexture(Screen.width, Screen.height, 24, RenderTextureFormat.Depth);
        pixelTexCreator = new RenderTexture(Screen.width, Screen.height, 8);

        pixelCam.targetTexture = pixelTexture;
        pixelDepth.targetTexture = pixelDepthTex;
        createPixelTex.targetTexture = pixelTexCreator;
        
        mainCam.depthTextureMode = mainCam.depthTextureMode | DepthTextureMode.Depth;

        pixelMaterial.SetTexture("_MainTex", pixelTexture);
        overlayMaterial.SetTexture("_OverlayDepth", pixelDepthTex);
        overlayMaterial.SetTexture("_OverlayTex", pixelTexture);

        //ASSERT: The pixelCam is initialized and within the scene. 
        //        The pixelDepth cam exists to create the render texture for the pixelCam

    }


    //PRE: This object satisfies the CI. The current fixed-rate frame has updated 
    //POST: The sprite targets are pixelated in the viewport of display 1 
    private void FixedUpdate()

    {
        overlayMaterial.SetFloat("_ResolutionX", pixelMaterial.GetFloat("_ResolutionX"));
        overlayMaterial.SetFloat("_ResolutionY", pixelMaterial.GetFloat("_ResolutionY"));
        overlayMaterial.SetFloat("_BoxSize", pixelMaterial.GetFloat("_BoxSize"));

        // after we make alterations to a camera, render it. 

        /* What is our process? 
         * We want to inject our render textures into overlayMaterial, render the main camera, and keep overlaying textures until all gameobjects are out of pixel layer
         * 
         */
        if (snapObjects)
        {
            pixelOverlayCreatorMat.SetTexture("_BaseTex", pixelTexCreator);
            pixelOverlayCreatorMat.SetTexture("_OverlayTex", pixelTexture);
            foreach (GameObject target in spriteTargets)
            {
                //SetTargetPosInPixelPostProcess(pixelCam, target);
                SetGameObjectToLayer(target, "Pixel");
                pixelCam.Render();
                createPixelTex.Render();
                SetGameObjectToLayer(target, "Standby");
            }
            overlayMaterial.SetTexture("_OverlayTex", pixelTexCreator);
        }
    }

    private void OnDestroy()
    {
        pixelTexture.Release();
        pixelDepthTex.Release();
        pixelTexCreator.Release();
    }

    /* 
     * 
     *  SPRITEIFY_MANAGER FUNCTIONS 
     * 
     */

    private Camera CreateChildOfMainCam(string tag, int display)
    
    {
        Camera cam;
        GameObject camGameObject = GameObject.FindGameObjectWithTag(tag);

        if (camGameObject == null)
        {
            //ASSERT: The scene does NOT contain a pixelCam. 
            cam = InitializeMainCamCopy(tag, display);
        }
        else
        {
            cam = camGameObject.GetComponent<Camera>();
        }

        return cam;
    }


    //PRE: This object satisfies the CI. There does not exist a camera called PixelCam with tag "PixelCam"
    //POST: Scene contains a new camera called PixelCam with tag "PixelCam" such that:
    //      The camera is a copy of the main camera but culls everything except the pixel layer
    //      The camera clears to a solid color set to (0,0,0,0) (transparent black)
    //      The camera outputs to display 2
    // Note: This function could be retooled to be a "create camera" function, but in this instance there is no need. 
    private Camera InitializeMainCamCopy(string tag, int display)
    {
        GameObject camObject = new GameObject(tag);
        camObject.AddComponent<Camera>();
        Camera cam = camObject.GetComponent<Camera>();
        
        cam.CopyFrom(Camera.main);
        cam.tag = tag;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.0f, 0.0f, 0.0f, 0.0f);
        cam.targetDisplay = display; // Camera outputs to display 2
        cam.transform.SetParent(Camera.main.transform);
        // ASSERT: pixelCam only renders the Pixel layer with a transparent black background 
        return (cam);
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
        foreach(GameObject target in spriteTargets)
        {
            SetGameObjectToLayer(target, layerName);
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
    private void SetTargetPosInPixelPostProcess(Camera cam, GameObject pixelationTarget)
    {
        Vector3 viewportPosition = cam.WorldToViewportPoint(pixelationTarget.transform.position);
        SnapTargetToPixelGrid(viewportPosition, cam, pixelationTarget);
        Vector4 viewportPositionAsVec4 = new Vector4(viewportPosition.x, viewportPosition.y, viewportPosition.z, 0);
        pixelMaterial.SetVector("_PixelationTargetPos", viewportPositionAsVec4);
    }


    //PRE: This object satisfies the CI.
    //      targetViewportPos is a valid Vector3, cam is a valid Camera, and pixelationTarget is a valid GameObject 
    //      In ViewPort space, the pixelationTarget's position is not necessarily inside of a specific pixel
    //POST: pixelationTarget's position is snapped to a specific pixel in Viewport Space
    private void SnapTargetToPixelGrid(Vector3 targetViewportPos, Camera cam, GameObject pixelationTarget)
    {
        float snappedPosX = Mathf.Floor(targetViewportPos.x * Screen.width) / Screen.width; // snap the pixelationTarget's x position to a specific pixel
        float snappedPosY = Mathf.Floor(targetViewportPos.y * Screen.height) / Screen.height; // snap the pixelationTarget's y position to a specific pixel
        Vector3 snappedPos = new Vector3(snappedPosX, snappedPosY, targetViewportPos.z);
        pixelationTarget.transform.position = cam.ViewportToWorldPoint(snappedPos);
    }


    //PRE: This object satisfies the CI. The spriteTargets are not pixelated in the Viewport
    //POST: Returns a renderTexture where the spriteTargets are pixelated independent of other objects 
    /*private RenderTexture CreatePixelTexture(Camera pixelCam)
    {
     
    }*/
}
