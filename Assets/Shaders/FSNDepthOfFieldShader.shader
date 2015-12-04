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
		Blend Off

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
		sampler2D		_MainTex;
		sampler2D		_BlurTex;
		sampler2D		_FgTex;
		sampler2D		_FgBlurTex;

		half4 _CurveParams;
		half4 _InvSourceSize;


		v2f vertHorizontal(appdata v)
		{
			v2f o;
			o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);

			const float2 uvOffset[6] =
			{
				float2(-2.8, -0.8),
				float2(-1.5, -0.4),
				float2(-0.5, 0),
				float2(0.5, 0),
				float2(1.5, 0.4),
				float2(2.8, 0.8)
			};

			float2 uv = v.uv;
			o.uv = uv;
			for (int i = 0; i < 6; i++)
			{
				o.uvSample[i] = uv + uvOffset[i] * 3 * _InvSourceSize;
			}

			return o;
		}

		v2f vert4Smp(appdata v)
		{
			v2f o;
			o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);

			const float2 uvOffset[4] =
			{
				float2(-1.8, +1.2),
				float2(+1.2, +1.8),
				float2(+1.8, -1.2),
				float2(-1.2, -1.8)
			};

			float2 uv = v.uv;
				o.uv = uv;
			for (int i = 0; i < 4; i++)
			{
				o.uvSample[i] = uv + uvOffset[i] * _InvSourceSize;
			}

			return o;
		}

		v2f vert(appdata v)
		{
			v2f o;
			o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
			float2 uv = v.uv;
			o.uv = uv;
			return o;
		}

		v2f vertVertical(appdata v)
		{
			v2f o;
			o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);

			const float2 uvOffset[6] =
			{
				float2(-0.8, -2.8),
				float2(-0.4, -1.5),
				float2(0, -0.5),
				float2(0, 0.5),
				float2(0.4, 1.5),
				float2(0.8, 2.8)
			};

			float2 uv = v.uv;
			o.uv = uv;
			for (int i = 0; i < 6; i++)
			{
				o.uvSample[i] = uv + uvOffset[i] * 3 * _InvSourceSize;
			}

			return o;
		}

		fixed4 fragCommon(v2f inp) : SV_Target
		{
			const float weight[6] = { 0.35, 0.65, 1.0, 1.0, 0.65, 0.35 };

			float2 uvcur = inp.uv;
			fixed4 col = tex2D(_MainTex, uvcur);
			float weightSum = 1.0;

			for (int i = 0; i < 6; i++)
			{
				float w = weight[i];
				float2 uvsmp = inp.uvSample[i].xy;
				col += tex2D(_MainTex, uvsmp) * w;
				weightSum += w;
			}

			return col / weightSum;
		}

		fixed4 fragDOF(v2f inp) : SV_Target
		{
			float2 uvcur = inp.uv;
			fixed4 colOrig = tex2D(_MainTex, uvcur);
			fixed4 colBlur = tex2D(_BlurTex, uvcur);
			for (int i = 0; i < 4; i++)
			{
				colBlur += tex2D(_BlurTex, inp.uvSample[i]);
			}
			colBlur = saturate(colBlur / 5.0);

			float dcur = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, uvcur);
			dcur = Linear01Depth(dcur);

			// BG
			half fd01_bg = _CurveParams.w + _CurveParams.z;
			half coc_bg = 0.0;
			if (dcur > fd01_bg)
				coc_bg = (dcur - fd01_bg);
			coc_bg = saturate(coc_bg * _CurveParams.y);

			half coc = coc_bg;
			
			half3 bgfinal = lerp(colOrig, colBlur, coc).rgb;

			// FG
			//fixed colFgA = tex2D(_FgTex, uvcur).a;
			fixed colFgA = tex2D(_FgTex, uvcur).r;
			//fixed4 colFgBlur = tex2D(_FgBlurTex, uvcur);
			/*
			for (int i = 0; i < 4; i++)
			{
				colFgA += tex2D(_FgTex, inp.uvSample[i]).a;
			}
			colFgA = saturate(colFgA / 5.0);
			*/

			half4 final = half4(lerp(bgfinal, colBlur.rgb, colFgA).rgb, 1);
			//half4 final = half4(bgfinal.rgb, 1);
			//half4 final = half4(colFg.rgb, 1);
			return final;
		}

		fixed4 fragFG(v2f inp) : SV_Target
		{
			float2 uvcur = inp.uv;
			fixed4 colOrig = tex2D(_MainTex, uvcur);

			float dcur = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, uvcur);
			dcur = Linear01Depth(dcur);

			half fd01_fg = (_CurveParams.w - _CurveParams.z);
			half coc_fg = 0.0;
			if (dcur < fd01_fg)
				coc_fg = (fd01_fg - dcur);
			coc_fg = saturate(coc_fg * _CurveParams.x);

			//return half4(colOrig.rgb, coc_fg);
			return half4(coc_fg, coc_fg, coc_fg, coc_fg);
		}

		fixed4 fragActualFG(v2f inp) : SV_Target
		{
			float2 uvcur = inp.uv;
			//fixed4 colFgOrig = tex2D(_FgTex, uvcur);
			fixed4 colFgOrig = tex2D(_MainTex, uvcur);	// _MainTex == _FgTex
			fixed4 colFgBlur = tex2D(_FgBlurTex, uvcur);

			//fixed coc = 2 * max(colFgBlur.a, colFgOrig.a) - colFgOrig.a;
			fixed coc = 2 * max(colFgBlur.r, colFgOrig.r) - colFgOrig.r;
			//return fixed4(colFgOrig.rgb, coc);
			return fixed4(coc, coc, coc, coc);
		}
		ENDCG

		Pass
		{
			Name "1. Horizontal Blur"

			ColorMask RGB

			CGPROGRAM
			#pragma vertex vertHorizontal
			#pragma fragment fragCommon

			ENDCG
		}

		Pass
		{
			Name "2. Vertical Blur"

			ColorMask RGB

			CGPROGRAM
			#pragma vertex vertVertical
			#pragma fragment fragCommon

			ENDCG
		}

		Pass
		{
			Name "3. DoF (BG)"

			ColorMask RGB

			CGPROGRAM
			#pragma vertex vert4Smp
			#pragma fragment fragDOF
			ENDCG

		}

		Pass
		{
			Name "4. Make Foreground"

			ColorMask RGB

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment fragFG
			ENDCG
		}

		Pass
		{
			Name "5. Actual Foreground"

			ColorMask RGB

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment fragActualFG
			ENDCG
		}
	}
}
