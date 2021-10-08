Shader "Custom/DirectionalFlow" {
	Properties{
		_Color("Color", Color) = (1,1,1,1)
		[NoScaleOffset] _MainTex("Deriv (AG) Height (B)", 2D) = "black" {}
		[NoScaleOffset] _FlowMap("Flow (RG)", 2D) = "black" {}
		_Tiling("Tiling", Float) = 1
		_Speed("Speed", Float) = 1
		_FlowStrength("Flow Strength", Float) = 1
		_HeightScale("Height Scale, Constant", Float) = 0.25
		_HeightScaleModulated("Height Scale, Modulated", Float) = 0.75
		_Glossiness("Smoothness", Range(0,1)) = 0.5
		_Metallic("Metallic", Range(0,1)) = 0.0
	}
		SubShader{
			Tags { "RenderType" = "Opaque" }
			LOD 200

			CGPROGRAM
			#pragma surface surf Standard fullforwardshadows vertex:vert
			#pragma target 3.0

			#include "Assets/Shaders/Flow.cginc"

			sampler2D _MainTex, _FlowMap;
			float _Tiling, _Speed, _FlowStrength;
			float _HeightScale, _HeightScaleModulated;

			struct Input {
				float2 uv_MainTex;
				float3 vertexNormal;
			};

			half _Glossiness;
			half _Metallic;
			fixed4 _Color;

			void vert(inout appdata_full v, out Input o) {
				UNITY_INITIALIZE_OUTPUT(Input, o);
				o.vertexNormal = normalize(v.normal);
			}

			float3 UnpackDerivativeHeight(float4 textureData) {
				float3 dh = textureData.agb;
				dh.xy = dh.xy * 2 - 1;
				return dh;
			}

			void surf(Input IN, inout SurfaceOutputStandard o) {
				float normalDot = dot(IN.vertexNormal, float3(0.0f, 1.0f, 0.0f));
				float localSpeed = _Speed * (1 - normalDot);
				float time = _Time.y * localSpeed;
				//float time = _Time.y * _Speed;
				float2 uvFlow = DirectionalFlowUVW(IN.uv_MainTex, float2(0, 1), _Tiling, time);
				float3 dh = UnpackDerivativeHeight(tex2D(_MainTex, uvFlow));
				fixed4 c = dh.z * dh.z * _Color;
				o.Albedo = c.rgb;
				o.Normal = normalize(float3(-dh.xy, 1));
				o.Metallic = _Metallic;
				o.Smoothness = _Glossiness;
				o.Alpha = c.a;
			}
			ENDCG
		}
			FallBack "Diffuse"
}