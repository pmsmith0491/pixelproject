// COPYRIGHT 2022 Peter Smith
Shader "Spriteify/SimplePixelation"
{
    /*
        Input: Takes a texture, resolution(x,y), box size, pixelation target position in viewport space
        ------

        Output: A render texture where each _BoxSize X _BoxSize area has the same color, creating "Macropixels" to generate a pixelation effect. 
        -------                                                 Macropixel color selection is performed relative to pixelation target's position.                

        Can be used for blitting from a Camera to a Texture and back again to make a simple pixelation effect,
                        or can be applied to an object to pixelate the texture (this does not pixelate the object, it simply "downscales" the texture). 
     */

    Properties
    {
        _MainTex("Main Tex", 2D) = "white" // Unpixelated Texture
        _ResolutionX("Resolution X", float) = 1920 // Resolution width of our returned texture
        _ResolutionY("Resolution Y", float) = 1080 // Resolution height of our returned texture
        _BoxSize("Box Size", float) = 8   // Box "width" of a MacroPixel
        _PixelationTargetPos("Pixelation Target Position", Vector) = (0, 0, 0, 0)
    }

        SubShader
    {
        Tags {"RenderType" = "Opaque" "RenderPipeline" = "UniversalRenderPipeline"}

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert // vert is the vertex function
            #pragma fragment frag // frag is the fragment function 

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"            

            struct Attributes
            {
                float4 positionOS   : POSITION;     // position in object space, only used if material is applied to an object with geometry 
                float2 uv           : TEXCOORD0;    // UV coordinate on the texture for the given vertex
            };

            struct Varyings
            {
                float4 positionHCS  : SV_POSITION; // position in clip space, only used if material is applied to an object with geometry  
                float2 uv           : TEXCOORD0;    // UV coordinate on the texture for the given vertex 
            };

            TEXTURE2D(_MainTex); // Declares _MainTex as a Texture2D object.
            SAMPLER(sampler_MainTex); // Declares the sampler for _MainTex.

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST; // Declares _MainTex_ST variable so that _MainTex variable is useable in the fragment shader.
                float _ResolutionX; // declares _ResolutionX as a variable 
                float _ResolutionY; // declares _ResolutionY as a variable 
                float _BoxSize; // declares _BoxSize as a variable 
                float4 _PixelationTargetPos; // declares _PixelationTargetPos as a variable 
            CBUFFER_END


            //PRE: IN is a valid Attributes object. 
            //POST: returns a Varyings object with uv corresponding to a UV in _MainTex, 
            //                and the object position in clip space
            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex); // performs the tiling and offset transformation
                return OUT;
            }


            //PRE: IN is a well defined Varyings object. There is a pixel in the return texture that needs to be colored. 
            //POST: current pixel is the bottom-left most pixel in the square cell of width _BoxSize relative to the pixelation target's position 
            half4 frag(Varyings IN) : SV_Target
            {
                float pixelSizeX = 1 / _ResolutionX;// size of pixel on x axis in normalized space
                float pixelSizeY = 1 / _ResolutionY;// size of pixel on y axis in normalized space
                float CellSizeX = (_BoxSize * pixelSizeX); // "Upscaled" pixel x size in normalized space 
                float CellSizeY = (_BoxSize * pixelSizeY); // "Upscaled" pixel y size in normalized space
                float bottomLeftPixelOfCellX = CellSizeX * floor(IN.uv.x / CellSizeX); // u coordinate of pixel at bottom most leftmost part of square
                float bottomLeftPixelOfCellY = CellSizeY * floor(IN.uv.y / CellSizeY); // v coordinate of pixel at bottom most leftmost part of square
                float2 bottomLeftPixelOfCell = float2(bottomLeftPixelOfCellX, bottomLeftPixelOfCellY); // the bottom left pixel of the cell

                float2 originPosition = float2(_PixelationTargetPos.x, _PixelationTargetPos.y); // the position of the target object's origin (0, 0) if there is no such origin 
                float2 bottomLeftOfOriginCell = float2(CellSizeX * floor(originPosition.x / CellSizeX), CellSizeY * floor(originPosition.y/CellSizeY)); // the position of the bottom left most pixel in the Origin's cell relative to the screen 

                float2 offset = float2(originPosition.x - bottomLeftOfOriginCell.x, originPosition.y - bottomLeftOfOriginCell.y); // the difference between the bottom left pixel of the origin's cell and the origin
             
                bottomLeftPixelOfCell = float2(bottomLeftPixelOfCellX + offset.x, bottomLeftPixelOfCellY + offset.y); // add the offset to pixelate relative to the pixelation target 

                half4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, bottomLeftPixelOfCell);

                return color;
            }
            ENDHLSL
        }
    }
}