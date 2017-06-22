// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

#warning Upgrade NOTE: unity_Scale shader variable was removed; replaced 'unity_Scale.w' with '1.0'

Shader "SD4M/Wet Reflection" {
Properties {
	_Shininess ("Shininess", Range (0.03, 1)) = 0.078125
	_TintValue ("Tint Value", Range (0, 1)) = 0
	_TintColor ("Tint Color", Color) = (0.5, 0.5, 0.5, 1)
	_SpecColor ("Specular Color", Color) = (0.5, 0.5, 0.5, 1)
	_GlossValue ("Gloss Value", Float) = 1.0
	_MainTex ("Base (RGB)", 2D) = "white" {}
	_ComplexTex ("Gloss (R)", 2D) = "white" {}
	_BumpMap ("Normalmap", 2D) = "bump" {}
	_RefMask ("RefMask", 2D) = "black" {}
	_RefIntensity ("RefIntensity", Float) = 1.0
	_NoiseMap ("NoiseMap", 2D) = "black" {}
	_CubeMap ("CubeMap", Cube) = "" {}
	_CubeMapIntensity ("CubeMapIntensity", Float) = 1.0
}
SubShader {
	ColorMask RGB
	Tags { "RenderType"="Opaque" "Queue" = "Geometry+1"}
	LOD 150

CGPROGRAM
#pragma surface surf MobileBlinnPhong exclude_path:prepass novertexlights vertex:vert
#pragma target 3.0
#pragma glsl

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
sampler2D _BumpMap;
half _Shininess;
fixed4 _TintColor;
fixed _TintValue;
half _GlossValue;
#ifndef UNITY_PASS_FORWARDADD
sampler2D _RefMap;
sampler2D _RefMask;
sampler2D _NoiseMap;
samplerCUBE _CubeMap;	
half _CubeMapIntensity;
half _RefIntensity;
#endif

struct Input {
	float2 uv_MainTex;
	float2 uv_BumpMap;

	float4 reflectionDirAndRim;
	float4 color: COLOR;
};

void vert (inout appdata_full v, out Input data)
{
	UNITY_INITIALIZE_OUTPUT(Input,data);
#ifndef UNITY_PASS_FORWARDADD
	float3 viewDir = normalize(WorldSpaceViewDir(v.vertex));
	float3 worldN = mul((float3x3)unity_ObjectToWorld, v.normal * 1.0);
	data.reflectionDirAndRim.xyz = reflect(-viewDir, worldN);
	float rim = saturate(dot(viewDir, worldN));
	data.reflectionDirAndRim.w = 1.0f - rim * rim;
#endif

}

void surf (Input IN, inout SurfaceOutput o) {
	fixed4 mainTex = tex2D(_MainTex, IN.uv_MainTex);
	fixed4 complexTex = tex2D(_ComplexTex, IN.uv_MainTex);
	mainTex.rgb *= _TintColor.rgb *_TintValue * IN.color.rgb;
	o.Normal = UnpackNormal (tex2D (_BumpMap, IN.uv_MainTex));
	o.Albedo = mainTex.rgb;
	o.Alpha = 0.0f;
	o.Gloss = complexTex.r * _GlossValue;
	o.Specular = _Shininess;

}
ENDCG 
}

Fallback "Mobile/VertexLit"
}
