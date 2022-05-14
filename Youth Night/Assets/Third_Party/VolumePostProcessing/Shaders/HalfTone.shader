Shader "Hidden/CustomPostProcess/HalfTone"
{
	Properties 
	{
		  [HideInInspector] _MainTex ("Texture", 2D) = "white" {}
	}

	SubShader
	{
		Cull Off
		ZWrite Off
		ZTest Off

		Pass
		{
			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

			struct appdata
			{
				uint vertexID : SV_VertexID;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;

			};

			TEXTURE2D_X(_MainTex);
			SAMPLER(sampler_MainTex);

			float _CellSize;
			float _DotSize;
			float _DotSmoothness;
			float4 _BackgroundColor;
			float4 _DotColor;

			v2f vert(appdata v)
			{
				v2f o;

				o.vertex = GetFullScreenTriangleVertexPosition(v.vertexID);
				o.uv = GetFullScreenTriangleTexCoord(v.vertexID);

				return o;
			}

			float4 frag(v2f i) : SV_Target
			{
				float4 src = SAMPLE_TEXTURE2D_X(_MainTex, sampler_MainTex, i.uv.xy);

				float3 texColor = src.rgb;

				//Create Cells
				float cellWidth = _CellSize / _ScreenParams.x;
				float cellHeight = _CellSize / _ScreenParams.y;
				float2 roundedUV;
				roundedUV.x = round(i.uv.x / cellWidth) * cellWidth;
				roundedUV.y = round(i.uv.y / cellHeight) * cellHeight;

				//Calculate Distance From Cell Center
				float2 distanceVector = i.uv - roundedUV;
				distanceVector.x = (distanceVector.x / _ScreenParams.y) * _ScreenParams.x;
				float distanceFromCenter = length(distanceVector);

				//Calculate Dot Size
				float dotSize = _DotSize / _ScreenParams.x;
				float4 roundedCol = SAMPLE_TEXTURE2D_X(_MainTex, sampler_MainTex, i.uv.xy); //use roundedUV instead of i.uv for different effect
				float luma = dot(roundedCol.rgb, float3(0.2126, 0.7152, 0.0722));
				dotSize *= (1 - luma);

				//Calculate Displayed Color
				float lerpAmount = smoothstep(dotSize, dotSize + _DotSmoothness, distanceFromCenter);

				_DotColor = lerp(_DotColor,src,1-_DotColor.a);
				_BackgroundColor = lerp(_BackgroundColor,src,1-_BackgroundColor.a);

				float4 col = lerp(_DotColor, _BackgroundColor, lerpAmount);
				return col;
		}
			ENDHLSL
		}
	}
}