Shader "Unlit/FlameShader"
{
    Properties
    {
        _AlbedoMap ("Albedo map", 2D) = "white" {}
        _RoughnessMap ("Roughness map", 2D) = "black" {}
        _NoiseTexture ("Noise texture", 2D) = "black" {}
        _Color1 ("Color 1", Color) = (1, 1, 1, 1)
        _Color2 ("Color 2", Color) = (1, 1, 1, 1)
        _Intensity ("Intensity", Range(0, 1)) = 1
    }
    SubShader
    {

        Pass
        {
            Tags { "RenderType"="Opaque"}
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "UnityLightingCommon.cginc"
            #include "Assets/Shaders/Utilities.cginc"

            struct VertexInput
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct FragmentInput
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : TEXCOORD1;
                float3 wPos : TEXCOORD2;
            };

            sampler2D _AlbedoMap;
            float4 _AlbedoMap_ST;
            sampler2D _RoughnessMap;
            float4 _RoughnessMap_ST;

            FragmentInput vert (VertexInput v)
            {
                FragmentInput o;
                float4 model = mul(UNITY_MATRIX_M, v.vertex);
                o.wPos = model;
                o.vertex = mul(UNITY_MATRIX_VP, model);
                o.uv = TRANSFORM_TEX(v.uv, _AlbedoMap);
                o.normal = UnityObjectToWorldNormal(v.normal);
                return o;
            }

            fixed4 frag(FragmentInput i) : SV_Target
            {
                float3 L = normalize(_WorldSpaceLightPos0);
                float3 V = normalize(_WorldSpaceCameraPos - i.wPos);
                float3 H = normalize(L + V);
                float3 N = normalize(i.normal);
                float Lambert = saturate(dot(L, N));
                float DiffuseLight = _LightColor0 * Lambert;
                float Gloss = tex2D(_RoughnessMap, i.uv).x;
                float SpecularLight = saturate(dot(H, N)) * (Lambert > 0);
                float SpecularExponent = exp2(1 + Gloss * 11);
                SpecularLight = pow(SpecularLight, SpecularExponent) * Gloss;
                return float4(_LightColor0.xyz * SpecularLight + tex2D(_AlbedoMap, i.uv) * DiffuseLight, 1.0);
            }
            ENDCG
        }

        Pass
        {
            Tags { "RenderType" = "Transparent" "Queue" = "Transparent" }
            Blend One One

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "Assets/Shaders/Utilities.cginc"

            struct VertexInput
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct FragmentInput
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            FragmentInput vert(VertexInput v)
            {
                FragmentInput o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            #define TAU 6.283185307
            #define PI 3.1415926535

            sampler2D _NoiseTexture;
            float4 _NoiseTexture_ST;
            float4 _Color1;
            float4 _Color2;
            float _Intensity;

            fixed4 frag(FragmentInput i) : SV_Target
            {
                float Noise = tex2D(_NoiseTexture, float2(i.uv.x + _Time.y / 5, i.uv.y + _Time.y)).x;
                fixed3 Mix = (1 - Noise) * _Color1 + Noise * _Color2;
                return _Intensity * fixed4(Mix, 1);
            }
            ENDCG
        }
    }
}
