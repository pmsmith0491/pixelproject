// This shader fills the mesh shape with a color predefined in the code.
Shader "Custom/URP Pixelation"
{
    // The properties block of the Unity shader. In this example this block is empty
    // because the output color is predefined in the fragment shader code.
    Properties
    { 
        _BaseMap("Base Map", 2D) = "white" {}
        _ResolutionX("ResolutionX", float) = 512
        _ResolutionY("ResolutionY", float) = 288
        _BoxSize("Box Size", float) = 8   // we want our box size to be ResolutionX / (AspectRatioX * AspectRatioY)
    }

    // The SubShader block containing the Shader code. 
            SubShader
        {
            Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }

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
                TEXTURE2D(_BaseMap);
                // This macro declares the sampler for the _BaseMap texture.
                SAMPLER(sampler_BaseMap);

                CBUFFER_START(UnityPerMaterial)
                    // The following line declares the _BaseMap_ST variable, so that you
                    // can use the _BaseMap variable in the fragment shader. The _ST 
                    // suffix is necessary for the tiling and offset function to work.
                    float4 _BaseMap_ST;
                CBUFFER_END

                Varyings vert(Attributes IN)
                {
                    Varyings OUT;
                    OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                    // The TRANSFORM_TEX macro performs the tiling and offset
                    // transformation.
                    OUT.uv = TRANSFORM_TEX(IN.uv, _BaseMap);
                    return OUT;
                }

                float _BoxSize;
                float _ResolutionX;
                float _ResolutionY;

                // The fragment shader definition.            
                half4 frag(Varyings IN) : SV_Target
                {
                    float pixelSizeX = 1 / _ResolutionX;// size of pixel on x axis in normalized space
                    float pixelSizeY = 1 / _ResolutionY;// size of pixel on y axis in normalized space
                    float CellSizeX = _BoxSize * pixelSizeX; // "Upscaled" pixel x size in normalized space 
                    float CellSizeY = _BoxSize * pixelSizeY; // "Upscaled" pixel y size in normalized space
                    float bottomLeftPixelOfCellX = CellSizeX * floor(IN.positionHCS.x / CellSizeX); // u coordinate of pixel at bottom most leftmost part of square
                    float bottomLeftPixelOfCellY = CellSizeY * floor(IN.positionHCS.y / CellSizeY); // v coordinate of pixel at bottom most leftmost part of square
                    float2 bottomLeftPixelOfCell = float2(bottomLeftPixelOfCellX, bottomLeftPixelOfCellY);

                    half4 col = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, bottomLeftPixelOfCell); // set color of pixel sampled from MainTex at position of bottom most left most pixel of the cell of size CellSizeX x CellSizeY

                    return col;
                }
    ENDHLSL
}
    }
}