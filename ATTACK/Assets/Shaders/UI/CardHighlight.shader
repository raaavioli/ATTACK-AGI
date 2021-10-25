Shader "Unlit/CardHighlight"
{
    Properties
    {
        _Color ("Color", Color) = (1, 1, 1, 1)
        _Angle ("Angle", Range(0, 1)) = 0
        _BorderWidth ("Border width", Range(0, 1)) = 0.1666
        _BorderFade ("Fade", Range(0, 1)) = 0
        _Rotating("Rotating", Int) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        BlendOp Add
        Blend One DstColor

        Pass
        {
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

            struct FragInput
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float2 centeredUV : TEXCOORD1;
            };

            float4 _Color;
            float _Angle;
            float _BorderWidth;
            float _BorderFade;
            int _Rotating;

            FragInput vert (VertexInput v)
            {
                FragInput o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.centeredUV = v.uv * 2 - 1;
                return o;
            }

            #define TAU 6.28318530718
            #define PI 3.14159265359
   
            #define CARD_WIDTH 300
            #define CARD_HEIGHT 216
            #define CARD_ASPECT (CARD_WIDTH / CARD_HEIGHT)

            fixed4 frag (FragInput i) : SV_Target
            {
                float len = saturate(length(i.centeredUV));
                float distToXEdge = 1 / 6.0;
                float distToYEdge = distToXEdge / CARD_ASPECT;
                float outerBorder = smoothstep(_BorderFade * distToXEdge, distToXEdge, 1 - abs(i.centeredUV.x));
                outerBorder *= smoothstep(_BorderFade * distToYEdge, distToYEdge, 1 - abs(i.centeredUV.y));

                float innerBorder = smoothstep(_BorderWidth + _BorderFade * distToXEdge, _BorderWidth + distToXEdge, 1 - abs(i.centeredUV.x));
                innerBorder *= smoothstep(_BorderWidth + _BorderFade * distToYEdge, _BorderWidth + distToYEdge, 1 - abs(i.centeredUV.y));
                innerBorder = 1 - innerBorder;
                float border = min(innerBorder, outerBorder);

                float Angle = (TAU - _Angle) * TAU;
                float circle = fmod(Angle + (PI + atan2(i.centeredUV.y, i.centeredUV.x)), TAU) / TAU;
                float t = 1 - saturate(InverseLerp(0.98, 1, circle));
                float t2 = saturate(InverseLerp(0.1, 1 - 1 / 8, circle));
                circle = min(t, t2);
                circle = max(circle, 1 - min(1, _Rotating));
                return float4(circle, circle, circle, circle * border) * _Color * (border + 0.0);
            }
            ENDCG
        }
    }
}
