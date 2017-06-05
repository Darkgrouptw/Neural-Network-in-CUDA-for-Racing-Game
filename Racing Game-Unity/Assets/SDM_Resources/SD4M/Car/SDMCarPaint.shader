// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "SD4M/Car/SDMCarPaint" {
	Properties{
		_MainTex("Base Color", 2D) = "white" {}
	_ComplexTex("Reflec(R) Plastic Spec(G) Layer(B)", 2D) = "white" {}
	_EmissiveMap("Emissive (RGB) FrontBack Mask (A)", 2D) = "black" {}
	_ItashaTex("Itasha", 2D) = "black" {}		//彩繪圖

	_MainColor("Diffuse and Ambient Color", Color) = (1,1,1,1)
		_ColorVector("Diffuse(X) Ambient(Y) Reflection(Z) LighttingScale(W)", Vector) = (0.7,0.1,0.2,1)

		SeperateLine1("********* Specular settings ********", Range(0.01, 10)) = 10

		_SpecularColor_1("Specular Color 1", Color) = (0,0,0,1)
		_SpecularColor_2("Specular Color 2", Color) = (0,0,0,1)
		_SpecularVector("Specular: Shininess_1(X) Amount_1(Y) Shininess_2(Z) Amount_2(W)", Vector) = (1,0.5,1,0.5)

		SeperateLine2("********* Back light settings ********", Range(0.01, 10)) = 10
		_ComplexVector("_EmiDayNight(X) _EmiBrake(Y) Plastic Reflection(z)", Vector) = (0,0,2,0)

		_Cube("Cubemap", Cube) = "" {}
	}
		SubShader{
		Tags{
		"Queue" = "Geometry+10"
		"RenderType" = "Opaque"
		"ObjectType" = "CarBody"
	}

		CGPROGRAM
//#pragma only_renderers gles d3d9 opengl
#pragma glsl
		//#pragma surface surf BlinnPhong vertex:vert noambient nolightmap noforwardadd 
#pragma surface surf CarPaintBlinnPhong vertex:vert noambient nolightmap noforwardadd 
#pragma target 3.0
#pragma multi_compile CombineOff CombineOn
#include "UnityCG.cginc"

		sampler2D _MainTex;
	sampler2D _ComplexTex;
	sampler2D _EmissiveMap;
	sampler2D _ItashaTex;
	samplerCUBE _Cube;

	half4 _MainColor;
	half4 _SpecularColor_1;
	half4 _SpecularColor_2;

	half4 _ColorVector;
	half4 _SpecularVector;
	half4 _ComplexVector;

	half3 SHLight;
	//---Internal use------------------------
	half layer;

	struct Input {
		float2 uv_MainTex;
		half3 worldRefl;
		half4 emisItashaUV; //xy為車燈UV, zw為彩繪UV
		half3 vLight;
	};

	struct appdata_custom {
		half4 vertex : POSITION;
		half3 normal : NORMAL;
		half4 texcoord : TEXCOORD0;
		half4 texcoord1: TEXCOORD1;
		half4 color : COLOR;
	};

	void vert(inout appdata_custom v, out Input data)
	{
		UNITY_INITIALIZE_OUTPUT(Input,data);
		float3 worldNormal = normalize(mul(unity_ObjectToWorld, float4(v.normal, 0.0)).xyz);
		data.vLight = ShadeSH9(float4(worldNormal, 1));
#if defined (CombineOn)
		data.emisItashaUV.xy = _ComplexVector.yx * 0.25f + v.texcoord.xy;
		data.emisItashaUV.x += 0.5f;
		data.emisItashaUV.zw = v.texcoord.xy * 2.0f;
#else
		data.emisItashaUV.xy = _ComplexVector.yx * 0.5f + v.texcoord.xy;
		data.emisItashaUV.zw = v.texcoord.xy;
#endif
	}

	void surf(Input IN, inout SurfaceOutput o) {

		half4 baseColor = tex2D(_MainTex, IN.uv_MainTex);
		half3 reflrgb = texCUBE(_Cube, IN.worldRefl).rgb;
		half4 complexColor = tex2D(_ComplexTex, IN.uv_MainTex);
		half4 itashaColor = tex2D(_ItashaTex, IN.emisItashaUV.zw);

		float invItashaAlpha = 1.0f - itashaColor.a;
		half3 itashaColorMulAlpha = itashaColor.rgb * itashaColor.a;

		layer = complexColor.b;
		//在device上，用if判斷反而比用很多的數學運算來的快           
		//            half isCarPaint = max(sign(layer - 0.7), 0);
		//            half isPlastic = max(sign(0.7 - layer), 0) * max(sign(layer - 0.3), 0);
		//            half isBackLight = max(sign(0.3 - layer), 0) * max(sign(layer - 0.1), 0);
		//            half isMirror = max(sign(0.1 - layer), 0);
		//			o.Albedo = baseColor.rgb * (1 - isCarPaint) + isCarPaint * baseColor.rgb * (_MainColor.rgb * invItashaAlpha + itashaColorMulAlpha);
		//			o.Albedo = o.Albedo * (1 - isMirror);		
		//
		//			half4 itashaColor = tex2D (_ItashaTex, IN.itashaPaintUV);  
		//			emisColor = emisColor * isBackLight;
		//			o.Emission = ambientPart + emisColor + reflrgb * (_ColorVector.z* o.Albedo * (1 -isPlastic) + isPlastic) * complexColor.r;
		//			o.Emission = o.Emission * (1 - isMirror) + reflrgb * isMirror;

		//            if(layer > 0.7)
		//    	        o.Emission = ambientPart + reflrgb * o.Albedo * complexColor.r * _ColorVector.z;
		//    	    else if(layer > 0.3)
		//    	    {
		//    	        o.Emission = ambientPart + reflrgb * complexColor.r;
		//    	    }
		//    	    else if(layer > 0.1)
		//    	    {
		//    	    	half4 emisColor = tex2D (_EmissiveMap, IN.emisItashaUV.xy); 
		//				o.Emission = ambientPart + emisColor + reflrgb * o.Albedo * complexColor.r * _ColorVector.z;
		//			}
		//            else
		//				o.Emission = reflrgb;

		//PC 上用step比較快，但device上用max比較快
		//            float isCarPaint = step(0.7, layer);
		//            float isPlastic = step(0.3, layer) * step(layer, 0.7);
		//            float isBackLight = step(0.1, layer) * step(layer, 0.3);
		//            float isMirror = step(0, layer) * step(layer, 0.1);

		//1 --車漆-- 0.7 --塑膠/金屬-- 0.3 --車燈-- 0.1 --後照鏡-- 0


		_SpecularColor_1.rgb = _SpecularColor_1.rgb * invItashaAlpha + itashaColorMulAlpha;
		_SpecularColor_2.rgb = _SpecularColor_2.rgb * invItashaAlpha + itashaColorMulAlpha;

		if (layer > 0.7)
		{
			o.Albedo = baseColor.rgb * (_MainColor.rgb * invItashaAlpha + itashaColorMulAlpha);
			half3 ambientPart = _ColorVector.y * o.Albedo;
			o.Emission = ambientPart + reflrgb * complexColor.r * _ColorVector.z;
		}
		else if (layer > 0.3)	//塑膠/金屬層的albedo不吃mainColor及彩繪
		{
			//塑膠/金屬層的specular會受彩繪的mask影響，所以外殼的彩繪必須要設對mask，
			//而內裝的塑膠金屬層要把complexColor.g畫成為0，否則會有奇怪的效果
			_SpecularVector.y = complexColor.g;
			_SpecularVector.w = 0;
			o.Albedo = baseColor.rgb;
			half3 ambientPart = _ColorVector.y * o.Albedo;
			o.Emission = ambientPart + reflrgb * complexColor.r *_ComplexVector.z;
		}
		else if (layer > 0.1)	//車燈層的albedo不吃mainColor及彩繪
		{
			o.Albedo = baseColor.rgb;
#if defined (CombineOn)
			half4 emisColor = tex2D(_MainTex, IN.emisItashaUV.xy);
#else
			half4 emisColor = tex2D(_EmissiveMap, IN.emisItashaUV.xy);
#endif
			half3 ambientPart = _ColorVector.y * o.Albedo;
			o.Emission = ambientPart + emisColor + reflrgb * complexColor.r * _ColorVector.z;
		}
		else
		{
			o.Albedo = 0;
			o.Emission = reflrgb;
		}

		o.Alpha = 1;

		SHLight = IN.vLight;
	}

	inline half4 LightingCarPaintBlinnPhong(SurfaceOutput s, half3 lightDir, half3 viewDir, half atten)
	{
		half3 H = normalize(lightDir + viewDir);
		half3 normalized_N = normalize(s.Normal);
		float NdotL = dot(normalized_N, lightDir);
		float NdotH = max(0, dot(normalized_N, H));

		half3 diffusePart = (NdotL*0.5 + 0.5) * _ColorVector.x * s.Albedo;
		half3 specularPart = 0;

		//不用layer的if來判斷是因為這樣反而慢，如果_SpecularVector.yw是0的時候，shader似乎會自己濾掉後面的power計算
		specularPart = _SpecularColor_1 * _SpecularVector.y * pow(NdotH, _SpecularVector.x * 128) +
			_SpecularColor_2 * _SpecularVector.w * pow(NdotH, _SpecularVector.z * 512);

		return half4((SHLight + _LightColor0.rgb) * (diffusePart + specularPart) * atten * _ColorVector.w, 1);
	}
	ENDCG
	}
		FallBack "Diffuse"
}
