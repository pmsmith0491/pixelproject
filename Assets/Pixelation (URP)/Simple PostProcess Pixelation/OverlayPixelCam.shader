Shader "Custom/OverlayPixelCam"
{
    // This shader draws a texture on the mesh.
        Properties
        {
            _MainTex("Base Map", 2D) = "white"
            _PixelTexture("_PixelTexture", 2D) = "white"
         }

            SubShader
        {
            Tags {"Queue" = "Transparent" "RenderType" = "Transparent" "RenderPipeline" = "UniversalRenderPipeline"}
            Blend SrcAlpha OneMinusSrcAlpha

            Pass
            {
                HLSLPROGRAM
                #pragma vertex vert
                #pragma fragment frag

                #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"            

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

                // This macro declares _BaseMap as a Texture2D object.
                TEXTURE2D(_MainTex);
                TEXTURE2D(_PixelTexture);

                // This macro declares the sampler for the _BaseMap texture.
                SAMPLER(sampler_MainTex);
                SAMPLER(sampler_PixelTexture);

                CBUFFER_START(UnityPerMaterial)
                    // The following line declares the _BaseMap_ST variable, so that you
                    // can use the _BaseMap variable in the fragment shader. The _ST 
                    // suffix is necessary for the tiling and offset function to work.
                    float4 _MainTex_ST;
                    float4 _PixelTexture_ST;
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

                half4 frag(Varyings IN) : SV_Target
                {
                    half4 colorClean = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);
                    half4 colorPixel = SAMPLE_TEXTURE2D(_PixelTexture, sampler_PixelTexture, IN.uv);
                    half4 color = colorPixel;

                    if (color.r == 1.0f && color.g == 0.0f && color.b == 1.0f) {
                        color = colorClean;
                    }

                    return color;
                }
                ENDHLSL
            }
        }
}
