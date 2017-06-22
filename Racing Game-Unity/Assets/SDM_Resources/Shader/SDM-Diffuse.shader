Shader "SD4M/Diffuse" {
Properties {
	_Color ("Main Color", Color) = (1,1,1,1)
	_MainTex ("Base (RGB)", 2D) = "white" {}
}
SubShader {
	ColorMask RGB
	Tags { "RenderType"="Opaque" "Queue" = "Geometry"}
	LOD 150

CGPROGRAM
#pragma surface surf Lambert

sampler2D _MainTex;
fixed4 _Color;

struct Input {
	float2 uv_MainTex;
	float4 color: COLOR;
};

void surf (Input IN, inout SurfaceOutput o) {
	fixed4 mainTex = tex2D(_MainTex, IN.uv_MainTex) * IN.color * _Color;
	o.Albedo = mainTex.rgb;
	o.Alpha = 0.0f;

}
ENDCG 
}

Fallback "Mobile/VertexLit"
}
