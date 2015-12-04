Shader "Hidden/FSNDepthOfFieldShader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always

		CGINCLUDE
		#include "UnityCG.cginc"

		struct appdata
		{
			float4 vertex : POSITION;
			float2 uv : TEXCOORD0;
		};

		struct v2f
		{
			float2 uv			: TEXCOORD0;
			float2 uvSample[6]	: TEXCOORD1;
			float4 vertex		: SV_POSITION;
		};

		sampler2D_float	_CameraDepthTexture;
		uniform float4	_MainTex_TexelSize;
		sampler2D		_GrabTexture;
		sampler2D		_MainTex;

		v2f vertHorizontal(appdata v)
		{
			v2f o;
			o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);

			const float2 uvOffset[6] =
			{
				float2(-3.0, 0),
				float2(-2.0, 0),
				float2(-1.0, 0),
				float2(1.0, 0),
				float2(2.0, 0),
				float2(3.0, 0)
			};

			float2 uv = v.uv;
			o.uv = uv;
			o.uvSample[0] = uv + uvOffset[0] * _MainTex_TexelSize;
			o.uvSample[1] = uv + uvOffset[1] * _MainTex_TexelSize;
			o.uvSample[2] = uv + uvOffset[2] * _MainTex_TexelSize;
			o.uvSample[3] = uv + uvOffset[3] * _MainTex_TexelSize;
			o.uvSample[4] = uv + uvOffset[4] * _MainTex_TexelSize;
			o.uvSample[5] = uv + uvOffset[5] * _MainTex_TexelSize;

			return o;
		}

		v2f vertVertical(appdata v)
		{
			v2f o;
			o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);

			const float2 uvOffset[6] =
			{
				float2(0, -3.0),
				float2(0, -2.0),
				float2(0, -1.0),
				float2(0, 1.0),
				float2(0, 2.0),
				float2(0, 3.0)
			};

			float2 uv = v.uv;
			o.uv = uv;
			o.uvSample[0] = uv + uvOffset[0] * _MainTex_TexelSize;
			o.uvSample[1] = uv + uvOffset[1] * _MainTex_TexelSize;
			o.uvSample[2] = uv + uvOffset[2] * _MainTex_TexelSize;
			o.uvSample[3] = uv + uvOffset[3] * _MainTex_TexelSize;
			o.uvSample[4] = uv + uvOffset[4] * _MainTex_TexelSize;
			o.uvSample[5] = uv + uvOffset[5] * _MainTex_TexelSize;

			return o;
		}

		fixed4 fragCommon(v2f i) : SV_Target
		{
			fixed4 col = tex2D(_MainTex, i.uv);

			const float weight[6] = { 0.35, 0.65, 1.0, 1.0, 0.65, 0.35 };
			const float weightSum = 4.0;

			fixed4 smpCol[6];
			smpCol[0] = tex2D(_MainTex, i.uvSample[0]) * weight[0];
			smpCol[1] = tex2D(_MainTex, i.uvSample[1]) * weight[1];
			smpCol[2] = tex2D(_MainTex, i.uvSample[2]) * weight[2];
			smpCol[3] = tex2D(_MainTex, i.uvSample[3]) * weight[3];
			smpCol[4] = tex2D(_MainTex, i.uvSample[4]) * weight[4];
			smpCol[5] = tex2D(_MainTex, i.uvSample[5]) * weight[5];

			return (col + smpCol[0] + smpCol[1] + smpCol[2] + smpCol[3] + smpCol[4] + smpCol[5]) / (1.0 + weightSum);
			//return col;
		}
		ENDCG

		Pass
		{
			Name "Horizontal Blur"

			CGPROGRAM
			#pragma vertex vertHorizontal
			#pragma fragment fragCommon

			ENDCG
		}

		Pass
		{
			Name "Vertical Blur"

			CGPROGRAM
			#pragma vertex vertVertical
			#pragma fragment fragCommon

			ENDCG
		}
	}
}
