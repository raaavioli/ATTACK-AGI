Shader "Unlit/RainbowTrailShader"
{
    Properties
    {
        _Intensity("Intensity", Range(1, 5)) = 1
        _Period("Period", Range(1, 100)) = 10
        _Gap("Gap", Range(1, 20)) = 10
        _SharpnessY("Sharpness Y", Range(1, 20)) = 1
    }
    SubShader
    {
        Tags { "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" "PreviewType" = "Plane" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

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
            int _Period;
            float _Gap;
            float _Intensity;
            float _SharpnessY;

            fixed3 rainbow(float t) {
                float3 Colors[5]; 
                Colors[0] = float3(227, 189, 217) / (255.0f);
                Colors[1] = float3(118, 110, 199) / (255.0f);
                Colors[2] = float3(118, 175, 236) / (255.0f);
                Colors[3] = float3(203, 153, 200) / (255.0f);
                Colors[4] = float3(81, 83, 199) / (255.0f);
                int ColorIndex = (int) (t * 5.0f);
                int Next = (ColorIndex + 1) % 5.0f;
                return lerp(Colors[ColorIndex], Colors[Next], (t * 5.0f - ColorIndex));
            }

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float pi = 3.14159265;

                float TransparencyX = pow((sin(i.uv.x * _Period * pi) + 1) / 2, _Gap);
                float TransparencyY = pow(1 - abs(0.5 - i.uv.y), _SharpnessY);

                return _Intensity * fixed4(rainbow(1 - i.uv.x), TransparencyX * TransparencyY);
            }
            ENDCG
        }
    }
}
