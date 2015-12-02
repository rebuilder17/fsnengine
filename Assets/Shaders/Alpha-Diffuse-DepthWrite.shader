Shader "FSNCustomShader/Diffuse-ZWrite" {
Properties {
	_Color ("Main Color", Color) = (1,1,1,1)
	_MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
}

	SubShader{
		Tags{ "RenderType" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
		LOD 200

		CGPROGRAM
#pragma surface surf Lambert alpha:blend

		sampler2D _MainTex;
		fixed4 _Color;

		struct Input {
			float2 uv_MainTex;
		};

		void surf(Input IN, inout SurfaceOutput o) {
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = c.rgb;
			o.Alpha = c.a;
		}
		ENDCG
	}

	//Fallback "Legacy Shaders/VertexLit"
	Fallback "Diffuse"
}



/*
SubShader {
Tags{ "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
LOD 200
ZWrite Off

Pass
{
Tags{ "LightMode" = "ShadowCaster" }
ZWrite On
ColorMask A
//AlphaTest Greater 0

CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#include "UnityCG.cginc"

sampler2D _MainTex;

struct v2f {
float4 pos : SV_POSITION;
float2 texCoords : TEXCOORD0;
};

v2f vert(appdata_base v)
{
v2f o;
o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
o.texCoords = v.texcoord;
return o;
}

half4 frag(v2f i) : SV_Target
{
//return half4 (0, 0, 0, tex2D(_MainTex, i.texCoords).a);
return half4 (0, 0, 0, 1);
}
ENDCG
}

CGPROGRAM
#pragma surface surf Lambert alpha:blend

sampler2D _MainTex;
fixed4 _Color;

struct Input {
float2 uv_MainTex;
};

void surf(Input IN, inout SurfaceOutput o) {
fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
o.Albedo = c.rgb;
o.Alpha = c.a;
}
ENDCG
}

//Fallback "Legacy Shaders/Transparent/VertexLit"
Fallback "Diffuse"
}
*/