// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "SD4M/Sky/RampSky" {
	Properties {
		_TopColor ("Top Color", Color) = (1,0.5,0.5,1)
		_Top ("Top", Float) = 100.0
		_BottomColor ("Bottom Color", Color) = (1,1,1,1)
		_Bottom ("Bottom", Float) = 0.0
	}
	SubShader {
		ColorMask RGB
		Tags { "RenderType"="Opaque" "Queue" = "Geometry+10"}
	    Pass {
	        Fog { Mode Off }
			Lighting Off
			ZWrite Off
			ColorMask RGB
	        CGPROGRAM

	        #pragma vertex vert
	        #pragma fragment frag
	        
	        float4 _TopColor;
	        float4 _BottomColor;
			float _Top;
			float _Bottom;

	        // vertex input: position, UV
	        struct appdata {
	            float4 vertex : POSITION;
	            float4 texcoord : TEXCOORD0;
	        };

	        struct v2f {
	            float4 pos : SV_POSITION;
				float4 color : COLOR;
	        };
	        
	        v2f vert (appdata v) {
	            v2f o;
	            o.pos = UnityObjectToClipPos( v.vertex );
				o.pos.z = min(o.pos.z, o.pos.w * 0.9999999f);
				float worldY = mul(unity_ObjectToWorld, v.vertex).y;
				float rampLerp = saturate((worldY - _Bottom) / (_Top - _Bottom));
				o.color = float4(lerp(_BottomColor, _TopColor, rampLerp).rgb, 0.0f);
	            return o;
	        }
	        
	        float4 frag( v2f i ) : COLOR {
	            return i.color;
	        }
	        ENDCG
	    }
	}
	FallBack "Diffuse"
}
