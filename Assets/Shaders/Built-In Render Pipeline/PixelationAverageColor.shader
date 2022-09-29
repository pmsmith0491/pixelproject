Shader "Custom/PixelationAverageColor"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _ResolutionX("ResolutionX", float) = 512
        _ResolutionY("ResolutionY", float) = 288
        _BoxSize("Box Size", float) = 8   // we want our box size to be ResolutionX / (AspectRatioX * AspectRatioY)
    }
        SubShader
        {
            // No culling or depth
            Cull Off ZWrite Off ZTest Always


            Pass
            {

                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag

                #include "UnityCG.cginc"

                sampler2D _MainTex;
                float _BoxSize;
                float _ResolutionX;
                float _ResolutionY;

                struct appdata
                {
                    float4 vertex : POSITION;
                    float2 uv : TEXCOORD0;
                };

                struct v2f
                {
                    float2 uv : TEXCOORD0;
                    float4 vertex : SV_POSITION;
                };

                v2f vert(appdata v)
                {
                    v2f o;
                    o.vertex = UnityObjectToClipPos(v.vertex);
                    o.uv = v.uv;
                    return o;
                }


                //PRE: i is a well defined v2f.
                //POST: returns color of leftmost bottommost pixel in cell of size 
                //      (_BoxSize / Window SizeX, _BoxSize / Window Size Y) in normalized space between 0 and 1.   
                fixed4 frag(v2f i) : SV_Target
                {
                    float pixelSizeX = 1 / _ResolutionX;// size of pixel on x axis in normalized space
                    float pixelSizeY = 1 / _ResolutionY;// size of pixel on y axis in normalized space
                    float CellSizeX = _BoxSize * pixelSizeX; // "Upscaled" pixel x size in normalized space 
                    float CellSizeY = _BoxSize * pixelSizeY; // "Upscaled" pixel y size in normalized space
               
                    
                    float lowestXOfCell = CellSizeX * floor(i.uv.x / CellSizeX); // u coordinate of pixel at lowest x value of cell
                    float lowestYOfCell = CellSizeY * floor(i.uv.y / CellSizeY); // v coordinate of pixel at lowest y value of cell
                    float highestXOfCell = CellSizeX * ceil(i.uv.x / CellSizeX); // u coordinate of pixel at highest x value of cell
                    float highestYOfCell = CellSizeY * ceil(i.uv.y / CellSizeY); // v coordinate of pixel at highest y value of cell


                    float2 bottomLeftPixelOfCell = float2(lowestXOfCell, lowestYOfCell); // leftmost bottommost pixel of cell
                    float2 bottomRightPixelOfCell = float2(highestXOfCell, lowestYOfCell); // rightmost bottommost pixel of cell
                    float2 topRightPixelOfCell = float2(highestXOfCell, highestYOfCell); // rightmost topmost pixel of cell
                    float2 topLeftPixelOfCell = float2(lowestXOfCell, highestYOfCell); // leftmost topmost pixel of cell

                    fixed4 averageColor = 0;

                    for (float x = 0; x < _BoxSize; x ++) {
                        float u = lowestXOfCell + (x * pixelSizeX);
                        for (float y = 0; y < _BoxSize; y++) {
                            float v = lowestYOfCell + (y * pixelSizeY);
                            averageColor += tex2D(_MainTex, float2(u, v));
                        }
                    }

                    averageColor = averageColor / (_BoxSize * _BoxSize);

                    return averageColor;
                }


                ENDCG

            }
        }
}
