Shader "Custom/DistortionFlow" {
	Properties{
		_Color("Color", Color) = (1,1,1,1)
		_MainTex("Albedo (RGB)", 2D) = "white" {}
		[NoScaleOffset] _FlowMap("Flow (RG, A noise)", 2D) = "black" {}
		_Glossiness("Smoothness", Range(0,1)) = 0.5
		_Metallic("Metallic", Range(0,1)) = 0.0
	}
		SubShader{
			Tags { "RenderType" = "Opaque" }
			LOD 200

			CGPROGRAM
			#pragma surface surf Standard fullforwardshadows
			#pragma target 3.0

			#include "Assets/Shaders/Flow.cginc"

			sampler2D _MainTex, _FlowMap;

			struct Input {
				float2 uv_MainTex;
			};

			half _Glossiness;
			half _Metallic;
			fixed4 _Color;

			void surf(Input IN, inout SurfaceOutputStandard o) {
				float2 flowVector = tex2D(_FlowMap, IN.uv_MainTex).rg * 2 - 1;
				float noise = tex2D(_FlowMap, IN.uv_MainTex).a;
				float time = _Time.y + noise;

				float3 uvwA = FlowUVW(IN.uv_MainTex, flowVector, time, false);
				float3 uvwB = FlowUVW(IN.uv_MainTex, flowVector, time, true);

				fixed4 texA = tex2D(_MainTex, uvwA.xy) * uvwA.z;
				fixed4 texB = tex2D(_MainTex, uvwB.xy) * uvwB.z;

				fixed4 c = (texA + texB) * _Color;

				o.Albedo = c.rgb;
				o.Metallic = _Metallic;
				o.Smoothness = _Glossiness;
				o.Alpha = c.a;
			}
			ENDCG
		}

			FallBack "Diffuse"
}