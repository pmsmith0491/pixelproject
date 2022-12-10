// COPYRIGHT 2022 Peter Smith
Shader "Spriteify/OverlayTwoTexturesIgnoringBlitInput"
{
    /*
    Takes two textures inputted from script, _BaseTex and _OverlayTex, and returns a texture with _OverlayTex "overlayed" on top of "MainTex".


    This specific version of this file is hard-coded for a "Pixelation" overlay texture


   */


   // This shader draws a texture on the mesh.
    Properties
    {
        _MainTex("_MainTex", 2D) = "white" // input from blit 
        _BaseTex("_BaseTex", 2D) = "white" // base texture inputted by script
        _OverlayTex("_OverlayTex", 2D) = "white" // Overlay Texture inputted by script  
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
            TEXTURE2D(_BaseTex);
            TEXTURE2D(_OverlayTex);


            // This macro declares the sampler for the _MainTex and _OverlayTex textures.
            SAMPLER(sampler_MainTex);
            SAMPLER(sampler_BaseTex);
            SAMPLER(sampler_OverlayTex);
  


            CBUFFER_START(UnityPerMaterial)
                // The following line declares the _MainTex_ST and _OverlayTex_ST variables, so that
                // _MainTex and _OverlayTex variables can be used in the fragment shader. The _ST 
                // suffix is necessary for the tiling and offset function to work.
                float4 _MainTex_ST;
                float4 _BaseTex_ST;
                float4 _OverlayTex_ST;
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
                half4 colorBase = SAMPLE_TEXTURE2D(_BaseTex, sampler_BaseTex, IN.uv);
                half4 colorOverlay = SAMPLE_TEXTURE2D(_OverlayTex, sampler_OverlayTex, IN.uv);
                half4 color = colorOverlay;

                if (colorOverlay.a < 1.0f) {
                    color = colorBase;
                }

                return color;
            }
            ENDHLSL
        }
    }
    }
