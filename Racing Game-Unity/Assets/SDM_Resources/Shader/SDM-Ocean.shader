Shader "SD4M/Ocean" {
Properties {
	_ReflectValue ("Reflect value", Range (0.03, 1)) = 0.5
	_AmbientColor ("Ambient Color", Color) = (0.5, 0.5, 0.5, 1)
	_MainTex ("Base (RGB)", 2D) = "white" {}
	_BumpMap ("Bumpmap", 2D) = "bump" {}
	_Cube ("Cubemap", CUBE) = "" {}
}
SubShader {
	ColorMask RGB
	Tags { "RenderType"="Opaque" "Queue" = "Geometry+5"}
	LOD 150

CGPROGRAM
//#pragma surface surf MobileBlinnPhong exclude_path:prepass nolightmap noforwardadd halfasview
#pragma surface surf Unlit exclude_path:prepass halfasview nolightmap noforwardadd novertexlights noambient 
#pragma target 3.0 
#pragma glsl
#pragma exclude_renderers d3d11_9x


sampler2D _MainTex;
sampler2D _BumpMap;
samplerCUBE _Cube;
float4 _AmbientColor;
float _ReflectValue;

struct Input {
	float2 uv_MainTex;
	float2 uv_BumpMap;
	float3 worldRefl; 
	INTERNAL_DATA  
};

half4 LightingUnlit (SurfaceOutput s, half3 lightDir, half atten)
{
	half4 c;
	c.rgb = s.Albedo;
	c.a = 0.0f;
	return c;
}

void surf (Input IN, inout SurfaceOutput o) {
	fixed4 mainTex = tex2D(_MainTex, IN.uv_MainTex+fixed2(_Time.x*0.7f, 0));
	o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_MainTex+fixed2(_Time.x*0.35f, 0)));
	float3 worldRefl = WorldReflectionVector (IN, o.Normal);
	fixed4 reflcol = texCUBE (_Cube, worldRefl);
	o.Albedo = mainTex.rgb*_AmbientColor;
	o.Alpha = mainTex.a; 
	o.Gloss = mainTex.a;
	o.Emission = reflcol*_ReflectValue;
}
ENDCG 
}

Fallback "Mobile/VertexLit"
}