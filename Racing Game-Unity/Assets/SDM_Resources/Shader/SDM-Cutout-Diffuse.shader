Shader "SD4M/Transparent/Cutout/Diffuse" {
Properties {
	_Color ("Main Color", Color) = (1,1,1,1)
	_MainTex ("Base (RGB)", 2D) = "white" {}
	_ComplexTex ("Trans (B)", 2D) = "white" {}
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
fixed4 _Color;

struct Input {
	float2 uv_MainTex;
	float4 color: COLOR;
};

void surf (Input IN, inout SurfaceOutput o) {
	fixed4 mainTex = tex2D(_MainTex, IN.uv_MainTex);
	fixed4 complexTex = tex2D(_ComplexTex, IN.uv_MainTex);
	mainTex.rgb *= _Color.rgb * IN.color.rgb;
	o.Albedo = mainTex.rgb;
	o.Alpha = complexTex.b * _Color.a;

}
ENDCG 
}

Fallback "Mobile/VertexLit"
}
