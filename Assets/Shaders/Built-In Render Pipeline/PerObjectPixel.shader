Shader "Custom/PerObjectPixel"
{
	Properties
	{
		_Color("Color", Color) = (0.5, 0.65, 1, 1)
		_MainTex("Main Texture", 2D) = "white" {}
		_ResolutionX("ResolutionX", float) = 512
		_ResolutionY("ResolutionY", float) = 288
		_BoxSize("Box Size", float) = 8   // we want our box size to be ResolutionX / (AspectRatioX * AspectRatioY)

		[HDR]
		_AmbientColor("Ambient Color", Color) = (0.4,0.4,0.4,1)
	}
		SubShader
		{
			Pass
			{
				Tags
				{
					"LightMode" = "ForwardBase"
					"PassFlags" = "OnlyDirectional"
				}

				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag

				#include "UnityCG.cginc"
				#include "Lighting.cginc"

				struct appdata
				{
					float4 vertex : POSITION;
					float4 uv : TEXCOORD0;
					float3 normal: NORMAL;
				};

				struct v2f
				{
					float4 pos : SV_POSITION;
					float2 uv : TEXCOORD0;
					float3 worldNormal: NORMAL;
				};

				sampler2D _MainTex;
				float4 _MainTex_ST;
				float _BoxSize;
				float _ResolutionX;
				float _ResolutionY;


				v2f vert(appdata v)
				{
					v2f o;
					o.pos = UnityObjectToClipPos(v.vertex);
					o.uv = TRANSFORM_TEX(v.uv, _MainTex);
					o.worldNormal = UnityObjectToWorldNormal(v.normal);
					return o;
				}

				float4 _Color;
				float4 _AmbientColor;


				float4 frag(v2f i) : SV_Target
				{

					float3 normal = normalize(i.worldNormal);
					float NdotL = dot(_WorldSpaceLightPos0, normal);
					float lightIntensity = NdotL > 0 ? 1 : 0; // if ndotl > 0, light intensity = 1, 0 otherwise
					float4 light = lightIntensity * _LightColor0; // changes color of pixel based on light color
					float pixelSizeX = 1 / _ResolutionX;// size of pixel on x axis in normalized space
					float pixelSizeY = 1 / _ResolutionY;// size of pixel on y axis in normalized space
					float CellSizeX = _BoxSize * pixelSizeX; // "Upscaled" pixel x size in normalized space 
					float CellSizeY = _BoxSize * pixelSizeY; // "Upscaled" pixel y size in normalized space
					float bottomLeftPixelOfCellX = CellSizeX * floor(i.uv.x / CellSizeX); // u coordinate of pixel at bottom most leftmost part of square
					float bottomLeftPixelOfCellY = CellSizeY * floor(i.uv.y / CellSizeY); // v coordinate of pixel at bottom most leftmost part of square

					float2 bottomLeftPixelOfCell = float2(bottomLeftPixelOfCellX, bottomLeftPixelOfCellY);

					float2 topRightPixel = float2(CellSizeX * ceil(i.uv.x / CellSizeX), CellSizeY * ceil(i.uv.y / CellSizeY));

					float2 middlePixel = float2(bottomLeftPixelOfCellX + (CellSizeX * 0.5), bottomLeftPixelOfCellY + (CellSizeY * 0.5));

					fixed4 col = tex2D(_MainTex, topRightPixel); // set color of pixel sampled from MainTex at position of bottom most left most pixel of the cell of size CellSizeX x CellSizeY


					return _Color * col * (_AmbientColor + light);
				}
				ENDCG
			}
		}
}