Shader "FSNCustomShader/UI-Default-ZWrite" {
Properties{
	//_Color("Main Color", Color) = (1,1,1,1)
	//_MainTex("Base (RGB) Trans (A)", 2D) = "white" {}

	[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
	_Color("Tint", Color) = (1, 1, 1, 1)
	_DepthCutOff("Depth Cutoff (A)", Float) = 0.5
}

SubShader{
	Tags{ "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }

	Cull Off
	Lighting Off
	ZWrite Off
	ZTest[unity_GUIZTestMode]
	//ZTest Always
	//Blend SrcAlpha OneMinusSrcAlpha
	
	//*
	Pass 
	{
		Name "ZWrite"
		//Tags{ "LightMode" = "ShadowCaster" "IgnoreProjector" = "True" "RenderType" = "Opaque" }
		Tags{ "LightMode" = "ShadowCaster" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
		ZWrite On
		//ZTest Always
		ColorMask 0
		Blend Off
		//AlphaTest Greater [_DepthCutOff]
		
		CGPROGRAM
#pragma vertex vert
#pragma fragment frag

#include "UnityCG.cginc"

	struct appdata_t {
		float4 vertex : POSITION;
		float4 color    : COLOR;
		float2 texcoord : TEXCOORD0;
	};

	struct v2f {
		float4 vertex : SV_POSITION;
		fixed4 color : COLOR;
		half2 texcoord : TEXCOORD0;
	};

	sampler2D _MainTex;
	float4 _MainTex_ST;
	fixed4 _Color;
	float _DepthCutOff;

	v2f vert(appdata_t v)
	{
		v2f o;
		o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
		o.color = v.color * _Color;
		o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
		return o;
	}

	fixed4 frag(v2f i) : SV_Target
	{
		fixed alpha = tex2D(_MainTex, i.texcoord).a * i.color.a;
		clip(alpha - _DepthCutOff);
		fixed4 col = fixed4(0, 0, 0, 0);

		//fixed4 col = tex2D(_MainTex, i.texcoord) * i.color;

		return col;
	}
		ENDCG
	}
	//*/
	
	Pass 
	{
		Name "ActualRender"
		Tags{ "IgnoreProjector" = "True" "RenderType" = "Transparent" }
		ZWrite Off
		Lighting Off
		Blend SrcAlpha OneMinusSrcAlpha
		//Blend Off

		CGPROGRAM
#pragma vertex vert
#pragma fragment frag

#include "UnityCG.cginc"

		struct appdata_t {
			float4 vertex : POSITION;
			float4 color    : COLOR;
			float2 texcoord : TEXCOORD0;
		};

		struct v2f {
			float4 vertex : SV_POSITION;
			fixed4 color : COLOR;
			half2 texcoord : TEXCOORD0;
		};

		sampler2D _MainTex;
		float4 _MainTex_ST;
		fixed4 _Color;

		v2f vert(appdata_t v)
		{
			v2f o;
			o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
			o.color = v.color * _Color;
			o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
			return o;
		}

		fixed4 frag(v2f i) : SV_Target
		{
			fixed4 col = tex2D(_MainTex, i.texcoord) * i.color;
			return col;
		}
		ENDCG
	}
}
	//Fallback "Legacy Shaders/VertexLit"
	//Fallback "Diffuse"

}
