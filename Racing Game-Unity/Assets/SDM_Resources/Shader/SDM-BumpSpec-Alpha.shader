Shader "SD4M/Transparent/Bumped Specular" {
Properties {
	_Shininess ("Shininess", Range (0.03, 1)) = 0.078125
	_SpecColor ("Specular Color", Color) = (0.5, 0.5, 0.5, 1)
    _TintValue ("Tint Value", Range (0, 3)) = 0
	_TintColor ("Tint Color", Color) = (0.5, 0.5, 0.5, 1)
	_GlossValue ("Gloss Value", Float) = 1.0
	_MainTex ("Base (RGB)", 2D) = "white" {}
	_ComplexTex ("Gloss (R) Trans (B)", 2D) = "white" {}
	_BumpMap ("Normalmap", 2D) = "bump" {}
}
SubShader {
	ColorMask RGB
	Tags { "RenderType"="Transparent" "Queue" = "Transparent" }
	LOD 150

CGPROGRAM
#pragma surface surf BlinnPhong exclude_path:prepass alpha

sampler2D _MainTex;
sampler2D _ComplexTex;
sampler2D _BumpMap;
half _Shininess;
fixed4 _TintColor;
fixed _TintValue;
half _GlossValue;

struct Input {
	float2 uv_MainTex;
	float2 uv_BumpMap;
	float4 color: COLOR;
};

void surf (Input IN, inout SurfaceOutput o) {
	fixed4 mainTex = tex2D(_MainTex, IN.uv_MainTex);
	fixed4 complexTex = tex2D(_ComplexTex, IN.uv_MainTex);
	o.Normal = UnpackNormal (tex2D (_BumpMap, IN.uv_BumpMap));
	o.Albedo = mainTex.rgb * IN.color.rgb * _TintColor.rgb * _TintValue;
	o.Alpha = complexTex.b;
	o.Gloss = complexTex.r * _GlossValue;
	o.Specular = _Shininess;

}
ENDCG 
}

Fallback "Mobile/VertexLit"
}
