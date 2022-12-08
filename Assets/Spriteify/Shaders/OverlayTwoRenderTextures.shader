// COPYRIGHT 2022 Peter Smith
Shader "Spriteify/OverlayTwoRenderTextures"
{
    /*
    Takes two textures, _MainTex and _OverlayTex, and returns a texture with _OverlayTex "overlayed" on top of "MainTex".
   
   
    This specific version of this file is hard-coded for a "Pixelation" overlay texture
   
   
   */


    // This shader draws a texture on the mesh.
    Properties
    {
        _MainTex("_MainTex", 2D) = "white" // Main Texture of the shader 
        _OverlayTex("_OverlayTex", 2D) = "white" // Overlay Texture of the Shader 
        _OverlayDepth("_OverlayDepth", 2D) = "white" // overlay depth texture
        _ResolutionX("Resolution X", float) = 1920 // Resolution width of our returned texture
        _ResolutionY("Resolution Y", float) = 1080 // Resolution height of our returned texture
        _BoxSize("Box Size", float) = 8   // Box "width" of a MacroPixel
    }

        SubShader
    {
        Tags {"RenderType" = "Opaque" "RenderPipeline" = "UniversalRenderPipeline"}

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"      
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

            struct Attributes
            {
                float4 positionOS   : POSITION;
                // The uv variable contains the UV coordinate on the texture for the
                // given vertex.
                float2 uv           : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS  : SV_POSITION;
                // The uv variable contains the UV coordinate on the texture for the
                // given vertex.
                float2 uv           : TEXCOORD0;
            };

            // This macro declares _MainTex and _OverlayTex as Texture2D objects.
            TEXTURE2D(_MainTex);
            TEXTURE2D(_OverlayTex);
            TEXTURE2D(_OverlayDepth);


            // This macro declares the sampler for the _MainTex and _OverlayTex textures.
            SAMPLER(sampler_MainTex);
            SAMPLER(sampler_OverlayTex);
            SAMPLER(sampler_OverlayDepth);


            CBUFFER_START(UnityPerMaterial)
                // The following line declares the _MainTex_ST and _OverlayTex_ST variables, so that
                // _MainTex and _OverlayTex variables can be used in the fragment shader. The _ST 
                // suffix is necessary for the tiling and offset function to work.
                float4 _MainTex_ST; 
                float4 _OverlayTex_ST;
                float4 _OverlayDepth_ST;
                float _ResolutionX; // declares _ResolutionX as a variable 
                float _ResolutionY; // declares _ResolutionY as a variable 
                float _BoxSize; // declares _BoxSize as a variable 
            CBUFFER_END


            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                // The TRANSFORM_TEX macro performs the tiling and offset
                // transformation.
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
                return OUT;
            }


            //PRE: IN is well defined 
            //POST: returns pixel of overlay iff depth smaller than pixel from main
            half4 frag(Varyings IN) : SV_Target
            {

                float pixelSizeX = 1 / _ResolutionX;// size of pixel on x axis in normalized space
                float pixelSizeY = 1 / _ResolutionY;// size of pixel on y axis in normalized space
                float CellSizeX = (_BoxSize * pixelSizeX); // "Upscaled" pixel x size in normalized space 
                float CellSizeY = (_BoxSize * pixelSizeY); // "Upscaled" pixel y size in normalized space
                float bottomLeftPixelOfCellX = CellSizeX * floor(IN.uv.x / CellSizeX); // u coordinate of pixel at bottom most leftmost part of square
                float bottomLeftPixelOfCellY = CellSizeY * floor(IN.uv.y / CellSizeY); // v coordinate of pixel at bottom most leftmost part of square
                float2 bottomLeftPixelOfCell = float2(bottomLeftPixelOfCellX, bottomLeftPixelOfCellY); // the bottom left pixel of the cell

                half4 colorMain = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);
                half4 colorOverlay = SAMPLE_TEXTURE2D(_OverlayTex, sampler_OverlayTex, IN.uv);
                half4 overlayDepth = SAMPLE_TEXTURE2D(_OverlayDepth, sampler_OverlayDepth, bottomLeftPixelOfCell);
                half4 color = colorMain;

#if UNITY_REVERSED_Z
                real depth = SampleSceneDepth(IN.uv);
#else 
                real depth = lerp(UNITY_NEAR_CLIP_VALUE, 1, SampleSceneDepth(IN.uv));
#endif 

#if UNITY_REVERSED_Z
                if (colorOverlay.a == 1.0f && overlayDepth.r > depth) {
#else 
                if (colorOverlay.a == 1.0f && overlayDepth.r < depth) {

#endif 
                    color = colorOverlay;
                }
                return color;
            }
            ENDHLSL
        }
    }
}
