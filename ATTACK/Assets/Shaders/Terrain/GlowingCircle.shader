Shader "Unlit/GlowingCircle"
{
    Properties
    {
        _Angle("Angle", Range(0, 1)) = 0
        _Color("Color", Color) = (1, 1, 1, 1)
        [IntRange] _Divisions("Divisions", Range(2, 5)) = 2
        _Exposure("Exposure", Range(1, 5)) = 1
        _CupHeight("Cup height", Range(0, 0.5)) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent"}

        Pass
        {
            Blend One One
            ZWrite Off
            Cull Off
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

            float _CupHeight;

            FragmentInput vert (VertexInput v)
            {
                FragmentInput o;
                float2 centeredUV = v.uv * 2 - float2(1, 1);
                o.uv = centeredUV;
                float invRadius = (1 - length(centeredUV));
                v.vertex.y += _CupHeight * (invRadius * 7 - 0.33);
                o.vertex = UnityObjectToClipPos(v.vertex);
                return o;
            }

            float4 _Color;
            float _Angle;
            float _Exposure;
            uint _Divisions;

            #define TAU 6.28318530718
            #define PI 3.14159265359

            fixed4 frag(FragmentInput i) : SV_Target
            {
                float len = saturate(length(i.uv));
                float centerValue = 0.95;
                float distToEdge = max(0.05 * (1 - _CupHeight), 0.01);
                float outer = smoothstep(centerValue, centerValue + distToEdge, len);
                float inner = 1 - smoothstep(centerValue - distToEdge, centerValue, len);
                float alpha = 1 - saturate(outer + inner);

                float Angle = (TAU - _Angle) * TAU;
                float circle = fmod(Angle + (PI + atan2(i.uv.y, i.uv.x)), TAU) / TAU;
                float div = 1 / (float)_Divisions;
                float t = 1 - saturate(InverseLerp(1 - min(div * 1/16, 0.95), 1, circle));
                float t2 = saturate(InverseLerp(0.5, 1 - div * 1/8, circle));
                circle = min(t, t2);
                alpha *= circle;

                float3 color = lerp(_Color.rgb, float3(1, 1, 1), alpha * alpha);

                return _Exposure * float4(color * alpha, alpha);
            }
            ENDCG
        }
    }
}
