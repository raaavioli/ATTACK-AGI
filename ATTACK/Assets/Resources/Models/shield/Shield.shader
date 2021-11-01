Shader "Custom/Shield2"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        [HDR] _Emission ("Emission", color) = (0,0,0)

        _FresnelColor ("FresnelColor", Color) = (1,1,1,1)
        _FresnelStrength ("Fresnel Strength", Range(0.25, 6)) = 1

        _TransOffset ("Transparancy Offset", Range(0, 0.3)) = 0
    }
    SubShader
    {
        Tags{ "RenderType"="Transparent" "Queue"="Transparent"}

		Blend SrcAlpha OneMinusSrcAlpha
		ZWrite off

        LOD 200

        CGPROGRAM

        #pragma surface surf Standard fullforwardshadows alpha:fade
        #pragma target 3.0

        sampler2D _MainTex;
        fixed4 _Color;

        struct Input
        {
            float2 uv_MainTex;
            float3 worldNormal;
            float3 viewDir;
            INTERNAL_DATA
        };

        half _Glossiness;
        half _Metallic;
        half _Emission;

        fixed4 _FresnelColor;
        float _FresnelStrength;

        float _TransOffset;

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a + _TransOffset;

            // Fresnel effect
            float fresnel = dot(IN.worldNormal, IN.viewDir);
            fresnel = saturate(1-fresnel);
            fresnel *= _FresnelStrength;
            o.Emission = _Emission + fresnel * _FresnelColor;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
