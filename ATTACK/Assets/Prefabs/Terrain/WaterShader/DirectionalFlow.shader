// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Custom/DirectionalFlow" {
	Properties{
		_Color("Color", Color) = (1,1,1,1)
		[NoScaleOffset] _MainTex("Deriv (AG) Height (B)", 2D) = "black" {}
		_Tiling("Tiling", Float) = 1
		_Speed("Speed", Float) = 1
		_Glossiness("Smoothness", Range(0,1)) = 0.5
	}
		SubShader{
			Tags { "RenderType" = "Opaque" }
			LOD 200

			CGPROGRAM
			#pragma surface surf Standard fullforwardshadows vertex:vert
			#pragma target 3.0

			#include "Assets/Shaders/Flow.cginc"

			sampler2D _MainTex;
			float _Tiling, _Speed;

			struct Input {
				float2 uv_MainTex;
				float3 vertexNormal;
			};

			half _Glossiness;
			half _Metallic;
			fixed4 _Color;

			void vert(inout appdata_full v, out Input o) {
				UNITY_INITIALIZE_OUTPUT(Input, o);
				o.vertexNormal = v.normal;// mul(unity_ObjectToWorld, float4(v.normal, 0.0)).xyz;
			}

			float3 UnpackDerivativeHeight(float4 textureData) {
				float3 dh = textureData.agb;
				dh.xy = dh.xy * 2 - 1;
				return dh;
			}

			float map(float value, float min1, float max1, float min2, float max2) {
				return min2 + (value - min1) * (max2 - min2) / (max1 - min1);
			}

			void surf(Input IN, inout SurfaceOutputStandard o) {
				float normalDot = dot(IN.vertexNormal, float3(0.0f, 1.0f, 0.0f));
				float localSpeed = - _Speed * (1 - normalDot);
				//float localSpeed = _Speed * map(normalDot, 0, 1, 0, -1);
				float time = _Time.y * localSpeed;
				float2 uvFlow = DirectionalFlowUVW(IN.uv_MainTex, float2(0, 1), _Tiling, time);
				float3 dh = UnpackDerivativeHeight(tex2D(_MainTex, uvFlow));
				fixed4 c = dh.z * dh.z * _Color;
				//o.Albedo = float3(normalDot, normalDot, normalDot) - 2;
				o.Albedo = IN.vertexNormal;
				o.Albedo = c.rgb;
				o.Normal = normalize(float3(-dh.xy, 1));
				o.Smoothness = _Glossiness;
				o.Alpha = c.a;
			}
			ENDCG
		}
			FallBack "Diffuse"
}