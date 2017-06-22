Shader "SD4M/Specular" {
Properties {
	_Shininess ("Shininess", Range (0.03, 1)) = 0.078125
	_TintValue ("Tint Value", Range (0, 1)) = 0
	_TintColor ("Tint Color", Color) = (0.5, 0.5, 0.5, 1)
	_SpecColor ("Specular Color", Color) = (0.5, 0.5, 0.5, 1)
	_GlossValue ("Gloss Value", Float) = 1.0
	_MainTex ("Base (RGB)", 2D) = "white" {}
	_ComplexTex ("Gloss (R)", 2D) = "white" {}
}
SubShader {
	ColorMask RGB
	Tags { "RenderType"="Opaque" "Queue" = "Geometry"}
	LOD 150

CGPROGRAM
#pragma surface surf MobileBlinnPhong exclude_path:prepass novertexlights

inline fixed4 LightingMobileBlinnPhong (SurfaceOutput s, fixed3 lightDir, half3 viewDir, fixed atten)
{
	half3 h = normalize(lightDir + viewDir);
	fixed diff = max (0, dot (s.Normal, lightDir));
	fixed nh = max (0, dot (s.Normal, h));
	fixed spec = pow (nh, s.Specular*128) * s.Gloss;
	
	fixed4 c;
	c.rgb = (s.Albedo * _LightColor0.rgb * diff + _SpecColor.rgb * _LightColor0.rgb * spec) * (atten*2);
	c.a = 0.0;
	return c;
}

sampler2D _MainTex;
sampler2D _ComplexTex;
half _Shininess;
fixed4 _TintColor;
fixed _TintValue;
half _GlossValue;


struct Input {
	float2 uv_MainTex;
	float4 color: COLOR;
};

void surf (Input IN, inout SurfaceOutput o) {
	fixed4 mainTex = tex2D(_MainTex, IN.uv_MainTex);
	fixed4 complexTex = tex2D(_ComplexTex, IN.uv_MainTex);
	mainTex.rgb *= _TintColor.rgb * _TintValue * IN.color.rgb;
	o.Albedo = mainTex.rgb;
	o.Alpha = 0.0f;
	o.Gloss = complexTex.r * _GlossValue;
	o.Specular = _Shininess;
}
ENDCG 
}

Fallback "Mobile/VertexLit"
}
