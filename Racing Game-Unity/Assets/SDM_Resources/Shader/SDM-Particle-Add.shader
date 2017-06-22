Shader "SD4M/Particle/Add" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_AddStrength ("AddStrength", Float)= 1.0
		_TintColor ("Tint Color", Color) = (1,1,1,1)
	}
	SubShader {
		Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
		Blend SrcAlpha One
		ColorMask RGB
		Cull Off Lighting Off ZWrite Off
		LOD 200
		Fog {Mode Off}

		BindChannels {
		Bind "Color", color
		Bind "Vertex", vertex
		Bind "TexCoord", texcoord
	}
		
		CGPROGRAM
		#pragma surface surf Unlit
		#pragma fragmentoption ARB_precision_hint_fastest

		half4 LightingUnlit (SurfaceOutput s, half3 lightDir, half atten) {
			half4 c;
			c.rgb = s.Albedo;
			c.a = s.Alpha;
			return c;
		}

		sampler2D _MainTex;
		half _AddStrength;
		half4 _TintColor;

		struct Input {
			fixed4 color : COLOR;
			float2 uv_MainTex;
		};

		void surf (Input IN, inout SurfaceOutput o) {
			half4 c = tex2D (_MainTex, IN.uv_MainTex);
			o.Albedo = (c.rgb*_TintColor*IN.color.rgb)*_AddStrength;
			o.Alpha = c.a*IN.color.a;
		}
		ENDCG
	} 
	FallBack "Diffuse"
}
