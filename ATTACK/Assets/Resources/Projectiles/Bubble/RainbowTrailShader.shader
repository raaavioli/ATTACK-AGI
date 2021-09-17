Shader "Unlit/RainbowTrailShader"
{
    Properties
    {
        _Color1("Color 1", Color) = (1, 1, 1, 1)
        _Color2("Color 2", Color) = (1, 1, 1, 1)
        _Color3("Color 3", Color) = (1, 1, 1, 1)
        _Color4("Color 4", Color) = (1, 1, 1, 1)
        _Color5("Color 5", Color) = (1, 1, 1, 1)
        [MaterialToggle] _Wrap("Wrap colors", Float) = 1
        _Intensity("Intensity", Range(1, 5)) = 1
        _Period("Period", Range(1, 100)) = 10
        _Gap("Gap", Range(1, 20)) = 10
        _SharpnessY("Sharpness Y", Range(1, 20)) = 1
        _OffsetX("Offset", Range(0, 1)) = 0
    }
    SubShader
    {
        Tags { "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" "PreviewType" = "Plane" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

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

            float4 _Color1;
            float4 _Color2;
            float4 _Color3;
            float4 _Color4;
            float4 _Color5;
            bool _Wrap;
            int _Period;
            float _Gap;
            float _Intensity;
            float _SharpnessY;
            float _OffsetX;

            fixed4 rainbow(float t) {
                float4 Colors[6]; 
                Colors[0] = _Color1;
                Colors[1] = _Color2;
                Colors[2] = _Color3;
                Colors[3] = _Color4; 
                Colors[4] = _Color5; 
                Colors[5] = _Color5;
                int ColorIndex = (int) (t * 5.0f);
                int Next = _Wrap ? (ColorIndex + 1) % 5.0f : ColorIndex + 1;
                return lerp(Colors[ColorIndex], Colors[Next], (t * 5.0f - ColorIndex));
            }

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = saturate(float2(v.uv.x + _OffsetX, v.uv.y));
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float pi = 3.14159265;

                float TransparencyX = pow((sin(i.uv.x * _Period * pi) + 1) / 2, _Gap);
                float TransparencyY = pow(1 - abs(0.5 - i.uv.y), _SharpnessY);

                return _Intensity * rainbow(1 - i.uv.x) * float4(1, 1, 1, TransparencyX * TransparencyY);
            }
            ENDCG
        }
    }
}
