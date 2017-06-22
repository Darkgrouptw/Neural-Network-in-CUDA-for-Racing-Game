Shader "SD4M/Transparent/Cutout/Emissive Reflection" {
Properties {
	_Color ("Main Color", Color) = (1,1,1,1)
	_MainTex ("Base (RGB)", 2D) = "white" {}
	_ComplexTex ("RefMask (G) Trans (B)", 2D) = "white" {}
	_EmissiveTex ("Emissive (RGB)", 2D) = "black" {}
	_Cube ("Cubemap", CUBE) = "" {}
	_EmissiveValue ("Emissive value", Float) = 0.5
	_CubeIntensity ("Cubemap Intensity", Float) = 0.5
	_Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
}
SubShader {
	ColorMask RGB
	Tags {"Queue"="AlphaTest" "IgnoreProjector"="True" "RenderType"="TransparentCutout"}
	LOD 150

CGPROGRAM
#pragma surface surf Lambert alphatest:_Cutoff

sampler2D _MainTex;
sampler2D _ComplexTex;
sampler2D _EmissiveTex;
samplerCUBE _Cube;
float _EmissiveValue;
fixed4 _Color;
half _CubeIntensity;

struct Input {
	float2 uv_MainTex;
	float2 uv_EmissiveTex;
	float3 worldRefl;
	float4 color: COLOR;
};

void surf (Input IN, inout SurfaceOutput o) {
	fixed4 mainTex = tex2D(_MainTex, IN.uv_MainTex) * IN.color * _Color;
	fixed4 complexTex = tex2D(_ComplexTex, IN.uv_MainTex);
	fixed4 emissiveTex = tex2D(_EmissiveTex, IN.uv_EmissiveTex) * _EmissiveValue;
	o.Albedo = mainTex.rgb;
	o.Emission = emissiveTex.rgb + texCUBE (_Cube, IN.worldRefl).rgb * complexTex.g * _CubeIntensity;
	o.Alpha = complexTex.b * IN.color.a * _Color.a;

}
ENDCG 
}

Fallback "Mobile/VertexLit"
}
