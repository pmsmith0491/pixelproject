// This shader draws a texture on the mesh.
Shader "Custom/SimplePixelation"
{
    Properties
    {
        _MainTex("Base Map", 2D) = "white"
        _ResolutionX("ResolutionX", float) = 512
        _ResolutionY("ResolutionY", float) = 288
        _BoxSize("Box Size", float) = 8   // we want our box size to be ResolutionX / (AspectRatioX * AspectRatioY)
        _PixelationTargetPos("_PixelationTargetPos", Vector) = (0, 0, 0, 0)
    }

        SubShader
    {
        Tags {"RenderType" = "Opaque" "RenderPipeline" = "UniversalRenderPipeline"}
       // Blend SrcAlpha OneMinusSrcAlpha

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
            // This macro declares the sampler for the _BaseMap texture.
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                // The following line declares the _BaseMap_ST variable, so that you
                // can use the _BaseMap variable in the fragment shader. The _ST 
                // suffix is necessary for the tiling and offset function to work.
                float4 _MainTex_ST;
                float _ResolutionX;
                float _ResolutionY;
                float _BoxSize;
                float4 _PixelationTargetPos;

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


                float pixelSizeX = 1 / _ResolutionX;// size of pixel on x axis in normalized space
                float pixelSizeY = 1 / _ResolutionY;// size of pixel on y axis in normalized space
                float CellSizeX = (_BoxSize * pixelSizeX); // "Upscaled" pixel x size in normalized space 
                float CellSizeY = (_BoxSize * pixelSizeY); // "Upscaled" pixel y size in normalized space
                float bottomLeftPixelOfCellX = CellSizeX * floor(IN.uv.x / CellSizeX); // u coordinate of pixel at bottom most leftmost part of square
                float bottomLeftPixelOfCellY = CellSizeY * floor(IN.uv.y / CellSizeY); // v coordinate of pixel at bottom most leftmost part of square
                // middlePixel = float2((bottomLeftPixelOfCellX + (CellSizeX * 0.5)) + offsetFromTargetX, (bottomLeftPixelOfCellY + (CellSizeY * 0.5)) + offsetFromTargetY);

                float2 bottomLeftPixelOfCell = float2(bottomLeftPixelOfCellX, bottomLeftPixelOfCellY);
                float2 originPosition = float2(_PixelationTargetPos.x, _PixelationTargetPos.y);
                float2 bottomLeftOfOriginCell = float2(CellSizeX * floor(originPosition.x / CellSizeX), CellSizeY * floor(originPosition.y/CellSizeY));
                float2 offset = float2(originPosition.x - bottomLeftOfOriginCell.x, originPosition.y - bottomLeftOfOriginCell.y);
                bottomLeftPixelOfCell = float2(bottomLeftPixelOfCellX + offset.x, bottomLeftPixelOfCellY + offset.y);

                // The SAMPLE_TEXTURE2D marco samples the texture with the given
                // sampler.
                half4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, bottomLeftPixelOfCell);
   
                return color;
            }
            ENDHLSL
        }
    }
}