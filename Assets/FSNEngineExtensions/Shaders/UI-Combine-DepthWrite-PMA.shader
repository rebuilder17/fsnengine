Shader "FSNCustomShader/UI-Combine-ZWrite-PMA" {
Properties{
	//_Color("Main Color", Color) = (1,1,1,1)
	//_MainTex("Base (RGB) Trans (A)", 2D) = "white" {}

	[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
	_Color("Tint", Color) = (1, 1, 1, 1)
	_DepthCutOff("Depth Cutoff (A)", Float) = 0.5

	[PerRendererData] _SubTexSourceUVs1("Sub Sprite 1 - Source UVs", Vector) = (0, 0, 0, 0)
	[PerRendererData] _SubTexTargetUVs1("Sub Sprite 1 - Target UVs", Vector) = (0, 0, 0, 0)
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
		Tags{ "LightMode" = "ShadowCaster" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
		ZWrite On
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
		fixed4 col = fixed4(1, 1, 1, 1);

		return col;
	}
		ENDCG
	}
	//*/
	
	//*
	Pass 
	{
		Name "ActualRender"
		Tags{ "IgnoreProjector" = "True" "RenderType" = "Transparent" }
		ZWrite Off
		Lighting Off
		Blend One OneMinusSrcAlpha
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
			float2 subuv[2] : TEXCOORD1;
		};

		sampler2D _MainTex;
		float4 _MainTex_ST;
		fixed4 _Color;
		float4 _SubTexSourceUVs1;
		float4 _SubTexTargetUVs1;

		v2f vert(appdata_t v)
		{
			v2f o;
			o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
			fixed4 colorMix = v.color * _Color;					
			o.color = fixed4(colorMix.rgb * colorMix.a, colorMix.a);// Premultiplied alpha에 맞게 Vertex 컬러로 지정된 alpha도 함께 계산해준다.
			o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);

			if (_SubTexTargetUVs1.z - _SubTexTargetUVs1.x > 0 && _SubTexTargetUVs1.w - _SubTexTargetUVs1.y)	// 이미지 덮어쓰기를 실제로 사용할 때만
			{
				o.subuv[0] = (o.texcoord - _SubTexTargetUVs1.xy) / (_SubTexTargetUVs1.zw - _SubTexTargetUVs1.xy);	// 0~1 로 uv 덮어쓰기 범위 설정
				o.subuv[1] = o.subuv[0] * (_SubTexSourceUVs1.zw - _SubTexSourceUVs1.xy) + _SubTexSourceUVs1.xy;		// 소스 uv 설정
			}
			return o;
		}

		fixed4 frag(v2f i) : SV_Target
		{
			fixed4 col;
			
			half2 texcoord = i.texcoord;
			float2 sub1area = i.subuv[0];
			float2 sub1src = i.subuv[1];

			if ((sub1area.x >= 0 && sub1area.x <= 1) && (sub1area.y >= 0 && sub1area.y <= 1))	// 덮어쓰기 범위에 들어온 경우 서브 텍스쳐 소스 uv를 사용
			{
				col = tex2D(_MainTex, sub1src) * i.color;
			}
			else
			{
				col = tex2D(_MainTex, texcoord) * i.color;
			}
			//return fixed4(col.rgb * col.a, col.a);
			return col;
		}
		ENDCG
	}
	//*/
}
	Fallback "Legacy Shaders/VertexLit"
	//Fallback "Diffuse"
}
