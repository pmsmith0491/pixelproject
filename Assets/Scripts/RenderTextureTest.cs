using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RenderTextureTest : MonoBehaviour
{


    public RenderTexture tex;

    // Start is called before the first frame update
    void Awake()
    {

        tex = new RenderTexture(Screen.width, Screen.height, 8);
        Debug.Assert(tex.Create(), "Failed to create camera blending render texture");


        Camera cam = GetComponent<Camera>();
        cam.targetTexture = tex;

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
