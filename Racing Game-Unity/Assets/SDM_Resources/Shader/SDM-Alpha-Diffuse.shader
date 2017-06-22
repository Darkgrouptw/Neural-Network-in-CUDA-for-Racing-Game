Shader "SD4M/Transparent/Diffuse" {
Properties {
	_Color ("Main Color", Color) = (1,1,1,1)
	_MainTex ("Base (RGB)", 2D) = "white" {}
	_ComplexTex ("Trans (B)", 2D) = "white" {}
}
SubShader {
	ColorMask RGB
	Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
	LOD 150

CGPROGRAM
#pragma surface surf Lambert alpha

sampler2D _MainTex;
sampler2D _ComplexTex;
fixed4 _Color;

struct Input {
	float2 uv_MainTex;
	float4 color: COLOR;
};

void surf (Input IN, inout SurfaceOutput o) {
	fixed4 c = fixed4(tex2D(_MainTex, IN.uv_MainTex).rgb, tex2D(_ComplexTex, IN.uv_MainTex).b) * IN.color * _Color;
	o.Albedo = c.rgb;
	o.Alpha = c.a;

}
ENDCG 
}

Fallback "Mobile/VertexLit"
}
