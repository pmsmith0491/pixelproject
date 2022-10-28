Shader "Custom/ToonShaderURP"
{
    Properties
    {
        _MainTex("BaseMap", 2D) = "white" {}
    }

        // Universal Render Pipeline subshader. If URP is installed this will be used.
            SubShader
        {

            Tags {"RenderType" = "Opaque" "RenderPipeline" = "UniversalRenderPipeline"}

            Pass
            {
                Tags { "LightMode" = "UniversalForward" }

                HLSLPROGRAM
                #pragma vertex vert
                #pragma fragment frag

                #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

                struct Attributes
                {
                    float4 positionOS   : POSITION;
                    float2 uv           : TEXCOORD0;
                };

                struct Varyings
                {
                    float2 uv           : TEXCOORD0;
                    float4 positionHCS  : SV_POSITION;
                };

                TEXTURE2D(_MainTex);
                SAMPLER(sampler_MainTex);

                CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                half4 _BaseColor;
                CBUFFER_END

                Varyings vert(Attributes IN)
                {
                    Varyings OUT;
                    OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                    OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
                    return OUT;
                }


                half4 frag(Varyings IN) : SV_Target
                {
                    half4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);
                    color.a = 1.0f;
                    return color;
                }
                ENDHLSL
            }
        }
}