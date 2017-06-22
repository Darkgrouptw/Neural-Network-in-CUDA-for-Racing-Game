Shader "SD4M/Transparent/Emissive" {
Properties {
	_Color ("Main Color", Color) = (1,1,1,1)
	_MainTex ("Base (RGB)", 2D) = "white" {}
	_ComplexTex ("Trans (B)", 2D) = "white" {}
	_EmissiveTex ("Emissive (RGB)", 2D) = "black" {}
	_EmissiveValue ("Emissive value", Float) = 0.5
}
SubShader {
	ColorMask RGB
	Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
	LOD 150

CGPROGRAM
#pragma surface surf Lambert alpha

sampler2D _MainTex;
sampler2D _ComplexTex;
sampler2D _EmissiveTex;
float _EmissiveValue;
fixed4 _Color;

struct Input {
	float2 uv_MainTex;
	float2 uv_EmissiveTex;
	float4 color: COLOR;
};

void surf (Input IN, inout SurfaceOutput o) {
	fixed4 mainTex = tex2D(_MainTex, IN.uv_MainTex) * IN.color * _Color;
	fixed4 emissiveTex = tex2D(_EmissiveTex, IN.uv_EmissiveTex) * _EmissiveValue;
	o.Albedo = mainTex.rgb;
	o.Emission = emissiveTex.rgb;
	o.Alpha = tex2D(_ComplexTex, IN.uv_MainTex).b * IN.color.a * _Color.a;

}
ENDCG 
}

Fallback "Mobile/VertexLit"
}
