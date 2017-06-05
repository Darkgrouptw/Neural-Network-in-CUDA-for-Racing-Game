Shader "SD4M/Car/SDMCarGlass" {
	Properties {
		SeperateLine1 ("********** One **********", Range (0.01, 10)) = 10	
	
		_OneColor("One Color", Color) = (0,0,0,1)
		
		SeperateLine2 ("********** Two **********", Range (0.01, 10)) = 10	
		
		_TwoColor("Two Color", Color) = (0,0,0,1)
		
		_ColorVector_1 ("Reflection_1(X) Refrection_1(Y) Reflection_2(Z) Refrection_2(W)", Vector) = (2,0.15,2,1)
		
		SeperateLine3 ("********* Three *********", Range (0.01, 10)) = 10	
		
		_ThreeColor("Three Color", Color) = (0,0,0,1)
		
//		SeperateLine4 ("********** Four *********", Range (0.01, 10)) = 10	
//		
//		_FourColor("Four Color", Color) = (0,0,0,1)
//		_FourReflFront	("Four Refl Front", Float) = 0
//		_FourReflSide	("Four Refl Side", Float) = 1
//		_FourReflSide2Front	("Four Refl Side2Front (<--)", Range (0.01, 10)) = 10
//		_FourRefrFront	("Four Refr Front", Float) = 0
//		_FourRefrSide	("Four Refr Side", Float) = 1	
		
		SeperateLine5 ("********** Five *********", Range (0.01, 10)) = 10	
		
		_FiveColor("Five Color", Color) = (0,0,0,1)
		
		_ColorVector_2 ("Reflection_3(X) Refrection_3(Y) Reflection_5(Z) Refrection_5(W)", Vector) = (1.25,0.2,2,0.15)
		
		SeperateLine6 ("********** Six **********", Range (0.01, 10)) = 10	
		
		_SixColor("Six Color", Color) = (0,0,0,1)
		
		_AmbientAmt ("Ambient Amount", Float) = 0
		_TrackAmbient ("Track Ambient", Float) = 0
		
		_ColorVector_3 ("Reflection_6(X) Refrection_6(Y)", Vector) = (1.25,0.2,2,0.15)
		
		_Spec2Color ("Specular Color2", Color) = (0, 0, 0, 0) 
		_ShininessTwo ("Shininess2", Range (0.01, 10)) = 10		
		
		_DiffuseTex ("Color (RGB) Range (A)", 2D) = "white" {}
	
		_Cube ("Cubemap", Cube) = "" {}												
	}
	SubShader {
		Tags { 
		  "Queue"="Transparent-10" 
		  "IgnoreProjector"="True" 
          "RenderType"="Transparent"
          "ObjectType"="CarBody"
        }
        Blend SrcAlpha OneMinusSrcAlpha
        
        Pass {
        	ZWrite On
        	ColorMask A
    	}
		
		CGPROGRAM
		//#pragma only_renderers gles d3d9
		#pragma glsl
		#pragma surface surf CarPaintBlinnPhong alpha noambient nodirlightmap noforwardadd
		#pragma target 3.0
		#pragma multi_compile RefitOff RefitOn
		#include "UnityCG.cginc"

		struct Input { 
			float2 uv_DiffuseTex;
			float3 worldRefl;
			float3 worldNormal;
		}; 
		
		sampler2D _DiffuseTex;
		
		fixed4 _OneColor;		
		fixed4 _TwoColor;		
		fixed4 _ColorVector_1;
		
		fixed4 _ThreeColor;
		
//		fixed4 _FourColor;
//		float _FourReflFront;
//		float _FourReflSide;
//		float _FourReflSide2Front;
//		float _FourRefrFront;
//		float _FourRefrSide;
		
		fixed4 _FiveColor;
		fixed4 _ColorVector_2;
		
		fixed4 _SixColor;				
	
		float _AmbientAmt;
		float _TrackAmbient;
		
		fixed4 _ColorVector_3;
	
		samplerCUBE _Cube;			
		
		fixed4 _Spec2Color;
		float _ShininessTwo;
		
//--Inside use------------------		
		float3 NormalWorld;
		float3 SHLight;
				
		
		void surf (Input IN, inout SurfaceOutput o) 
		{	
			NormalWorld = normalize(IN.worldNormal);
			float3 ReflWorld = normalize(IN.worldRefl);
			float NdotR = max(0, dot(NormalWorld, ReflWorld));		

		    SHLight = ShadeSH9(float4(NormalWorld, 1.0));																																								
			
			fixed4 refl = fixed4(0.0, 0.0, 0.0, 0.0);

			refl = texCUBE(_Cube, ReflWorld);
						
			fixed4 diff = tex2D(_DiffuseTex, IN.uv_DiffuseTex);
			fixed  diffa = diff.a;
						
			float _reflside, _refrside;
			if(diffa > 0.9)
			{
				o.Albedo = _OneColor.rgb * diff.rgb;
				_reflside = _ColorVector_1.x;
				_refrside = _ColorVector_1.y;
			}
			else if(diffa > 0.7)
			{
				o.Albedo = _TwoColor.rgb * diff.rgb;
				_reflside = _ColorVector_1.z;
				_refrside = _ColorVector_1.w;
			}
			else if(diffa > 0.5)
			{
				o.Albedo = _ThreeColor.rgb * diff.rgb;
				_reflside = _ColorVector_2.x;
				_refrside = _ColorVector_2.y;		
			}
//			else if(diffa > 0.3)
//			{
//				o.Albedo = _FourColor.rgb * diff.rgb;
//				_reflside = _FourReflSide;
//				_reflfront = _FourReflFront;
//				_reflside2front = _FourReflSide2Front;
//				_refrside = _FourRefrSide;
//				_refrfront = _FourRefrFront;				
//			}
			else if(diffa > 0.1)
			{
				o.Albedo = _FiveColor.rgb * diff.rgb;
				_reflside = _ColorVector_2.z;
				_refrside = _ColorVector_2.w;	
			}
			else
			{
				o.Albedo = _SixColor.rgb * diff.rgb;
				_reflside = _ColorVector_3.x;
				_refrside = _ColorVector_3.y;
			}									
			
			o.Emission = refl.rgb * _reflside;

			o.Emission += o.Albedo * _AmbientAmt * _TrackAmbient;			

			o.Alpha = _refrside;	
		}
		
		inline fixed4 LightingCarPaintBlinnPhong (SurfaceOutput s, fixed3 lightDir, half3 viewDir, fixed atten)
		{	
			//lightDir and viewDir are both in Worldspace
			float3 H = normalize(lightDir + viewDir);
			float NdotH = max(0, dot(NormalWorld, H));
			fixed diff = max(0, dot(NormalWorld, lightDir));

			fixed3 diffRGB = s.Albedo * diff;
			float3 specRGB2 = (_Spec2Color.rgb * pow(NdotH, _ShininessTwo * 512));

			return fixed4(fixed3((diffRGB + specRGB2) * (SHLight + _LightColor0.rgb) * atten), 0);	
		} 
		
		ENDCG
	} 
	FallBack "Transparent/VertexLit"
}

