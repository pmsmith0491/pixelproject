Shader "Custom/NewUnlitShader"
{
    Properties
    {
        _MainTex("BaseMap", 2D) = "white" {}
        _PixelSize("PixelSize", Float) = 5
    }

        // Universal Render Pipeline subshader. If URP is installed this will be used.
        SubShader
    {

        Tags {"Queue" = "Transparent" "RenderType" = "Transparent" "RenderPipeline" = "UniversalRenderPipeline"}
        Blend SrcAlpha OneMinusSrcAlpha
        //Cull back



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
            half _PixelSize;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
                return OUT;
            }



            /*
            * PRE: color is a valid half4, posX and posY are valid halfs
            * POST:
                returns the pixel's color based upon its position in a size x size grid.

                ex: 5x5

                # # # # #
                # # # # #
                # # * # #
                # # # # #
                # # # # #
                
                # pixels are drawn transparent, * is opaque.

                */
            half pixel_transparent(half4 color, half posX, half posY) {
                
                half alpha = color.a;
                half xfactor = step(fmod(abs(floor(posX)), _PixelSize), 0.999);
                half yfactor = step(fmod(abs(floor(posY - _PixelSize)), _PixelSize), 0.999);
                alpha = (alpha * xfactor) * (alpha * yfactor);


                // So why are you cutting out the transparency when you could just make it the same color as the pixel in the center? 

                /*
                is it because you can control the alpha but you can't control the location of color in screen pixels?
                

                idea: have another texture that gets what the screen is currently looking at, then the positions of the object
                           line up with the UVs of the texture. Sample the texture using the position, and use that as the color 
                           instead of doing transparency operations. 

                           unfortunately, this will not create uniform squares, it'll just fill the sillhoutte 
                
                */

                return alpha;
            }


            //PRE: IN is a valid Varyings object
            //POST: returns transparent if current pixel is not the center of _PixelSize x PixelSize region. 
            //      returns opaque pixel otherwise. 
            half4 frag(Varyings IN) : SV_Target
            {
                half4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);
                color.a = color.a = (IN.positionHCS.x + IN.positionHCS.y) % 2;
                return color;                
            }
            ENDHLSL
        }
    }
}