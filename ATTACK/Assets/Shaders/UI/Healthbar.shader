Shader "Unlit/HealthBar"
{
    Properties
    {
        _Health("Health", Range(0, 1)) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        blend SrcAlpha OneMinusSrcAlpha

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

            struct FragmentInput
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float2 centeredUV : TEXCOORD1;
            };

            FragmentInput vert (VertexInput v)
            {
                FragmentInput o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.centeredUV = v.uv * 2 - 1;
                return o;
            }

            #define BAR_WIDTH 300
            #define BAR_HEIGHT 216
            #define BAR_ASPECT (BAR_WIDTH / BAR_HEIGHT)
            #define PI 3.1415926535

            float _Health;

            fixed4 frag(FragmentInput i) : SV_Target
            {
                // Shape
                float steps = 40;
                float radius = 5;
                float centerLineY = clamp(i.uv.y * steps, radius, steps - radius);
                float2 centerLine = float2(0.5 * 4, centerLineY);
                float2 stretchedUV = float2(i.uv.x * 4, i.uv.y * steps);
                float2 distToLine = 1 - length(centerLine - stretchedUV);
                float borderMask = 1 - smoothstep(0, 0.2, distToLine * distToLine);
                float clampedDist = clamp(distToLine, 0.0, 1.0);
                float mask = smoothstep(0, 0.001, clampedDist * clampedDist);
                float highlight = pow(1 - abs(i.centeredUV.x + 0.2), 4);
                fixed3 highlight3 = fixed3(highlight, highlight, highlight);

                // Missing health
                float lowHealth = 0.4;
                float missingFade = 0.001 + 0.1 * clamp(InverseLerp(0.0, lowHealth, _Health), 0.0, 1.0);
                float missingHealth = clamp(InverseLerp(_Health + missingFade, _Health, (i.uv.y - 0.1) * 1.2), 0.1, 1);

                // Color
                float t = clamp(InverseLerp(lowHealth, 0, _Health), 0.0, 1.0) * sin(2 * 2 * PI * _Time.z / (1 + _Health * 5));
                fixed3 red = (1 - t) * fixed3(1, 0, 0) + t * fixed3(0.4, 0.1, 0.1);
                fixed3 green = fixed3(0.1, 0.5, 0.1);
                float colorT = clamp(_Health * 1.2, 0.0, 1.0);
                fixed3 color = ((1 - colorT) * red + colorT * green) * missingHealth;
                fixed3 borderColor = fixed3(0.2, 0.2, 0.2) * color;

                return fixed4(lerp(color + 0.5 * highlight3, borderColor, borderMask), mask);
            }
            ENDCG
        }
    }
}
