using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteifyManager : MonoBehaviour
{
    [SerializeField]
    List<Transform> spriteTargets;

    private void Awake()
    {
        // need this to create render textures, i.e. every time the target transform changes  
        var pixelTexture = createPixelTexture();


    }

    private void FixedUpdate()
    {
        
    }


    private RenderTexture createPixelTexture()
    {

        var result = new RenderTexture(Screen.width, Screen.height, 8);
        Debug.Assert(result.Create(), "failed to create render texture for camera");


        //  update the spritePositions and snap them to the pixel grid. 
        foreach (Transform transform in spriteTargets)
        {


            // get the positions of the origin AT LEAST 
            // for every spriteTarget, create a render texture, pixelate the render texture, combine the render textures



        }
        return result;
    }

}
