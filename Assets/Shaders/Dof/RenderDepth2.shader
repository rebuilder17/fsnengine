Shader "Custom/RenderDepth 2"
 {
     Properties
     {
         _MainTex ("Base (RGB)", 2D) = "white" {}
         _DepthLevel ("Depth Level", float) = 1
         _DepthOffset ("Depth Offset", float) = 0
     }
     SubShader
     {
         Pass
         {
             CGPROGRAM
 
             #pragma vertex vert
             #pragma fragment frag
             #include "UnityCG.cginc"
             
             uniform sampler2D _MainTex;
             uniform sampler2D _CameraDepthTexture;
             uniform half _DepthLevel;
             uniform half _DepthOffset;
             uniform half _Focus;
             uniform half4 _MainTex_TexelSize;
 
             struct inp
             {
                 float4 pos : POSITION;
                 half2 uv : TEXCOORD0;
             };
 
             struct outp
             {
                 float4 pos : SV_POSITION;
                 half2 uv : TEXCOORD0;
             };
 
 
             outp vert(inp i)
             {
                 outp o;
                 o.pos = mul(UNITY_MATRIX_MVP, i.pos);
                 o.uv = MultiplyUV(UNITY_MATRIX_TEXTURE0, i.uv);
 
                 // why do we need this? cause sometimes the image I get is flipped. see: http://docs.unity3d.com/Manual/SL-PlatformDifferences.html
                 #if UNITY_UV_STARTS_AT_TOP
                 if (_MainTex_TexelSize.y < 0)
                         o.uv.y = 1 - o.uv.y;
                 #endif
 
                 return o;
             }
             
             half4 frag(outp o) : COLOR
             {
                 half depth = Linear01Depth(UNITY_SAMPLE_DEPTH(tex2D(_CameraDepthTexture, o.uv)));
                 depth = abs(depth-_Focus);
                 depth = pow(depth, _DepthLevel);
                 return depth;
             }
             
             ENDCG
         }
     } 
 }