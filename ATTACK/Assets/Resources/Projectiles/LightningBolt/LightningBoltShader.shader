Shader "Particles/LightningBoltShader"
{
    Properties
    {
        _Color("Color", Color) = (1, 1, 1, 1)
        _MaxLightCoreIntensity("Max light core intensity", Range(0.1, 5)) = 1
    }
    SubShader
    {
        Tags { "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" "PreviewType" = "Plane" }
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            float4 _Color;
            float _MaxLightCoreIntensity;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float pi = 3.14159265;

                // 1 at center of object, 0 at edge.
                float CenterYDist = abs(0.5 - i.uv.y) * 2;
                float CenterXDist = abs(0.5 - i.uv.x) * 2;
                float DistToCenter = max(0, 1 - length(float2(CenterXDist, CenterYDist)));

                float t = sin(i.uv.x * 13 * pi);
                float CoreColorRatio = lerp(DistToCenter, 1, t);

                t = sin(i.uv.x * 8 * pi);
                float MinCoreIntensity = _MaxLightCoreIntensity / 2;
                float CoreIntensity = lerp(MinCoreIntensity, _MaxLightCoreIntensity, t);
                float4 CoreColor = float4(CoreIntensity, CoreIntensity, CoreIntensity, 1);

                float4 Res = lerp(_Color, CoreColor, CoreColorRatio);
                return float4(Res.xyz, pow(DistToCenter, 1.5));
            }
            ENDCG
        }
    }
}
